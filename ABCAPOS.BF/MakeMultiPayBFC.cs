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
    public class MakeMultiPayBFC : MasterDetailBFC<MakeMultiPay, v_MakeMultiPay, MakeMultiPayDetail, v_MakeMultiPayDetail, MakeMultiPayModel, MakeMultiPayDetailModel>
    {
        private void ValidasiCodeExist(MakeMultiPayModel header,string Code)
        {
            if (header.Code != header.LastCode)
            {
                var CodeExist = new ABCAPOSDAC().RetreiveMakeMultiPayByCode(header.Code);
                if (CodeExist != null)
                    throw new Exception("REFERENCE NO. Can't Duplicate");
            }
        }
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetMakeMultiPayCode(MakeMultiPayModel header)
        {
            //var prefixSetting = new PrefixSettingBFC().Retrieve();
            //var makeMultiPayPrefix = "";

            //if (prefixSetting != null)
            //    makeMultiPayPrefix = prefixSetting.InvoicePrefix;

            //var warehouse = new WarehouseBFC().RetrieveByID(header.WarehouseID);
            //var year = DateTime.Now.Year.ToString().Substring(2, 2);

            //var prefix = makeMultiPayPrefix + year + "-" + warehouse.Code + "-";
            var code = new ABCAPOSDAC().RetrieveMakeMultiPaymentMaxCode(7);

            return code;
        }

        protected override GenericDetailDAC<MakeMultiPayDetail, MakeMultiPayDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<MakeMultiPayDetail, MakeMultiPayDetailModel>("MakeMultiPayID", "ItemNo", false);
        }

        protected override GenericDetailDAC<v_MakeMultiPayDetail, MakeMultiPayDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_MakeMultiPayDetail, MakeMultiPayDetailModel>("MakeMultiPayID", "ItemNo", false);
        }

        protected override GenericDAC<MakeMultiPay, MakeMultiPayModel> GetMasterDAC()
        {
            return new GenericDAC<MakeMultiPay, MakeMultiPayModel>("ID", false, "Date DESC");
        }

        protected override GenericDAC<v_MakeMultiPay, MakeMultiPayModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_MakeMultiPay, MakeMultiPayModel>("ID", false, "Date DESC");
        }

        public override void Create(MakeMultiPayModel header, List<MakeMultiPayDetailModel> details)
        {
            if (header.Code.Length <= 0)
                header.Code = GetMakeMultiPayCode(header);

            using (TransactionScope trans = new TransactionScope())
            {
                this.ValidasiCodeExist(header, header.Code);

                base.Create(header, details);

                PostAccounting(header.ID,header.Status);

                trans.Complete();
            }
        }

        public override void Update(MakeMultiPayModel header, List<MakeMultiPayDetailModel> details)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                this.ValidasiCodeExist(header, header.Code);

                base.Update(header, details);

                PostAccounting(header.ID,header.Status);

                trans.Complete();
            }
        }

        public void Validate(MakeMultiPayModel obj, List<MakeMultiPayDetailModel> details)
        {
            foreach (var detail in details)
            {
                var bill = new PurchaseBillBFC().RetrieveByID(detail.PurchaseBillID);
                if (detail.Amount + detail.DiscountTaken > bill.OutstandingAmount)
                {
                    throw new Exception("Payment amount for " + detail.Code + " is greater than due");
                }
            }
        }

        public void Void(long MakeMultiPayID, string voidRemarks, string userName)
        {
            var MakeMultiPay = RetrieveByID(MakeMultiPayID);
            var oldStatus = MakeMultiPay.Status;
            MakeMultiPay.Status = (int)MPL.DocumentStatus.Void;
            MakeMultiPay.VoidRemarks = voidRemarks;
            MakeMultiPay.ApprovedDate = DateTime.Now;
            MakeMultiPay.ApprovedBy = userName;

            using (TransactionScope trans = new TransactionScope())
            {
                Update(MakeMultiPay);

                PostAccounting(MakeMultiPayID,MakeMultiPay.Status);

                trans.Complete();
            }

        }

        public void PreFillWithPurchaseBillData(MakeMultiPayModel header, long purchaseBillID)
        {
            var source = new PurchaseBillBFC().RetrieveByID(purchaseBillID);
            header.Date = DateTime.Now;
            header.VendorID = source.VendorID;
            header.VendorName = source.VendorName;
            header.CurrencyID = source.CurrencyID;
            header.CurrencyName = source.CurrencyName;
            header.ExchangeRate = source.ExchangeRate;
            header.Title = source.Remarks;
            // prepopulate "Sum" field with bill's due amount
            header.AmountHelp = source.Amount + source.TaxAmount - source.PaymentAmount - source.CreditedAmount;

            var payableBills = new PurchaseBillBFC().RetrievePayableBills(header.VendorID, header.CurrencyID);
            var returnDetails = new List<MakeMultiPayDetailModel>();

            foreach (var bill in payableBills)
            {
                var detail = new MakeMultiPayDetailModel();
                detail.PurchaseBillID = bill.ID;
                detail.Code = bill.Code;
                detail.Subtotal = bill.Amount - Convert.ToDecimal(bill.DiscountAmount);
                detail.TaxTotal = bill.TaxAmount;
                detail.OriginalAmount = detail.Subtotal + detail.TaxTotal;
                detail.PaymentAmount = bill.PaymentAmount;
                detail.CreditedAmount = bill.CreditedAmount;
                detail.AmountDue = detail.OriginalAmount - detail.PaymentAmount - detail.CreditedAmount;
                detail.DueDate = bill.DueDate;
                detail.Date = bill.Date;

                // prepopulate source bill with Amount Due
                if (detail.PurchaseBillID == purchaseBillID)
                {
                    detail.AmountStr = detail.AmountDue.ToString("N2");
                }

                returnDetails.Add(detail);
            }

            header.Details = returnDetails;
        }

        public List<MakeMultiPayDetailModel> RetrieveByBillID(long purchaseBillID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var payDetailsQuery = from detail in ent.v_MakeMultiPayDetail
                                  where detail.PurchaseBillID == purchaseBillID 
                                    && detail.Status != (int)MPL.DocumentStatus.Void
                                  select detail;
            var payDetails = payDetailsQuery.ToList();

            return ObjectHelper.CopyList<v_MakeMultiPayDetail, MakeMultiPayDetailModel>(payDetails);
        }

       

        #region Post to Accounting Result

        public void PostAccounting(long MakeMultiPayID, int Status)
        {
            var payment = RetrieveByID(MakeMultiPayID);

            new ABCAPOSDAC().DeleteAccountingResults(MakeMultiPayID, AccountingResultDocumentType.MakeMultiPay);

            if (payment != null)
            {
                if (Status != (int)MPL.DocumentStatus.Void)
                    CreateAccountingResult(MakeMultiPayID);
            }

        }


        private void CreateAccountingResult(long makeMultiPayID)
        {
            var payment = RetrieveByID(makeMultiPayID);

            var accountingResultList = new List<AccountingResultModel>();

            var paymentAmount = payment.TotalAmount * payment.ExchangeRate;

            accountingResultList = AddToAccountingResultList(accountingResultList, payment, (long)PostingAccount.HutangDagang, AccountingResultType.Debit, paymentAmount, "Hutang dagang make multi pay " + payment.Code);

            accountingResultList = AddToAccountingResultList(accountingResultList, payment, payment.AccountID, AccountingResultType.Credit, paymentAmount, "Bank/kas make multi pay " + payment.Code);

            new AccountingResultBFC().Posting(accountingResultList);
        }

        private List<AccountingResultModel> AddToAccountingResultList(List<AccountingResultModel> resultList, MakeMultiPayModel obj, long accountID, AccountingResultType resultType, decimal amount, string remarks)
        {
            if (amount > 0)
            {
                var account = new AccountBFC().RetrieveByID(accountID);
                var result = new AccountingResultModel();

                result.DocumentID = obj.ID;
                result.DocumentType = (int)AccountingResultDocumentType.MakeMultiPay;
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
    }

}
