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
    public class BillCreditBFC : MasterDetailBFC<BillCredit, v_BillCredit, BillCreditDetail, v_BillCreditDetail, BillCreditModel, BillCreditDetailModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetBillCreditCode()
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var BillCreditPrefix = "";

            if (prefixSetting != null)
                BillCreditPrefix = prefixSetting.VendorReturnCreditPrefix;

            var code = new ABCAPOSDAC().RetrieveBillCreditMaxCode(BillCreditPrefix, 5);

            return code;
        }

        protected override GenericDetailDAC<BillCreditDetail, BillCreditDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<BillCreditDetail, BillCreditDetailModel>("BillCreditID", "ItemNo", false);
        }

        protected override GenericDetailDAC<v_BillCreditDetail, BillCreditDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_BillCreditDetail, BillCreditDetailModel>("BillCreditID", "ItemNo", false);
        }

        protected override GenericDAC<BillCredit, BillCreditModel> GetMasterDAC()
        {
            return new GenericDAC<BillCredit, BillCreditModel>("ID", false, "Date DESC");
        }

        protected override GenericDAC<v_BillCredit, BillCreditModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_BillCredit, BillCreditModel>("ID", false, "Date DESC");
        }

        public override void Create(BillCreditModel header, List<BillCreditDetailModel> details)
        {
            header.Code = GetBillCreditCode();
            //CalculatePODiscount(header, details);
            base.Create(header, details);

            PostAccounting(header.ID);
           
            //OnCreatedUpdated(header.ID, "Create");
        }

        public override void Update(BillCreditModel header, List<BillCreditDetailModel> details)
        {
            base.Update(header, details);

            PostAccounting(header.ID);
        }

        public void UpdateDetails(long poID, List<BillCreditDetailModel> poDetails)
        {
            var dac = new ABCAPOSDAC();
            var BillCredit = RetrieveByID(poID);

            using (TransactionScope trans = new TransactionScope())
            {
                GetDetailDAC().DeleteByParentID(poID);
                //dac.CreateBillCreditDetails(poID, poDetails);

                trans.Complete();
            }
        }

        public void Validate(BillCreditModel obj, List<BillCreditDetailModel> details)
        {
            decimal POTotal = 0;

            foreach (var detail in details)
            {
                if (detail.ProductID == 0)
                    throw new Exception("Product not chosen");

                if (detail.Quantity == 0)
                    throw new Exception("Qty Product cannot be zero");

                var total = Convert.ToDecimal(detail.Quantity) * detail.AssetPrice;
                POTotal = Convert.ToDecimal(total);

                if (POTotal == 0)
                    throw new Exception("Total must be higher than zero");
            }
        }

        public void Approve(long BillCreditID, string userName)
        {
            var BillCredit = RetrieveByID(BillCreditID);

            BillCredit.Status = (int)MPL.DocumentStatus.Approved;
            BillCredit.ApprovedBy = userName;
            BillCredit.ApprovedDate = DateTime.Now;
            Update(BillCredit);
        }

        public void Void(long BillCreditID, string voidRemarks, string userName)
        {
            var BillCredit = RetrieveByID(BillCreditID);
            var oldStatus = BillCredit.Status;
            BillCredit.Status = (int)MPL.DocumentStatus.Void;
            BillCredit.VoidRemarks = voidRemarks;
            BillCredit.ApprovedDate = DateTime.Now;
            BillCredit.ApprovedBy = userName;

            Update(BillCredit);

            PostAccounting(BillCreditID);
        }

        public void PreFillWithReturnData(BillCreditModel header, long p)
        {
            var poBFC = new VendorReturnBFC();
            var source = poBFC.RetrieveByID(p);
            if (source == null)
                return;

            header.Code = GetBillCreditCode();
            header.Date = DateTime.Now;
            header.VendorID = source.SupplierID;
            header.VendorCode = source.VendorCode;
            header.VendorName = source.VendorName;
            header.CurrencyID = source.CurrencyID;
            header.CurrencyName = source.CurrencyName;
            header.ExchangeRate = source.ExchangeRate;
            header.VendorReturnID = source.ID;
            header.VendorReturnCode = source.Code;
            header.TaxNumber = source.TaxNumber;
            header.Title = source.Title;
            header.SupplierReference = source.SupplierReference;

            header.DepartmentID = source.DepartmentID;
            header.DepartmentName = source.DepartmentName;
            header.WarehouseID = source.WarehouseID;
            header.WarehouseName = source.WarehouseName;
            header.IsFakturPajak = source.IsFPPembelian;
            header.SupplierFPNo = source.SupplierFPNo;

            header.TotalApplied = 0;

            var sourceDetails = poBFC.RetrieveDetails(p);
            var returnDetails = new List<BillCreditDetailModel>();

            foreach (var sourceDetail in sourceDetails)
            {
                if (sourceDetail.CreatedCreditQuantity < sourceDetail.CreatedDeliveryQuantity)
                {
                    var detail = new BillCreditDetailModel();
                    detail.VendorReturnItemNo = sourceDetail.ItemNo;
                    detail.ProductID = sourceDetail.ProductID;
                    detail.ProductCode = sourceDetail.ProductCode;
                    detail.ProductName = sourceDetail.ProductName;
                    detail.Quantity = (double)sourceDetail.CreatedDeliveryQuantity - sourceDetail.CreatedCreditQuantity;
                    detail.ConversionID = sourceDetail.ConversionID;
                    detail.ConversionName = sourceDetail.ConversionName;
                    detail.TaxType = sourceDetail.TaxType;
                    detail.Remarks = sourceDetail.Remarks;
                    detail.AssetPrice = (decimal)sourceDetail.AssetPrice;
                    detail.Barcode = sourceDetail.Barcode;
                    returnDetails.Add(detail);
                }
            }

            header.Details = returnDetails;
        }

        #region Post to Accounting Result

        public void PostAccounting(long billCreditID)
        {
            var payment = RetrieveByID(billCreditID);

            new ABCAPOSDAC().DeleteAccountingResults(billCreditID, AccountingResultDocumentType.BillCredit);

            if (payment.Status != (int)MPL.DocumentStatus.Void)
                CreateAccountingResult(billCreditID);
        }

        private void CreateAccountingResult(long billCreditID)
        {
            var billCredit = RetrieveByID(billCreditID);

            var accountingResultList = new List<AccountingResultModel>();

            decimal totalAmount = (billCredit.Total + billCredit.TaxAmount) * billCredit.ExchangeRate;

            accountingResultList = AddToAccountingResultList(accountingResultList, billCredit, (long)PostingAccount.HutangDagang, AccountingResultType.Debit, totalAmount, "Hutang dagang bill credit " + billCredit.Code);

            accountingResultList = AddToAccountingResultList(accountingResultList, billCredit, (long)PostingAccount.PersediaanBahanBaku, AccountingResultType.Credit, totalAmount, "Persediaan Bahan Baku bill credit " + billCredit.Code);

            new AccountingResultBFC().Posting(accountingResultList);
        }

        private List<AccountingResultModel> AddToAccountingResultList(List<AccountingResultModel> resultList, BillCreditModel obj, long accountID, AccountingResultType resultType, decimal amount, string remarks)
        {
            if (amount > 0)
            {
                var account = new AccountBFC().RetrieveByID(accountID);
                var result = new AccountingResultModel();

                result.DocumentID = obj.ID;
                result.DocumentType = (int)AccountingResultDocumentType.BillCredit;
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

        public List<BillCreditModel> RetrieveByVendorReturnID(long p)
        {
            return new ABCAPOSDAC().RetrieveBillCreditByVendorReturnID(p);
        }

        public int RetrieveBillCreditCount(List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveBillCreditCount(selectFilters);
        }

        public List<BillCreditModel> RetrieveBillCredit(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveBillCredit(startIndex, amount, sortParameter, selectFilters);
        }
    }

}
