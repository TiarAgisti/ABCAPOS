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

namespace ABCAPOS.BF
{
    public class AccountingBFC : MasterDetailBFC<Accounting, Accounting, IncomeAccountingDetail, v_IncomeAccountingDetail, AccountingModel, IncomeAccountingDetailModel>
    {
        private void CreateExpenseAccountingDetails(long accountingID, List<ExpenseAccountingDetailModel> retails)
        {
            var dac = new ABCAPOSDAC();
            var itemNo = 1;

            foreach (var retail in retails)
            {
                retail.AccountingID = accountingID;
                retail.ItemNo = itemNo++;

                dac.CreateExpenseAccountingDetail(retail);
            }
        }

        private void UpdateExpenseAccountingDetails(long accountingID, List<ExpenseAccountingDetailModel> retails)
        {
            var dac = new ABCAPOSDAC();

            dac.DeleteExpenseAccountingDetail(accountingID);
            CreateExpenseAccountingDetails(accountingID, retails);
        }

        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetAccountingCode()
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var accountingPrefix = "";

            if (prefixSetting != null)
                accountingPrefix = prefixSetting.AccountingPrefix;

            var code = new ABCAPOSDAC().RetrieveAccountingMaxCode(accountingPrefix, 5);

            return code;
        }

        protected override GenericDetailDAC<IncomeAccountingDetail, IncomeAccountingDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<IncomeAccountingDetail, IncomeAccountingDetailModel>("AccountingID", "ItemNo", false);
        }

        protected override GenericDetailDAC<v_IncomeAccountingDetail, IncomeAccountingDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_IncomeAccountingDetail, IncomeAccountingDetailModel>("AccountingID", "ItemNo", false);
        }

        protected override GenericDAC<Accounting, AccountingModel> GetMasterDAC()
        {
            return new GenericDAC<Accounting, AccountingModel>("ID", false, "Code DESC");
        }

        protected override GenericDAC<Accounting, AccountingModel> GetMasterViewDAC()
        {
            return new GenericDAC<Accounting, AccountingModel>("ID", false, "Code DESC");
        }

        public override void Create(AccountingModel header, List<IncomeAccountingDetailModel> details)
        {
            if (string.IsNullOrEmpty(header.Code))
                header.Code = GetAccountingCode();

            using (TransactionScope trans = new TransactionScope())
            {
                base.Create(header, details);

                CreateExpenseAccountingDetails(header.ID, header.Expenses);
                new AccountingResultBFC().CreateAccountingResultsFromAccounting(header.ID);

                trans.Complete();
            }
        }

        public override void Update(AccountingModel header, List<IncomeAccountingDetailModel> details)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                base.Update(header, details);

                UpdateExpenseAccountingDetails(header.ID, header.Expenses);
                new AccountingResultBFC().UpdateAccountingResultsFromAccounting(header.ID);

                trans.Complete();
            }
        }

        public void Validate(AccountingModel header)
        {
            if (string.IsNullOrEmpty(header.Code))
                throw new Exception("Code must be filled");
        }

        public void Void(long accountingID, string voidRemarks, string userName)
        {
            var accounting = RetrieveByID(accountingID);

            accounting.Status = (int)MPL.DocumentStatus.Void;
            accounting.VoidRemarks = voidRemarks;
            accounting.ApprovedBy = "";
            accounting.ApprovedDate = SystemConstants.UnsetDateTime;

            new ABCAPOSDAC().DeleteAccountingResults(accounting.ID, AccountingResultDocumentType.Accounting);
            Update(accounting);
        }

        public List<AccountingModel> Retrieve(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters, bool showVoidDocuments)
        {
            return new ABCAPOSDAC().RetrieveAccounting(startIndex, amount, sortParameter, selectFilters, showVoidDocuments);
        }

        public int RetrieveCount(List<SelectFilter> selectFilters, bool showVoidDocuments)
        {
            return new ABCAPOSDAC().RetrieveAccountingCount(selectFilters, showVoidDocuments);
        }

        public List<ExpenseAccountingDetailModel> RetrieveExpenses(long accountingID)
        {
            return new ABCAPOSDAC().RetrieveExpenseAccountingDetails(accountingID);
        }

        public string RetrieveThisMonthExpenseCount()
        {
            return new ABCAPOSDAC().RetrieveThisMonthExpenseCount();
        }

    }
}
