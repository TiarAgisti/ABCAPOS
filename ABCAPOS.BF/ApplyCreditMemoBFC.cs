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
    public class ApplyCreditMemoBFC : MasterDetailBFC<ApplyCreditMemo, v_ApplyCreditMemo, ApplyCreditMemoDetail, v_ApplyCreditMemoDetail, ApplyCreditMemoModel, ApplyCreditMemoDetailModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetApplyCreditMemoCode()
        {
            //var prefixSetting = new PrefixSettingBFC().Retrieve();
            //var ApplyCreditMemoPrefix = "";

            //if (prefixSetting != null)
            //    ApplyCreditMemoPrefix = prefixSetting.CustomerReturnCreditPrefix;

            var code = new ABCAPOSDAC().RetrieveApplyCreditMemoMaxCode("", 5);

            return code;
        }

        protected override GenericDetailDAC<ApplyCreditMemoDetail, ApplyCreditMemoDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<ApplyCreditMemoDetail, ApplyCreditMemoDetailModel>("ApplyCreditMemoID", "ItemNo", false);
        }

        protected override GenericDetailDAC<v_ApplyCreditMemoDetail, ApplyCreditMemoDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_ApplyCreditMemoDetail, ApplyCreditMemoDetailModel>("ApplyCreditMemoID", "ItemNo", false);
        }

        protected override GenericDAC<ApplyCreditMemo, ApplyCreditMemoModel> GetMasterDAC()
        {
            return new GenericDAC<ApplyCreditMemo, ApplyCreditMemoModel>("ID", false, "Code DESC");
        }

        protected override GenericDAC<v_ApplyCreditMemo, ApplyCreditMemoModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_ApplyCreditMemo, ApplyCreditMemoModel>("ID", false, "Code DESC");
        }

        public override void Create(ApplyCreditMemoModel header, List<ApplyCreditMemoDetailModel> details)
        {
            header.Code = GetApplyCreditMemoCode();
            using (TransactionScope trans = new TransactionScope())
            {

                base.Create(header, details);
                trans.Complete();
            }
        }

        public void Validate(ApplyCreditMemoModel obj, List<ApplyCreditMemoDetailModel> details)
        {
            if (details.Sum(p => p.Amount) > obj.CeilingAmount)
                throw new Exception("Total exceeds credit amount");
            foreach (var detail in details)
            {
                var bill = new InvoiceBFC().RetrieveByID(detail.InvoiceID);
                if (detail.Amount > bill.OutstandingAmount)
                {
                    throw new Exception("Credit amount for " + detail.Code + " is greater than due");
                }
            }
        }

        public void Void(long ApplyCreditMemoID, string voidRemarks, string userName)
        {
            var ApplyCreditMemo = RetrieveByID(ApplyCreditMemoID);
            var oldStatus = ApplyCreditMemo.Status;
            ApplyCreditMemo.Status = (int)MPL.DocumentStatus.Void;
            ApplyCreditMemo.VoidRemarks = voidRemarks;
            ApplyCreditMemo.ApprovedDate = DateTime.Now;
            ApplyCreditMemo.ApprovedBy = userName;

            using (TransactionScope trans = new TransactionScope())
            {
                Update(ApplyCreditMemo);
                trans.Complete();
            }

        }

        public List<ApplyCreditMemoModel> RetrieveByCreditMemoID(long p)
        {
            return new ABCAPOSDAC().RetrieveApplyCreditMemoByCreditMemoID(p);
        }

        public void PreFillWithCreditMemoData(ApplyCreditMemoModel header, long p)
        {
            var creditMemoBFC = new CreditMemoBFC();
            var source = creditMemoBFC.RetrieveByID(p);
            if (source == null)
                return;

            header.Code = GetApplyCreditMemoCode();
            header.Date = DateTime.Now;
            header.CreditMemoID = source.ID;
            header.CreditMemoCode = source.Code;
            header.CustomerID = source.CustomerID;
            header.CustomerName = source.CustomerName;
            header.CurrencyID = source.CurrencyID;
            header.CurrencyName = source.CurrencyName;
            header.ExchangeRate = source.ExchangeRate;
            header.Title = source.Title;
            header.CeilingAmount = source.TotalUnapplied;
            header.CreditRemaining = source.TotalUnapplied;

            var payableInvoices = new InvoiceBFC().RetrievePayableInvoices(header.CustomerID);
            var returnDetails = new List<ApplyCreditMemoDetailModel>();

            foreach (var invoice in payableInvoices)
            {
                var detail = new ApplyCreditMemoDetailModel();
                detail.InvoiceID = invoice.ID;
                detail.Code = invoice.Code;
                detail.Subtotal = invoice.Amount;
                detail.TaxTotal = invoice.TaxAmount;
                detail.OriginalAmount = detail.Subtotal + detail.TaxTotal;
                detail.PaymentAmount = invoice.PaymentAmount;
                detail.CreditedAmount = invoice.CreditedAmount;
                detail.AmountDue = detail.OriginalAmount - detail.PaymentAmount - detail.CreditedAmount;
                detail.DueDate = invoice.DueDate;

                if (Math.Round(detail.AmountDue, 2) > 0)
                    returnDetails.Add(detail);

                //returnDetails.Add(detail);
            }

            header.Details = returnDetails;
        }

        public List<ApplyCreditMemoDetailModel> RetrieveByInvoiceID(long invoiceID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var payDetailsQuery = from detail in ent.v_ApplyCreditMemoDetail
                                  where detail.InvoiceID == invoiceID
                                    && detail.Status != (int)MPL.DocumentStatus.Void
                                  select detail;
            return ObjectHelper.CopyList<v_ApplyCreditMemoDetail, ApplyCreditMemoDetailModel>(payDetailsQuery.ToList());
        }

        public void AddOtherPayableInvoices(ApplyCreditMemoModel header, List<ApplyCreditMemoDetailModel> details)
        {
            var payableInvoices = new InvoiceBFC().RetrievePayableInvoices(header.CustomerID);
            var invoiceIDs = details.Select(p => p.InvoiceID);
            foreach (var invoice in payableInvoices)
            {
                if (!invoiceIDs.Contains(invoice.ID))
                {
                    var detail = new ApplyCreditMemoDetailModel();
                    detail.InvoiceID = invoice.ID;
                    detail.Code = invoice.Code;
                    detail.Subtotal = invoice.Amount;
                    detail.TaxTotal = invoice.TaxAmount;
                    detail.OriginalAmount = detail.Subtotal + detail.TaxTotal;
                    detail.PaymentAmount = invoice.PaymentAmount;
                    detail.CreditedAmount = invoice.CreditedAmount;
                    detail.AmountDue = detail.OriginalAmount - detail.PaymentAmount - detail.CreditedAmount;
                    detail.DueDate = invoice.DueDate;

                    if (Math.Round(detail.AmountDue, 2) > 0)
                        details.Add(detail);
                    //detail.Date = invoice.Date;

                    //details.Add(detail);
                }
            }
        }
    }

}
