using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using ABCAPOS.DA;
using System.Transactions;
using ABCAPOS.Util;
using ABCAPOS.ReportEDS;

namespace ABCAPOS.BF
{
    public class CreditMemoBFC : MasterDetailBFC<CreditMemo, v_CreditMemo, CreditMemoDetail, v_CreditMemoDetail, CreditMemoModel, CreditMemoDetailModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetCreditMemoCode()
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var invoicePrefix = "";

            if (prefixSetting != null)
                invoicePrefix = prefixSetting.CustomerReturnCreditPrefix;

            var code = new ABCAPOSDAC().RetrieveCreditMemoMaxCode(invoicePrefix, 5);

            return code;
        }

        protected override GenericDetailDAC<CreditMemoDetail, CreditMemoDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<CreditMemoDetail, CreditMemoDetailModel>("CreditMemoID", "ItemNo", false);
        }

        protected override GenericDetailDAC<v_CreditMemoDetail, CreditMemoDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_CreditMemoDetail, CreditMemoDetailModel>("CreditMemoID", "ItemNo", false);
        }

        protected override GenericDAC<CreditMemo, CreditMemoModel> GetMasterDAC()
        {
            return new GenericDAC<CreditMemo, CreditMemoModel>("ID", false, "Date DESC");
        }

        protected override GenericDAC<v_CreditMemo, CreditMemoModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_CreditMemo, CreditMemoModel>("ID", false, "Date DESC");
        }

        public void Validate(CreditMemoModel inv, List<CreditMemoDetailModel> invDetails)
        {
            var obj = RetrieveByID(inv.ID);
            var soDetails = new CustomerReturnBFC().RetrieveDetails(inv.CustomerReturnID);

            var extDetails = new List<CreditMemoDetailModel>();

            if (obj != null)
                extDetails = RetrieveDetails(obj.ID);

            foreach (var invDetail in invDetails)
            {
                if (invDetail.Quantity == 0)
                {
                    var product = new ProductBFC().RetrieveByID(invDetail.ProductID);

                    throw new Exception("Product Qty " + product.Code + " cannot be zero");
                }

                //if (soDetail.Quantity < invQty + soDetail.CreatedInvQuantity)
                //    throw new Exception("CreditMemod quantity of " + invDetail.ProductCode + " will be greater than ordered");
            }
        }

        public override void Create(CreditMemoModel header, List<CreditMemoDetailModel> details)
        {
            header.Code = GetCreditMemoCode();

            using (TransactionScope trans = new TransactionScope())
            {
                // TODO: Credit Memo - investigate why details carry wrong TotalAmount TotalPPN
                foreach (var detail in details)
                {
                    detail.TotalAmount = (decimal)detail.Quantity * detail.Price;
                    if (detail.TaxType == 2)
                    {
                        detail.TotalPPN = detail.TotalAmount * (decimal)0.1;
                    }
                    else
                        detail.TotalPPN = 0;
                }
                base.Create(header, details);

                trans.Complete();
            }
        }

        public override void Update(CreditMemoModel header, List<CreditMemoDetailModel> details)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                base.Update(header, details);

                trans.Complete();
            }
        }

        public void CreateCreditMemoByCustomerReturn(CreditMemoModel header, long salesOrderID)
        {
            var source = new CustomerReturnBFC().RetrieveByID(salesOrderID);

            if (source != null)
            {
                header.Code = GetCreditMemoCode();

                header.CustomerReturnID = salesOrderID;
                header.CustomerReturnCode = source.Code;
                header.CustomerID = source.CustomerID;

                var customer = new CustomerBFC().RetrieveByID(source.CustomerID);
                header.PriceLevelID = customer.PriceLevelID ;
                header.CustomerCode = source.CustomerCode;
                header.CustomerName = source.CustomerName;
                header.SalesmanID = customer.SalesRep;
                header.SalesReference = customer.SalesRepName;
                header.Date = DateTime.Now;
                header.RefSO = source.RefSO;
                header.RefDO = source.RefDO;
                header.Title = source.Title;
                header.WarehouseID = source.WarehouseID;
                header.WarehouseName = source.WarehouseName;

                header.DepartmentID = source.DepartmentID;
                header.DepartmentName = source.DepartmentName;

                header.SalesEffectiveDate = DateTime.Now;
                header.ExcludeCommisions = source.ExcludeCommisions;

                header.SalesmanID = source.SalesmanID;
                header.EmployeeID = source.EmployeeID;
                header.EmployeeName = source.EmployeeName;

                var soDetails = new CustomerReturnBFC().RetrieveDetails(salesOrderID);
                var headerDetails = new List<CreditMemoDetailModel>();

                foreach (var soDetail in soDetails)
                {
                    if (soDetail.CreditedQuantity < soDetail.ReceivedQuantity)
                    {
                        var detail = new CreditMemoDetailModel();

                        detail.CustomerReturnItemNo = soDetail.ItemNo;
                        detail.ProductID = soDetail.ProductID;
                        detail.Barcode = soDetail.Barcode;
                        detail.ProductCode = soDetail.ProductCode;
                        detail.ProductName = soDetail.ProductName;
                        detail.Quantity = soDetail.ReceivedQuantity - soDetail.CreditedQuantity;
                        detail.ConversionID = soDetail.ConversionID;
                        detail.ConversionName = soDetail.ConversionName;
                        detail.TaxType = soDetail.TaxType;
                        detail.PriceLevelID = soDetail.PriceLevelID;
                        detail.PriceLevelName = soDetail.PriceLevelName;
                        detail.Price = detail.AssetPrice = soDetail.Price;
                        detail.Remarks = soDetail.Remarks;
                        detail.TotalAmount = (decimal)detail.Quantity * detail.AssetPrice;
                        if (detail.TaxType == 2)
                            detail.TotalPPN = detail.TotalAmount * (decimal)(0.1);
                        else
                            detail.TotalPPN = 0;
                        detail.Total = Math.Round(detail.TotalAmount + detail.TotalPPN, 0);

                        //var itemLoc = new ItemLocationBFC().RetrieveByProductIDWarehouseID(soDetail.ProductID, source.WarehouseID);
                        //if (itemLoc != null)
                        //    detail.StockQty = itemLoc.QtyOnHand;
                        //else
                        //    detail.StockQty = 0;

                        headerDetails.Add(detail);
                    }
                }

                header.Details = headerDetails;
            }
        }

        public void Void(long creditMemoID, string voidRemarks, string userName)
        {
            var creditMemo = RetrieveByID(creditMemoID);
            var oldStatus = creditMemo.Status;
            creditMemo.Status = (int)MPL.DocumentStatus.Void;
            creditMemo.VoidRemarks = voidRemarks;
            creditMemo.ApprovedDate = DateTime.Now;
            creditMemo.ApprovedBy = userName;

            using (TransactionScope trans = new TransactionScope())
            {
                Update(creditMemo);

                trans.Complete();
            }

        }

        #region Post to Accounting Result

        public void PostAccounting(long creditMemoID)
        {
            var creditMemo = RetrieveByID(creditMemoID);

            new ABCAPOSDAC().DeleteAccountingResults(creditMemoID, AccountingResultDocumentType.CreditMemo);

            if (creditMemo.Status != (int)MPL.DocumentStatus.Void)
                CreateAccountingResult(creditMemoID);
        }

        private void CreateAccountingResult(long creditMemoID)
        {
            var creditMemo = RetrieveByID(creditMemoID);

            var accountingResultList = new List<AccountingResultModel>();

            decimal totalAmount = creditMemo.Total * creditMemo.ExchangeRate;

            accountingResultList = AddToAccountingResultList(accountingResultList, creditMemo, (long)PostingAccount.ReturPenjualan, AccountingResultType.Debit, totalAmount, "Retur penjualan credit memo " + creditMemo.Code);

            accountingResultList = AddToAccountingResultList(accountingResultList, creditMemo, (long)PostingAccount.PiutangDagang, AccountingResultType.Credit, totalAmount, "Piutang dagang credit memo " + creditMemo.Code);

            new AccountingResultBFC().Posting(accountingResultList);
        }

        private List<AccountingResultModel> AddToAccountingResultList(List<AccountingResultModel> resultList, CreditMemoModel obj, long accountID, AccountingResultType resultType, decimal amount, string remarks)
        {
            if (amount > 0)
            {
                var account = new AccountBFC().RetrieveByID(accountID);
                var result = new AccountingResultModel();

                result.DocumentID = obj.ID;
                result.DocumentType = (int)AccountingResultDocumentType.CreditMemo;
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

        #region Notification

        //public int RetrieveUncreatedPaymentCount(CustomerGroupModel customerGroup)
        //{
        //    return new ABCAPOSDAC().RetrieveUncreatedPaymentCreditMemoCount(customerGroup);
        //}

        //public int RetrieveUnapprovedCreditMemoCount(CustomerGroupModel customerGroup)
        //{
        //    return new ABCAPOSDAC().RetrieveUnapprovedCreditMemoCount(customerGroup);
        //}

        //public int RetrieveUnpaidCount(CustomerGroupModel customerGroup)
        //{
        //    return new ABCAPOSDAC().RetrieveUnpaidCreditMemoCount(customerGroup);
        //}

        //public int RetrieveOverdueCreditMemoCount(CustomerGroupModel customerGroup)
        //{
        //    return new ABCAPOSDAC().RetrieveOverdueCreditMemoCount(customerGroup);
        //}

        //public int RetrieveNotOverdueCount()
        //{
        //    return new ABCAPOSDAC().RetrieveNotOverdueCreditMemoCount();
        //}

        //public string RetrieveThisMonthCreditMemoCount()
        //{
        //    return new ABCAPOSDAC().RetrieveThisMonthCreditMemoCount();
        //}

        //public string RetrieveThisMonthOutstandingCreditMemoCount()
        //{
        //    return new ABCAPOSDAC().RetrieveThisMonthOutstandingCreditMemoCount();
        //}

        //public List<CreditMemoDetailModel> RetrieveDetailsBySOID(long soID)
        //{
        //    return new ABCAPOSDAC().RetrieveCreditMemoDetailBySOID(soID);
        //}

        #endregion

        public ABCAPOSReportEDSC.InvoiceDTRow RetrievePrintOut(long creditMemoID)
        {
            return new ABCAPOSReportDAC().RetrieveCreditMemoPrintOut(creditMemoID);
        }
        
        public ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable RetrieveDetailPrintOut(long creditMemoID)
        {
            return new ABCAPOSReportDAC().RetrieveCreditMemoDetailPrintOut(creditMemoID);
        }

        public List<CreditMemoModel> RetrieveByCustomerReturnID(long p)
        {
            return new ABCAPOSDAC().RetrieveCreditMemoByCustomerReturnID(p);
        }
    }
}
