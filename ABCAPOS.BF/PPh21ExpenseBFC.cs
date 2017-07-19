using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using System.Transactions;
using ABCAPOS.Util;
using System.Globalization;
using ABCAPOS.DA;

namespace ABCAPOS.BF
{
    public class PPh21ExpenseBFC : GenericBFC<PPh21Expense, PPh21Expense, PPh21ExpenseModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        protected override GenericDAC<PPh21Expense, PPh21ExpenseModel> GetDAC()
        {
            return new GenericDAC<PPh21Expense, PPh21ExpenseModel>("ID", false, "Date DESC");
        }

        protected override GenericDAC<PPh21Expense, PPh21ExpenseModel> GetViewDAC()
        {
            return new GenericDAC<PPh21Expense, PPh21ExpenseModel>("ID", false, "Date DESC");
        }

        public override void Create(PPh21ExpenseModel dr)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                base.Create(dr);

                PostAccounting(dr.ID);

                trans.Complete();
            }
        }

        public override void Update(PPh21ExpenseModel dr)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                base.Update(dr);

                UpdateAccounting(dr.ID);

                trans.Complete();
            }
        }

        private void PostAccounting(long pph21ExpenseID)
        {
            var obj = RetrieveByID(pph21ExpenseID);
            var accountingResultList = new List<AccountingResultModel>();

            var pph21ExpenseAccount = new AccountBFC().RetrieveByUserCode(SystemConstants.PPh21AccountUserCode);

            for (int i = 1; i <= 12; i++)
            {
                var accountingResult = new AccountingResultModel();
                accountingResult.DocumentID = pph21ExpenseID;
                accountingResult.DocumentType = (int)AccountingResultDocumentType.PPh21;
                accountingResult.Type = (int)AccountingResultType.Debit;
                accountingResult.Date = new DateTime(obj.Year, i, 1);
                accountingResult.AccountID = pph21ExpenseAccount.ID;
                accountingResult.Amount = obj.Amount;
                accountingResult.DebitAmount = obj.Amount;
                accountingResult.Remarks = "Beban PPh Pasal 21 Bulan " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i);
                accountingResultList.Add(accountingResult);

                accountingResult = new AccountingResultModel();
                accountingResult.DocumentID = pph21ExpenseID;
                accountingResult.DocumentType = (int)AccountingResultDocumentType.PPh21;
                accountingResult.Type = (int)AccountingResultType.Credit;
                accountingResult.Date = new DateTime(obj.Year, i, 1);
                //accountingResult.AccountID = pph21ExpenseAccount.ReferenceID;
                accountingResult.Amount = obj.Amount;
                accountingResult.CreditAmount = obj.Amount;
                accountingResult.Remarks = "Beban PPh Pasal 21 Bulan " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i);
                accountingResultList.Add(accountingResult);
            }



            new AccountingResultBFC().Posting(accountingResultList);
        }

        private void UpdateAccounting(long pph21ExpenseID)
        {
            var obj = RetrieveByID(pph21ExpenseID);
            var accountingResultList = new AccountingResultBFC().Retrieve(pph21ExpenseID, AccountingResultDocumentType.PPh21);
            
            foreach (var accountingResult in accountingResultList)
            {
                if (accountingResult.Type == (int)AccountingResultType.Debit)
                {
                    accountingResult.Amount = obj.Amount;
                    accountingResult.DebitAmount = obj.Amount;
                    accountingResult.CreditAmount = 0;
                }
                else
                {
                    accountingResult.Amount = obj.Amount;
                    accountingResult.CreditAmount = obj.Amount;
                    accountingResult.DebitAmount = 0;
                }

                new AccountingResultBFC().Update(accountingResult);
            }
        }
    }
}
