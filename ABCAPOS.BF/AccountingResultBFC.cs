using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using ABCAPOS.Util;
using ABCAPOS.DA;

namespace ABCAPOS.BF
{
    public class AccountingResultBFC : GenericBFC<AccountingResult, v_AccountingResult, AccountingResultModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        protected override GenericDAC<AccountingResult, AccountingResultModel> GetDAC()
        {
            return new GenericDAC<AccountingResult, AccountingResultModel>("ID", false);
        }

        protected override GenericDAC<v_AccountingResult, AccountingResultModel> GetViewDAC()
        {
            return new GenericDAC<v_AccountingResult, AccountingResultModel>("ID", false);
        }

        private void CreateIncomeResults(List<IncomeAccountingDetailModel> incomeDetails)
        {
            foreach (var incomeDetail in incomeDetails)
            {
                var result = new AccountingResultModel();

                var account = new AccountBFC().RetrieveByID(incomeDetail.AccountID);

                //result.DocumentID = incomeDetail.AccountingID;
                //result.DocumentDetailItemNo = incomeDetail.ItemNo;
                //result.DocumentType = (int)AccountingResultDocumentType.Accounting;
                //result.Type = (int)AccountingResultType.Debit;
                //result.Date = incomeDetail.Date;
                ////result.AccountID = account.ReferenceID;
                //result.DocumentNo = incomeDetail.DocumentNo;
                //result.Amount = incomeDetail.Amount;
                //result.DebitAmount = incomeDetail.Amount;
                //result.Remarks = incomeDetail.Remarks;

                //Create(result);

                result = new AccountingResultModel();
                result.DocumentID = incomeDetail.AccountingID;
                result.DocumentDetailItemNo = incomeDetail.ItemNo;
                result.DocumentType = (int)AccountingResultDocumentType.Accounting;
                result.Type = (int)AccountingResultType.Debit;
                result.Date = incomeDetail.Date;
                result.AccountID = incomeDetail.AccountID;
                result.DocumentNo = incomeDetail.DocumentNo;
                result.Amount = incomeDetail.Amount;
                result.DebitAmount = incomeDetail.Amount;
                result.Remarks = incomeDetail.Remarks;

                Create(result);
            }

        }

        private void CreateExpenseResults(List<ExpenseAccountingDetailModel> expenseDetails)
        {
            foreach (var expenseDetail in expenseDetails)
            {
                var result = new AccountingResultModel();

                //result.DocumentID = expenseDetail.AccountingID;
                //result.DocumentDetailItemNo = expenseDetail.ItemNo;
                //result.DocumentType = (int)AccountingResultDocumentType.Accounting;
                //result.Type = (int)AccountingResultType.Debit;
                //result.Date = expenseDetail.Date;
                //result.AccountID = expenseDetail.AccountID;
                //result.DocumentNo = expenseDetail.DocumentNo;
                //result.Amount = expenseDetail.Amount;
                //result.DebitAmount = expenseDetail.Amount;
                //result.Remarks = expenseDetail.Remarks;

                //Create(result);

                var account = new AccountBFC().RetrieveByID(expenseDetail.AccountID);

                result = new AccountingResultModel();
                result.DocumentID = expenseDetail.AccountingID;
                result.DocumentDetailItemNo = expenseDetail.ItemNo;
                result.DocumentType = (int)AccountingResultDocumentType.Accounting;
                result.Type = (int)AccountingResultType.Credit;
                result.Date = expenseDetail.Date;
                result.AccountID = account.ID;
                result.DocumentNo = expenseDetail.DocumentNo;
                result.Amount = expenseDetail.Amount;
                result.CreditAmount = expenseDetail.Amount;
                result.Remarks = expenseDetail.Remarks;

                Create(result);

            }

        }

        public void CreateAccountingResultsFromAccounting(long accountingID)
        {
            var dac = new ABCAPOSDAC();

            var incomeDetails = new AccountingBFC().RetrieveDetails(accountingID);
            var expenseDetails = new AccountingBFC().RetrieveExpenses(accountingID);

            using (TransactionScope trans = new TransactionScope())
            {
                CreateIncomeResults(incomeDetails);
                CreateExpenseResults(expenseDetails);

                trans.Complete();
            }
        }

        public void UpdateAccountingResultsFromAccounting(long accountingID)
        {
            var dac = new ABCAPOSDAC();

            using (TransactionScope trans = new TransactionScope())
            {
                dac.DeleteAccountingResults(accountingID, AccountingResultDocumentType.Accounting);
                CreateAccountingResultsFromAccounting(accountingID);

                trans.Complete();
            }
        }

        public void Posting(List<AccountingResultModel> results)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                foreach (var result in results)
                    Create(result);

                trans.Complete();
            }
        }

        public void Void(long documentID, AccountingResultDocumentType docType)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                new ABCAPOSDAC().DeleteAccountingResults(documentID, docType);

                trans.Complete();
            }
        }

        public List<AccountingResultModel> Retrieve(long documentID, AccountingResultDocumentType docType)
        {
            return new ABCAPOSDAC().RetrieveAccountingResult(documentID, docType);
        }
    }
}
