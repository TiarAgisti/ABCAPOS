using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using ABCAPOS.DA;
using System.Transactions;
using ABCAPOS.ReportEDS;
using ABCAPOS.Util;
using MPL;

namespace ABCAPOS.BF
{
    public class IncomeExpenseBFC : MasterDetailBFC<IncomeExpense, IncomeExpense, IncomeExpenseDetail,IncomeExpenseDetail, IncomeExpenseModel, IncomeExpenseDetailModel>
    {

        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetIncomeExpenseCode(int category)
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var incomeExpensePrefix = "";

            if (prefixSetting != null)
            {
                if (category == 1)
                    incomeExpensePrefix = prefixSetting.ExpenseBankPrefix;
                else if (category == 2)
                    incomeExpensePrefix = prefixSetting.IncomeBankPrefix;
                else if (category == 3)
                    incomeExpensePrefix = prefixSetting.CashInPrefix;
                else if (category == 4)
                    incomeExpensePrefix = prefixSetting.CashOutPrefix;
            }

            var code = new ABCAPOSDAC().RetrieveIncomeExpenseMaxCode(incomeExpensePrefix, 4);

            return code;
        }

        protected override GenericDetailDAC<IncomeExpenseDetail, IncomeExpenseDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<IncomeExpenseDetail, IncomeExpenseDetailModel>("IncomeExpenseID", "ItemNo", false);
        }

        protected override GenericDetailDAC<IncomeExpenseDetail, IncomeExpenseDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<IncomeExpenseDetail, IncomeExpenseDetailModel>("IncomeExpenseID", "ItemNo", false);
        }

        protected override GenericDAC<IncomeExpense, IncomeExpenseModel> GetMasterDAC()
        {
            return new GenericDAC<IncomeExpense, IncomeExpenseModel>("ID", false, "Code DESC");
        }

        protected override GenericDAC<IncomeExpense, IncomeExpenseModel> GetMasterViewDAC()
        {
            return new GenericDAC<IncomeExpense, IncomeExpenseModel>("ID", false, "Code DESC");
        }

        public override void Create(IncomeExpenseModel header, List<IncomeExpenseDetailModel> details)
        {
            header.Code = GetIncomeExpenseCode(header.CategoryID);

            base.Create(header, details);
        }

        public void CopyTransaction(IncomeExpenseModel header, long incomeExpenseID)
        {
            var incomeExpense = RetrieveByID(incomeExpenseID);
            var incomeExpenseDetails = RetrieveDetails(incomeExpenseID);

            ObjectHelper.CopyProperties(incomeExpense, header);

            header.Status = (int)IncomeExpenseStatus.New;

            var details = new List<IncomeExpenseDetailModel>();

            foreach (var incomeExpenseDetail in incomeExpenseDetails)
            {
                var detail = new IncomeExpenseDetailModel();

                ObjectHelper.CopyProperties(incomeExpenseDetail, detail);
                details.Add(detail);
            }

            header.Details = details;
        }

        public void Validate(IncomeExpenseModel obj, List<IncomeExpenseDetailModel> details)
        {
            //if(string.IsNullOrEmpty(obj.POCustomerNo))
            //    obj.POCustomerNo= 
        }

        public void Void(long incomeExpenseID, string voidRemarks, string userName)
        {
            var incomeExpense = RetrieveByID(incomeExpenseID);

            incomeExpense.Status = (int)IncomeExpenseStatus.Void;
            incomeExpense.VoidRemarks = voidRemarks;
            incomeExpense.ApprovedDate = DateTime.Now;
            incomeExpense.ApprovedBy = userName;

            using (TransactionScope trans = new TransactionScope())
            {
                //OnVoid(quotationID, voidRemarks, userName);
                Update(incomeExpense);

                trans.Complete();
            }
        }

        public List<IncomeExpenseModel> RetrieveIncomeExpenseByCategory(int categoryID, int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            if (amount == null)
                amount = SystemConstants.ItemPerPage;

            return new ABCAPOSDAC().RetrieveIncomeExpenseByCategory(categoryID, startIndex, (int)amount, sortParameter, selectFilters);
        }

        public int RetrieveIncomeExpenseByCategoryCount(int categoryID, List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveIncomeExpenseByCategoryCount(categoryID, selectFilters);
        }

        public ABCAPOSReportEDSC.IncomeExpenseDTRow RetrievePrintOut(long incomeExpenseID)
        {
            return new ABCAPOSReportDAC().RetrieveIncomeExpensePrintOut(incomeExpenseID);
        }

        public ABCAPOSReportEDSC.IncomeExpenseDetailDTDataTable RetrieveDetailPrintOut(long incomeExpenseID)
        {
            return new ABCAPOSReportDAC().RetrieveIncomeExpenseDetailPrintOut(incomeExpenseID);
        }

    }
    
}
