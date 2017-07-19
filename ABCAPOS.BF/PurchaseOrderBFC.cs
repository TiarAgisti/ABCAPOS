using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using ABCAPOS.DA;
using ABCAPOS.Util;
using System.Transactions;
using ABCAPOS.ReportEDS;
using MPL;

namespace ABCAPOS.BF
{
    public class PurchaseOrderBFC : MasterDetailBFC<PurchaseOrder, v_PurchaseOrder, PurchaseOrderDetail, v_PurchaseOrderDetail, PurchaseOrderModel, PurchaseOrderDetailModel>
    {
        private void UpdateHppByPO(long purchaseOrderID)
        {
            var count = new ABCAPOSDAC().RetreiveCountRelatedBuildByPO(purchaseOrderID);
            if (count > 0)
            {
                var Build = new ABCAPOSDAC().RetreiveBuildByPO(purchaseOrderID);
                if (Build != null)
                {
                    double Total = 0;
                    var BuildDetail = new ABCAPOSDAC().RetreiveDetailBuildByPO(Build.AssemblyBuildID);
                    if (BuildDetail != null)
                    {
                        foreach (var detail in BuildDetail)
                        {
                            double amount = 0;
                            amount = detail.MovingOutQty * detail.ContainerPrice;
                            Total = Total + amount;
                        }
                    }
                    var BuildHeader = new AssemblyBuildBFC().RetrieveByID(Build.AssemblyBuildID);
                    if (BuildHeader != null)
                    {
                        BuildHeader.Hpp = Convert.ToDecimal(Total) / BuildHeader.QtyBuild;
                        new AssemblyBuildBFC().Update(BuildHeader);

                        var Log = new LogBFC().RetreiveLogByLogIDProductID(BuildHeader.LogID, BuildHeader.ProductID);
                        if (Log != null)
                        {
                            new ABCAPOSDAC().DeleteLogByLogIDContainerIDProductID(Log.LogID, Log.ContainerID, Log.ProductID);

                            var LogDetail = new LogDetailModel();
                            LogDetail.LogID = Log.LogID;
                            LogDetail.ItemNo = Log.ItemNo;
                            LogDetail.ContainerID = Log.ContainerID;
                            LogDetail.ProductID = Log.ProductID;
                            LogDetail.MovingInQty = Log.MovingInQty;
                            LogDetail.MovingInValue = Convert.ToDouble(BuildHeader.Hpp);
                            LogDetail.MovingOutQty = 0;
                            LogDetail.MovingOutValue = 0;
                            new ABCAPOSDAC().CreateLog(LogDetail);

                            var container = new ContainerBFC().RetrieveByID(Log.ContainerID);
                            if (container != null)
                            {
                                container.Price =Convert.ToDouble(BuildHeader.Hpp);
                                new ContainerBFC().Update(container);
                            }
                        }
                    }
                }
            }
        }

        protected override GenericDetailDAC<PurchaseOrderDetail, PurchaseOrderDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<PurchaseOrderDetail, PurchaseOrderDetailModel>("PurchaseOrderID", "LineSequenceNumber", false);
        }

        protected override GenericDetailDAC<v_PurchaseOrderDetail, PurchaseOrderDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_PurchaseOrderDetail, PurchaseOrderDetailModel>("PurchaseOrderID", "LineSequenceNumber", false);
        }

        protected override GenericDAC<PurchaseOrder, PurchaseOrderModel> GetMasterDAC()
        {
            return new GenericDAC<PurchaseOrder, PurchaseOrderModel>("ID", false, "Date DESC");
        }

        protected override GenericDAC<v_PurchaseOrder, PurchaseOrderModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_PurchaseOrder, PurchaseOrderModel>("ID", false, "Date DESC");
        }

        public override void Create(PurchaseOrderModel header, List<PurchaseOrderDetailModel> details)
        {
            header.Code = GetPurchaseOrderCode(header);
            header.IsBillable = false;
            header.IsPayable = false;
            header.IsReceivable = true;
            header.StatusDescription = "Pending Approval";
            //CalculatePODiscount(header, details);
            using (TransactionScope trans = new TransactionScope())
            {
                base.Create(header, details);

                if (header.CopyCurrencyValueToMaster)
                    new CurrencyDateBFC().Create(header.CurrencyID, header.ExchangeRate);

                trans.Complete();
            }
            //OnCreatedUpdated(header.ID, "Create");
        }

        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetPurchaseOrderCode(PurchaseOrderModel header)
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var purchaseOrderPrefix = "";

            if (prefixSetting != null)
                purchaseOrderPrefix = prefixSetting.PurchasePrefix;

            var warehouse = new WarehouseBFC().RetrieveByID(header.WarehouseID);
            var year = DateTime.Now.Year.ToString().Substring(2, 2);

            var prefix = purchaseOrderPrefix + year + "-" + warehouse.Code + "-";
            var code = new ABCAPOSDAC().RetrievePurchaseOrderMaxCode(prefix, 7);

            return code;
        }

        public void UpdateDetails(long poID, List<PurchaseOrderDetailModel> poDetails)
        {
            var dac = new ABCAPOSDAC();
            var purchaseOrder = RetrieveByID(poID);

            using (TransactionScope trans = new TransactionScope())
            {
                //base.Update(purchaseOrder, poDetails);
                GetDetailDAC().DeleteByParentID(poID);
                dac.CreatePODetails(poID, poDetails);
                //GetDetailDAC().Create(poID, poDetails);

                trans.Complete();
            }
        }

        public void UpdateStatus(long purchaseOrderID)
        {
            var order = RetrieveByID(purchaseOrderID);
            string receiptStatus;
            string billStatus;
            string paymentStatus;

            var pdList = new PurchaseDeliveryBFC().RetrieveByPOID(order.ID);
            var qtyReceived = pdList.Sum(p => p.Quantity);
            var poList = new PurchaseOrderBFC().RetrieveDetails(purchaseOrderID);
            var qtyBilled = poList.Sum(p => p.CreatedPBQuantity);
            var pBillList = new PurchaseBillBFC().RetrieveByPOID(order.ID);
            var amountBilled = pBillList.Sum(p => p.Amount + p.TaxAmount);
            var amountPaid = pBillList.Sum(p => p.CreatedPaymentAmount);

            if (order.Status == (int)MPL.DocumentStatus.New)
            {
                order.StatusDescription = "Pending Approval";
                Update(order);
                return;
            }
            else if (order.Status == (int)MPL.DocumentStatus.Void)
            {
                order.StatusDescription = "Void";
                Update(order);
                return;
            }

            if (qtyReceived == 0)
            {
                order.IsReceivable = true;
                receiptStatus = "Pending Receipt";
            }

            else if (qtyReceived == order.Quantity)
            {
                order.IsReceivable = false;
                receiptStatus = "";
            }
            else
            {
                order.IsReceivable = true;
                receiptStatus = "Partially Received";
            }

            if (qtyBilled == order.Quantity)
            {
                order.IsBillable = false;
                billStatus = "Fully Billed";
            }
            else if (qtyBilled >= qtyReceived)
            {
                order.IsBillable = true;
                billStatus = "";
            }
            else
            {
                order.IsBillable = true;
                billStatus = "Pending Billing";
            }


            if (amountBilled == amountPaid)
            {
                order.IsPayable = false;
                if (qtyBilled == order.Quantity)
                {
                    paymentStatus = "Paid in Full";
                    billStatus = "";
                }
                else
                    paymentStatus = "";
            }
            else
            {
                order.IsPayable = true;
                paymentStatus = "Pending Payment";
            }

            var combinator1 = "";
            if (receiptStatus.Length > 0 && billStatus.Length > 0)
                combinator1 = " / ";
            //var combinator2 = "";
            //if (billStatus.Length > 0 && paymentStatus.Length > 0)
            //    combinator2 = " / ";
            //else if (billStatus.Length == 0 
            //    && (receiptStatus.Length > 0 && paymentStatus.Length > 0))
            //    combinator2 = " / ";

            order.StatusDescription = receiptStatus
                + combinator1 + billStatus;
            //+ combinator2 + paymentStatus;

            Update(order);
        }

        public void CopyTransaction(PurchaseOrderModel header, long purchaseOrderID)
        {
            var purchaseOrder = RetrieveByID(purchaseOrderID);
            var purchaseOrderDetails = RetrieveDetails(purchaseOrderID);

            ObjectHelper.CopyProperties(purchaseOrder, header);

            header.Status = (int)MPL.DocumentStatus.New;

            var details = new List<PurchaseOrderDetailModel>();

            foreach (var purchaseOrderDetail in purchaseOrderDetails)
            {
                var detail = new PurchaseOrderDetailModel();

                ObjectHelper.CopyProperties(purchaseOrderDetail, detail);

                details.Add(detail);
            }

            header.Details = details;
        }

        public void CopyTransactionBooking(PurchaseOrderModel header, long bookingOrderID)
        {
            var bookingOrder = new BookingOrderBFC().RetrieveByID(bookingOrderID);
            var bookingOrderDetails = new BookingOrderBFC().RetrieveDetails(bookingOrderID);

            ObjectHelper.CopyProperties(bookingOrder, header);

            header.BookingOrderID = bookingOrderID;
            header.BookingOrderCode = bookingOrder.Code;
            header.Status = (int)MPL.DocumentStatus.New;

            var details = new List<PurchaseOrderDetailModel>();

            foreach (var bookingOrderDetail in bookingOrderDetails)
            {
                var detail = new PurchaseOrderDetailModel();

                ObjectHelper.CopyProperties(bookingOrderDetail, detail);
                detail.BookingOrderItemNo = bookingOrderDetail.ItemNo;
                detail.Quantity = bookingOrderDetail.Quantity - bookingOrderDetail.CreatedPOQuantity;
                detail.QtyRemaining = detail.Quantity;

                var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                var itemLoc = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductID, header.WarehouseID);
                if (itemLoc != null)
                    detail.StockQty = itemLoc.QtyOnHand / unitRate;
                else
                    detail.StockQty = 0;

                if (itemLoc != null)
                    detail.StockAvailable = itemLoc.QtyAvailable / unitRate;
                else
                    detail.StockAvailable = 0;

                details.Add(detail);
            }

            header.Details = details;
        }

        public void ErrorTransaction(PurchaseOrderModel header, List<PurchaseOrderDetailModel> details)
        {

            foreach (var detail in details)
            {
                var product = new ProductBFC().RetrieveByID(detail.ProductID);

                if (product != null)
                    detail.ProductName = product.ProductName;
            }

            header.Details = details;
        }

        public void Validate(PurchaseOrderModel obj, List<PurchaseOrderDetailModel> details)
        {
            decimal POTotal = 0;
            var boDetails = new BookingOrderBFC().RetrieveDetails(obj.BookingOrderID);

            foreach (var detail in details)
            {
                //detail.Discount = 0;
                //if (detail.Discount == null)
                //    detail.Discount = 0;

                detail.Remarks = "";
                detail.AssetPriceInDollar = 0;
                detail.Price = 0;

                if (detail.ProductID == 0)
                    throw new Exception("Product not chosen");



                var total = Convert.ToDecimal(detail.Quantity) * detail.AssetPrice;
                POTotal = Convert.ToDecimal(total);



                if (boDetails != null)
                {
                    var query = from i in boDetails
                                where i.ProductID == detail.ProductID && i.ItemNo == detail.BookingOrderItemNo
                                select i;

                    var boDetail = query.FirstOrDefault();
                    if (boDetail != null)
                    {
                        var poQty = detail.Quantity;

                        if ((boDetail.Quantity - boDetail.CreatedPOQuantity) < poQty)
                            throw new Exception("Jumlah PO tidak boleh lebih dari Booking");
                    }
                }
            }
        }

        public void UpdateValidation(PurchaseOrderModel obj, List<PurchaseOrderDetailModel> details)
        {
            var receipt = new PurchaseDeliveryBFC().RetrieveByPOID(obj.ID);
            if (receipt != null)
            {
                foreach (var detail in details)
                {
                    if (detail.ConversionIDTemp != 0)
                        detail.ConversionID = detail.ConversionIDTemp;

                    if (detail.Quantity < detail.CreatedPDQuantity)
                        throw new Exception("new QTY" + detail.ProductCode + "cannot less than Qty Received");

                    if (detail.Quantity < detail.CreatedPBQuantity)
                        throw new Exception("new QTY" + detail.ProductCode + "cannot less than Qty Billed");

                }
            }
        }

        public void OnApproved(long purchaseOrderID, string userName)
        {
            var po = RetrieveByID(purchaseOrderID);
            var poDetails = RetrieveDetails(purchaseOrderID);

            foreach (var poDetail in poDetails)
                new ABCAPOSDAC().RecalculateLastBuyPrice(poDetail.ProductID, po.SupplierID);
        }

        public void PostAccounting(long purchaseOrderID, string userName)
        {
            var accountingResultList = new AccountingResultBFC().Retrieve(purchaseOrderID, AccountingResultDocumentType.PurchaseOrder);

            if (accountingResultList == null || accountingResultList.Count == 0)
            {
                var purchaseOrder = RetrieveByID(purchaseOrderID);

                decimal purchaseAmount = 0;
                decimal hutangAmount = 0;
                decimal purchaseDiscount = 0;

                var result = new AccountingResultModel();
                ////Beban pengiriman
                //result = new AccountingResultModel();
                //result.DocumentID = purchaseOrder.ID;
                //result.DocumentType = (int)AccountingResultDocumentType.PurchaseOrder;
                //result.Type = (int)AccountingResultType.Credit;
                //result.Date = purchaseOrder.ApprovedDate;
                //result.AccountID = SystemConstants.CostDeliveryAccount;
                //result.DocumentNo = purchaseOrder.Code;
                //result.Amount = purchaseOrder.CostExpedition;
                //result.DebitAmount = purchaseOrder.CostExpedition;
                //result.Remarks = purchaseOrder.Shipment + "," + purchaseOrder.Code;
                //accountingResultList.Add(result);

                //result = new AccountingResultModel();
                //result.DocumentID = purchaseOrder.ID;
                //result.DocumentType = (int)AccountingResultDocumentType.PurchaseOrder;
                //result.Type = (int)AccountingResultType.Debit;
                //result.Date = purchaseOrder.ApprovedDate;
                //result.AccountID = SystemConstants.CashAccount;
                //result.DocumentNo = purchaseOrder.Code;
                //result.Amount = purchaseOrder.CostExpedition;
                //result.CreditAmount = purchaseOrder.CostExpedition;
                //result.Remarks = purchaseOrder.Shipment + "," + purchaseOrder.Code;
                //accountingResultList.Add(result);

                //Pembelian
                //result = new AccountingResultModel();
                result.DocumentID = purchaseOrder.ID;
                result.DocumentType = (int)AccountingResultDocumentType.PurchaseOrder;
                result.Type = (int)AccountingResultType.Credit;
                result.Date = purchaseOrder.ApprovedDate;
                result.AccountID = SystemConstants.PurchaseAccount;
                result.DocumentNo = purchaseOrder.Code;
                result.Amount = purchaseOrder.POTotal;
                result.DebitAmount = purchaseOrder.POTotal;
                result.Remarks = purchaseOrder.VendorName + "," + purchaseOrder.Code;
                accountingResultList.Add(result);

                if (purchaseOrder.TaxValue > 0)
                {
                    result = new AccountingResultModel();
                    result.DocumentID = purchaseOrder.ID;
                    result.DocumentType = (int)AccountingResultDocumentType.PurchaseOrder;
                    result.Type = (int)AccountingResultType.Credit;
                    result.Date = purchaseOrder.ApprovedDate;
                    result.AccountID = SystemConstants.TaxIncomeAccount;
                    result.DocumentNo = purchaseOrder.Code;
                    result.Amount = purchaseOrder.TaxValue * purchaseOrder.ExchangeRate;
                    result.DebitAmount = purchaseOrder.TaxValue * purchaseOrder.ExchangeRate;
                    result.Remarks = purchaseOrder.VendorName + "," + purchaseOrder.Code;
                    accountingResultList.Add(result);
                }

                result = new AccountingResultModel();
                result.DocumentID = purchaseOrder.ID;
                result.DocumentType = (int)AccountingResultDocumentType.PurchaseOrder;
                result.Type = (int)AccountingResultType.Debit;
                result.Date = purchaseOrder.ApprovedDate;
                result.AccountID = SystemConstants.OutcomeAccount;
                result.DocumentNo = purchaseOrder.Code;
                result.Amount = purchaseOrder.POTotal - purchaseOrder.DiscountTotal + (purchaseOrder.TaxValue * purchaseOrder.ExchangeRate);
                result.CreditAmount = purchaseOrder.POTotal - purchaseOrder.DiscountTotal + (purchaseOrder.TaxValue * purchaseOrder.ExchangeRate);
                result.Remarks = purchaseOrder.VendorName + "," + purchaseOrder.Code;
                accountingResultList.Add(result);

                if (purchaseOrder.DiscountTotal > 0)
                {
                    result = new AccountingResultModel();
                    result.DocumentID = purchaseOrder.ID;
                    result.DocumentType = (int)AccountingResultDocumentType.PurchaseOrder;
                    result.Type = (int)AccountingResultType.Debit;
                    result.Date = purchaseOrder.ApprovedDate;
                    result.AccountID = SystemConstants.PurchaseDiscountAccount;
                    result.DocumentNo = purchaseOrder.Code;
                    result.Amount = purchaseOrder.DiscountTotal;
                    result.CreditAmount = purchaseOrder.DiscountTotal;
                    result.Remarks = purchaseOrder.VendorName + "," + purchaseOrder.Code;
                    accountingResultList.Add(result);
                }

                new AccountingResultBFC().Posting(accountingResultList);

            }
        }

        public void Approve(long purchaseOrderID, string userName)
        {
            var purchaseOrder = RetrieveByID(purchaseOrderID);

            purchaseOrder.Status = (int)MPL.DocumentStatus.Approved;
            purchaseOrder.ApprovedBy = userName;
            purchaseOrder.ApprovedDate = DateTime.Now;

            Update(purchaseOrder);
            OnApproved(purchaseOrderID, userName);

            if (purchaseOrder.BookingOrderID != 0)
                new BookingOrderBFC().UpdateStatus(purchaseOrder.BookingOrderID);

            //using (TransactionScope trans = new TransactionScope())
            //{
            //    Update(purchaseOrder);
            //    OnApproved(purchaseOrderID, userName);

            //    if (purchaseOrder.BookingOrderID != 0)
            //        new BookingOrderBFC().UpdateStatus(purchaseOrder.BookingOrderID);

            //    trans.Complete();
            //}
        }

        public void Void(long purchaseOrderID, string voidRemarks, string userName)
        {
            var purchaseOrder = RetrieveByID(purchaseOrderID);
            var oldStatus = purchaseOrder.Status;
            purchaseOrder.Status = (int)MPL.DocumentStatus.Void;
            purchaseOrder.VoidRemarks = voidRemarks;
            purchaseOrder.ApprovedDate = DateTime.Now;
            purchaseOrder.ApprovedBy = userName;

            using (TransactionScope trans = new TransactionScope())
            {
                Update(purchaseOrder);

                if (purchaseOrder.BookingOrderID != 0)
                    new BookingOrderBFC().UpdateStatus(purchaseOrder.BookingOrderID);

                trans.Complete();
            }
        }

        public void CreatedByStockMinimum(PurchaseOrderModel header, long productID)
        {
            var product = new ProductBFC().RetrieveByID(productID);
            var PODetails = new List<PurchaseOrderDetailModel>();
            if (product != null)
            {
                var detail = new PurchaseOrderDetailModel();
                detail.ProductID = product.ID;
                detail.ProductCode = product.Code;
                detail.ProductName = product.ProductName;

                PODetails.Add(detail);
                header.Details = PODetails;
            }
        }

        public void UpdatePriceInContainerLogDetail(long purchaseorderID)
        {
            var Log = new LogBFC().RetreiveLogByPurchaseOrderID(purchaseorderID);
            if (Log != null)
            {
                foreach (var Logs in Log)
                {
                    var containerDetail = new ContainerBFC().RetreiveByContainerIDProductIDWarehouseID(Logs.ContainerID, Logs.ProductID, Logs.WarehouseID);
                    if (containerDetail != null)
                    {
                        var price = new ABCAPOSDAC().RetreivePriceBypurchaseOrderID(Logs.purchaseorderID, Logs.ProductID);
                        if (price != null)
                        {
                            containerDetail.Price = Convert.ToDouble(price.Total) / price.Quantity;
                            new ContainerBFC().Update(containerDetail);
                        }
                    }
                }
                this.UpdateHppByPO(purchaseorderID);
            }
        }

        public void UpdatePriceInPurchaseBill(long purchaseOrderID)
        {
            var RelatedPO = new ABCAPOSDAC().RetreiveRelatedPOandPurchaseBill(purchaseOrderID);
            if (RelatedPO != null)
            {
                foreach (var detail in RelatedPO)
                {
                    new ABCAPOSDAC().DeletePurchasebilldetailbyPbID(detail.PurchaseBillID, detail.ItemNo, detail.ProductID);
                    var PurchasebillDetail = new PurchaseBillDetailModel();
                    decimal totalAmount = (Convert.ToDecimal(detail.QuantityPurchaseBill) * detail.AssetPrice);
                    decimal totalTax = 0;
                    if (detail.TaxTypePO == 2)
                    {
                        totalTax = (totalAmount - Convert.ToDecimal(detail.Discount)) * Convert.ToDecimal(0.1);
                    }
                    PurchasebillDetail.PurchaseBillID = detail.PurchaseBillID;
                    PurchasebillDetail.ItemNo = detail.ItemNo;
                    PurchasebillDetail.PurchaseOrderItemNo = detail.PurchaseOrderItemNo;
                    PurchasebillDetail.ProductID = detail.ProductID;
                    PurchasebillDetail.Quantity = detail.QuantityPurchaseBill;
                    PurchasebillDetail.Remarks = "";
                    PurchasebillDetail.Total = (totalAmount - Convert.ToDecimal(detail.Discount)) + totalTax;
                    PurchasebillDetail.AssetPrice = detail.AssetPrice;
                    PurchasebillDetail.Discount = detail.Discount;

                    new ABCAPOSDAC().CreatePurchasebillDetail(PurchasebillDetail);

                    //var PurchaseBill = new PurchaseBillBFC().RetrieveByID(detail.PurchaseBillID);

                    //PurchaseBill.Amount = Convert.ToDecimal(Math.Round(RelatedPO.Sum(p => p.TotalAmountPBDetail), 2));
                    //PurchaseBill.DiscountAmount = Math.Round(RelatedPO.Sum(p => p.Discount), 2);
                    //PurchaseBill.TaxAmount = Convert.ToDecimal(Math.Round(RelatedPO.Sum(p => p.TotalPPNPBDetail), 2));
                    //PurchaseBill.GrandTotal = Math.Round(RelatedPO.Sum(p => p.TotalPBDetail), 2);
                    //new PurchaseBillBFC().Update(PurchaseBill);

                }
                //var pb = new PurchaseBillBFC().RetreivePBByPurchaseOrderID(purchaseOrderID);
                //if (pb != null)
                //{
                   
                //    //pb.Amount = Convert.ToDecimal(Math.Round(RelatedPO.Sum(p => p.TotalAmountPBDetail), 2));
                //    //pb.DiscountAmount = Math.Round(RelatedPO.Sum(p => p.Discount), 2);
                //    //pb.TaxAmount = Convert.ToDecimal(Math.Round(RelatedPO.Sum(p => p.TotalPPNPBDetail), 2));
                //    //pb.GrandTotal = Math.Round(RelatedPO.Sum(p => p.TotalPBDetail), 2);
                //    //new PurchaseBillBFC().Update(pb);
                //}
            }
        }

        public void UpdatePriceInPurchaseBillHedaer(long purchaseOrderID)
        {
            var RelatedPO = new ABCAPOSDAC().RetreiveRelatedPOandPurchaseBill(purchaseOrderID);
            if (RelatedPO != null)
            {
                foreach (var detail in RelatedPO)
                {
                    var purchasebill = new PurchaseBillBFC().RetrieveByID(detail.PurchaseBillID);
                    var pb = new PurchaseBillBFC().RetrieveDetails(detail.PurchaseBillID);

                    purchasebill.ID = detail.PurchaseBillID;
                    purchasebill.Amount = pb.Sum(p => p.AssetPrice * Convert.ToDecimal(p.Quantity));
                    purchasebill.DiscountAmount = pb.Sum(p => p.Discount);

                    purchasebill.TaxAmount = pb.Sum(p => Convert.ToDecimal(p.TaxAmount));
                    purchasebill.GrandTotal = pb.Sum(p => Convert.ToDouble(p.Total));
                    new PurchaseBillBFC().Update(purchasebill);
                }
            }
        }

        public void PreparePOByWorkOrder(PurchaseOrderModel header,List<PurchaseOrderDetailModel>details, long productID)
        {
            var product = new ProductBFC().RetrieveByID(productID);
            if (product != null)
            {
                //var header = new PurchaseOrderModel();
                //var poDetails = new List<PurchaseOrderDetailModel>();
                var detail = new PurchaseOrderDetailModel();

                header.WarehouseID = 2;
                header.WarehouseName = "Factory";
                detail.ProductID = product.ID;
                detail.ProductCode = product.Code;
                detail.ProductName = product.ProductName;
                detail.ConversionID = product.PurchaseUnitID;
                var unit = new UnitBFC().GetUnitDetailByID(product.PurchaseUnitID);
                detail.ConversionName = unit.Name;
                detail.TaxType = (int)TaxType.PPN;

                detail.UnitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                var itemLoc = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductID, header.WarehouseID);
                if (itemLoc != null)
                {
                    detail.StockQty = itemLoc.QtyOnHand / detail.UnitRate;
                    detail.StockQtyHidden = itemLoc.QtyOnHand / detail.UnitRate;
                    detail.StockAvailable = itemLoc.QtyAvailable / detail.UnitRate;
                    detail.StockAvailableHidden = itemLoc.QtyAvailable / detail.UnitRate;
                }
                else
                {
                    detail.StockQty = 0;
                    detail.StockQtyHidden = 0;
                    detail.StockAvailable = 0;
                    detail.StockAvailableHidden = 0;
                }
                details.Add(detail);
                header.Details = details;
            }
        }

        public ABCAPOSReportEDSC.PurchaseOrderDTRow RetrievePrintOut(long purchaseOrderID)
        {
            return new ABCAPOSReportDAC().RetrievePurchaseOrderPrintOut(purchaseOrderID);
        }

        public List<v_PurchaseOrderDetail> RetrieveDetailPrintOut(long purchaseOrderID)
        {
            return new ABCAPOSReportDAC().RetrievePurchaseOrderDetailPrintOut(purchaseOrderID);
        }

        public List<PurchaseOrderModel> RetrieveByBOID(long bookingOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from j in ent.v_PurchaseOrder
                        where j.BookingOrderID == bookingOrderID && j.Status != (int)MPL.DocumentStatus.Void
                        select j;

            return ObjectHelper.CopyList<v_PurchaseOrder, PurchaseOrderModel>(query.ToList());
        }

        public int RetrieveUncreatedPurchaseDeliveryCount(List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveUncreatedPurchaseDeliveryPOCount(selectFilters);
        }

        public List<PurchaseOrderModel> RetrieveUncreatedPurchaseDelivery(int startIndex, int? amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            if (amount == null)
                amount = SystemConstants.ItemPerPage;

            return new ABCAPOSDAC().RetrieveUncreatedPurchaseDeliveryPO(startIndex, (int)amount, sortParameter, selectFilters);
        }

        public int RetrieveUncreatedPurchaseBillCount(List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveUncreatedPurchaseBillOrderCount(selectFilters);
        }

        public List<PurchaseOrderModel> RetrieveUncreatedPurchaseBill(int startIndex, int? amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            if (amount == null)
                amount = SystemConstants.ItemPerPage;

            return new ABCAPOSDAC().RetrieveUncreatedPurchaseBillOrder(startIndex, (int)amount, sortParameter, selectFilters);
        }

        public List<PurchaseOrderModel> RetrieveAutoComplete(string key)
        {
            return new ABCAPOSDAC().RetrievePurchaseOrderAutoComplete(key);
        }

        public PurchaseOrderModel RetrieveByCode(string purchaseCode)
        {
            return new ABCAPOSDAC().RetrievePurchaseOrderByCode(purchaseCode);
        }

        public List<TermsOfPaymentModel> RetrieveAllTerms()
        {
            return new ABCAPOSDAC().RetrieveAllTerms();
        }

        public List<PurchaseOrderModel> Retrieve(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters, bool showVoidDocuments)
        {
            return new ABCAPOSDAC().RetrievePurchaseOrder(startIndex, amount, sortParameter, selectFilters, showVoidDocuments);
        }

        public int RetrieveCount(List<SelectFilter> selectFilters, bool showVoidDocuments)
        {
            return new ABCAPOSDAC().RetrievePurchaseOrderCount(selectFilters, showVoidDocuments);
        }

        

        #region Notification

        public int RetrieveUnapprovedPurchaseOrderCount()
        {
            return new ABCAPOSDAC().RetrieveUnapprovedPurchaseOrderCount();
        }

        public int RetrieveUnpaidPurchaseOrderCount()
        {
            return new ABCAPOSDAC().RetrieveUnpaidPOCount();
        }

        public string RetrieveThisMonthPurchaseOrderCount()
        {
            return new ABCAPOSDAC().RetrieveThisMonthPurchaseOrderCount();
        }

        public int RetrieveVoidPurchaseOrderCount()
        {
            return new ABCAPOSDAC().RetrieveVoidPurchaseOrderCount();
        }

        #endregion

        //private void CalculatePODiscount(PurchaseOrderModel header, List<PurchaseOrderDetailModel> details)
        //{
        //    decimal POTotal = 0;
        //    decimal discountTotal = 0;

        //    foreach (var detail in details)
        //    {
        //        if (detail.Discount == 0)
        //            detail.Discount = 0;

        //        var total = Convert.ToDecimal(detail.Quantity) * detail.AssetPriceInDollar;
        //        POTotal += Convert.ToDecimal(total);

        //        var discount = Convert.ToDecimal(detail.Quantity) * detail.Discount;
        //        discountTotal += Convert.ToDecimal(discount);
        //    }

        //    header.POTotalDollar = POTotal;
        //    header.DiscountTotalDollar = discountTotal;
        //    header.POTotal = POTotal * header.ExchangeRate;
        //    header.DiscountTotal = discountTotal * header.ExchangeRate;
        //}

        //public void OnCreatedUpdated(long purchaseOrderID, string operation)
        //{
        //    var purchaseOrder = RetrieveByID(purchaseOrderID);

        //    if (purchaseOrder != null)
        //    {
        //        var purchaseOrderDetails = RetrieveDetails(purchaseOrderID);

        //        foreach (var poDetail in purchaseOrderDetails)
        //        {
        //            var product = new ProductBFC().RetrieveByID(poDetail.ProductID);

        //            //plus in details 
        //            //var details = new ProductBFC().RetrieveDetails(product.ID);

        //            double quantity = Convert.ToDouble(poDetail.Quantity);
        //            if (operation == "Create")
        //            {
        //                if (quantity > 0)
        //                {
        //                    var productDetail = new ProductBFC().RetrieveDetails(poDetail.ProductID);
        //                    var itemNo = productDetail.Count + 1;

        //                    ProductDetailModel detail = new ProductDetailModel();
        //                    detail.ProductID = poDetail.ProductID;
        //                    detail.ItemNo = itemNo;
        //                    detail.Date = purchaseOrder.Date;
        //                    detail.Price = Convert.ToDecimal(poDetail.Price);
        //                    detail.AssetPrice = Convert.ToDecimal(poDetail.AssetPrice);
        //                    detail.Quantity = 0;
        //                    detail.QtyStart = Convert.ToDouble(poDetail.Quantity);
        //                    detail.IsSaved = true;
        //                    detail.PurchaseOrderID = purchaseOrderID;
        //                    productDetail.Add(detail);

        //                    product.Details = productDetail;
        //                }
        //            }
        //            else if (operation == "Update")
        //            {
        //                if (quantity > 0)
        //                {
        //                    var productDetails = new ProductBFC().RetrieveDetails(poDetail.ProductID);

        //                    foreach (var productDetail in productDetails)
        //                    {
        //                        if (productDetail.Date == purchaseOrder.Date && productDetail.PurchaseOrderID == purchaseOrder.ID)
        //                        {
        //                            productDetail.Price = Convert.ToDecimal(poDetail.Price);
        //                            productDetail.AssetPrice = Convert.ToDecimal(poDetail.AssetPrice);
        //                            productDetail.QtyStart = Convert.ToDouble(poDetail.Quantity);
        //                        }
        //                    }

        //                    product.Details = productDetails;
        //                }
        //            }
        //            new ProductBFC().Update(product, product.Details);
        //        }
        //    }
        //}

        //public List<PurchaseBillModel> RetrieveUncreatedPurchasePayment(int startIndex, int? amount, string sortParameter, List<SelectFilter> selectFilters)
        //{
        //    if (amount == null)
        //        amount = SystemConstants.ItemPerPage;

        //    return new ABCAPOSDAC().RetrieveUncreatedPaymentBill(startIndex, (int)amount, sortParameter, selectFilters);
        //}

        //public int RetrieveUncreatedPurchasePaymentCount(List<SelectFilter> selectFilters)
        //{
        //    return new ABCAPOSDAC().RetrieveUncreatedPaymentBillCount(selectFilters);
        //}

        //public void OnVoid(long purchaseOrderID, string voidRemarks, string userName)
        //{
        //    var purchaseOrder = RetrieveByID(purchaseOrderID);

        //    if (purchaseOrder != null)
        //    {
        //        var purchaseOrderDetails = RetrieveDetails(purchaseOrderID);

        //        foreach (var purchaseOrderDetail in purchaseOrderDetails)
        //        {
        //            var product = new ProductBFC().RetrieveByID(purchaseOrderDetail.ProductID);
        //            var productDets = new ProductBFC().RetrieveDetails(purchaseOrderDetail.ProductID);

        //            product.StockQty -= Convert.ToDouble(purchaseOrderDetail.Quantity);

        //            ProductDetailModel removeDetail = new ProductDetailModel();
        //            foreach (var prodDet in productDets)
        //            {
        //                if (prodDet.Date == purchaseOrder.Date && prodDet.PurchaseOrderID == purchaseOrder.ID)
        //                {
        //                    //NOT DONE
        //                    //removeDetail = prodDet;
        //                    prodDet.Quantity = 0;
        //                }
        //            }
        //            //productDets.Remove(removeDetail);

        //            new ProductBFC().Update(product, productDets);
        //        }

        //        var purchasePaymentList = new PurchasePaymentBFC().RetrieveByPO(purchaseOrder.ID);
        //        foreach (var purchasePayment in purchasePaymentList)
        //            new PurchasePaymentBFC().Void(purchasePayment.ID, voidRemarks, userName);
        //    }
        //}

        //public void UpdateCreatedPDQuantity(long poID, List<PurchaseDeliveryDetailModel> pdDetails)
        //{
        //    var poDetails = RetrieveDetails(poID);

        //    foreach (var poDetail in poDetails)
        //    {
        //        var pdDetail = from i in pdDetails
        //                        where i.ProductID == poDetail.ProductID && i.PurchaseOrderItemNo == poDetail.ItemNo && i.ConversionID == poDetail.ConversionID
        //                        select i;

        //        var newPdDetail = pdDetail.FirstOrDefault();

        //        poDetail.CreatedPDQuantity += newPdDetail.SelisihQuantity;
        //    }

        //    UpdateDetails(poID, poDetails);
        //}

        //public void UpdateCreatedPBQuantity(long poID, List<PurchaseBillDetailModel> pbDetails)
        //{
        //    var poDetails = RetrieveDetails(poID);

        //    foreach (var poDetail in poDetails)
        //    {
        //        var pbDetail = from i in pbDetails
        //                       where i.ProductID == poDetail.ProductID && i.PurchaseOrderItemNo == poDetail.ItemNo
        //                       select i;

        //        var newPbDetail = pbDetail.FirstOrDefault();

        //        poDetail.CreatedPBQuantity += newPbDetail.SelisihQuantity;
        //    }

        //    UpdateDetails(poID, poDetails);
        //}
    }
}
