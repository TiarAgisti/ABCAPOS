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
    public class ApplyBillCreditBFC : MasterDetailBFC<ApplyBillCredit, v_ApplyBillCredit, ApplyBillCreditDetail, v_ApplyBillCreditDetail, ApplyBillCreditModel, ApplyBillCreditDetailModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetApplyBillCreditCode()
        {
            //var prefixSetting = new PrefixSettingBFC().Retrieve();
            //var ApplyBillCreditPrefix = "";

            //if (prefixSetting != null)
            //    ApplyBillCreditPrefix = prefixSetting.VendorReturnCreditPrefix;

            var code = new ABCAPOSDAC().RetrieveApplyBillCreditMaxCode("", 5);

            return code;
        }

        protected override GenericDetailDAC<ApplyBillCreditDetail, ApplyBillCreditDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<ApplyBillCreditDetail, ApplyBillCreditDetailModel>("ApplyBillCreditID", "ItemNo", false);
        }

        protected override GenericDetailDAC<v_ApplyBillCreditDetail, ApplyBillCreditDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_ApplyBillCreditDetail, ApplyBillCreditDetailModel>("ApplyBillCreditID", "ItemNo", false);
        }

        protected override GenericDAC<ApplyBillCredit, ApplyBillCreditModel> GetMasterDAC()
        {
            return new GenericDAC<ApplyBillCredit, ApplyBillCreditModel>("ID", false, "Code DESC");
        }

        protected override GenericDAC<v_ApplyBillCredit, ApplyBillCreditModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_ApplyBillCredit, ApplyBillCreditModel>("ID", false, "Code DESC");
        }

        public override void Create(ApplyBillCreditModel header, List<ApplyBillCreditDetailModel> details)
        {
            header.Code = GetApplyBillCreditCode();
            base.Create(header, details);
        }

        public void Validate(ApplyBillCreditModel obj, List<ApplyBillCreditDetailModel> details)
        {
            if (details.Sum(p => p.Amount) > obj.CeilingAmount)
                throw new Exception("Total exceeds credit amount");
            foreach (var detail in details)
            {
                var bill = new PurchaseBillBFC().RetrieveByID(detail.PurchaseBillID);
                if (detail.Amount > bill.OutstandingAmount)
                {
                    throw new Exception("Credit amount for " + detail.Code + " is greater than due");
                }
            }
        }

        public void Void(long ApplyBillCreditID, string voidRemarks, string userName)
        {
            var ApplyBillCredit = RetrieveByID(ApplyBillCreditID);
            var oldStatus = ApplyBillCredit.Status;
            ApplyBillCredit.Status = (int)MPL.DocumentStatus.Void;
            ApplyBillCredit.VoidRemarks = voidRemarks;
            ApplyBillCredit.ApprovedDate = DateTime.Now;
            ApplyBillCredit.ApprovedBy = userName;

            using (TransactionScope trans = new TransactionScope())
            {
                Update(ApplyBillCredit);
                trans.Complete();
            }

        }

        public void PreFillWithBillCreditData(ApplyBillCreditModel header, long p)
        {
            var billCreditBFC = new BillCreditBFC();
            var source = billCreditBFC.RetrieveByID(p);
            if (source == null)
                return;

            header.Code = GetApplyBillCreditCode();
            header.Date = DateTime.Now;
            header.BillCreditID = source.ID;
            header.BillCreditCode = source.Code;
            header.VendorID = source.VendorID;
            header.VendorName = source.VendorName;
            header.CurrencyID = source.CurrencyID;
            header.CurrencyName = source.CurrencyName;
            header.ExchangeRate = source.ExchangeRate;
            header.Title = source.Title;
            header.CeilingAmount = source.TotalUnapplied;
            header.CreditRemaining = source.TotalUnapplied;

            var payableBills = new PurchaseBillBFC().RetrievePayableBills(header.VendorID, header.CurrencyID);
            var returnDetails = new List<ApplyBillCreditDetailModel>();

            foreach (var bill in payableBills)
            {
                var detail = new ApplyBillCreditDetailModel();
                detail.PurchaseBillID = bill.ID;
                detail.Code = bill.Code;
                detail.Subtotal = bill.Amount;
                detail.TaxTotal = bill.TaxAmount;
                detail.OriginalAmount = detail.Subtotal + detail.TaxTotal;
                detail.PaymentAmount = bill.PaymentAmount;
                detail.CreditedAmount = bill.CreditedAmount;
                detail.AmountDue = detail.OriginalAmount - detail.PaymentAmount - detail.CreditedAmount;
                detail.DueDate = bill.DueDate;

                returnDetails.Add(detail);
            }

            header.Details = returnDetails;
        }

        public List<ApplyBillCreditModel> RetrieveByBillCreditID(long p)
        {
            return new ABCAPOSDAC().RetrieveApplyBillCreditByBillCreditID(p);
        }

        public List<ApplyBillCreditDetailModel> RetrieveByBillID(long billID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from detail in ent.v_ApplyBillCreditDetail
                        where detail.PurchaseBillID == billID && detail.Status != (int)MPL.DocumentStatus.Void
                        select detail;

            return ObjectHelper.CopyList<v_ApplyBillCreditDetail, ApplyBillCreditDetailModel>(query.ToList());
        }

        public int RetrieveApplyBillCreditCount(List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveApplyBillCreditCount(selectFilters);
        }

        public List<ApplyBillCreditModel> RetrieveApplyBillCredit(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveApplyBillCredit(startIndex, amount, sortParameter, selectFilters);
        }
    }

}
