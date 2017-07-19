using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using ABCAPOS.DA;
using ABCAPOS.Models;
using System.Transactions;
using ABCAPOS.EDM;
using MPL.Business;

namespace ABCAPOS.BF
{
    public class PurchaseBillBFC : MasterDetailBFC<PurchaseBill, v_PurchaseBill, PurchaseBillDetail, v_PurchaseBillDetail, PurchaseBillModel, PurchaseBillDetailModel>
    {

        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetPurchaseBillCode(PurchaseBillModel header)
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var PurchaseBillPrefix = "";

            if (prefixSetting != null)
                PurchaseBillPrefix = prefixSetting.PurchaseBillPrefix;

            var warehouse = new WarehouseBFC().RetrieveByID(header.WarehouseID);
            var year = DateTime.Now.Year.ToString().Substring(2, 2);

            var prefix = PurchaseBillPrefix + year + "-" + warehouse.Code + "-";
            var code = new ABCAPOSDAC().RetrievePurchaseBillMaxCode(prefix, 7);

            return code;
        }

        protected override GenericDetailDAC<PurchaseBillDetail, PurchaseBillDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<PurchaseBillDetail, PurchaseBillDetailModel>("PurchaseBillID", "ItemNo", false);
        }

        protected override GenericDetailDAC<v_PurchaseBillDetail, PurchaseBillDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_PurchaseBillDetail, PurchaseBillDetailModel>("PurchaseBillID", "ItemNo", false);
        }

        protected override GenericDAC<PurchaseBill, PurchaseBillModel> GetMasterDAC()
        {
            return new GenericDAC<PurchaseBill, PurchaseBillModel>("ID", false, "Date DESC");
        }

        protected override GenericDAC<v_PurchaseBill, PurchaseBillModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_PurchaseBill, PurchaseBillModel>("ID", false, "Date DESC");
        }

        public override void Create(PurchaseBillModel header, List<PurchaseBillDetailModel> details)
        {
            //Code is now editable
            if (header.Code.Length == 0)
            {
                header.Code = GetPurchaseBillCode(header);
            }
            //OnCreatePurchaseBill(header, details);

            AssignValues(header, details);

            base.Create(header, details);

            OnUpdated(header.ID);

            PostAccounting(header.ID);

            new PurchaseOrderBFC().UpdateStatus(header.PurchaseOrderID);
        }

        public override void Update(PurchaseBillModel header, List<PurchaseBillDetailModel> details)
        {
            //Please dont change the location after base.Update
            updateFromOldQty(header, details);
            AssignValues(header, details);

            //foreach (var detail in details)
            //{
            //    detail.Total = (detail.TotalAmount - Convert.ToDecimal(detail.Discount)) + detail.TotalPPN;
            //}
            //header.Amount = Math.Round(details.Sum(p => p.TotalAmount), 2);
            //header.DiscountAmount = Math.Round(details.Sum(p => p.Discount), 2);
            //header.TaxAmount = Math.Round(details.Sum(p => p.TotalPPN), 2);
            //header.GrandTotal = Convert.ToDouble(Math.Round(details.Sum(p => p.Total), 2));

            base.Update(header, details);
            OnUpdated(header.ID);

            PostAccounting(header.ID);
           
            new PurchaseOrderBFC().UpdateStatus(header.PurchaseOrderID);
        }

        public void updateFromOldQty(PurchaseBillModel header, List<PurchaseBillDetailModel> details)
        {
            var oldDetails = RetrieveDetails(header.ID);
            
            foreach (var detail in details)
            {
                var oldDetail = from i in oldDetails
                                where i.ProductID == detail.ProductID && i.ItemNo == detail.ItemNo
                                select i;

                var oldPbDetail = oldDetail.FirstOrDefault();

                detail.SelisihQuantity = detail.Quantity - oldPbDetail.Quantity;

            }
            // no more need, CreatedPBQuantity is automated by View Table
            //Update CreatedPBQuantity
            //new PurchaseOrderBFC().UpdateCreatedPBQuantity(header.PurchaseOrderID, details);
            
        }

        public void CreateByPurchaseOrder(PurchaseBillModel pb, long poID)
        {
            var purchaseOrder = new PurchaseOrderBFC().RetrieveByID(poID);

            if (purchaseOrder != null)
            {
                //GetPurchaseBillCode(header);

                pb.PurchaseOrderID = poID;
                pb.PurchaseOrderCode = purchaseOrder.Code;
                pb.PurchaseOrderDate = purchaseOrder.Date;
                pb.PurchaseOrderTitle = purchaseOrder.Title;
                pb.VendorCode = purchaseOrder.VendorCode;
                pb.VendorName = purchaseOrder.VendorName;
                pb.TaxNumber = purchaseOrder.TaxNumber;
                pb.POSupplierNo = purchaseOrder.POSupplierNo;

                var warehouse = new WarehouseBFC().RetrieveByID(purchaseOrder.WarehouseID);
                pb.WarehouseID = warehouse.ID;
                pb.WarehouseName = warehouse.Name;

                var department = new DepartmentBFC().RetrieveByID(purchaseOrder.DepartmentID);

                if (department != null)
                    pb.DepartmentName = department.Name;
                pb.CurrencyID = purchaseOrder.CurrencyID;
                pb.CurrencyName = purchaseOrder.CurrencyName;
                pb.ExchangeRate = purchaseOrder.ExchangeRate;

                pb.TermOfPayment = Convert.ToInt32(purchaseOrder.TermsID);
                pb.TermOfPaymentName = purchaseOrder.TermsOfName;

                pb.DueDate = pb.Date.AddDays(purchaseOrder.Terms);

                var poDetails = new PurchaseOrderBFC().RetrieveDetails(poID);
                var pbDetails = new List<PurchaseBillDetailModel>();

                foreach (var poDetail in poDetails)
                {
                    if (poDetail.Quantity - poDetail.CreatedPBQuantity > 0)
                    {
                        var detail = new PurchaseBillDetailModel();

                        detail.PurchaseOrderItemNo = poDetail.ItemNo;
                        detail.ProductID = poDetail.ProductID;
                        detail.Barcode = poDetail.Barcode;
                        detail.ProductCode = poDetail.ProductCode;
                        detail.ProductName = poDetail.ProductName;
                        detail.ConversionName = poDetail.ConversionName;
                        detail.TaxType = poDetail.TaxType;
                        detail.AssetPrice = poDetail.AssetPrice;
                        detail.Discount = poDetail.Discount;
                        detail.QtyPO = Convert.ToDouble(poDetail.Quantity);
                        detail.QtyReceive = Convert.ToDouble(poDetail.CreatedPDQuantity);
                        detail.CreatedPOQuantity = poDetail.Quantity;
                        detail.CreatedPBQuantity = poDetail.CreatedPBQuantity;
                        detail.CreatedPDQuantity = poDetail.CreatedPDQuantity;
                        //detail.QtyRemain = Convert.ToDouble(poDetail.Quantity - poDetail.CreatedPBQuantity);
                        detail.QtyRemain = detail.Quantity = poDetail.OutstandingPBQuantity;//Convert.ToDouble(poDetail.Quantity - poDetail.CreatedPBQuantity);
                        detail.StrQuantity = detail.QtyRemain.ToString("N5");
                        detail.Total = (detail.TotalAmount - Convert.ToDecimal(detail.Discount)) + detail.TotalPPN;
                        detail.StrTotal = detail.Total.ToString("N2");
                        if (poDetail.OutstandingPBQuantity < 0)
                        {
                            detail.StrQuantity = "0";
                            detail.Quantity = 0;
                        }
                        detail.Remarks = poDetail.Remarks;

                        var itemLoc = new ItemLocationBFC().RetrieveByProductIDWarehouseID(poDetail.ProductID, purchaseOrder.WarehouseID);
                        if (itemLoc != null)
                            detail.StockQty = itemLoc.QtyOnHand;
                        else
                            detail.StockQty = 0;

                        pbDetails.Add(detail);
                    }
                }

                pb.Amount = Math.Round(pbDetails.Sum(p => p.TotalAmount),2);
                pb.DiscountAmount = Math.Round(pbDetails.Sum(p => p.Discount), 2);
                pb.TaxAmount = Math.Round(pbDetails.Sum(p => p.TotalPPN),2);
                pb.GrandTotal = Convert.ToDouble(Math.Round(pbDetails.Sum(p => p.Total),2));
                
                pb.Details = pbDetails;
                //OnCreatePurchaseBill(pb, pbDetails);
            }

        }

        public void AssignValues(PurchaseBillModel pb, List<PurchaseBillDetailModel> pbDetails)
        {
            foreach (var pbDetail in pbDetails)
            {
       
                pbDetail.Remarks = "";
                if (!string.IsNullOrEmpty(pbDetail.StrQuantity))
                    pbDetail.Quantity = Convert.ToDouble(pbDetail.StrQuantity);
                else
                    pbDetail.Quantity = 0;

                pbDetail.Total = Math.Round((pbDetail.TotalAmount - Convert.ToDecimal(pbDetail.Discount)) + pbDetail.TotalPPN,2);

                //if (!string.IsNullOrEmpty(pbDetail.StrTotal))
                //    pbDetail.Total = Convert.ToDecimal(pbDetail.StrTotal);
                //else
                //    pbDetail.Total = 0;
            }
            if (pb.ApprovedBy == null)
                pb.ApprovedBy = pb.ModifiedBy;
            if (pb.ApprovedDate == null)
                pb.ApprovedDate = pb.ModifiedDate;
            pb.Amount = Math.Round(pbDetails.Sum(p => p.TotalAmount),2);
            pb.DiscountAmount = Math.Round(pbDetails.Sum(p => p.Discount), 2);
            pb.TaxAmount = Math.Round(pbDetails.Sum(p => p.TotalPPN),2);
            pb.GrandTotal = Convert.ToDouble(Math.Round(pbDetails.Sum(p => p.Total),2));
        }

        public void Validate(PurchaseBillModel pb, List<PurchaseBillDetailModel> pbDetails)
        {
            var obj = RetrieveByID(pb.ID);
            var poDetails = new PurchaseOrderBFC().RetrieveDetails(pb.PurchaseOrderID);

            var extDetails = new List<PurchaseBillDetailModel>();

            if (obj != null)
                extDetails = RetrieveDetails(obj.ID);

            foreach (var pbDetail in pbDetails)
            {
                if (pbDetail.Quantity == 0)
                {
                    var product = new ProductBFC().RetrieveByID(pbDetail.ProductID);

                    throw new Exception("Product Qty " + product.Code + " cannot be zero");
                }

                var query = from i in poDetails
                            where i.ProductID == pbDetail.ProductID && i.ItemNo == pbDetail.PurchaseOrderItemNo
                            select i;

                var poDetail = query.FirstOrDefault();
                var pdQty = pbDetail.Quantity;

                //if (obj != null)
                //{
                //    var extDetail = (from i in extDetails
                //                     where i.ProductID == pbDetail.ProductID && i.ItemNo == pbDetail.PurchaseOrderItemNo
                //                     select i).FirstOrDefault();

                //    poDetail.CreatedPBQuantity -= extDetail.Quantity;
                //}

                if (poDetail.OutstandingPBQuantity < pdQty)
                    throw new Exception("Billed quantity of " + pbDetail.ProductCode + " will be greater than ordered");
            }
            
        }

        public void UpdateValidation(PurchaseBillModel pd, List<PurchaseBillDetailModel> pdDetails)
        {
            var obj = RetrieveByID(pd.ID);
            var poDetails = new PurchaseOrderBFC().RetrieveDetails(pd.PurchaseOrderID);

            var extDetails = new List<PurchaseBillDetailModel>();

            if (obj != null)
                extDetails = RetrieveDetails(obj.ID);

            foreach (var pdDetail in pdDetails)
            {
                pdDetail.Remarks = "";
                if (!string.IsNullOrEmpty(pdDetail.StrQuantity))
                    pdDetail.Quantity = Convert.ToDouble(pdDetail.StrQuantity);
                else
                    pdDetail.Quantity = 0;
            }
        }

        public void OnUpdated(long pdID)
        {
            var pd = RetrieveByID(pdID);

            if (pd != null)
            {
                var purchaseOrder = new PurchaseOrderBFC().RetrieveByID(pd.PurchaseOrderID);

                if (purchaseOrder != null)
                {
                    var pbDetails = RetrieveDetailsByPOID(purchaseOrder.ID);
                    var poDetails = new PurchaseOrderBFC().RetrieveDetails(purchaseOrder.ID);

                    var fulfilledCount = 0;

                    foreach (var poDetail in poDetails)
                    {
                        //var query = from i in pbDetails
                        //            where i.ProductID == poDetail.ProductID && i.PurchaseOrderItemNo == poDetail.ItemNo
                        //            select i.Quantity;

                        //poDetail.CreatedPBQuantity = query.Sum();

                        if (poDetail.Quantity == poDetail.CreatedPBQuantity)
                            fulfilledCount += 1;
                    }

                    if (pbDetails.Count == 0)
                        purchaseOrder.HasPB = false;
                    else
                        purchaseOrder.HasPB = true;

                    if (fulfilledCount == poDetails.Count)
                    {
                        purchaseOrder.IsPBFulfilled = true;
                        //purchaseOrder.Status = (int)PurchaseOrderStatus.PB;
                    }
                    else
                    {
                        purchaseOrder.IsPBFulfilled = false;
                        //purchaseOrder.Status = (int)PurchaseOrderStatus.PartialBill;
                    }

                    new PurchaseOrderBFC().Update(purchaseOrder, poDetails);
                    new PurchaseOrderBFC().UpdateDetails(purchaseOrder.ID, poDetails);
                   
                }
            }
        }

        public void Void(long purchaseBillID, string voidRemarks, string userName)
        {
            var purchaseBill = RetrieveByID(purchaseBillID);
            var oldStatus = purchaseBill.Status;

            //if (oldStatus == (int)MPL.DocumentStatus.Approved)
            //    OnVoid(purchaseBillID, voidRemarks, userName);

            purchaseBill.Status = (int)MPL.DocumentStatus.Void;
            purchaseBill.VoidRemarks = voidRemarks;
            purchaseBill.ApprovedBy = userName;
            purchaseBill.ApprovedDate = DateTime.Now;
            Update(purchaseBill);

            OnUpdated(purchaseBillID);

            new PurchaseOrderBFC().UpdateStatus(purchaseBill.PurchaseOrderID);

            PostAccounting(purchaseBillID);
        }

        #region Post to Accounting Result

        public void PostAccounting(long purchaseBillID)
        {
            var purchaseDelivery = RetrieveByID(purchaseBillID);

            new ABCAPOSDAC().DeleteAccountingResults(purchaseBillID, AccountingResultDocumentType.PurchaseBill);

            if (purchaseDelivery != null)
            {
                if (purchaseDelivery.Status != (int)MPL.DocumentStatus.Void)
                    CreateAccountingResult(purchaseBillID);
            }
        }

        private void CreateAccountingResult(long purchaseBillID)
        {
            var purchaseBill = RetrieveByID(purchaseBillID);
            var purchaseBillDetails = RetrieveDetails(purchaseBillID);

            decimal totalWithoutTax = 0;
            decimal taxAmount = 0;

            var accountingResultList = new List<AccountingResultModel>();

            totalWithoutTax = (purchaseBillDetails.Sum(p => p.TotalAmount)) * purchaseBill.ExchangeRate;
            taxAmount = (purchaseBillDetails.Sum(p => p.TotalPPN)) * purchaseBill.ExchangeRate;

            accountingResultList = AddToAccountingResultList(accountingResultList, purchaseBill, (int)PostingAccount.HutangDagangYangBelumDifakturkan, AccountingResultType.Debit, totalWithoutTax, "Hutang dagang yang belum difakturkan purchase bill " + purchaseBill.Code);

            accountingResultList = AddToAccountingResultList(accountingResultList, purchaseBill, (int)PostingAccount.UangMukaPPN, AccountingResultType.Debit, taxAmount, "Pajak purchase bill " + purchaseBill.Code);

            decimal totalAmount = totalWithoutTax + taxAmount;
            accountingResultList = AddToAccountingResultList(accountingResultList, purchaseBill, (int)PostingAccount.HutangDagang, AccountingResultType.Credit, totalAmount, "Hutang dagang purchase bill " + purchaseBill.Code);

            new AccountingResultBFC().Posting(accountingResultList);
        }

        private List<AccountingResultModel> AddToAccountingResultList(List<AccountingResultModel> resultList, PurchaseBillModel obj, long accountID, AccountingResultType resultType, decimal amount, string remarks)
        {
            if (amount > 0)
            {
                var account = new AccountBFC().RetrieveByID(accountID);
                var result = new AccountingResultModel();

                result.DocumentID = obj.ID;
                result.DocumentType = (int)AccountingResultDocumentType.PurchaseBill;
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

        public int RetrieveUncreatedPurchasePaymentBillCount(List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveUncreatedPurchasePaymentBillCount(selectFilters);
        }

        public List<PurchaseBillModel> RetrieveUncreatedPurchasePaymentBill(int startIndex, int? amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            if (amount == null)
                amount = SystemConstants.ItemPerPage;

            return new ABCAPOSDAC().RetrieveUncreatedPurchasePaymentBill(startIndex, (int)amount, sortParameter, selectFilters);
        }

        public List<PurchaseBillModel> RetrieveByPOID(long purchaseOrderID)
        {
            return new ABCAPOSDAC().RetrievePBByPOID(purchaseOrderID);
        }

        public List<PurchaseBillModel> RetrieveByBOID(long bookingOrderID)
        {
            var poList = new PurchaseOrderBFC().RetrieveByBOID(bookingOrderID);
            var pdList = new List<PurchaseBillModel>();

            foreach (var po in poList)
            {
                var pdList2 = new ABCAPOSDAC().RetrievePBByPOID(po.ID);
                pdList.AddRange(pdList2);
            }

            return pdList;
        }

        public PurchaseBillModel RetreivePBByPurchaseOrderID(long purchaseOrderID)
        {
            return new ABCAPOSDAC().RetrievePBByPurchaseOrderID(purchaseOrderID);
        }

        public List<PurchaseBillDetailModel> RetrieveDetailsByPDID(long pdID)
        {
            return new ABCAPOSDAC().RetrievePurchaseBillDetailByPDID(pdID);
        }

        public List<PurchaseBillDetailModel> RetrieveDetailsByPOID(long poID)
        {
            return new ABCAPOSDAC().RetrievePurchaseBillDetailByPOID(poID);
        }

        public List<PurchaseBillDetailModel> RetreivePurchasebillByProductID(long productID)
        {
            return new ABCAPOSDAC().RetreivePurchaseBillByProductID(productID);
        }

        public List<PurchaseBillModel> RetrievePayableBills(long vendorID, long currencyID)
        {
            return new ABCAPOSDAC().RetrievePurchaseBillByVendorID(vendorID, currencyID);
        }

        public List<PurchaseBillModel> CodeExists(string code)
        {
            return new ABCAPOSDAC().RetrievePurchaseBillByCode(code);
        }
    }
}
