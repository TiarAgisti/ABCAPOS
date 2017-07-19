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
    public class CashSalesBFC : MasterDetailBFC<CashSales, v_CashSales, CashSalesDetail, v_CashSalesDetail, CashSalesModel, CashSalesDetailModel>
    {
        private void CalculateSODiscount(CashSalesModel header, List<CashSalesDetailModel> details)
        {
            decimal SOTotal = 0;
            decimal discountTotal = 0;

            foreach (var detail in details)
            {
                if (detail.Discount == 0)
                    detail.Discount = 0;

                var total = Convert.ToDecimal(detail.Quantity) * detail.Price;
                SOTotal += Convert.ToDecimal(total);

                var discount = Convert.ToDecimal(detail.Quantity) * detail.Discount;
                discountTotal += Convert.ToDecimal(discount);
            }

            header.SOTotal = SOTotal;
            header.DiscountTotal = discountTotal;
        }

        #region PostingTableLog
        public void CreateLog(CashSalesModel header)
        {
            try
            {
                if (header != null)
                {
                    var details = base.RetrieveDetails(header.ID);
                    if (details != null)
                    {
                        var LogHeader = new LogModel();
                        var LogDetails = new List<LogDetailModel>();

                        LogHeader.WarehouseID = header.WarehouseID;
                        LogHeader.Date = header.Date;
                        LogHeader.DocType = (int)DocType.CashSales;

                        foreach (var detail in details)
                        {
                            var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                            var baseQuantity = detail.Quantity * unitRate;

                            double QtyRemaining = baseQuantity;
                            do
                            {
                                var LogDetail = new LogDetailModel();
                                var container = new ContainerBFC().RetreiveByProductIDWarehouseID(detail.ProductID, header.WarehouseID);
                                if (container != null)
                                {
                                    var containerHeader = new ContainerModel();
                                    containerHeader.ID = container.ID;
                                    containerHeader.ProductID = container.ProductID;
                                    containerHeader.WarehouseID = container.WarehouseID;
                                    double qty = (QtyRemaining > container.Qty) ? container.Qty : QtyRemaining;
                                    QtyRemaining = QtyRemaining - qty;
                                    containerHeader.Qty = container.Qty - qty;
                                    containerHeader.Price = container.Price;
                                    new ContainerBFC().Update(containerHeader);

                                    LogDetail.ContainerID = container.ID;
                                    LogDetail.ProductID = detail.ProductID;
                                    LogDetail.MovingOutQty = qty;
                                    LogDetails.Add(LogDetail);
                                }
                                else
                                {
                                    var containerHeader = new ContainerModel();
                                    containerHeader.ProductID = detail.ProductID;
                                    containerHeader.WarehouseID = header.WarehouseID;
                                    containerHeader.Qty -= QtyRemaining;
                                    containerHeader.Price = 0;
                                    new ContainerBFC().Create(containerHeader);

                                    LogDetail.ContainerID = containerHeader.ID;
                                    LogDetail.ProductID = detail.ProductID;
                                    LogDetail.MovingOutQty = QtyRemaining;
                                    QtyRemaining = 0;
                                    LogDetails.Add(LogDetail);
                                }
                            } while (QtyRemaining > 0);
                        }
                        new LogBFC().Create(LogHeader, LogDetails);
                        header.LogID = LogHeader.ID;
                        base.Update(header);
                    }

                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        private void UndoContainerStock(CashSalesModel header)
        {
            try
            {
                var Log = new LogBFC().RetrieveByID(header.LogID);
                if (Log != null)
                {
                    var details = base.RetrieveDetails(header.ID);
                    if (details != null)
                    {
                        foreach (var detail in details)
                        {
                            var container = new ContainerBFC().RetreiveByLogIDproductIDdocType(header.LogID, detail.ProductID, (int)DocType.DeliveryOrder);
                            if (container != null)
                            {
                                var containerDetail = new ContainerBFC().RetreiveByContainerIDProductIDWarehouseID(container.ContainerID, detail.ProductID, header.WarehouseID);
                                if (containerDetail != null)
                                {
                                    var LogDetail = new LogBFC().RetreiveByLogIDContainerIDProductIDWarehouseID(header.LogID, container.ContainerID, detail.ProductID, header.WarehouseID);
                                    if (LogDetail != null)
                                    {
                                        var ContainerHeader = new ContainerModel();
                                        ContainerHeader.ID = container.ContainerID;
                                        ContainerHeader.ProductID = detail.ProductID;
                                        ContainerHeader.WarehouseID = header.WarehouseID;
                                        ContainerHeader.Qty = containerDetail.Qty + LogDetail.MovingOutQty;
                                        ContainerHeader.Price = containerDetail.Price;
                                        new ContainerBFC().Update(ContainerHeader);

                                        new ABCAPOSDAC().DeleteLogByLogIDContainerIDProductID(header.LogID, ContainerHeader.ID, detail.ProductID);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void UpdateLog(CashSalesModel header, List<CashSalesDetailModel> details)
        {
            try
            {
                var LogHeader = new LogModel();
                var Log = new LogBFC().RetrieveByID(header.LogID);
                if (Log != null)
                {
                    //var details = base.RetrieveDetails(header.ID);
                    LogHeader.Date = header.Date;
                    LogHeader.WarehouseID = header.WarehouseID;
                    LogHeader.DocType = (int)DocType.CashSales;

                    new LogBFC().Update(LogHeader);
                    var itemNo = 1;
                    foreach (var detail in details)
                    {
                        var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                        var baseQuantity = detail.Quantity * unitRate;

                        double QtyRemaining = baseQuantity;
                        do
                        {
                            var LogDetail = new LogDetailModel();
                            var container = new ContainerBFC().RetreiveByProductIDWarehouseID(detail.ProductID, header.WarehouseID);
                            if (container != null)
                            {
                                var containerHeader = new ContainerModel();
                                containerHeader.ID = container.ID;
                                containerHeader.ProductID = container.ProductID;
                                containerHeader.WarehouseID = container.WarehouseID;
                                double qty = (QtyRemaining > container.Qty) ? container.Qty : QtyRemaining;
                                containerHeader.Qty = container.Qty - qty;
                                containerHeader.Price = container.Price;
                                new ContainerBFC().Update(containerHeader);


                                LogDetail.ContainerID = container.ID;
                                LogDetail.LogID = header.LogID;
                                LogDetail.ItemNo = itemNo++;
                                LogDetail.ProductID = detail.ProductID;
                                LogDetail.MovingOutQty = qty;
                                QtyRemaining = QtyRemaining - qty;

                                new ABCAPOSDAC().CreateLog(LogDetail);
                            }
                            else
                            {
                                var containerHeader = new ContainerModel();
                                //var containerdetail = new ContainerBFC().RetreiveByLogIDproductIDdocType(header.LogID, detail.ProductID, (int)DocType.DeliveryOrder);
                                //containerHeader.ID = containerdetail.ID;
                                containerHeader.ProductID = detail.ProductID;
                                containerHeader.WarehouseID = header.WarehouseID;
                                containerHeader.Qty -= QtyRemaining;
                                containerHeader.Price = 0;
                                new ContainerBFC().Create(containerHeader);

                                LogDetail.ContainerID = containerHeader.ID;
                                LogDetail.LogID = header.LogID;
                                LogDetail.ItemNo = itemNo++;
                                LogDetail.ProductID = detail.ProductID;
                                LogDetail.MovingOutQty = QtyRemaining;
                                //LogDetail.MovingOutValue = 0;
                                QtyRemaining = 0;

                                new ABCAPOSDAC().CreateLog(LogDetail);
                            }
                        } while (QtyRemaining > 0);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void VoidLog(CashSalesModel header)
        {
            try
            {
                var LogHeader = new LogModel();
                var Log = new LogBFC().RetrieveByID(header.LogID);
                if (Log != null)
                {
                    var details = base.RetrieveDetails(header.ID);
                    var itemNo = 1;
                    foreach (var detail in details)
                    {
                        var LogDetails = new LogDetailModel();

                        var container = new ContainerBFC().RetreiveByLogIDproductIDdocType(header.LogID, detail.ProductID, (int)DocType.DeliveryOrder);
                        if (container != null)
                        {
                            var containerDetail = new ContainerBFC().RetreiveByContainerIDProductIDWarehouseID(container.ContainerID, detail.ProductID, header.WarehouseID);
                            if (containerDetail != null)
                            {
                                var LogDetail = new LogBFC().RetreiveByLogIDContainerIDProductIDWarehouseID(header.LogID, container.ContainerID, detail.ProductID, header.WarehouseID);
                                if (LogDetail != null)
                                {
                                    var ContainerHeader = new ContainerModel();
                                    ContainerHeader.ID = container.ContainerID;
                                    ContainerHeader.ContainerID = container.ContainerID;
                                    ContainerHeader.ProductID = detail.ProductID;
                                    ContainerHeader.WarehouseID = header.WarehouseID;
                                    ContainerHeader.Qty = containerDetail.Qty + LogDetail.MovingOutQty;
                                    ContainerHeader.Price = containerDetail.Price;
                                    new ContainerBFC().Update(ContainerHeader);

                                    new ABCAPOSDAC().DeleteLogByLogIDContainerIDProductID(header.LogID, ContainerHeader.ID, detail.ProductID);
                                    LogDetails.ContainerID = ContainerHeader.ID;
                                    LogDetails.LogID = header.LogID;
                                    LogDetails.ItemNo = itemNo++;
                                    LogDetails.ProductID = detail.ProductID;
                                    LogDetails.MovingOutQty = 0;
                                    //LogDetails.MovingOutValue = 0;

                                    new ABCAPOSDAC().CreateLog(LogDetails);
                                }
                            }
                        }
                    }

                    LogHeader.ID = Log.ID;
                    LogHeader.Date = header.Date;
                    LogHeader.WarehouseID = header.WarehouseID;
                    LogHeader.DocType = (int)DocType.Void;
                    new LogBFC().Update(LogHeader);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        #endregion

        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetCashSalesCode(CashSalesModel header)
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var cashSalesPrefix = "";

            if (prefixSetting != null)
                cashSalesPrefix = prefixSetting.CashSalesPrefix;

            var warehouse = new WarehouseBFC().RetrieveByID(header.WarehouseID);
            var year = DateTime.Now.Year.ToString().Substring(2, 2);

            var prefix = cashSalesPrefix + year + "-" + warehouse.Code + "-";
            var code = new ABCAPOSDAC().RetrieveCashSalesMaxCode(prefix, 7);

            return code;
        }

        protected override GenericDetailDAC<CashSalesDetail, CashSalesDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<CashSalesDetail, CashSalesDetailModel>("CashSalesID", "ItemNo", false);
        }

        protected override GenericDetailDAC<v_CashSalesDetail, CashSalesDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_CashSalesDetail, CashSalesDetailModel>("CashSalesID", "ItemNo", false);
        }

        protected override GenericDAC<CashSales, CashSalesModel> GetMasterDAC()
        {
            return new GenericDAC<CashSales, CashSalesModel>("ID", false, "Date DESC");
        }

        protected override GenericDAC<v_CashSales, CashSalesModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_CashSales, CashSalesModel>("ID", false, "Date DESC");
        }

        public CashSalesModel RetrieveByCode(string salesCode)
        {
            return new ABCAPOSDAC().RetrieveCashSalesByCode(salesCode);
        }

        public void ErrorTransaction(CashSalesModel header, List<CashSalesDetailModel> details)
        {

            foreach (var detail in details)
            {
                var product = new ProductBFC().RetrieveByID(detail.ProductID);

                if (product != null)
                {
                    detail.ProductName = product.ProductName;
                    var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                    detail.StockQty = detail.StockQtyHidden / unitRate;
                    detail.StockAvailable = detail.StockAvailableHidden / unitRate;
                    var beforePPN = Convert.ToDecimal(detail.Quantity) * detail.Price;
                    decimal Total = 0;
                    if (detail.TaxType == (int)TaxType.NonTax)
                        Total = beforePPN;
                    else
                        Total = (beforePPN * Convert.ToDecimal(0.1)) + beforePPN;
                    header.SubTotal += Total;
                }

            }

            header.Details = details;
        }

        public override void Create(CashSalesModel header, List<CashSalesDetailModel> details)
        {
            header.Code = GetCashSalesCode(header);
            CalculateSODiscount(header, details);
            header.IsPayable = false;
            header.StatusDescription = "Paid";

            using (TransactionScope trans = new TransactionScope())
            {
                this.CreateLog(header);

                base.Create(header, details);

                PostAccounting(header.ID,header.Status);

                trans.Complete();
            }
        }

        public bool UpdateInventory(string key)
        {
            //var cashSalesID = int.Parse(key);

            var cashSales = RetrieveByID(key);

            if (cashSales == null)
                return false;

            var details = RetrieveDetails(cashSales.ID);

            foreach (var detail in details)
            {
                var qtyCustomUnit = detail.Quantity;
                var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                var qtyBaseUnit = detail.Quantity * unitRate;

                var itemLocation = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductID, cashSales.WarehouseID);
                if (itemLocation != null)
                {
                    itemLocation.QtyOnHand -= qtyBaseUnit;
                    itemLocation.QtyAvailable -= qtyBaseUnit;
                    new ItemLocationBFC().Update(itemLocation);
                }
                else
                {
                    new ItemLocationBFC().Create(detail.ProductID,
                      cashSales.WarehouseID,
                      -qtyBaseUnit,
                      -qtyBaseUnit);
                }
            }
            return true;
        }

        public void UpdateSOItemLocationBin(CashSalesModel header, List<CashSalesDetailModel> details)
        {
            foreach (var detail in details)
            {
                var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                var baseSelisihQuantity = detail.SelisihQuantity * unitRate;
                var itemLocation = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductID, header.WarehouseID);
                if (itemLocation != null)
                {
                    itemLocation.QtyAvailable -= baseSelisihQuantity;
                    itemLocation.QtyOnHand -= baseSelisihQuantity;

                    new ItemLocationBFC().Update(itemLocation);
                }
            }
        }

        public void updateFromOldQty(CashSalesModel header, List<CashSalesDetailModel> details)
        {
            var oldDetails = RetrieveDetails(header.ID);

            foreach (var detail in details)
            {
                var oldDetail = from i in oldDetails
                                where i.ProductID == detail.ProductID && i.ItemNo == detail.ItemNo
                                select i;

                var oldPdDetail = oldDetail.FirstOrDefault();

                if (oldPdDetail != null)
                    detail.SelisihQuantity = detail.Quantity - oldPdDetail.Quantity;
                else
                    detail.SelisihQuantity = detail.Quantity;

            }

            //Update CreatedDOQuantity
            //new CashSalesBFC().UpdateCreatedPDQuantity(header.PurchaseOrderID, details);
            //Update ItemLocation
            //Update Bin
            UpdateSOItemLocationBin(header, details);


        }

        public void voidFromOldQty(CashSalesModel header, List<CashSalesDetailModel> details)
        {
            var oldDetails = RetrieveDetails(header.ID);

            foreach (var detail in details)
            {
                detail.SelisihQuantity = -detail.Quantity;
            }

            //Update CreatedDOQuantity
            //new CashSalesBFC().UpdateCreatedPDQuantity(header.PurchaseOrderID, details);
            //Update ItemLocation
            //Update Bin
            UpdateSOItemLocationBin(header, details);


        }

        public override void Update(CashSalesModel header, List<CashSalesDetailModel> details)
        {
            CalculateSODiscount(header, details);

            using (TransactionScope trans = new TransactionScope())
            {
                this.UndoContainerStock(header);

                this.UpdateLog(header,details);

                updateFromOldQty(header, details);
                base.Update(header, details);
                UpdateStatus(header.ID);

                PostAccounting(header.ID,header.Status);

                trans.Complete();
            }
        }

        public void CopyTransaction(CashSalesModel header, long cashSalesID)
        {
            var cashSales = RetrieveByID(cashSalesID);
            var cashSalesDetails = RetrieveDetails(cashSalesID);

            ObjectHelper.CopyProperties(cashSales, header);

            header.Status = (int)MPL.DocumentStatus.New;

            var details = new List<CashSalesDetailModel>();

            foreach (var cashSalesDetail in cashSalesDetails)
            {
                var detail = new CashSalesDetailModel();

                ObjectHelper.CopyProperties(cashSalesDetail, detail);
                detail.HPP = detail.AssetPrice;

                details.Add(detail);
            }

            header.Details = details;
        }

        public void UpdateStatus(long cashSalesID)
        {
            var order = RetrieveByID(cashSalesID);
            string paymentStatus;

            var amountInvoiced = order.Amount + order.TaxAmount;
            var amountPaid = order.CreatedPaymentAmount;
            var qtyInvoiced = order.Quantity;
            order.IsPayable = false;

            if (order.Status == (int)MPL.DocumentStatus.Approved)
            {    
                order.StatusDescription = "Paid";
                Update(order);
                return;
            }
            else if (order.Status == (int)MPL.DocumentStatus.Void)
            {
                order.StatusDescription = "Void";
                Update(order);
                return;
            }

            //if (amountInvoiced == amountPaid)
            //{
            //    order.IsPayable = false;
            //    if (qtyInvoiced == order.Quantity)
            //    {
            //        paymentStatus = "Paid in Full";
            //        //invoiceStatus = "";
            //    }
            //    else
            //        paymentStatus = "";
            //}
            //else
            //{
            //    order.IsPayable = true;
            //    paymentStatus = "Pending Payment";
            //}

            //order.StatusDescription =  paymentStatus;

            Update(order);
        }

        public void UpdateDetails(long cashSalesID, List<CashSalesDetailModel> spkDetails)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                GetDetailDAC().DeleteByParentID(cashSalesID);
                GetDetailDAC().Create(cashSalesID, spkDetails);

                trans.Complete();
            }
        }

        public void Void(long cashSalesID, string voidRemarks, string userName)
        {
            var cashSales = RetrieveByID(cashSalesID);

            cashSales.Status = (int)MPL.DocumentStatus.Void;
            cashSales.VoidRemarks = voidRemarks;
            cashSales.ApprovedDate = DateTime.Now;
            cashSales.ApprovedBy = userName;
            
            using (TransactionScope trans = new TransactionScope())
            {
                this.VoidLog(cashSales);

                OnVoid(cashSalesID, voidRemarks, userName);

                PostAccounting(cashSalesID,cashSales.Status);

                Update(cashSales);

                trans.Complete();
            }
        }

        public void OnVoid(long cashSalesID, string voidRemarks, string userName)
        {
            var cashSales = RetrieveByID(cashSalesID);
            var cashSalesDetails = RetrieveDetails(cashSalesID);

            voidFromOldQty(cashSales, cashSalesDetails);
        }

        #region Post to Accounting Result

        public void PostAccounting(long cashSalesID, int Status)
        {
            var cashSales = RetrieveByID(cashSalesID);

            new ABCAPOSDAC().DeleteAccountingResults(cashSalesID, AccountingResultDocumentType.CashSales);

            if (Status != (int)MPL.DocumentStatus.Void)
                CreateAccountingResult(cashSalesID);
        }

        private void CreateAccountingResult(long cashSalesID)
        {
            var cashSales = RetrieveByID(cashSalesID);
            var cashSalesDetails = RetrieveDetails(cashSalesID);

            decimal subtotalAmount = 0;
            decimal taxAmount = 0;
            decimal basePriceTotal = 0;

            var accountingResultList = new List<AccountingResultModel>();

            foreach (var cashSalesDetail in cashSalesDetails)
            {
                var product = new ProductBFC().RetrieveByID(cashSalesDetail.ProductID);

                if (product != null)
                {
                    basePriceTotal += Convert.ToDecimal(cashSalesDetail.Quantity) * product.BasePrice;
                }
            }

            subtotalAmount = cashSales.SubTotal * cashSales.ExchangeRate;
            taxAmount = cashSales.TaxValue * cashSales.ExchangeRate;

            var totalSales = subtotalAmount + taxAmount;

            accountingResultList = AddToAccountingResultList(accountingResultList, cashSales, (long)PostingAccount.KasBesar, AccountingResultType.Debit, totalSales, "Kas/Bank cash sales " + cashSales.Code);

            accountingResultList = AddToAccountingResultList(accountingResultList, cashSales, (long)PostingAccount.PenjualanTunai, AccountingResultType.Credit, subtotalAmount, "Penjualan tunai cash sales " + cashSales.Code);

            accountingResultList = AddToAccountingResultList(accountingResultList, cashSales, (long)PostingAccount.HutangDagangPPN, AccountingResultType.Credit, taxAmount, "Hutang dagang PPN cash sales " + cashSales.Code);

            accountingResultList = AddToAccountingResultList(accountingResultList, cashSales, (long)PostingAccount.HargaPokokPenjualan, AccountingResultType.Debit, basePriceTotal, "HPP cash sales " + cashSales.Code);

            accountingResultList = AddToAccountingResultList(accountingResultList, cashSales, (long)PostingAccount.PersediaanBarangJadi, AccountingResultType.Credit, basePriceTotal, "Persediaan dalam jadi cash sales " + cashSales.Code);

            new AccountingResultBFC().Posting(accountingResultList);
        }

        private List<AccountingResultModel> AddToAccountingResultList(List<AccountingResultModel> resultList, CashSalesModel obj, long accountID, AccountingResultType resultType, decimal amount, string remarks)
        {
            if (amount > 0)
            {
                var account = new AccountBFC().RetrieveByID(accountID);
                var result = new AccountingResultModel();

                result.DocumentID = obj.ID;
                result.DocumentType = (int)AccountingResultDocumentType.CashSales;
                result.Type = (int)resultType;
                result.Date = obj.Date;
                result.AccountID = account.ID;
                result.DocumentNo = obj.Code;
                result.Amount = amount;

                if (resultType == AccountingResultType.Debit)
                    result.DebitAmount = amount;
                else
                    result.CreditAmount = amount;

                result.Remarks = remarks;

                resultList.Add(result);
            }

            return resultList;
        }
        #endregion

        public void Validate(CashSalesModel obj, List<CashSalesDetailModel> details)
        {
            foreach (var detail in details)
            {
                if (detail.Discount == null)
                    detail.Discount = 0;

                if (detail.ProductID == 0)
                    throw new Exception("Produk belum dipilih");

                if (detail.Quantity == 0)
                    throw new Exception("Qty Produk tidak boleh nol");

                var product = new ProductBFC().RetrieveByID(detail.ProductID);
                var customer = new CustomerBFC().RetrieveByID(obj.CustomerID);

                var total = Convert.ToDecimal(detail.Quantity) * detail.Price;
                var totalPrice = total - (total * detail.Discount);

                //if (totalPrice == 0)
                //    throw new Exception("Total harus lebih besar dari nol");

                var warehouse = new WarehouseBFC().RetrieveByID(obj.WarehouseID);
                var qtyCustomUnit = detail.Quantity;
                var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                var qtyBaseUnit = detail.Quantity * unitRate;

                //var itemLocation = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductID, obj.WarehouseID);
                //if (itemLocation == null)
                //{
                //    throw new Exception("Produk "+ product.Code +" tidak ada di gudang " + warehouse.Name);
                //}
                //else
                //{
                //if (itemLocation.QtyOnHand < qtyBaseUnit)
                //    throw new Exception("Produk " + product.Code + " tidak ada stok fisik di gudang " + warehouse.Name);
                //}

            }
        }

        public void UpdateValidation(CashSalesModel obj, List<CashSalesDetailModel> details)
        {
            foreach (var detail in details)
            {
                if (detail.ConversionIDTemp != 0)
                    detail.ConversionID = detail.ConversionIDTemp;
            }
            //var delivery = new DeliveryOrderBFC().RetrieveByCashSalesID(obj.ID);
            //if (delivery != null)
            //{
            //    foreach (var detail in details)
            //    {
            //        if (detail.ConversionIDTemp != 0)
            //            detail.ConversionID = detail.ConversionIDTemp;

            //        if (detail.Quantity < detail.CreatedDOQuantity)
            //            throw new Exception("new QTY" + detail.ProductCode + "cannot less than Qty Fulfilled");

            //        if (detail.Quantity < detail.CreatedInvQuantity)
            //            throw new Exception("new QTY" + detail.ProductCode + "cannot less than Qty Invoiced");

            //    }
            //    //Validate(obj, details);
            //}
        }

        public ABCAPOSReportEDSC.InvoiceDTRow RetrievePrintOut(long cashSalesID)
        {
            return new ABCAPOSReportDAC().RetrieveCashSalesPrintOut(cashSalesID);
        }

        public ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable RetrieveDetailPrintOut(long cashSalesID)
        {
            return new ABCAPOSReportDAC().RetrieveCashSalesDetailPrintOut(cashSalesID);
        }

    }
}
