using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using MPL;
using MPL.Business;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using ABCAPOS.Util;
using System.Data.SqlClient;
using System.Data.Objects.SqlClient;

namespace ABCAPOS.DA
{
    public class ABCAPOSDAC
    {
        private void ApplySorting<T>(ref IQueryable<T> query, string defaultSortField, string sortParameter)
        {
            if (string.IsNullOrEmpty(sortParameter.Trim()))
                sortParameter = defaultSortField;

            if (sortParameter.Trim().EndsWith(" DESC"))
            {
                sortParameter = sortParameter.Replace(" DESC", "");
                query = query.OrderByDescending(sortParameter);
            }
            else
            {
                query = query.OrderBy(sortParameter);
            }
        }

        private void ApplyFilter<T>(ref IQueryable<T> query, List<SelectFilter> filters)
        {
            if (filters != null)
            {
                object[] param = null;
                string statements = SelectFilter.Build(filters, out param);

                if (!string.IsNullOrEmpty(statements))
                    query = query.Where(statements, param);
            }
        }

        #region Account

        public string RetrieveAccountMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Account
                        where i.Code.StartsWith(prefix)
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "0000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public AccountModel RetrieveAccountByCode(string accountCode)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Account
                        where i.Code == accountCode
                        select i;

            var obj = new AccountModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public AccountModel RetrieveAccountByUserCode(string accountUserCode)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Account
                        where i.UserCode == accountUserCode
                        select i;

            var obj = new AccountModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public List<AccountModel> RetrieveAccount(bool isActive)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Account
                        where i.IsActive == isActive
                        select i;

            return ObjectHelper.CopyList<v_Account, AccountModel>(query.ToList());
        }

        public List<AccountModel> RetrieveAccountAutoComplete(string key)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Account
                        where (i.UserCode.ToLower().Contains(key.ToLower()) ||
                               i.Name.ToLower().Contains(key.ToLower())) &&
                              i.IsActive == true
                        select i;

            return ObjectHelper.CopyList<v_Account, AccountModel>(query.Take(SystemConstants.AutoCompleteItemCount).ToList());
        }

        public List<AccountModel> RetrieveAccountInvoicePaymentAutoComplete()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Account
                        where i.IsActive == true && i.InvoicePaymentAccount == 1
                        select i;

            return ObjectHelper.CopyList<v_Account, AccountModel>(query.Take(SystemConstants.AutoCompleteItemCount).ToList());
        }

        public List<AccountModel> RetrieveAccountInvoicePaymentAutoComplete(string key)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Account
                        where (i.UserCode.ToLower().Contains(key.ToLower()) ||
                               i.Name.ToLower().Contains(key.ToLower())) &&
                              i.IsActive == true && i.InvoicePaymentAccount == 1
                        select i;

            return ObjectHelper.CopyList<v_Account, AccountModel>(query.Take(SystemConstants.AutoCompleteItemCount).ToList());
        }

        public List<AccountModel> RetrieveDebitAccount(string key)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_DebitAccount
                        where (i.UserCode.ToLower().Contains(key.ToLower()) ||
                               i.Description.ToLower().Contains(key.ToLower()))
                        select i;

            return ObjectHelper.CopyList<v_DebitAccount, AccountModel>(query.Take(SystemConstants.AutoCompleteItemCount).ToList());
        }

        public List<AccountModel> RetrieveCreditAccount(string key)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_CreditAccount
                        where (i.UserCode.ToLower().Contains(key.ToLower()) ||
                               i.Description.ToLower().Contains(key.ToLower()))
                        select i;

            return ObjectHelper.CopyList<v_CreditAccount, AccountModel>(query.Take(SystemConstants.AutoCompleteItemCount).ToList());
        }

        #endregion

        #region Account Configuration

        public string RetrieveAccountConfigurationMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.AccountConfiguration
                        where i.Code.StartsWith(prefix) && i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "0000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        #endregion

        #region Accounting

        public void CreateExpenseAccountingDetail(ExpenseAccountingDetailModel expenseAccountingModel)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            ExpenseAccountingDetail obj = new ExpenseAccountingDetail();
            ObjectHelper.CopyProperties(expenseAccountingModel, obj);
            ent.AddToExpenseAccountingDetail(obj);
            ent.SaveChanges();
        }

        public void DeleteExpenseAccountingDetail(long accountingID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.ExpenseAccountingDetail
                        where i.AccountingID == accountingID
                        select i;

            foreach (var obj in query.AsEnumerable())
                ent.DeleteObject(obj);

            ent.SaveChanges();
        }

        public string RetrieveAccountingMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Accounting
                        where i.Code.StartsWith(prefix) && i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "0000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public List<ExpenseAccountingDetailModel> RetrieveExpenseAccountingDetails(long accountingID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_ExpenseAccountingDetail
                        where i.AccountingID == accountingID
                        select i;

            return ObjectHelper.CopyList<v_ExpenseAccountingDetail, ExpenseAccountingDetailModel>(query.ToList());
        }

        public string RetrieveThisMonthExpenseCount()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_ExpenseAccountingDetail
                        where SqlFunctions.DatePart("month", i.Date) == MPL.Configuration.CurrentDateTime.Month && SqlFunctions.DatePart("year", i.Date) == MPL.Configuration.CurrentDateTime.Year
                        select i;

            return "Rp. " + Convert.ToDecimal(query.Sum(p => p.Amount)).ToString("N0");
        }

        public string RetrieveMonthlyExpense()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            List<MonthlyDataModel> monthlyDatas = new List<MonthlyDataModel>();

            var query = from i in ent.v_ExpenseAccountingDetail
                        group i by new { month = SqlFunctions.DatePart("month", i.Date), year = SqlFunctions.DatePart("year", i.Date) } into d
                        select new { dt = d.Key.month, dt2 = d.Key.year, amount = d.Sum(g => (g.Amount)) };
            var query2 = from i in ent.AccountingResult
                         where (i.AccountID == 10 || i.AccountID == 36) && i.DocumentDetailItemNo == 0
                         select i;

            for (int i = 5; i >= 0; i--)
            {
                MonthlyDataModel monthlyData = new MonthlyDataModel();
                int chartMonth = DateTime.Now.AddMonths(-i).Month;
                int chartYear = DateTime.Now.AddMonths(-i).Year;

                var aMonth = from j in query
                             where j.dt == chartMonth && j.dt2 == chartYear
                             select j;
                var aMonth2 = from j in query2
                              where SqlFunctions.DatePart("month", j.Date) == chartMonth && SqlFunctions.DatePart("year", j.Date) == chartYear
                              select j;

                monthlyData.Month = new IDNumericSayer().SayMonth(DateTime.Now.AddMonths(-i).Month).Substring(0, 3) + " " + DateTime.Now.AddMonths(-i).Year.ToString().Substring(2, 2);
                monthlyData.Amount = Convert.ToDecimal(aMonth.Sum(p => p.amount) + aMonth2.Sum(p => p.DebitAmount - p.CreditAmount)).ToString("N0").Replace(".", "");
                monthlyData.color = "#837e7c";
                monthlyDatas.Add(monthlyData);
            }

            var json = JSONHelper.ToJSON(monthlyDatas);

            return json;
        }

        public List<AccountingModel> RetrieveAccounting(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters, bool showVoidDocuments)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Accounting
                        select i;

            if (!showVoidDocuments)
                query = from i in ent.Accounting
                        where i.Status > 0
                        select i;

            ApplySorting<Accounting>(ref query, "Code", sortParameter);
            ApplyFilter<Accounting>(ref query, selectFilters);

            return ObjectHelper.CopyList<Accounting, AccountingModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public int RetrieveAccountingCount(List<SelectFilter> selectFilters, bool showVoidDocuments)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Accounting
                        select i;

            if (!showVoidDocuments)
                query = from i in ent.Accounting
                        where i.Status > 0
                        select i;

            ApplyFilter<Accounting>(ref query, selectFilters);

            return query.Count();
        }
        #endregion

        #region Accounting Result

        public void DeleteAccountingResults(long documentID, AccountingResultDocumentType docType)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.AccountingResult
                        where i.DocumentID == documentID && i.DocumentType == (int)docType
                        select i;

            foreach (var obj in query.AsEnumerable())
                ent.DeleteObject(obj);

            ent.SaveChanges();
        }

        public List<AccountingResultModel> RetrieveAccountingResult(long documentID, AccountingResultDocumentType docType)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.AccountingResult
                        where i.DocumentID == documentID && i.DocumentType == (int)docType
                        select i;

            return ObjectHelper.CopyList<AccountingResult, AccountingResultModel>(query.ToList());
        }

        #endregion

        #region Allowance

        public string RetrieveAllowanceMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Allowance
                        where i.Code.StartsWith(prefix)
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 10000;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "0000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        #endregion

        #region Apply Bill Credit
        public string RetrieveApplyBillCreditMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.ApplyBillCredit
                        where i.Code.StartsWith(prefix) && i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString();
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "0000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public List<ApplyBillCreditModel> RetrieveApplyBillCreditByBillCreditID(long billCredit)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_ApplyBillCredit
                        where i.BillCreditID == billCredit
                        select i;

            return ObjectHelper.CopyList<v_ApplyBillCredit, ApplyBillCreditModel>(query.ToList());
        }

        public int RetrieveApplyBillCreditCount(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_ApplyBillCredit
                        where i.Status != (int)MPL.DocumentStatus.Void
                        select i;
            ApplyFilter<v_ApplyBillCredit>(ref query, selectFilters);
            return query.Count();
        }

        public List<ApplyBillCreditModel> RetrieveApplyBillCredit(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_ApplyBillCredit
                        where i.Status != (int)MPL.DocumentStatus.Void
                        select i;
            ApplySorting<v_ApplyBillCredit>(ref query, "DATE DESC", sortParameter);
            ApplyFilter<v_ApplyBillCredit>(ref query, selectFilters);
            return ObjectHelper.CopyList<v_ApplyBillCredit, ApplyBillCreditModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        #endregion

        #region Apply Credit Memo
        public string RetrieveApplyCreditMemoMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.ApplyCreditMemo
                        where i.Code.StartsWith(prefix) && i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString();
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "0000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public List<ApplyCreditMemoModel> RetrieveApplyCreditMemoByCreditMemoID(long CreditMemo)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_ApplyCreditMemo
                        where i.CreditMemoID == CreditMemo && i.Status != (int)MPL.DocumentStatus.Void
                        select i;

            return ObjectHelper.CopyList<v_ApplyCreditMemo, ApplyCreditMemoModel>(query.ToList());
        }

        #endregion

        #region AssemblyBuild
        public string RetreiveAssemblyBuildMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.AssemblyBuild
                        where i.Code.StartsWith(prefix) && i.Code.Length == prefix.Length + length
                        select i.Code;
            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "000000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;
            return maxCode;
        }

        public AssemblyBuildDetailModel RetreiveQtyByAssemblyID(long assemblyBuildID, long productDetailID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.AssemblyBuildDetail
                        where i.AssemblyBuildID == assemblyBuildID && i.ProductDetailID == productDetailID
                        select i;

            var obj = new AssemblyBuildDetailModel();
            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public List<AssemblyBuildModel> RetreiveBuildByWOID(long workOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_AssemblyBuild
                        where i.WorkOrderID == workOrderID && i.Status != (int)AssemblyBuildStatus.Void
                        select i;
            return ObjectHelper.CopyList<v_AssemblyBuild,AssemblyBuildModel>(query.ToList());
        }

        public List<AssemblyBuildModel> RetreiveBuildByproductID(long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_AssemblyBuild
                        where i.ProductID == productID && i.Status != (int)AssemblyBuildStatus.Void
                        select i;
            return ObjectHelper.CopyList<v_AssemblyBuild, AssemblyBuildModel>(query.OrderByDescending(p => p.Date).ToList());
            //return ObjectHelper.CopyList<v_AssemblyBuild, AssemblyBuildModel>(query.OrderByDescending(p => p.Date).Take(SystemConstants.ItemPerPage).ToList());
        }

        public List<AssemblyBuildModel> RetrieveListBuild(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_AssemblyBuild
                        select i;

            ApplySorting<v_AssemblyBuild>(ref query, "DATE DESC", sortParameter);
            ApplyFilter<v_AssemblyBuild>(ref query, selectFilters);
            return ObjectHelper.CopyList<v_AssemblyBuild, AssemblyBuildModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public List<AssemblyBuildModel> RetrieveListBuildFG(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_AssemblyBuild
                        where i.ItemTypeID == 4
                        select i;

            ApplySorting<v_AssemblyBuild>(ref query, "DATE DESC", sortParameter);
            ApplyFilter<v_AssemblyBuild>(ref query, selectFilters);
            return ObjectHelper.CopyList<v_AssemblyBuild, AssemblyBuildModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public int RetreiveListBuildCountFG(List<SelectFilter> SelectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_AssemblyBuild
                        where i.ItemTypeID == 4
                        select i;

            ApplyFilter<v_AssemblyBuild>(ref query, SelectFilters);
            return query.Count();
        }
        public int RetreiveListBuildCountResi(List<SelectFilter> SelectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_ResiBill
                        select i;

            ApplyFilter<v_ResiBill>(ref query, SelectFilters);
            return query.Count();
        }

        public int RetreiveListBuildCount(List<SelectFilter> SelectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_AssemblyBuild
                        select i;

            ApplyFilter<v_AssemblyBuild>(ref query, SelectFilters);
            return query.Count();
        }

        #endregion

        #region Assembly UnBuild
        public string RetreiveUnBuildMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.AssemblyUnBuild
                        where i.Code.StartsWith(prefix) && i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "000000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;
            return maxCode;
        }

        public int RetreiveListUnBuildCountFG(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_AssemblyUnBuild
                        where i.ItemTypeID == (int)ItemTypeProduct.FinishGood
                        select i;
            ApplyFilter<v_AssemblyUnBuild>(ref query, selectFilters);
            return query.Count();
        }

        public int RetreiveListUnBuildCount(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_AssemblyUnBuild
                        select i;
            ApplyFilter<v_AssemblyUnBuild>(ref query, selectFilters);
            return query.Count();
        }

        public List<AssemblyUnBuildModel> RetreiveListUnBuildFG(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_AssemblyUnBuild
                        where i.ItemTypeID == (int)ItemTypeProduct.FinishGood
                        select i;
            ApplySorting<v_AssemblyUnBuild>(ref query, "DATE DESC", sortParameter);
            ApplyFilter<v_AssemblyUnBuild>(ref query, selectFilters);
            return ObjectHelper.CopyList<v_AssemblyUnBuild, AssemblyUnBuildModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public List<AssemblyUnBuildModel> RetreiveListUnBuild(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_AssemblyUnBuild
                        select i;
            ApplySorting<v_AssemblyUnBuild>(ref query, "DATE DESC", sortParameter);
            ApplyFilter<v_AssemblyUnBuild>(ref query, selectFilters);
            return ObjectHelper.CopyList<v_AssemblyUnBuild, AssemblyUnBuildModel>(query.Skip(startIndex).Take(amount).ToList());
        }
        #endregion

        #region Attendance

        public string RetrieveAttendanceMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Attendance
                        where i.Code.StartsWith(prefix)
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 10000;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "0000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        #endregion

        #region Attendance Setting

        public AttendanceSettingModel RetrieveAttendanceSetting()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.AttendanceSetting
                        select i;

            AttendanceSettingModel obj = new AttendanceSettingModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }
        #endregion

        #region Base Price Location

        public void CreateBasePriceLocation(BasePriceLocationModel baseLoc)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            BasePriceLocation obj = new BasePriceLocation();
            ObjectHelper.CopyProperties(baseLoc, obj);
            ent.AddToBasePriceLocation(obj);
            ent.SaveChanges();

            baseLoc.ID = obj.ID;
        }

        public List<BasePriceLocationModel> RetrieveBasePriceLocation()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.BasePriceLocation
                        select i;

            return ObjectHelper.CopyList<BasePriceLocation, BasePriceLocationModel>(query.ToList());
        }

        public BasePriceLocationModel RetrieveBasePriceLocationByProductID(long productID, long warehouseID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.BasePriceLocation
                        where i.ProductID == productID && i.WarehouseID == warehouseID
                        select i;

            var obj = new BasePriceLocationModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public List<BasePriceLocationModel> RetrieveBasePriceLocationsByProductID(long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_BasePriceLocation
                        where i.ProductID == productID
                        select i;

            return ObjectHelper.CopyList<v_BasePriceLocation, BasePriceLocationModel>(query.ToList());
        }

        public void DeleteBasePriceLocation(long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.BasePriceLocation
                        where i.ProductID == productID
                        select i;

            foreach (var obj in query.AsEnumerable())
                ent.DeleteObject(obj);

            ent.SaveChanges();
        }

        #endregion

        #region Bin

        public List<BinModel> RetrieveAllBin()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Bin
                        select i;

            return ObjectHelper.CopyList<v_Bin, BinModel>(query.ToList());
        }

        public BinModel RetrieveBinByCode(string binName, long warehouseID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Bin
                        where i.Name == binName && i.WarehouseID == warehouseID
                        select i;

            var obj = new BinModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public List<BinModel> RetrieveBinAutoComplete(string key, long warehouseID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Bin
                        where i.Name.ToLower().Contains(key.ToLower()) && i.WarehouseID == warehouseID && i.IsActive == true
                        select i;

            ApplySorting<Bin>(ref query, "Name", "");
            return ObjectHelper.CopyList<Bin, BinModel>(query.Take(SystemConstants.AutoCompleteItemCount).ToList());
        }

        public List<long> RetrieveBinIDs()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Bin
                        select i.ID;

            return query.ToList();
        }


        #endregion

        #region Bin Product Warehouse

        public void CreateBinProductWarehouse(BinProductWarehouseModel binProduct)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            BinProductWarehouse obj = new BinProductWarehouse();
            ObjectHelper.CopyProperties(binProduct, obj);
            ent.AddToBinProductWarehouse(obj);
            ent.SaveChanges();

            binProduct.ID = obj.ID;
        }
        /// <summary>
        /// Returns ALL BinProductWarehouse
        /// </summary>
        /// <returns>ALL BinProductWarehouse</returns>
        public List<BinProductWarehouseModel> RetrieveBinProductWarehouse()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.BinProductWarehouse
                        select i;

            return ObjectHelper.CopyList<BinProductWarehouse, BinProductWarehouseModel>(query.ToList());
        }
        /// <summary>
        /// Returns List of BinProductWarehouseModel that has the specified ProductID and WarehouseID
        /// </summary>
        /// <param name="productID"></param>
        /// <param name="warehouseID"></param>
        /// <returns></returns>
        public List<BinProductWarehouseModel> RetrieveBinProductWarehouse(long productID, long warehouseID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_BinProductWarehouse
                        where i.ProductID == productID && i.WarehouseID == warehouseID
                        select i;

            if (query.FirstOrDefault() == null)
                return new List<BinProductWarehouseModel>();

            return ObjectHelper.CopyList<v_BinProductWarehouse, BinProductWarehouseModel>(query.ToList());
        }

        /// <summary>
        /// Returns List of BinProductWarehouseModel that are NOT for 
        /// the specified ProductID and WarehouseID
        /// </summary>
        /// <param name="productID"></param>
        /// <param name="warehouseID"></param>
        /// <returns></returns>
        public List<BinProductWarehouseModel> RetrieveBinProductWarehouseInverse(long productID, long warehouseID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_BinProductWarehouse
                        where i.ProductID != productID || i.WarehouseID != warehouseID
                        select i;

            if (query.FirstOrDefault() == null)
                return null;

            return ObjectHelper.CopyList<v_BinProductWarehouse, BinProductWarehouseModel>(query.ToList());
        }


        /// <summary>
        /// Returns Default BinProductWarehouseModel for specified ProductID and WarehouseID
        /// </summary>
        /// <param name="productID"></param>
        /// <param name="warehouseID"></param>
        /// <returns></returns>
        public BinProductWarehouseModel RetrieveBinProductWarehouseDefault(long productID, long warehouseID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_BinProductWarehouse
                        where i.ProductID == productID && i.WarehouseID == warehouseID && i.IsDefaultBin == true
                        select i;

            var obj = new BinProductWarehouseModel();

            if (query.FirstOrDefault() == null)
                return null;

            ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);

            return obj;
        }

        public BinProductWarehouseModel RetrieveBin(long binID, long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.BinProductWarehouse
                        where i.BinID == binID && i.ProductID == productID
                        select i;

            var obj = new BinProductWarehouseModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public BinProductWarehouseModel RetrieveBinProductByBinIDProductID(long binID, long productID, long warehouseID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.BinProductWarehouse
                        where i.BinID == binID && i.ProductID == productID && i.WarehouseID == warehouseID
                        select i;

            var obj = new BinProductWarehouseModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        #endregion

        #region Bill Credit
        public string RetrieveBillCreditMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.BillCredit
                        where i.Code.StartsWith(prefix) && i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "0000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public List<VendorReturnModel> RetrieveUncreatedBillCreditReturn(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_VendorReturn
                        where i.IsCreditFulfilled == false && i.Status == (int)MPL.DocumentStatus.Approved
                        && i.IsCreditable == true
                        select i;

            ApplySorting<v_VendorReturn>(ref query, "Code DESC", sortParameter);
            ApplyFilter<v_VendorReturn>(ref query, selectFilters);

            return ObjectHelper.CopyList<v_VendorReturn, VendorReturnModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public int RetrieveUncreatedBillCreditReturnCount(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_VendorReturn
                        where i.IsCreditFulfilled == false && i.Status == (int)MPL.DocumentStatus.Approved
                        && i.IsCreditable == true
                        select i;

            ApplyFilter<v_VendorReturn>(ref query, selectFilters);
            return query.Count();
        }

        public List<BillCreditModel> RetrieveBillCreditByVendorReturnID(long p)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from j in ent.v_BillCredit
                        where j.VendorReturnID == p && j.Status != (int)MPL.DocumentStatus.Void
                        select j;

            return ObjectHelper.CopyList<v_BillCredit, BillCreditModel>(query.ToList());
        }

        public int RetrieveBillCreditCount(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_BillCredit
                        where i.Status != 0
                        select i;
            ApplyFilter<v_BillCredit>(ref query, selectFilters);
            return query.Count();
        }

        public List<BillCreditModel> RetrieveBillCredit(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_BillCredit
                        where i.Status != (int)MPL.DocumentStatus.Void
                        select i;
            ApplySorting<v_BillCredit>(ref query, "DATE DESC", sortParameter);
            ApplyFilter<v_BillCredit>(ref query, selectFilters);
            return ObjectHelper.CopyList<v_BillCredit, BillCreditModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        #endregion

        #region Booking Order

        public List<BookingOrderModel> RetrieveBookingOrder(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters, bool showVoidDocuments)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_BookingOrder
                        select i;

            if (!showVoidDocuments)
                query = from i in ent.v_BookingOrder
                        where i.Status > 0
                        select i;

            ApplySorting<v_BookingOrder>(ref query, "Code", sortParameter);
            ApplyFilter<v_BookingOrder>(ref query, selectFilters);

            return ObjectHelper.CopyList<v_BookingOrder, BookingOrderModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public int RetrieveBookingOrderCount(List<SelectFilter> selectFilters, bool showVoidDocuments)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_BookingOrder
                        select i;

            if (!showVoidDocuments)
                query = from i in ent.v_BookingOrder
                        where i.Status > 0
                        select i;

            ApplyFilter<v_BookingOrder>(ref query, selectFilters);

            return query.Count();
        }

#endregion

        #region Booking Sales

        public List<BookingSalesModel> RetrieveBookingSales(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters, bool showVoidDocuments)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_BookingSales
                        select i;

            if (!showVoidDocuments)
                query = from i in ent.v_BookingSales
                        where i.Status > 0
                        select i;

            ApplySorting<v_BookingSales>(ref query, "Code", sortParameter);
            ApplyFilter<v_BookingSales>(ref query, selectFilters);

            return ObjectHelper.CopyList<v_BookingSales, BookingSalesModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public int RetrieveBookingSalesCount(List<SelectFilter> selectFilters, bool showVoidDocuments)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_BookingSales
                        select i;

            if (!showVoidDocuments)
                query = from i in ent.v_BookingSales
                        where i.Status > 0
                        select i;

            ApplyFilter<v_BookingSales>(ref query, selectFilters);

            return query.Count();
        }

        #endregion

        #region Cash Sales

        public string RetrieveCashSalesMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.CashSales
                        where i.Code.StartsWith(prefix)
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "000000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public CashSalesModel RetrieveCashSalesByCode(string cashSalesCode)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_CashSales
                        where i.Code == cashSalesCode
                        select i;

            var obj = new CashSalesModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }
        #endregion

        #region Company Setting
        public void UpdateCompanySetting(CompanySettingModel companySetting)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.CompanySetting
                        select i;

            var obj = query.FirstOrDefault();
            ObjectHelper.CopyProperties(companySetting, obj);
            ent.SaveChanges();
        }

        public CompanySettingModel RetrieveCompanySetting()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.CompanySetting
                        select i;

            CompanySettingModel obj = new CompanySettingModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }
        #endregion

        #region Container
        public ContainerModel RetreiveContainerByProductIDWarehouseID(long productID, long warehouseID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.Container
                        where i.ProductID == productID && i.WarehouseID == warehouseID && i.Qty > 0
                        select i;

            var obj = new ContainerModel();
            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public ContainerModel RetreiveByContainerIDByProductIDWarehouseID(long containerID,long productID, long warehouseID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_Container
                        where  i.ID==containerID && i.ProductID == productID && i.WarehouseID == warehouseID
                        select i;

            var obj = new ContainerModel();
            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public ContainerModel RetreiveByLogIDProductIDDocType(long logID, long productID, int doctype)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_RelatedLogByContainer
                        where i.LogID==logID && i.ProductID==productID && i.DocType==doctype
                        select i;

            var obj = new ContainerModel();
            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }
        #endregion

        #region Conversion

        public List<UnitModel> RetrieveUnits(bool isActive)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Unit
                        where i.IsActive == isActive
                        select i;

            return ObjectHelper.CopyList<Unit, UnitModel>(query.ToList());
        }

        public List<UnitDetailModel> RetrieveUnitsByProductID(long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var product = RetrieveProductByID(productID);

            var query = from i in ent.UnitDetail
                        where i.UnitID == product.UnitTypeID
                        select i;

            return ObjectHelper.CopyList<UnitDetail, UnitDetailModel>(query.ToList());
        }

        public List<UnitDetailModel> RetrieveUnitsByProductIDInversed(long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var product = RetrieveProductByID(productID);

            var query = from i in ent.UnitDetail
                        where i.UnitID != product.UnitTypeID
                        select i;

            return ObjectHelper.CopyList<UnitDetail, UnitDetailModel>(query.ToList());
        }

        public List<UnitDetailModel> RetrieveUnitsAll()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.UnitDetail
                        where i.IsActive == true
                        select i;

            return ObjectHelper.CopyList<UnitDetail, UnitDetailModel>(query.ToList());
        }

        public double RetrieveUnitRateByUnitID(long unitID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.UnitDetail
                        where i.ID == unitID
                        select i;
            if (query.FirstOrDefault() != null && query.FirstOrDefault().Rate != null)
                return (double)query.FirstOrDefault().Rate;
            else
                return 1;
        }

        public UnitDetail RetrieveUnitDetailByID(long unitDetailID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.UnitDetail
                        where i.ID == unitDetailID
                        select i;

            return query.FirstOrDefault();
        }

        public Unit RetrieveUnitByID(long unitID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.Unit
                        where i.ID == unitID
                        select i;

            return query.FirstOrDefault();
        }

        public List<UnitDetailModel> RetreiveUnitDetailID(long UnitID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.UnitDetail
                        where i.UnitID == UnitID
                        select i;

            return ObjectHelper.CopyList<UnitDetail, UnitDetailModel>(query.ToList());
        }

        public UnitDetail RetrieveUnitDetailByUnitIDAndPluralAbbreviation(long unitID, string PluralAbbreviation)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.UnitDetail
                        where i.UnitID == unitID && i.PluralAbbreviation == PluralAbbreviation
                        select i;

            return query.FirstOrDefault();
        }

        #endregion

        #region Credit Memo

        public string RetrieveCreditMemoMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.CreditMemo
                        where i.Code.StartsWith(prefix) && i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "0000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public List<CustomerReturnModel> RetrieveCustomerReturnBySalesOrder(long salesOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_CustomerReturn
                        where i.SalesOrderID == salesOrderID && i.Status != (int)MPL.DocumentStatus.Void
                        select i;

            return ObjectHelper.CopyList<v_CustomerReturn, CustomerReturnModel>(query.ToList());
        }

        public List<CustomerReturnModel> RetrieveUncreatedCreditMemoReturn(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_CustomerReturn
                        where i.IsFullyCredited == false && i.Status == (int)MPL.DocumentStatus.Approved
                        && i.IsCreditable == true
                        select i;

            ApplySorting<v_CustomerReturn>(ref query, "Code DESC", sortParameter);
            ApplyFilter<v_CustomerReturn>(ref query, selectFilters);

            return ObjectHelper.CopyList<v_CustomerReturn, CustomerReturnModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public int RetrieveUncreatedCreditMemoReturnCount(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_CustomerReturn
                        where i.IsFullyCredited == false && i.Status == (int)MPL.DocumentStatus.Approved
                        && i.IsCreditable == true
                        select i;

            ApplyFilter<v_CustomerReturn>(ref query, selectFilters);
            return query.Count();
        }

        public List<CreditMemoModel> RetrieveCreditMemoByCustomerReturnID(long p)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from j in ent.v_CreditMemo
                        where j.CustomerReturnID == p && j.Status != (int)MPL.DocumentStatus.Void
                        select j;

            return ObjectHelper.CopyList<v_CreditMemo, CreditMemoModel>(query.ToList());
        }

        #endregion

        #region Currency Date
        public CurrencyDateModel RetrieveCurrencyDate(long currencyID, DateTime date)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.CurrencyDate
                        where i.CurrencyID == currencyID && i.Date == date
                        select i;

            CurrencyDateModel obj = new CurrencyDateModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public List<CurrencyDateModel> RetrieveCurrencyDate(long currencyID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.CurrencyDate
                        where i.CurrencyID == currencyID
                        orderby i.Date
                        select i;

            return ObjectHelper.CopyList<CurrencyDate, CurrencyDateModel>(query.ToList());
        }
        #endregion

        #region Customer

        public string RetrieveCustomerMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Customer
                        where i.Code.StartsWith(prefix) //&& i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Count().ToString();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = code.ToString();
            //numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public CustomerModel RetrieveCustomerByCode(string customerCode)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Customer
                        where i.Code == customerCode
                        select i;

            var obj = new CustomerModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public CustomerModel RetrieveCustomerByCodeOrName(string key)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Customer
                        where i.Code == key || i.Name == key
                        select i;

            var obj = new CustomerModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public CustomerModel RetrieveAutoCompleteCustomer(string customerName)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Customer
                        where i.Code == customerName || i.Name == customerName
                        select i;

            var obj = new CustomerModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public List<CustomerModel> RetrieveCustomerAutoComplete(string key)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Customer
                        where (i.Name.ToLower().Contains(key.ToLower()) ||
                               i.Code.ToLower().Contains(key.ToLower())) &&
                               i.IsActive == true
                        select i;

            ApplySorting<Customer>(ref query, "Code", "");
            return ObjectHelper.CopyList<Customer, CustomerModel>(query.Take(SystemConstants.AutoCompleteItemCount).ToList());
        }
        #endregion

        #region Customer Return
        public string RetrieveCustomerReturnMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.CustomerReturn
                        where i.Code.StartsWith(prefix) && i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "0000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public List<CustomerReturnModel> RetrieveUncreatedReceiptsCustomerReturn(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_CustomerReturn
                        where i.IsFullyReceived == false && i.Status == (int)MPL.DocumentStatus.Approved
                        && i.IsReceivable == true
                        select i;

            ApplySorting<v_CustomerReturn>(ref query, "Code DESC", sortParameter);
            ApplyFilter<v_CustomerReturn>(ref query, selectFilters);

            return ObjectHelper.CopyList<v_CustomerReturn, CustomerReturnModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public int RetrieveUncreatedReceiptsCustomerReturnCount(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_CustomerReturn
                        where i.IsFullyReceived == false && i.Status == (int)MPL.DocumentStatus.Approved
                        && i.IsReceivable == true
                        select i;

            ApplyFilter<v_CustomerReturn>(ref query, selectFilters);
            return query.Count();
        }

        #endregion

        #region Delivery Order

        public string RetrieveDeliveryOrderMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.DeliveryOrder
                        where i.Code.StartsWith(prefix) && i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "000000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public List<DeliveryOrderModel> RetrieveDeliveryOrderBySalesOrderID(long salesOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from j in ent.v_DeliveryOrder
                        where j.SalesOrderID == salesOrderID && j.Status != (int)DeliveryOrderStatus.Void
                        select j;

            return ObjectHelper.CopyList<v_DeliveryOrder, DeliveryOrderModel>(query.ToList());
        }

        public List<DeliveryOrderModel> RetrieveDeliveryOrderBySalesOrderID(long salesOrderID, DeliveryOrderStatus deliveryStatus)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from j in ent.v_DeliveryOrder
                        where j.SalesOrderID == salesOrderID && j.Status == (int)deliveryStatus
                        select j;

            return ObjectHelper.CopyList<v_DeliveryOrder, DeliveryOrderModel>(query.ToList());
        }

        public List<DeliveryOrderDetailModel> RetrieveDeliveryOrderDetailBySalesOrderID(long salesOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.DeliveryOrderDetail
                        from j in ent.DeliveryOrder
                        where i.DeliveryOrderID == j.ID && j.SalesOrderID == salesOrderID && j.Status != (int)MPL.DocumentStatus.Void
                        select i;

            return ObjectHelper.CopyList<DeliveryOrderDetail, DeliveryOrderDetailModel>(query.ToList());
        }

        public int RetrieveUnapprovedDeliveryOrderCount(CustomerGroupModel customerGroup)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_DeliveryOrder
                        where i.Status == (int)MPL.DocumentStatus.New && i.CustomerGroupID == customerGroup.ID
                        select i;

            return query.Count();
        }

        public int RetrieveVoidDeliveryOrderCount(CustomerGroupModel customerGroup)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_DeliveryOrder
                        where i.Status == (int)MPL.DocumentStatus.Void && SqlFunctions.DatePart("month", i.Date) == MPL.Configuration.CurrentDateTime.Month
                        && SqlFunctions.DatePart("year", i.Date) == MPL.Configuration.CurrentDateTime.Year && i.CustomerGroupID == customerGroup.ID
                        select i;

            return query.Count();
        }

        public List<DeliveryOrderModel> RetreiveDOByCustomerID(long customerID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_DeliveryOrder
                        where i.CustomerID == customerID
                        select i;

            return ObjectHelper.CopyList<v_DeliveryOrder, DeliveryOrderModel>(query.OrderByDescending(p => p.Date).Take(SystemConstants.ItemPerPage).ToList());
                        
        }

        public List<DeliveryOrderModel> RetrieveDOByCustomerIDExpeditionIDStartDateEndDate(long customerID, long expeditionID, DateTime startDate, DateTime endDate)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_DeliveryOrder
                        where i.CustomerID == customerID && i.ExpeditionID == expeditionID && i.HasResi == false && i.Date >= startDate && i.Date <= endDate && i.Status != (int)MPL.DocumentStatus.Void
                        select i;

            return ObjectHelper.CopyList<v_DeliveryOrder, DeliveryOrderModel>(query.ToList());
        }

        #endregion

        #region Department

        public string RetrieveDepartmentMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Department
                        where i.Code.StartsWith(prefix) && i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 0;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "0000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public List<DepartmentModel> RetrieveDepartment(bool isActive)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Department
                        where i.IsActive == isActive
                        select i;

            return ObjectHelper.CopyList<v_Department, DepartmentModel>(query.ToList());
        }

        #endregion

        #region Expedition
        public List<ExpeditionModel> RetrieveExpeditionAutoComplete(string key)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Expedition
                        where (i.Name.ToLower().Contains(key.ToLower()) ||
                               i.Code.ToLower().Contains(key.ToLower())) &&
                               i.IsActive == true
                        select i;

            ApplySorting<Expedition>(ref query, "Code", "");
            return ObjectHelper.CopyList<Expedition, ExpeditionModel>(query.Take(SystemConstants.AutoCompleteItemCount).ToList());
        }


        public ExpeditionModel RetrieveExpeditionByCodeOrName(string key)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Expedition
                        where i.Code == key || i.Name == key
                        select i;

            var obj = new ExpeditionModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }
        #endregion

        #region Formulasi
        /* penambahan by tiar for manufactur */

        public void CreateFormulasi(FormulasiModel Fo)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            Formulasi obj = new Formulasi();
            ObjectHelper.CopyProperties(Fo, obj);
            ent.AddToFormulasi(obj);
            ent.SaveChanges();

            Fo.ProductID = obj.ProductID;
        }

        public List<FormulasiModel> RetreiveFormulasi()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Formulasi
                        select i;

            return ObjectHelper.CopyList<Formulasi, FormulasiModel>(query.ToList());
        }

        public FormulasiModel RetreiveByProductID(long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.Formulasi
                        where i.ProductID == productID
                        select i;
            var obj = new FormulasiModel();
            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public FormulasiModel RetreiveFormulasiByProductID(long productID, long productDetailID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.Formulasi
                        where i.ProductID == productID && i.ProductDetailID == productDetailID
                        select i;
            var obj = new FormulasiModel();
            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public List<FormulasiModel> RetreiveVwFormulasiByProductID(long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_Formulasi
                        where i.ProductID == productID
                        select i;

            return ObjectHelper.CopyList<v_Formulasi, FormulasiModel>(query.ToList());
        }

        public void DeleteFormulasi(long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Formulasi
                        where i.ProductID == productID
                        select i;
            foreach (var obj in query.AsEnumerable())
                ent.DeleteObject(obj);

            ent.SaveChanges();
        }
        /* end penambahan*/
        #endregion

        #region Income Expense

        public string RetrieveIncomeExpenseMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.IncomeExpense
                        where i.Code.StartsWith(prefix) && i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "0000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public List<IncomeExpenseModel> RetrieveIncomeExpenseByCategory(int categoryID, int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.IncomeExpense
                        where i.CategoryID == categoryID
                        select i;

            ApplySorting<IncomeExpense>(ref query, "Code DESC", sortParameter);
            ApplyFilter<IncomeExpense>(ref query, selectFilters);

            return ObjectHelper.CopyList<IncomeExpense, IncomeExpenseModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public int RetrieveIncomeExpenseByCategoryCount(int categoryID, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.IncomeExpense
                        where i.CategoryID == categoryID
                        select i;

            ApplyFilter<IncomeExpense>(ref query, selectFilters);

            return query.Count();
        }

        #endregion

        #region Invoice

        public string RetrieveInvoiceMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Invoice
                        where i.Code.StartsWith(prefix) && i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "000000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public List<InvoiceModel> RetrieveInvoiceBySalesOrder(long salesOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Invoice
                        where i.SalesOrderID == salesOrderID && i.Status != 0
                        select i;

            return ObjectHelper.CopyList<v_Invoice, InvoiceModel>(query.ToList());
        }

        public InvoiceModel RetrieveInvoiceByDeliveryOrder(long deliveryOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Invoice
                        where i.SalesOrderID == deliveryOrderID && i.Status != 0
                        select i;

            InvoiceModel invoice = new InvoiceModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), invoice);
            else
                return null;

            return invoice;
        }

        public List<InvoiceModel> RetrieveInvoiceByCustomerIDStartEndDate(long customerID, DateTime dateFrom, DateTime dateTo)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Invoice
                        where i.CustomerID == customerID && i.Status != (int)MPL.DocumentStatus.Void && i.Date >= dateFrom && i.Date <= dateTo
                        select i;

            return ObjectHelper.CopyList<v_Invoice, InvoiceModel>(query.ToList());
        }

        public List<InvoiceModel> RetrieveInvoicesByCustomerID(long customerID, PaymentStatus payStat)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            switch (payStat)
            {
                case PaymentStatus.Paid:
                    var query = from i in ent.v_Invoice
                                where i.CustomerID == customerID
                                    && ((i.Amount + i.TaxAmount) <= (i.PaymentAmount + i.CreditedAmount)
                                    || i.Status == (int)InvoiceStatus.Paid)
                                select i;
                    return ObjectHelper.CopyList<v_Invoice, InvoiceModel>(query.ToList());
                case PaymentStatus.Unpaid:
                    var query2 = from i in ent.v_Invoice
                                 where i.CustomerID == customerID
                                     && (i.Amount + i.TaxAmount) > (i.PaymentAmount + i.CreditedAmount)
                                     && i.Status != 0
                                     && i.Status != (int)InvoiceStatus.Paid
                                 select i;
                    return ObjectHelper.CopyList<v_Invoice, InvoiceModel>(query2.ToList());
                default:
                    var query3 = from i in ent.v_Invoice
                                 where i.CustomerID == customerID
                                     && i.Status != 0
                                 select i;
                    return ObjectHelper.CopyList<v_Invoice, InvoiceModel>(query3.ToList());
            }


        }

        public List<InvoiceModel> RetrieveUncreatedPaymentInvoice(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Invoice
                        where i.OutstandingAmount > 0 //&& i.Status >= (int)InvoiceStatus.New
                        select i;

            ApplySorting<v_Invoice>(ref query, "Code DESC", sortParameter);
            ApplyFilter<v_Invoice>(ref query, selectFilters);

            return ObjectHelper.CopyList<v_Invoice, InvoiceModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public int RetrieveUncreatedPaymentInvoiceCount(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Invoice
                        where i.OutstandingAmount > 0 //&& i.Status >= (int)InvoiceStatus.New
                        select i;

            ApplyFilter<v_Invoice>(ref query, selectFilters);

            return query.Count();
        }

        public int RetrieveUncreatedPaymentInvoiceCount(CustomerGroupModel customerGroup)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Invoice
                        where i.Amount - i.PaymentAmount - i.CreditedAmount > 0 && i.Status >= (int)InvoiceStatus.Approved && i.CustomerGroupID == customerGroup.ID
                        select i;

            return query.Count();
        }

        public int RetreiveInvoiceNotification(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_InvoiceNotification
                        where i.StatusDesc == "Open"
                        select i;

            return query.Count();
        }

        public int RetrieveUnapprovedInvoiceCount(CustomerGroupModel customerGroup)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Invoice
                        where i.Status == (int)InvoiceStatus.New && i.CustomerGroupID == customerGroup.ID
                        select i;

            return query.Count();
        }

        public int RetrieveUnpaidInvoiceCount(CustomerGroupModel customerGroup)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Invoice
                        where i.Status == (int)InvoiceStatus.Approved && i.CustomerGroupID == customerGroup.ID
                        select i;

            return query.Count();
        }

        public int RetrieveOverdueInvoiceCount(CustomerGroupModel customerGroup)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Invoice
                        where (i.Amount + i.TaxAmount - i.PaymentAmount - i.CreditedAmount) > 0 && i.DueDate <= DateTime.Now && i.Status == (int)InvoiceStatus.Approved && i.CustomerGroupID == customerGroup.ID
                        select i;

            return query.Count();
        }

        public int RetrieveNotOverdueInvoiceCount()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Invoice
                        where (i.Amount + i.TaxAmount - i.PaymentAmount - i.CreditedAmount) > 0 && i.DueDate >= DateTime.Now && i.Status == (int)InvoiceStatus.Approved //&& i.Status <= (int)InvoiceStatus.Paid && i.DueDate >= DateTime.Today
                        select i;

            return query.Count();
        }

        public decimal RetrieveUnpaidInvoiceGrandTotal(DateTime startDate, DateTime endDate)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Invoice
                        where i.Status == (int)InvoiceStatus.Approved && i.Date >= startDate && i.Date <= endDate
                        select i;

            var list = ObjectHelper.CopyList<v_Invoice, InvoiceModel>(query.ToList());

            return list.Sum(p => p.GrandTotal);
        }

        public string RetrieveThisMonthInvoiceCount()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Invoice
                        where i.Status >= (int)InvoiceStatus.Approved && SqlFunctions.DatePart("month", i.Date) == MPL.Configuration.CurrentDateTime.Month && SqlFunctions.DatePart("month", i.Date) == MPL.Configuration.CurrentDateTime.Month && SqlFunctions.DatePart("year", i.Date) == MPL.Configuration.CurrentDateTime.Year
                        select i;

            return "Rp. " + Convert.ToDecimal(query.Sum(p => p.Amount + p.TaxAmount)).ToString("N0");
        }

        public string RetrieveThisMonthOutstandingInvoiceCount()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Invoice
                        where i.Status != (int)InvoiceStatus.Void
                        select i;

            return "Rp. " + Convert.ToDecimal(query.Sum(p => (p.Amount + p.TaxAmount) - p.PaymentAmount - p.CreditedAmount)).ToString("N0");
        }

        public string RetrieveMonthlyInvoice()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            List<MonthlyDataModel> monthlyDatas = new List<MonthlyDataModel>();

            var query = from i in ent.v_SalesOrder
                        where i.Status >= (int)InvoiceStatus.Approved
                        group i by new { month = SqlFunctions.DatePart("month", i.Date), year = SqlFunctions.DatePart("year", i.Date) } into d
                        select new { dt = d.Key.month, year = d.Key.year, amount = d.Sum(g => (g.SubTotal)) };

            for (int i = 5; i >= 0; i--)
            {
                MonthlyDataModel monthlyData = new MonthlyDataModel();
                int chartMonth = DateTime.Now.AddMonths(-i).Month;
                int chartYear = DateTime.Now.AddMonths(-i).Year;

                var aMonth = from j in query
                             where j.dt == chartMonth && j.year == chartYear
                             select j;

                monthlyData.Month = new IDNumericSayer().SayMonth(DateTime.Now.AddMonths(-i).Month).Substring(0, 3) + " " + DateTime.Now.AddMonths(-i).Year.ToString().Substring(2, 2);
                monthlyData.Amount = Convert.ToDecimal(aMonth.Sum(p => p.amount)).ToString("N0").Replace(",", "");
                monthlyData.color = "#0072c6";
                monthlyDatas.Add(monthlyData);
            }

            var json = JSONHelper.ToJSON(monthlyDatas);

            return json;
        }

        public string RetrieveMonthlyInvoicePayment()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            List<MonthlyDataModel> monthlyDatas = new List<MonthlyDataModel>();

            var query = from i in ent.v_Invoice
                        where i.Status > 0
                        group i by new { month = SqlFunctions.DatePart("month", i.Date), year = SqlFunctions.DatePart("year", i.Date) } into d
                        select new { dt = d.Key.month, year = d.Key.year, amount = d.Sum(g => (g.Amount - g.CreditedAmount)) };

            for (int i = 5; i >= 0; i--)
            {
                MonthlyDataModel monthlyData = new MonthlyDataModel();
                int chartMonth = DateTime.Now.AddMonths(-i).Month;
                int chartYear = DateTime.Now.AddMonths(-i).Year;

                var aMonth = from j in query
                             where j.dt == chartMonth && j.year == chartYear
                             select j;

                monthlyData.Month = new IDNumericSayer().SayMonth(DateTime.Now.AddMonths(-i).Month).Substring(0, 3) + " " + DateTime.Now.AddMonths(-i).Year.ToString().Substring(2, 2);
                monthlyData.Amount = Convert.ToDecimal(aMonth.Sum(p => p.amount)).ToString("N0").Replace(",", "");
                monthlyData.color = "#0072c6";
                monthlyDatas.Add(monthlyData);
            }

            var json = JSONHelper.ToJSON(monthlyDatas);

            return json;
        }

        public string RetrieveMonthlyPO()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            List<MonthlyDataModel> monthlyDatas = new List<MonthlyDataModel>();

            var query = from i in ent.v_PurchaseOrder
                        where i.Status != (int)MPL.DocumentStatus.Void
                        group i by new { month = SqlFunctions.DatePart("month", i.Date), year = SqlFunctions.DatePart("year", i.Date) } into d
                        select new { dt = d.Key.month, year = d.Key.year, amount = d.Sum(g => (g.SubTotal)) };

            for (int i = 5; i >= 0; i--)
            {
                MonthlyDataModel monthlyData = new MonthlyDataModel();
                int chartMonth = DateTime.Now.AddMonths(-i).Month;
                int chartYear = DateTime.Now.AddMonths(-i).Year;
                var aMonth = from j in query
                             where j.dt == chartMonth && j.year == chartYear
                             select j;

                monthlyData.Month = new IDNumericSayer().SayMonth(DateTime.Now.AddMonths(-i).Month).Substring(0, 3) + " " + DateTime.Now.AddMonths(-i).Year.ToString().Substring(2, 2);
                monthlyData.Amount = Convert.ToDecimal(aMonth.Sum(p => p.amount)).ToString("N0").Replace(",", "");
                monthlyData.color = "#319f10";
                monthlyDatas.Add(monthlyData);
            }

            var json = JSONHelper.ToJSON(monthlyDatas);

            return json;
        }

        public string RetrieveMonthlyBillingPayment()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            List<MonthlyDataModel> monthlyDatas = new List<MonthlyDataModel>();

            var query = from i in ent.v_PurchaseBill
                        where i.Status > 0
                        group i by new { month = SqlFunctions.DatePart("month", i.Date), year = SqlFunctions.DatePart("year", i.Date) } into d
                        select new { dt = d.Key.month, year = d.Key.year, amount = d.Sum(g => (g.Amount - g.CreditedAmount)) };

            for (int i = 5; i >= 0; i--)
            {
                MonthlyDataModel monthlyData = new MonthlyDataModel();
                int chartMonth = DateTime.Now.AddMonths(-i).Month;
                int chartYear = DateTime.Now.AddMonths(-i).Year;

                var aMonth = from j in query
                             where j.dt == chartMonth && j.year == chartYear
                             select j;

                monthlyData.Month = new IDNumericSayer().SayMonth(DateTime.Now.AddMonths(-i).Month).Substring(0, 3) + " " + DateTime.Now.AddMonths(-i).Year.ToString().Substring(2, 2);
                monthlyData.Amount = Convert.ToDecimal(aMonth.Sum(p => p.amount)).ToString("N0").Replace(",", "");
                monthlyData.color = "#319f10";
                monthlyDatas.Add(monthlyData);
            }

            var json = JSONHelper.ToJSON(monthlyDatas);

            return json;
        }

        public List<SalesOrderModel> RetrieveUncreatedInvoiceOrder(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_SalesOrder
                        where i.Status >= (int)MPL.DocumentStatus.Approved && i.IsInvoiceable == true// && (i.CreatedDOQuantity - i.CreatedInvQuantity) > 0
                        select i;

            ApplySorting<v_SalesOrder>(ref query, "Code DESC", sortParameter);
            ApplyFilter<v_SalesOrder>(ref query, selectFilters);

            return ObjectHelper.CopyList<v_SalesOrder, SalesOrderModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public int RetrieveUncreatedInvoiceOrderCount(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_SalesOrder
                        where i.Status >= (int)MPL.DocumentStatus.Approved && i.IsInvoiceable == true //(i.CreatedDOQuantity - i.CreatedInvQuantity) > 0
                        select i;

            ApplyFilter<v_SalesOrder>(ref query, selectFilters);

            return query.Count();
        }

        public List<InvoiceModel> RetrieveInvBySOID(long salesOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from j in ent.v_Invoice
                        where j.SalesOrderID == salesOrderID && j.Status != (int)PurchaseBillStatus.Void
                        select j;

            return ObjectHelper.CopyList<v_Invoice, InvoiceModel>(query.ToList());
        }

        public List<InvoiceDetailModel> RetrieveInvoiceDetailBySOID(long soID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.InvoiceDetail
                        from j in ent.Invoice
                        where i.InvoiceID == j.ID && j.SalesOrderID == soID && j.Status != (int)InvoiceStatus.Void
                        select i;

            return ObjectHelper.CopyList<InvoiceDetail, InvoiceDetailModel>(query.ToList());
        }

        public List<InvoiceModel> RetreiveInvByCustomerID(long customerID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_Invoice
                        where i.CustomerID == customerID && i.Status > 0
                        select i;

            return ObjectHelper.CopyList<v_Invoice, InvoiceModel>(query.OrderByDescending(p => p.Date).Take(SystemConstants.ItemPerPage).ToList());
        }

        public List<InvoiceModel> RetrieveAveragePaymentOverDue(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_AveragePaymentOverDue
                        select i;

            ApplySorting<v_AveragePaymentOverDue>(ref query, "DATE DESC", sortParameter);
            ApplyFilter<v_AveragePaymentOverDue>(ref query, selectFilters);
            return ObjectHelper.CopyList<v_AveragePaymentOverDue, InvoiceModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public int RetrieveAveragePaymentOverDueCount(List<SelectFilter> selectFilter)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_AveragePaymentOverDue
                        select i;
            ApplyFilter<v_AveragePaymentOverDue>(ref query,selectFilter);
            return query.Count();
        }

        public void CreateInvoiceResiDetail(InvoiceResiDetailModel Detail)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            InvoiceResiDetail obj = new InvoiceResiDetail();
            ObjectHelper.CopyProperties(Detail, obj);
            ent.AddToInvoiceResiDetail(obj);
            ent.SaveChanges();
        }

        public void DeleteInvoiceResiDetail(long InvoiceID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.InvoiceResiDetail
                        where i.InvoiceID == InvoiceID
                        select i;
            foreach (var obj in query.AsEnumerable())
                ent.DeleteObject(obj);

            ent.SaveChanges();
        }

        public List<InvoiceResiDetailModel> RetrieveInvoiceResiByInvoiceID(long invoiceID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_InvoiceResiDetail
                        where i.InvoiceID == invoiceID
                        select i;
            return ObjectHelper.CopyList<v_InvoiceResiDetail, InvoiceResiDetailModel>(query.OrderByDescending(p => p.ResiDate).ToList());
        }

        public List<InvoiceResiDetailModel> RetrieveInvoiceResiByResiID(long resiID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_InvoiceResiDetail
                        where i.ResiID == resiID
                        select i;
            return ObjectHelper.CopyList<v_InvoiceResiDetail, InvoiceResiDetailModel>(query.ToList());
        }

        public int RetrieveInvoiceCount(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_Invoice
                        where i.Status != (int)MPL.DocumentStatus.Void
                        select i;

            ApplyFilter<v_Invoice>(ref query, selectFilters);
            return query.Count();
        }

        public List<InvoiceModel> RetrieveInvoice(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_Invoice
                        where i.Status != (int)MPL.DocumentStatus.Void
                        select i;

            ApplySorting<v_Invoice>(ref query, "Date DESC", sortParameter);
            ApplyFilter<v_Invoice>(ref query, selectFilters);
            return ObjectHelper.CopyList<v_Invoice, InvoiceModel>(query.Skip(startIndex).Take(amount).ToList());
        }
        #endregion

        #region Item Location

        public void CreateItemLocation(ItemLocationModel itemLoc)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            ItemLocation obj = new ItemLocation();
            ObjectHelper.CopyProperties(itemLoc, obj);
            ent.AddToItemLocation(obj);
            ent.SaveChanges();

            itemLoc.ID = obj.ID;
        }

        public List<ItemLocationModel> RetrieveItemLocation()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.ItemLocation
                        select i;

            return ObjectHelper.CopyList<ItemLocation, ItemLocationModel>(query.ToList());
        }

        public ItemLocationModel RetrieveItemLocationByProductID(long productID, long warehouseID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.ItemLocation
                        where i.ProductID == productID && i.WarehouseID == warehouseID
                        select i;

            var obj = new ItemLocationModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public List<ItemLocationModel> RetrieveItemLocationsByProductID(long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_ItemLocation
                        where i.ProductID == productID
                        select i;

            if (query.FirstOrDefault() != null)
                return ObjectHelper.CopyList<v_ItemLocation, ItemLocationModel>(query.ToList());
            else
                return null;
        }

        #endregion

        #region Last Buy Price

        public void RecalculateLastBuyPrice(long productID, long vendorID)
        {
            string spName = "sp_RecalculateLastBuyPrice";
            var command = DbEngine.CreateCommand(spName, true) as SqlCommand;
            command.Parameters.AddWithValue("@productID", productID);
            command.Parameters.AddWithValue("@vendorID", vendorID);
            DbEngine.ExecuteNonQuery(command);
        }

        public LastBuyPriceModel RetrieveLastBuyPrice(long productID, long vendorID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.LastBuyPrice
                        where i.ProductID == productID && i.VendorID == vendorID
                        select i;

            var obj = new LastBuyPriceModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public LastBuyPriceModel RetrieveLastExchangeRate(long vendorID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.LastBuyPrice
                        where i.VendorID == vendorID
                        select i;

            var obj = new LastBuyPriceModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.OrderByDescending(p => p.ID).FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        #endregion
        
        #region LimitStock
        /* penambahan by tiar for manufactur */
        public void CreateLimitStock(LimitStockModel Ls)
        {
            try
            {
                ABCAPOSEntities ent = new ABCAPOSEntities();
                LimitStock obj = new LimitStock();
                ObjectHelper.CopyProperties(Ls, obj);
                ent.AddToLimitStock(obj);
                ent.SaveChanges();

                Ls.ID = obj.ID;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public List<LimitStockModel> RetrieveLimitStock()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.LimitStock
                        select i;

            return ObjectHelper.CopyList<LimitStock, LimitStockModel>(query.ToList());
        }

        public LimitStockModel RetrieveLimitStockByProductID(long productID, long warehouseID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.LimitStock
                        where i.ProductID == productID && i.WarehouseID == warehouseID
                        select i;

            var obj = new LimitStockModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public List<LimitStockModel> RetrieveLimitStocksByProductID(long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_LimitStock
                        where i.ProductID == productID
                        select i;

            return ObjectHelper.CopyList<v_LimitStock, LimitStockModel>(query.ToList());
        }

        public void DeleteLimitStock(long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.LimitStock
                        where i.ProductID == productID
                        select i;

            foreach (var obj in query.AsEnumerable())
                ent.DeleteObject(obj);

            ent.SaveChanges();
        }
        /* end penambahan*/
        #endregion

        #region Log
        public void DeleteLogByLogIDContainerIDProductID(long LogID,long containerID,long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.LogDetail
                        where i.LogID == LogID && i.ContainerID==containerID && i.ProductID==productID
                        select i;

            foreach (var obj in query.AsEnumerable())
                ent.DeleteObject(obj);

            ent.SaveChanges();
        }

        public void DeleteLogByLogID(long logID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.LogDetail
                        where i.LogID == logID
                        select i;

            foreach (var obj in query.AsEnumerable())
                ent.DeleteObject(obj);

            ent.SaveChanges();
        }

        public void CreateLog(LogDetailModel Ldm)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            LogDetail obj = new LogDetail();
            ObjectHelper.CopyProperties(Ldm, obj);
            ent.AddToLogDetail(obj);
            ent.SaveChanges();

            Ldm.LogID = obj.LogID;
        }

        public LogDetailModel RetreiveLogByLogIDContainerIDProductIDWarehouseID(long logID, long containerID, long productID, long warehouseID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_LogDetail
                        where i.LogID == logID && i.ContainerID == containerID && i.ProductID == i.ProductID && i.WarehouseID == warehouseID
                        select i;
            var obj = new LogDetailModel();
            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public LogDetailModel RetreivePriceByLogIDProductIDWarehouseID(long logID, long productID, long warehouseID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_LogDetail
                        where i.LogID == logID && i.ProductID == productID && i.WarehouseID == warehouseID
                        select i;

            var obj = new LogDetailModel();
            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public LogDetailModel RetreiveLogByLogIDProductID(long logID, long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.LogDetail
                        where i.LogID == logID && i.ProductID == productID
                        select i;
            var obj = new LogDetailModel();
            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public List<LogDetailModel> RetreiveLogByPurchaseOrderID(long purchaseOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_RelatedLogByPurchaseOrder
                        where i.purchaseorderID == purchaseOrderID
                        select i;
            return ObjectHelper.CopyList<v_RelatedLogByPurchaseOrder, LogDetailModel>(query.ToList());
        }

        public List<LogDetailModel> RetreiveProductQtyOnLogDetail(long productID, long warehouseID, DateTime date)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_LogDetail
                        where i.ProductID == productID && i.WarehouseID == warehouseID && i.Date <= date
                        select i;
            return ObjectHelper.CopyList<v_LogDetail, LogDetailModel>(query.ToList());
        }

        public List<StockMovementModel> RetrieveStockMovement(long productID, int transactionType)
        {
            string spName = "sp_Rpt_StockMovement";

            var command = DbEngine.CreateCommand(spName, true) as SqlCommand;
            command.Parameters.AddWithValue("@productID", productID);
            command.Parameters.AddWithValue("@transactionType", transactionType);

            return DbEngine.FillList<StockMovementModel>(command).Take(50).ToList();
        }

        public List<StockMovementModel> RetrieveStockMovement(long productID, int transactionType,DateTime startDate,DateTime endDate, int startIndex, int amount)
        {
            string spName = "sp_Rpt_StockMovement";

            var command = DbEngine.CreateCommand(spName, true) as SqlCommand;
            command.Parameters.AddWithValue("@productID", productID);
            command.Parameters.AddWithValue("@transactionType", transactionType);
            command.Parameters.AddWithValue("@startDate", startDate);
            command.Parameters.AddWithValue("@endDate", endDate);

            return DbEngine.FillList<StockMovementModel>(command).Skip(startIndex).Take(amount).OrderBy(p=>p.LogID).ToList();
        }

        public int RetrieveStockMovementCount(long productID, int transactionType, DateTime startDate, DateTime endDate)
        {
            string spName = "sp_Rpt_StockMovement";

            var command = DbEngine.CreateCommand(spName, true) as SqlCommand;
            command.Parameters.AddWithValue("@productID", productID);
            command.Parameters.AddWithValue("@transactionType", transactionType);
            command.Parameters.AddWithValue("@startDate", startDate);
            command.Parameters.AddWithValue("@endDate", endDate);

            return DbEngine.FillList<StockMovementModel>(command).Count();
        }
        #endregion

        #region Multiple Invoicing

        public string RetrieveMultipleInvoicingMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.MultipleInvoicing
                        where i.Code.StartsWith(prefix) && i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "000000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public List<InvoiceModel> RetrieveUncreatedMultipleInvoicing(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_UncreatedMultipleInvoicing
                        select i;
            //where i.Status >= (int)SalesOrderStatus.Approved && (i.CreatedDOQuantity - i.CreatedInvQuantity) > 0

            ApplySorting<v_UncreatedMultipleInvoicing>(ref query, "CustomerName DESC", sortParameter);
            ApplyFilter<v_UncreatedMultipleInvoicing>(ref query, selectFilters);

            return ObjectHelper.CopyList<v_UncreatedMultipleInvoicing, InvoiceModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public int RetrieveUncreatedMultipleInvoicingCount(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_UncreatedMultipleInvoicing
                        //where i.Status >= (int)SalesOrderStatus.Approved && (i.CreatedDOQuantity - i.CreatedInvQuantity) > 0
                        select i;

            ApplyFilter<v_UncreatedMultipleInvoicing>(ref query, selectFilters);

            return query.Count();
        }

        public void CreateMultipleInvoiceItem(MultipleInvoiceItemModel multipleInvItem)
        {
            
            ABCAPOSEntities ent = new ABCAPOSEntities();
            MultipleInvoiceItem obj = new MultipleInvoiceItem();
            ObjectHelper.CopyProperties(multipleInvItem, obj);
            ent.AddToMultipleInvoiceItem(obj);
            ent.SaveChanges();
        }

        public void DeleteMultipleInvoiceItem(long multipleInvID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.MultipleInvoiceItem
                        where i.MultipleInvoicingID == multipleInvID
                        select i;

            foreach (var obj in query.AsEnumerable())
                ent.DeleteObject(obj);

            ent.SaveChanges();
        }

        public List<MultipleInvoiceItemModel> RetrieveMultipleInvoiceItems(long multipleInvID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_MultipleInvoiceItem
                        where i.MultipleInvoicingID == multipleInvID
                        select i;

            return ObjectHelper.CopyList<v_MultipleInvoiceItem, MultipleInvoiceItemModel>(query.ToList());
        }

        public List<MultipleInvoiceItemModel> RetrieveMultipleInvoiceItemsGroup(long multipleInvID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_MultipleInvoiceItem
                        where i.MultipleInvoicingID == multipleInvID
                        group i by new { i.MultipleInvoicingID, i.ProductID, i.ProductName, i.ConversionName, i.TaxType, Price = i.Price } //Math.Round((decimal) i.Price, 2)
                            into g
                            select new
                            {
                                MultipleInvoicingID = g.Key.MultipleInvoicingID,
                                ProductID = g.Key.ProductID,
                                ProductName = g.Key.ProductName,
                                ConversionName = g.Key.ConversionName,
                                TaxType = g.Key.TaxType,
                                Price = g.Key.Price,
                                Quantity = g.Sum(h => h.Quantity)
                            };
            int listCount = query.ToList().Count;
            List<MultipleInvoiceItemModel> multipleInvoices = new List<MultipleInvoiceItemModel>();
            for (int i = 0; i < listCount; i++)
            {
                MultipleInvoiceItemModel item = new MultipleInvoiceItemModel();
                item.MultipleInvoicingID = query.ToList()[i].MultipleInvoicingID;
                item.ItemNo = i + 1;
                item.ProductID = Convert.ToInt64(query.ToList()[i].ProductID);
                item.ProductName = query.ToList()[i].ProductName;
                item.Quantity = Convert.ToDouble(query.ToList()[i].Quantity);
                item.ConversionName = query.ToList()[i].ConversionName;
                item.TaxType = Convert.ToInt32(query.ToList()[i].TaxType);
                item.Price = Convert.ToDecimal(query.ToList()[i].Price);
                if (item.TaxType == 2)
                {
                    item.TotalAmount = new IDNumericSayer().Pembulatan(Math.Round(Convert.ToDecimal(item.Price) * Convert.ToDecimal(item.Quantity), 0));
                    item.TotalPPN = new IDNumericSayer().Pembulatan(Math.Round(Convert.ToDecimal(Convert.ToDouble(item.TotalAmount) * 0.1), 0));//Convert.ToDecimal(.ToString("N2"))
                }
                else
                {
                    item.TotalAmount = new IDNumericSayer().Pembulatan(Math.Round(Convert.ToDecimal(item.Price) * Convert.ToDecimal(item.Quantity), 0));//.ToString("N2")
                    item.TotalPPN = 0;
                }
                item.GrossAmount = item.TotalAmount + item.TotalPPN;
                multipleInvoices.Add(item);
            }
            return multipleInvoices;

        }
        #endregion

        #region Multi Payment

        public string RetrieveMakeMultiPaymentMaxCode(int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.MakeMultiPay
                        select i.ID;

            var extMaxCode = query.Max();

            extMaxCode++;

            return extMaxCode.ToString("D" + length);
        }

        #endregion

        #region Multi Pay Sales

        public string RetrieveMakeMultiPaymentSalesMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.MakeMultiPaySales
                        where i.Code.StartsWith(prefix) && i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "000000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public List<MakeMultiPaySalesModel> RetreiveDataPostingUlangInvoicePayment()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_PostingUlangInvoicePayment
                        select i;

            return ObjectHelper.CopyList<v_PostingUlangInvoicePayment, MakeMultiPaySalesModel>(query.OrderBy(p => p.Date).ToList());
        }

        public List<MakeMultiPaySalesModel> RetreiveInvPaymentByCustomerID(long customerID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_MakeMultiPaySales
                        where i.CustomerID == customerID
                        select i;
            return ObjectHelper.CopyList<v_MakeMultiPaySales, MakeMultiPaySalesModel>(query.OrderByDescending(p => p.Date).Take(SystemConstants.ItemPerPage).ToList());
        }
        #endregion

        #region Payment

        public string RetrievePaymentMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Payment
                        where i.Code.StartsWith(prefix) //&& i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "000000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public List<PaymentModel> RetrievePaymentByInvoice(long invoiceID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Payment
                        where i.InvoiceID == invoiceID
                        select i;

            return ObjectHelper.CopyList<Payment, PaymentModel>(query.ToList());
        }

        public int RetrieveVoidPaymentCount(CustomerGroupModel customerGroup)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Payment
                        where i.Status == (int)MPL.DocumentStatus.Void && SqlFunctions.DatePart("month", i.Date) == MPL.Configuration.CurrentDateTime.Month
                        && SqlFunctions.DatePart("year", i.Date) == MPL.Configuration.CurrentDateTime.Year && i.CustomerGroupID == customerGroup.ID
                        select i;

            return query.Count();
        }

        public int RetrieveUnapprovedPaymentCount(CustomerGroupModel customerGroup)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Payment
                        where i.CustomerGroupID == customerGroup.ID && SqlFunctions.DatePart("month", i.Date) == MPL.Configuration.CurrentDateTime.Month
                        && SqlFunctions.DatePart("year", i.Date) == MPL.Configuration.CurrentDateTime.Year //i.Status == (int)MPL.DocumentStatus.New && 
                        select i;

            return query.Count();
        }

        public string RetrieveThisPaymentCount()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Payment
                        where SqlFunctions.DatePart("month", i.Date) == MPL.Configuration.CurrentDateTime.Month && SqlFunctions.DatePart("year", i.Date) == MPL.Configuration.CurrentDateTime.Year
                        select i;

            return "Rp. " + Convert.ToDecimal(query.Sum(p => p.Amount)).ToString("N0");
        }

        #endregion

        #region Payment Method

        public List<PaymentMethodModel> RetrievePaymentMethod(bool isActive)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.PaymentMethod
                        where i.IsActive == isActive
                        select i;

            return ObjectHelper.CopyList<PaymentMethod, PaymentMethodModel>(query.ToList());
        }

        #endregion

        #region PostingUlang

        public List<PostingUlangModel> RetreiveDataPostingUlangStock()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_PostingUlang
                        select i;

            return ObjectHelper.CopyList<v_PostingUlang, PostingUlangModel>(query.OrderBy(p => p.Date).ToList());
        }

        #endregion

        #region Prefix Setting
        public void UpdatePrefixSetting(PrefixSettingModel prefixSetting)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.PrefixSetting
                        select i;

            var obj = query.FirstOrDefault();
            ObjectHelper.CopyProperties(prefixSetting, obj);
            ent.SaveChanges();
        }

        public PrefixSettingModel RetrievePrefixSetting()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.PrefixSetting
                        select i;

            PrefixSettingModel obj = new PrefixSettingModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }
        #endregion

        #region Product

        public string RetrieveProductMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Product
                        where i.Code.StartsWith(prefix) && i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "0000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public int RetrieveLowQtyProductCount()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Product
                        where i.StockQty < 500
                        select i;

            return query.Count();
        }

        public ProductModel RetrieveProductByCode(string productCode)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Product
                        where i.Code == productCode && i.IsActive == true
                        select i;

            var obj = new ProductModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public ProductModel RetrieveProductByID(long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Product
                        where i.ID == productID
                        select i;

            var obj = new ProductModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public List<ProductModel> RetrieveProductAutoComplete(string key)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Product
                        where (//i.Code.ToLower().Contains(key.ToLower()) ||
                               i.ProductName.ToLower().Contains(key.ToLower()) ||
                               i.Code.ToLower().Contains(key.ToLower())) &&
                            //(i.Type == null || i.Type == "") &&
                              i.IsActive == true
                        select i;

            ApplySorting<Product>(ref query, "Code", "");
            return ObjectHelper.CopyList<Product, ProductModel>(query.Take(SystemConstants.AutoCompleteItemCount).ToList());
        }

        public List<ProductModel> RetrieveProductAutoCompleteOnSales(string key)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Product
                        where (//i.Code.ToLower().Contains(key.ToLower()) ||
                               i.ProductName.ToLower().Contains(key.ToLower()) ||
                               i.Code.ToLower().Contains(key.ToLower())) &&
                            //(i.Type == "A" || i.Type == "B") &&
                              i.IsActive == true && i.ItemTypeID == 4
                        select i;

            ApplySorting<Product>(ref query, "Code", "");
            return ObjectHelper.CopyList<Product, ProductModel>(query.Take(SystemConstants.AutoCompleteItemCount).ToList());
        }

        public List<ProductModel> RetrieveProductAutoCompleteAll(string key)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Product
                        where (//i.Code.ToLower().Contains(key.ToLower()) ||
                               i.ProductName.ToLower().Contains(key.ToLower()) ||
                               i.Code.ToLower().Contains(key.ToLower())) &&
                              i.IsActive == true
                        select i;

            ApplySorting<Product>(ref query, "Code", "");
            return ObjectHelper.CopyList<Product, ProductModel>(query.Take(SystemConstants.AutoCompleteItemCount).ToList());
        }

        public List<ProductModel> RetreiveProductAutoCompleteWorkOrder(string key)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Product
                        where (//i.Code.ToLower().Contains(key.ToLower()) ||
                               i.ProductName.ToLower().Contains(key.ToLower()) ||
                               i.Code.ToLower().Contains(key.ToLower())) &&
                              i.IsActive == true && (i.ItemTypeID == 3 || i.ItemTypeID == 4)
                        select i;

            ApplySorting<Product>(ref query, "Code", "");
            return ObjectHelper.CopyList<Product, ProductModel>(query.Take(SystemConstants.AutoCompleteItemCount).ToList());
        }

        public List<ProductModel> RetreiveProductAutoCompleteWorkOrderFG(string key)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Product
                        where (//i.Code.ToLower().Contains(key.ToLower()) ||
                               i.ProductName.ToLower().Contains(key.ToLower()) ||
                               i.Code.ToLower().Contains(key.ToLower())) &&
                              i.IsActive == true && i.ItemTypeID == 4
                        select i;

            ApplySorting<Product>(ref query, "Code", "");
            return ObjectHelper.CopyList<Product, ProductModel>(query.Take(SystemConstants.AutoCompleteItemCount).ToList());
        }

        public List<ProductDetailModel> RetrieveProductDetailDesc(List<ProductDetailModel> details)
        {
            //ApplySorting<v_ProductDetail>(ref query, "ItemNo DESC", "");

            return details.OrderByDescending(p => p.ItemNo).ToList();
        }

        public List<ProductModel> RetrieveSalesOrderItemSoldList(List<SelectFilter> selectFilters, int year)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_SalesOrderItemSold
                        select i;

            ApplyFilter<v_SalesOrderItemSold>(ref query, selectFilters);

            var query2 = from i in query
                         where i.QtySold > 0
                         group i by new { i.CustomerID, i.CustomerName, i.ProductCode, i.ProductName }
                             into g
                             select new
                             {
                                 CustomerID = g.Key.CustomerID,
                                 CustomerName = g.Key.CustomerName,
                                 ProductCode = g.Key.ProductCode,
                                 ProductName = g.Key.ProductName,
                                 QtySold = g.Sum(h => h.QtySold)
                             };
            List<ProductModel> products = new List<ProductModel>();

            foreach (var item in query2.ToList())
            {
                var product = new ProductModel();
                product.CustomerID = Convert.ToInt64(item.CustomerID);
                product.CustomerName = item.CustomerName;
                product.ProductCode = item.ProductCode;
                product.ProductName = item.ProductName;
                product.QtySold = Convert.ToDouble(item.QtySold);

                products.Add(product);
            }

            if (selectFilters.Count > 0)
            {
                //products = products.OrderBy(p => p.CustomerName).OrderBy(p => p.ProductCode).OrderByDescending(p => p.QtySold).ToList();
                products = products.ToList();
            }
            else
            {
                products = products.OrderByDescending(p => p.QtySold).Take(20).ToList();
            }
            return products.OrderByDescending(p => p.QtySold).ToList();

        }

        public List<ProductModel> RetrieveProductItemSoldList(List<SelectFilter> selectFilters, int year)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_SalesOrderItemSold
                       
                        select i;

            ApplyFilter<v_SalesOrderItemSold>(ref query, selectFilters);

            var query2 = from i in query
                         where i.QtySold > 0
                         group i by new { i.ProductID, i.ProductCode, i.ProductName }
                             into g
                             select new
                             {
                                 ProductID = g.Key.ProductID,
                                 ProductCode = g.Key.ProductCode,
                                 ProductName = g.Key.ProductName,
                                 QtySold = g.Sum(h => h.QtySold)
                             };
            List<ProductModel> products = new List<ProductModel>();

            foreach (var item in query2.ToList())
            {
                var product = new ProductModel();
                product.ID = Convert.ToInt64(item.ProductID);
                product.ProductCode = item.ProductCode;
                product.ProductName = item.ProductName;
                product.QtySold = Convert.ToDouble(item.QtySold);

                products.Add(product);
            }
            products = products.OrderByDescending(p => p.QtySold).ToList();
            if (selectFilters.Count > 0)
                products = products.ToList();
            else
                products = products.Take(20).ToList();
            return products;
        }
        #endregion

        #region Price Level

        public List<PriceLevelModel> RetrievePriceLevels(bool isActive)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.PriceLevel
                        where i.IsActive == isActive
                        select i;

            return ObjectHelper.CopyList<PriceLevel, PriceLevelModel>(query.ToList());
        }

        public PriceLevelModel RetrievePriceLevelByName(string name)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.PriceLevel
                        where i.Description.ToUpper() == name.ToUpper()
                        select i;

            var obj = new PriceLevelModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        #endregion

        #region Purchase Bill

        public string RetrievePurchaseBillMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.PurchaseBill
                        where i.Code.StartsWith(prefix)
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "000000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public List<PurchaseBillDetailModel> RetrievePurchaseBillDetailByPDID(long pdID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.PurchaseBillDetail
                        from j in ent.PurchaseBill
                        where i.PurchaseBillID == j.ID && j.PurchaseDeliveryID == pdID && j.Status != (int)PurchaseBillStatus.Void
                        select i;

            return ObjectHelper.CopyList<PurchaseBillDetail, PurchaseBillDetailModel>(query.ToList());
        }

        public List<PurchaseBillDetailModel> RetrievePurchaseBillDetailByPOID(long poID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.PurchaseBillDetail
                        from j in ent.PurchaseBill
                        where i.PurchaseBillID == j.ID && j.PurchaseOrderID == poID && j.Status != (int)PurchaseBillStatus.Void
                        select i;

            return ObjectHelper.CopyList<PurchaseBillDetail, PurchaseBillDetailModel>(query.ToList());
        }

        public List<PurchaseBillDetailModel> RetreivePurchaseBillByProductID(long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = (from i in ent.PurchaseBillDetail
                         from j in ent.PurchaseBill
                         where i.PurchaseBillID == j.ID && i.ProductID == productID && j.Status != (int)PurchaseBillStatus.Void
                         select i).OrderByDescending(p => p.PurchaseBillID);

            return ObjectHelper.CopyList<PurchaseBillDetail, PurchaseBillDetailModel>(query.ToList());
        }

        /// <summary>
        ///  Retrieves payable purchase bills for vendor ID with specific Currency only
        /// </summary>
        /// <param name="vendorID"></param>
        /// <param name="currencyID">Currency ID</param>
        /// <returns>List of payable purchase bills for vendorID</returns>
        public List<PurchaseBillModel> RetrievePurchaseBillByVendorID(long vendorID, long currencyID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from j in ent.v_PurchaseBill
                        where j.VendorID == vendorID
                            && (j.Amount + j.TaxAmount) > (j.PaymentAmount + j.CreditedAmount)
                            && j.CurrencyID == currencyID && j.Status != (int)MPL.DocumentStatus.Void
                        select j;

            return ObjectHelper.CopyList<v_PurchaseBill, PurchaseBillModel>(query.OrderByDescending(p=>p.Date).ToList());
        }

        public List<PurchaseBillModel> RetrievePurchaseBillByCode(string code)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from j in ent.v_PurchaseBill
                        where j.Code == code && j.Status != 0
                        select j;

            return ObjectHelper.CopyList<v_PurchaseBill, PurchaseBillModel>(query.ToList());
        }

        public List<PurchaseBillModel> RetrievePBByPOID(long purchaseOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from j in ent.v_PurchaseBill
                        where j.PurchaseOrderID == purchaseOrderID && j.Status != (int)PurchaseBillStatus.Void
                        select j;

            return ObjectHelper.CopyList<v_PurchaseBill, PurchaseBillModel>(query.ToList());
        }

        public PurchaseBillModel RetrievePBByPurchaseOrderID(long purchaseOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from j in ent.v_PurchaseBill
                       where j.PurchaseOrderID == purchaseOrderID && j.Status != (int)PurchaseBillStatus.Void
                       select j;

            var obj = new PurchaseBillModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public void CreatePODetails(long poID, List<PurchaseOrderDetailModel> poDetails)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            foreach (var poDetail in poDetails)
            {
                poDetail.PurchaseOrderID = poID;
                var itemNo = poDetail.ItemNo;
                poDetail.ItemNo = itemNo;


                PurchaseOrderDetail obj = new PurchaseOrderDetail();
                ObjectHelper.CopyProperties(poDetail, obj);
                ent.AddToPurchaseOrderDetail(obj);
            }

            ent.SaveChanges();
        }

        #endregion

        #region Purchase Delivery

        public string RetrievePurchaseDeliveryMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.PurchaseDelivery
                        where i.Code.StartsWith(prefix)
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "000000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public List<PurchaseOrderModel> RetrieveUncreatedPurchaseDeliveryPO(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchaseOrder
                        where i.IsPDFulfilled == false && i.Status == (int)MPL.DocumentStatus.Approved && i.IsReceivable == true
                        select i;

            ApplySorting<v_PurchaseOrder>(ref query, "Code DESC", sortParameter);
            ApplyFilter<v_PurchaseOrder>(ref query, selectFilters);

            return ObjectHelper.CopyList<v_PurchaseOrder, PurchaseOrderModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public int RetrieveUncreatedPurchaseDeliveryPOCount(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchaseOrder
                        where i.IsPDFulfilled == false && i.Status == (int)MPL.DocumentStatus.Approved && i.IsReceivable == true
                        select i;

            ApplyFilter<v_PurchaseOrder>(ref query, selectFilters);

            return query.Count();
        }

        public List<PurchaseOrderModel> RetrieveUncreatedPurchaseBillOrder(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchaseOrder
                        where i.Status == (int)MPL.DocumentStatus.Approved && i.IsBillable == true // && (i.CreatedPDQuantity - i.CreatedPBQuantity) > 0  
                        select i;

            ApplySorting<v_PurchaseOrder>(ref query, "Code DESC", sortParameter);
            ApplyFilter<v_PurchaseOrder>(ref query, selectFilters);

            return ObjectHelper.CopyList<v_PurchaseOrder, PurchaseOrderModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public int RetrieveUncreatedPurchaseBillOrderCount(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchaseOrder
                        where i.Status == (int)MPL.DocumentStatus.Approved && i.IsBillable == true // && (i.CreatedPDQuantity - i.CreatedPBQuantity) > 0  
                        select i;

            ApplyFilter<v_PurchaseOrder>(ref query, selectFilters);

            return query.Count();
        }

        public List<PurchaseDeliveryDetailModel> RetrievePurchaseDeliveryDetailByPOID(long poID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.PurchaseDeliveryDetail
                        from j in ent.PurchaseDelivery
                        where i.PurchaseDeliveryID == j.ID && j.PurchaseOrderID == poID && j.Status != (int)PurchaseDeliveryStatus.Void
                        select i;

            return ObjectHelper.CopyList<PurchaseDeliveryDetail, PurchaseDeliveryDetailModel>(query.ToList());
        }

        public List<PurchaseDeliveryModel> RetrievePDByPOID(long purchaseOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from j in ent.v_PurchaseDelivery
                        where j.PurchaseOrderID == purchaseOrderID && j.Status != (int)PurchaseDeliveryStatus.Void
                        select j;

            return ObjectHelper.CopyList<v_PurchaseDelivery, PurchaseDeliveryModel>(query.ToList());
        }

        public PurchaseDeliveryModel RetrievePurchaseDeliveryByPOSupplierNo(string poSupplierNo)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.PurchaseDelivery
                        where i.POSupplierNo == poSupplierNo
                        select i;

            PurchaseDeliveryModel obj = new PurchaseDeliveryModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }
        #endregion

        #region Purchase Order

        public string RetrievePurchaseOrderMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.PurchaseOrder
                        where i.Code.StartsWith(prefix) //&& i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "000000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }
        // TODO: (low priority)
        public int RetrieveVoidPurchaseOrderCount()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchaseOrder
                        where i.Status == (int)MPL.DocumentStatus.Void && SqlFunctions.DatePart("month", i.Date) == MPL.Configuration.CurrentDateTime.Month
                        select i;

            return query.Count();
        }

        // TODO: (low priority)
        public string RetrieveThisMonthPurchaseOrderCount()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchaseOrder
                        where i.Status == (int)MPL.DocumentStatus.New && SqlFunctions.DatePart("month", i.Date) == MPL.Configuration.CurrentDateTime.Month
                        select i;

            return "Rp. " + Convert.ToDecimal(query.Sum(p => p.POTotal + p.DiscountTotal)).ToString("N0");

        }
        // TODO: (low priority)
        public int RetrieveUnapprovedPurchaseOrderCount()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchaseOrder
                        where i.Status == (int)MPL.DocumentStatus.New
                        select i;

            return query.Count();
        }
        // TODO: (low priority)
        public int RetrieveUnpaidPOCount()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchaseOrder
                        where i.Status == (int)MPL.DocumentStatus.Approved
                        select i;

            return query.Count();
        }

        public List<PurchaseOrderModel> RetrievePurchaseOrderAutoComplete(string key)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchaseOrder
                        where i.Code.ToLower().Contains(key.ToLower()) && i.Status == (int)MPL.DocumentStatus.Approved
                        select i;

            return ObjectHelper.CopyList<v_PurchaseOrder, PurchaseOrderModel>(query.Take(SystemConstants.AutoCompleteItemCount).ToList());
        }

        public PurchaseOrderModel RetrievePurchaseOrderByCode(string purchaseCode)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchaseOrder
                        where i.Code == purchaseCode
                        select i;

            var queryList = from j in ent.v_PurchaseOrderDetail
                            where j.PurchaseOrderID == query.FirstOrDefault().ID
                            select j;

            var obj = new PurchaseOrderModel();

            if (query.FirstOrDefault() != null)
            {
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
                ObjectHelper.CopyProperties(queryList.ToList(), obj.Details);
            }
            else
                return null;

            return obj;
        }

        public List<PurchaseOrderModel> RetrievePurchaseOrder(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters, bool showVoidDocuments)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchaseOrder
                        select i;

            if (!showVoidDocuments)
                query = from i in ent.v_PurchaseOrder
                        where i.Status > 0
                        select i;

            ApplySorting<v_PurchaseOrder>(ref query, "Date DESC", sortParameter);
            ApplyFilter<v_PurchaseOrder>(ref query, selectFilters);

            return ObjectHelper.CopyList<v_PurchaseOrder, PurchaseOrderModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public int RetrievePurchaseOrderCount(List<SelectFilter> selectFilters, bool showVoidDocuments)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchaseOrder
                        select i;

            if (!showVoidDocuments)
                query = from i in ent.v_PurchaseOrder
                        where i.Status > 0
                        select i;

            ApplyFilter<v_PurchaseOrder>(ref query, selectFilters);

            return query.Count();
        }

        public int RetrieveResiBillCount(List<SelectFilter> selectFilters, bool showVoidDocuments)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_ResiBill
                        select i;

            if (!showVoidDocuments)
                query = from i in ent.v_ResiBill
                        where i.Status > 0
                        select i;

            ApplyFilter<v_ResiBill>(ref query, selectFilters);

            return query.Count();
        }

        public List<ResiBillModel> RetrieveResiBill(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters, bool showVoidDocuments)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_ResiBill
                        select i;

            if (!showVoidDocuments)
                query = from i in ent.v_ResiBill
                        where i.Status > 0
                        select i;

            ApplySorting<v_ResiBill>(ref query, "Date DESC", sortParameter);
            ApplyFilter<v_ResiBill>(ref query, selectFilters);

            return ObjectHelper.CopyList<v_ResiBill, ResiBillModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public PurchaseOrderDetailModel RetreivePriceBypurchaseOrderID(long purchaseorderID, long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_PurchaseOrderDetail
                        where i.PurchaseOrderID==purchaseorderID && i.ProductID==productID
                        select i;
            var obj = new PurchaseOrderDetailModel();
            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public List<PurchaseOrderDetailModel> RetreiveRelatedPOandPurchaseBill(long purchaseOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_RelatedPOandPurchaseBill
                        where i.PurchaseOrderID == purchaseOrderID
                        select i;

            return ObjectHelper.CopyList<v_RelatedPOandPurchaseBill, PurchaseOrderDetailModel>(query.ToList());
        }

        public List<PurchaseOrderDetailModel> RetreivePurchaseBillByPurchaseOrder(long purchasebillID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_RelatedPOandPurchaseBill
                        where i.PurchaseBillID == purchasebillID
                        select i;

            return ObjectHelper.CopyList<v_RelatedPOandPurchaseBill, PurchaseOrderDetailModel>(query.ToList());
        }
        //for update price from po to purchase bill
        public void DeletePurchasebilldetailbyPbID(long purchasebillID, int ItemNo,long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.PurchaseBillDetail
                        where i.PurchaseBillID == purchasebillID && i.ItemNo==ItemNo && i.ProductID==productID
                        select i;
            foreach (var obj in query.AsEnumerable())
                ent.DeleteObject(obj);

            ent.SaveChanges();
        }

        public void CreatePurchasebillDetail(PurchaseBillDetailModel Pbd)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            PurchaseBillDetail obj = new PurchaseBillDetail();
            ObjectHelper.CopyProperties(Pbd, obj);
            ent.AddToPurchaseBillDetail(obj);
            ent.SaveChanges();

            Pbd.PurchaseBillID = obj.PurchaseBillID;
        }

        public int RetreiveCountRelatedBuildByPO(long purchaseOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_RelatedBuildByPO
                        where i.purchaseorderID == purchaseOrderID
                        select i;
            return query.Count();
            
        }

        public PurchaseOrderDetailModel RetreiveBuildByPO(long purchaseOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_RelatedBuildByPO
                        where i.purchaseorderID == purchaseOrderID
                        select i;

            var obj = new PurchaseOrderDetailModel();
            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public List<PurchaseOrderDetailModel> RetreiveDetailBuildByPO(long assemblyBuildID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_RelatedBuildByPO
                        where i.AssemblyBuildID == assemblyBuildID
                        select i;
            return ObjectHelper.CopyList<v_RelatedBuildByPO, PurchaseOrderDetailModel>(query.ToList());
        }

        #endregion

        #region Purchase Payment

        public string RetrievePurchasePaymentMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.PurchasePayment
                        where i.Code.StartsWith(prefix) && i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "000000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public List<PurchasePaymentModel> RetrievePurchasePaymentByBill(long purchaseBillID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.PurchasePayment
                        where i.PurchaseBillID == purchaseBillID
                        select i;

            return ObjectHelper.CopyList<PurchasePayment, PurchasePaymentModel>(query.ToList());
        }

        public List<PurchaseBillModel> RetrieveUncreatedPurchasePaymentBill(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchaseBill
                        where (i.Amount + i.TaxAmount) - i.PaymentAmount - i.CreditedAmount > 0 && i.Status >= (int)PurchaseBillStatus.New
                        select i;

            ApplySorting<v_PurchaseBill>(ref query, "Code DESC", sortParameter);
            ApplyFilter<v_PurchaseBill>(ref query, selectFilters);

            return ObjectHelper.CopyList<v_PurchaseBill, PurchaseBillModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public int RetrieveUncreatedPurchasePaymentBillCount(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchaseBill
                        where (i.Amount + i.TaxAmount) - i.PaymentAmount - i.CreditedAmount > 0 && i.Status >= (int)PurchaseBillStatus.New
                        select i;

            ApplyFilter<v_PurchaseBill>(ref query, selectFilters);

            return query.Count();
        }


        public int RetrieveUnapprovedPurchasePaymentCount()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchasePayment
                        where SqlFunctions.DatePart("month", i.Date) == MPL.Configuration.CurrentDateTime.Month
                        && SqlFunctions.DatePart("year", i.Date) == MPL.Configuration.CurrentDateTime.Year//i.Status == (int)MPL.DocumentStatus.New 
                        select i;

            return query.Count();
        }

        public int RetrieveVoidPurchasePaymentCount()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchasePayment
                        where i.Status == (int)MPL.DocumentStatus.Void && SqlFunctions.DatePart("month", i.Date) == MPL.Configuration.CurrentDateTime.Month
                        && SqlFunctions.DatePart("year", i.Date) == MPL.Configuration.CurrentDateTime.Year
                        select i;

            return query.Count();
        }

        public MakeMultiPayModel RetreiveMakeMultiPayByCode(string Code)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_MakeMultiPay
                        where i.Code == Code && i.Status > 0
                        select i;
            var obj = new MakeMultiPayModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        #endregion

        #region Purchase Return

        public string RetrievePurchaseReturnMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.PurchaseReturn
                        where i.Code.StartsWith(prefix) && i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "0000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public int RetrieveUncreatedPRPOCount(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchaseOrder
                        where i.HasPR == false && i.Status >= (int)MPL.DocumentStatus.Approved
                        select i;

            ApplyFilter<v_PurchaseOrder>(ref query, selectFilters);
            return query.Count();
        }

        public List<PurchaseOrderModel> RetrieveUncreatedPRPO(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchaseOrder
                        where i.HasPR == false && i.Status >= (int)MPL.DocumentStatus.Approved
                        select i;

            ApplySorting<v_PurchaseOrder>(ref query, "Code DESC", sortParameter);
            ApplyFilter<v_PurchaseOrder>(ref query, selectFilters);

            return ObjectHelper.CopyList<v_PurchaseOrder, PurchaseOrderModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public List<PurchaseReturnDetailModel> RetrievePurchaseReturnDetailByPRID(long prID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchaseReturnDetail
                        where i.PurchaseReturnID == prID && i.Status != (int)PurchaseDeliveryStatus.Void
                        select i;

            return ObjectHelper.CopyList<v_PurchaseReturnDetail, PurchaseReturnDetailModel>(query.ToList());
        }

        #endregion

        #region Rate

        public decimal RetrieveRate()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Rate
                        where i.ID == 1
                        select i;

            return Convert.ToDecimal(query.FirstOrDefault().Value);
        }

        public void UpdateRate(RateModel rate)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Rate
                        select i;

            var obj = query.FirstOrDefault();

            ObjectHelper.CopyProperties(rate, obj);
            ent.SaveChanges();
        }

        public List<RateModel> RetrieveRate(bool isActive)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Rate
                        where i.IsActive == isActive
                        select i;

            return ObjectHelper.CopyList<Rate, RateModel>(query.ToList());
        }
        #endregion

        #region Return Receipt
        public string RetrieveReturnReceiptMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.ReturnReceipt
                        where i.Code.StartsWith(prefix) && i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "0000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public List<ReturnReceiptDetailModel> RetrieveReturnReceiptDetailByCustomerReturnID(long poID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_ReturnReceiptDetail
                        from j in ent.v_ReturnReceipt
                        where i.ReturnReceiptID == j.ID && j.CustomerReturnID == poID && j.Status != (int)MPL.DocumentStatus.Void
                        select i;

            return ObjectHelper.CopyList<v_ReturnReceiptDetail, ReturnReceiptDetailModel>(query.ToList());
        }

        public List<ReturnReceiptModel> RetrieveReturnReceiptByCustomerReturnID(long purchaseOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from j in ent.v_ReturnReceipt
                        where j.CustomerReturnID == purchaseOrderID && j.Status != (int)MPL.DocumentStatus.Void
                        select j;

            return ObjectHelper.CopyList<v_ReturnReceipt, ReturnReceiptModel>(query.ToList());
        }

        #endregion

        #region Resi
        public ResiModel RetrieveResiCode(string resiCode)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.Resi
                        where i.Code == resiCode && i.Status != (int)ResiStatus.Void
                        select i;
            var obj = new ResiModel();
            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;
            return obj;
        }

        public List<ResiModel> RetrieveResiByCustomerID(long customerID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_Resi
                        where i.Status == (int)ResiStatus.PendingBilling && !i.IsHasInvoice && !i.IsCoverExpeditionByABCA
                        select i;
            return ObjectHelper.CopyList<v_Resi, ResiModel>(query.ToList());
        }
        #endregion

        #region Resi bill
        public List<ResiBillModel> RetrieveUncreatedResiPayment(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_ResiBill
                        where i.Status == (int)MPL.DocumentStatus.Approved //&& i.IsCoverExpeditionByABCA
                        select i;
            ApplySorting<v_ResiBill>(ref query, "DATE DESC", sortParameter);
            ApplyFilter<v_ResiBill>(ref query, selectFilters);
            return ObjectHelper.CopyList<v_ResiBill, ResiBillModel>(query.Skip(startIndex).Take(amount).ToList());
        }
        public int RetrieveUncreatedResiPaymentCount(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_ResiBill
                        where i.Status == (int)MPL.DocumentStatus.Approved
                        select i;

            ApplyFilter<v_ResiBill>(ref query, selectFilters);
            return query.Count();
        }
        public List<ResiBillModel> RetrieveResiBillByExpeditionID(long expeditionID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_ResiBill
                        where i.ExpeditionID == expeditionID && i.Status == (int)MPL.DocumentStatus.Approved && i.TotalAmount > i.PaymentAmount
                        select i;

            return ObjectHelper.CopyList<v_ResiBill, ResiBillModel>(query.OrderByDescending(p => p.Date).ToList());
        }
        public List<ResiBillDetailModel> RetrieveResiBillDetailByResiID(long resiID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_ResiBillDetail
                        where i.ResiID == resiID
                        select i;
            return ObjectHelper.CopyList<v_ResiBillDetail, ResiBillDetailModel>(query.ToList());
        }
        #endregion

        #region Resi Payment
        public ResiPaymentModel RetrieveResiPaymentCode(string resipaymentCode)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.ResiPayment
                        where i.Code == resipaymentCode && i.Status != (int)ResiStatus.Void
                        select i;
            var obj = new ResiPaymentModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;
            return obj;
        }

        public List<ResiPaymentDetailModel> RetrieveResiPaymentDetailByResiBillID(long resiBillID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_ResiPaymentDetail
                        where i.ResiBillID == resiBillID
                        select i;
            return ObjectHelper.CopyList<v_ResiPaymentDetail, ResiPaymentDetailModel>(query.ToList());
        }

        //public List<ResiPaymentDetailModel> RetrieveResiPaymentByIsCover(long headerID, bool isCover)
        //{
        //    ABCAPOSEntities ent = new ABCAPOSEntities();
        //    var query = from i in ent.v_ResiPaymentDetail
        //                where i.HeaderID == headerID && i.IsCoverExpeditionByABCA == isCover
        //                select i;
        //    return ObjectHelper.CopyList<v_ResiPaymentDetail, ResiPaymentDetailModel>(query.ToList());
        //}
        #endregion

        #region Role
        public void CreateRole(RoleModel role)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            Role obj = new Role();
            ObjectHelper.CopyProperties(role, obj);
            ent.AddToRole(obj);
            ent.SaveChanges();

            role.ID = obj.ID;
        }

        public void CreateRoleDetails(int roleID, List<RoleDetailModel> roleDetailList)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            foreach (var roleDetail in roleDetailList)
            {
                roleDetail.RoleID = roleID;

                RoleDetail obj = new RoleDetail();
                ObjectHelper.CopyProperties(roleDetail, obj);
                ent.AddToRoleDetail(obj);
            }

            ent.SaveChanges();
        }

        public void DeleteRoleDetails(long roleID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.RoleDetail
                        where i.RoleID == roleID
                        select i;

            foreach (var obj in query.AsEnumerable())
                ent.DeleteObject(obj);

            ent.SaveChanges();
        }

        public List<RoleModel> RetrieveRole(bool isActive)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Role
                        where i.IsActive == isActive
                        select i;

            return ObjectHelper.CopyList<Role, RoleModel>(query.ToList());
        }

        public List<RoleModel> RetreiveRoleAdministrator(bool isActive)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.Role
                        where i.IsActive == isActive && i.ID != (int)PermissionStatus.production && i.ID != (int)PermissionStatus.root
                        select i;
            return ObjectHelper.CopyList<Role, RoleModel>(query.ToList());
        }

        public List<RoleModel> RetreiveListRoleAdministrator(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.Role
                        where i.IsActive == true && i.ID != (int)PermissionStatus.production && i.ID != (int)PermissionStatus.root
                        select i;

            ApplySorting<Role>(ref query, "ID", sortParameter);
            ApplyFilter<Role>(ref query, selectFilters);
            return ObjectHelper.CopyList<Role, RoleModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public List<RoleModel> RetreiveListRoleRoot(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.Role
                        where i.IsActive == true
                        select i;

            ApplySorting<Role>(ref query, "ID", sortParameter);
            ApplyFilter<Role>(ref query, selectFilters);
            return ObjectHelper.CopyList<Role, RoleModel>(query.Skip(startIndex).Take(amount).ToList());
        }
        public List<RoleDetailModel> RetrieveRoleDetails(int roleID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.RoleDetail
                        where i.RoleID == roleID
                        select i;

            return ObjectHelper.CopyList<RoleDetail, RoleDetailModel>(query.ToList());
        }

        public List<RoleDetailModel> RetrieveRoleActions(string moduleID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.RoleDetail
                        where i.ModuleID == moduleID
                        select i;

            return ObjectHelper.CopyList<RoleDetail, RoleDetailModel>(query.ToList());
        }

        public List<string> RetrieveRoleActions(int roleID, string moduleID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.RoleDetail
                        where i.RoleID == roleID && i.ModuleID == moduleID
                        select i.Action;

            return query.ToList();
        }
        #endregion

        #region Salesman

        public string RetrieveSalesmanMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Salesman
                        where i.Code.StartsWith(prefix)
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "0000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public List<SalesmanModel> RetrieveSalesman(bool isActive)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Salesman
                        where i.IsActive == isActive
                        select i;

            return ObjectHelper.CopyList<Salesman, SalesmanModel>(query.ToList());
        }

        #endregion

        #region Sales Order

        public string RetrieveSalesOrderMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.SalesOrder
                        where i.Code.StartsWith(prefix) //&& i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "000000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public List<SalesOrderModel> RetrieveUncreatedDeliveryOrderSalesOrder(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_SalesOrder
                        where i.IsDeliveryOrderFulfilled == false && i.Status == (int)MPL.DocumentStatus.Approved
                        && i.IsDeliverable == true
                        select i;

            ApplySorting<v_SalesOrder>(ref query, "Code DESC", sortParameter);
            ApplyFilter<v_SalesOrder>(ref query, selectFilters);

            return ObjectHelper.CopyList<v_SalesOrder, SalesOrderModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public int RetrieveUncreatedDeliveryOrderSalesOrderCount(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_SalesOrder
                        where i.IsDeliveryOrderFulfilled == false && i.Status == (int)MPL.DocumentStatus.Approved
                        && i.IsDeliverable == true
                        select i;

            ApplyFilter<v_SalesOrder>(ref query, selectFilters);
            return query.Count();
        }

        public int RetrieveVoidSalesOrderCount(CustomerGroupModel customerGroup)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_SalesOrder
                        where i.Status == (int)MPL.DocumentStatus.Void && SqlFunctions.DatePart("month", i.Date) == MPL.Configuration.CurrentDateTime.Month
                        && SqlFunctions.DatePart("year", i.Date) == MPL.Configuration.CurrentDateTime.Year
                        select i;

            return query.Count();
        }

        public int RetrieveUnapprovedSalesOrderCount(CustomerGroupModel customerGroup)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_SalesOrder
                        where i.Status == (int)MPL.DocumentStatus.New
                        select i;

            return query.Count();
        }

        public int RetrieveUncreatedDeliveryOrderSalesOrderCount(CustomerGroupModel customerGroup)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_SalesOrder
                        where i.IsDeliveryOrderFulfilled == false && i.Status != (int)MPL.DocumentStatus.Void
                        && i.IsDeliverable == true
                        select i;

            return query.Count();
        }

        public int RetrieveUnfulfillDeliveryOrderCount()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_SalesOrder
                        where i.IsDeliverable == true && i.Status >= 3
                        select i;

            return query.Count();
        }

        public SalesOrderModel RetrieveSalesByCode(string salesCode)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_SalesOrder
                        where i.Code == salesCode
                        select i;

            var obj = new SalesOrderModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public List<DeliveryOrderModel> RetrieveDOBySOID(long salesOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from j in ent.v_DeliveryOrder
                        where j.SalesOrderID == salesOrderID && j.Status != (int)MPL.DocumentStatus.Void
                        select j;

            return ObjectHelper.CopyList<v_DeliveryOrder, DeliveryOrderModel>(query.ToList());
        }

        public List<SalesOrderModel> RetreiveByCustomerID(long customerID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_SalesOrder
                        where i.CustomerID == customerID && i.Status > 0
                        select i;
            return ObjectHelper.CopyList<v_SalesOrder, SalesOrderModel>(query.OrderByDescending(p => p.Date).Take(SystemConstants.ItemPerPage).ToList());
        }
        #endregion

        #region Staff

        public List<StaffModel> RetrieveStaff(bool isActive)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Staff
                        where i.IsActive == isActive
                        select i;

            return ObjectHelper.CopyList<Staff, StaffModel>(query.ToList());
        }

        public List<StaffModel> RetrieveStaff(StaffType staffType, bool isActive)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            string strStaffType = staffType.ToString();

            var query = from i in ent.Staff
                        where i.IsActive == isActive && i.JobTitle == strStaffType
                        select i;

            return ObjectHelper.CopyList<Staff, StaffModel>(query.ToList());
        }

        public List<StaffModel> RetrieveStaff(string jobTitle, bool isActive)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Staff
                        where i.IsActive == isActive && i.JobTitle == jobTitle
                        select i;

            return ObjectHelper.CopyList<Staff, StaffModel>(query.ToList());
        }

        public List<StaffModel> RetrieveEmployeeAutoComplete(string key)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Staff
                        where i.Name.ToLower().Contains(key.ToLower()) && i.IsActive == true
                        select i;

            return ObjectHelper.CopyList<Staff, StaffModel>(query.Take(SystemConstants.AutoCompleteItemCount).ToList());
        }

        public List<StaffModel> RetrieveEmployeeAutoComplete(string key, bool isSalesRep)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Staff
                        where i.Name.ToLower().Contains(key.ToLower()) && i.IsSalesRep == isSalesRep && i.IsActive == true
                        select i;

            return ObjectHelper.CopyList<Staff, StaffModel>(query.Take(SystemConstants.AutoCompleteItemCount).ToList());
        }

        public List<StaffModel> RetrieveEmployeeAutoCompleteByJobTitle(string key, string jobTitle)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Staff
                        where i.Name.ToLower().Contains(key.ToLower()) && i.JobTitle == jobTitle
                        select i;

            return ObjectHelper.CopyList<Staff, StaffModel>(query.Take(SystemConstants.AutoCompleteItemCount).ToList());
        }

        public StaffModel RetrieveEmployeeByCode(string employeeName)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Staff
                        where i.Name == employeeName
                        select i;

            var obj = new StaffModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public StaffModel RetrieveEmployeeByUserName(string userName)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Staff
                        where i.UserName == userName
                        select i;

            var obj = new StaffModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public List<string> RetrieveEmployeeUserNames()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Staff
                        where i.UserName != null && i.IsActive == true
                        select i.UserName;

            return query.ToList();

        }
        #endregion

        #region Stock Adjustment

        public string RetrieveInventoryAdjustmentMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.InventoryAdjustment
                        where i.Code.StartsWith(prefix) //&& i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "000000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public void CreateSADetails(long masterID, List<InventoryAdjustmentDetailModel> details)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            foreach (var detail in details)
            {
                detail.InventoryAdjustmentID = masterID;
                InventoryAdjustmentDetail obj = new InventoryAdjustmentDetail();
                ObjectHelper.CopyProperties(detail, obj);
                ent.AddToInventoryAdjustmentDetail(obj);
            }

            ent.SaveChanges();
        }


        #endregion

        #region Terms Of Payment

        public List<TermsOfPaymentModel> RetrieveAllTerms()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.TermsOfPayment
                        select i;

            return ObjectHelper.CopyList<TermsOfPayment, TermsOfPaymentModel>(query.ToList());
        }

        #endregion

        #region Transfer Order
        public string RetrieveTransferOrderMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.TransferOrder
                        where i.Code.StartsWith(prefix) && i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "000000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public void CreateTODetails(long toID, List<TransferOrderDetailModel> toDetails)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            foreach (var toDetail in toDetails)
            {
                toDetail.TransferOrderID = toID;
                var itemNo = toDetail.ItemNo;
                toDetail.ItemNo = itemNo;

                TransferOrderDetail obj = new TransferOrderDetail();
                ObjectHelper.CopyProperties(toDetail, obj);
                ent.AddToTransferOrderDetail(obj);
            }

            ent.SaveChanges();
        }

        public List<TransferOrderModel> RetrieveUncreatedTransferDeliveryTO(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_TransferOrder
                        where i.HasDO == false && i.Status == (int)TransferOrderStatus.PendingFulfillment
                        select i;

            ApplySorting<v_TransferOrder>(ref query, "Code DESC", sortParameter);
            ApplyFilter<v_TransferOrder>(ref query, selectFilters);

            return ObjectHelper.CopyList<v_TransferOrder, TransferOrderModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public int RetrieveUncreatedTransferDeliveryTOCount(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_TransferOrder
                        where i.HasDO == false && i.Status == (int)TransferOrderStatus.PendingFulfillment
                        select i;

            ApplyFilter<v_TransferOrder>(ref query, selectFilters);

            return query.Count();
        }

        public List<TransferOrderModel> RetrieveUncreatedTransferReceiptTO(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_TransferOrder
                        where i.IsReceived == false && i.Status >= (int)TransferOrderStatus.PendingReceipt
                        select i;

            ApplySorting<v_TransferOrder>(ref query, "Code DESC", sortParameter);
            ApplyFilter<v_TransferOrder>(ref query, selectFilters);

            return ObjectHelper.CopyList<v_TransferOrder, TransferOrderModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public int RetrieveUncreatedTransferReceiptTOCount(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_TransferOrder
                        where i.IsReceived == false && i.Status >= (int)TransferOrderStatus.PendingReceipt
                        select i;

            ApplyFilter<v_TransferOrder>(ref query, selectFilters);

            return query.Count();
        }

        public List<TransferOrderModel> RetrieveTransferOrderAutoComplete(string key)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_TransferOrder
                        where i.Code.ToLower().Contains(key.ToLower())
                        select i;

            return ObjectHelper.CopyList<v_TransferOrder, TransferOrderModel>(query.Take(SystemConstants.AutoCompleteItemCount).ToList());
        }

        public TransferOrderModel RetrieveTransferOrderByCode(string transferCode)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_TransferOrder
                        where i.Code == transferCode
                        select i;

            var queryList = from j in ent.v_TransferOrderDetail
                            where j.TransferOrderID == query.FirstOrDefault().ID
                            select j;

            var obj = new TransferOrderModel();

            if (query.FirstOrDefault() != null)
            {
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
                ObjectHelper.CopyProperties(queryList.ToList(), obj.Details);
            }
            else
                return null;

            return obj;
        }

        public TransferOrderDetailModel RetreiveQtyReceivedTransferOrder(long transferOrderID, long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_TransferOrderDetail
                        where i.TransferOrderID == transferOrderID && i.ProductID == productID
                        select i;
            var obj = new TransferOrderDetailModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public int RetrieveTransferOrderUnDelivery()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_TransferOrder
                        where i.QtyOrdered - i.QtyDelivered > 0
                        select i;

            return query.Count();
        }

        public List<TransferOrderModel> RetrieveTransferOrder(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_TransferOrder
                        where i.Status != (int)MPL.DocumentStatus.Void
                        select i;
            ApplySorting<v_TransferOrder>(ref query, "DATE DESC", sortParameter);
            ApplyFilter<v_TransferOrder>(ref query, selectFilters);
            return ObjectHelper.CopyList<v_TransferOrder, TransferOrderModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public int RetrieveTransferOrderCount(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_TransferOrder
                        where i.Status != (int)MPL.DocumentStatus.Void
                        select i;

            ApplyFilter<v_TransferOrder>(ref query, selectFilters);
            return query.Count();
        }

        #endregion

        #region Transfer Delivery

        public string RetrieveTransferDeliveryMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.TransferDelivery
                        where i.Code.StartsWith(prefix) //&& i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "000000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public List<TransferDeliveryModel> RetrieveTransferDeliveryByTransferOrderID(long transferOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from j in ent.TransferDelivery
                        where j.TransferOrderID == transferOrderID && j.Status != (int)DeliveryOrderStatus.Void
                        select j;

            return ObjectHelper.CopyList<TransferDelivery, TransferDeliveryModel>(query.ToList());
        }

        public List<TransferDeliveryModel> RetrieveTransferDeliveryByTransferOrderID(long transferOrderID, DeliveryOrderStatus status)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from j in ent.TransferDelivery
                        where j.TransferOrderID == transferOrderID && j.Status == (int)status
                        select j;

            return ObjectHelper.CopyList<TransferDelivery, TransferDeliveryModel>(query.ToList());
        }

        public List<TransferDeliveryDetailModel> RetrieveTransferDeliveryDetailByTransferOrderID(long transferOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.TransferDeliveryDetail
                        from j in ent.TransferDelivery
                        where i.TransferDeliveryID == j.ID && j.TransferOrderID == transferOrderID && j.Status != (int)MPL.DocumentStatus.Void
                        select i;

            return ObjectHelper.CopyList<TransferDeliveryDetail, TransferDeliveryDetailModel>(query.ToList());
        }

        //public int RetrieveUnapprovedTransferDeliveryCount(CustomerGroupModel customerGroup)
        //{
        //    ABCAPOSEntities ent = new ABCAPOSEntities();

        //    var query = from i in ent.v_DeliveryOrder
        //                where i.Status == (int)MPL.DocumentStatus.New && i.CustomerGroupID == customerGroup.ID
        //                select i;

        //    return query.Count();
        //}

        //public int RetrieveVoidDeliveryOrderCount(CustomerGroupModel customerGroup)
        //{
        //    ABCAPOSEntities ent = new ABCAPOSEntities();

        //    var query = from i in ent.v_DeliveryOrder
        //                where i.Status == (int)MPL.DocumentStatus.Void && SqlFunctions.DatePart("month", i.Date) == MPL.Configuration.CurrentDateTime.Month
        //                && SqlFunctions.DatePart("year", i.Date) == MPL.Configuration.CurrentDateTime.Year && i.CustomerGroupID == customerGroup.ID
        //                select i;

        //    return query.Count();
        //}

        #endregion

        #region Transfer Receipt

        public string RetrieveTransferReceiptMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.TransferReceipt
                        where i.Code.StartsWith(prefix) //&& i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "000000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public List<TransferReceiptModel> RetrieveTransferReceiptByTransferOrderID(long transferOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from j in ent.TransferReceipt
                        where j.TransferOrderID == transferOrderID && j.Status != (int)DeliveryOrderStatus.Void
                        select j;

            return ObjectHelper.CopyList<TransferReceipt, TransferReceiptModel>(query.ToList());
        }

        public List<TransferReceiptDetailModel> RetrieveTransferReceiptDetailByTransferOrderID(long transferOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.TransferReceiptDetail
                        from j in ent.TransferReceipt
                        where i.TransferReceiptID == j.ID && j.TransferOrderID == transferOrderID && j.Status != (int)MPL.DocumentStatus.Void
                        select i;

            return ObjectHelper.CopyList<TransferReceiptDetail, TransferReceiptDetailModel>(query.ToList());
        }

        public TransferOrderDetailModel RetrieveQtyReceivedTransferReceipt(long transferOrderID,int itemNo, long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_TransferOrderDetail
                        where i.TransferOrderID == transferOrderID && i.ItemNo == itemNo && i.ProductID == productID
                        select i;

            var obj = new TransferOrderDetailModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        //public int RetrieveUnapprovedTransferReceiptCount(CustomerGroupModel customerGroup)
        //{
        //    ABCAPOSEntities ent = new ABCAPOSEntities();

        //    var query = from i in ent.v_DeliveryOrder
        //                where i.Status == (int)MPL.DocumentStatus.New && i.CustomerGroupID == customerGroup.ID
        //                select i;

        //    return query.Count();
        //}

        //public int RetrieveVoidDeliveryOrderCount(CustomerGroupModel customerGroup)
        //{
        //    ABCAPOSEntities ent = new ABCAPOSEntities();

        //    var query = from i in ent.v_DeliveryOrder
        //                where i.Status == (int)MPL.DocumentStatus.Void && SqlFunctions.DatePart("month", i.Date) == MPL.Configuration.CurrentDateTime.Month
        //                && SqlFunctions.DatePart("year", i.Date) == MPL.Configuration.CurrentDateTime.Year && i.CustomerGroupID == customerGroup.ID
        //                select i;

        //    return query.Count();
        //}

        #endregion

        #region Vendor

        public string RetrieveVendorMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Vendor
                        where i.Code.StartsWith(prefix)
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "0000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public List<VendorModel> RetrieveVendorAutoComplete(string key)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Vendor
                        where (i.Name.ToLower().Contains(key.ToLower()) ||
                               i.Code.ToLower().Contains(key.ToLower()))
                        select i;

            ApplySorting<Vendor>(ref query, "Code", "");
            return ObjectHelper.CopyList<Vendor, VendorModel>(query.Take(SystemConstants.AutoCompleteItemCount).ToList());
        }

        public VendorModel RetrieveVendorByCode(string vendorCode)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Vendor
                        where i.Code == vendorCode || i.Name == vendorCode
                        select i;

            var obj = new VendorModel();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }
        #endregion

        #region Vendor Return

        public List<VendorReturnModel> RetrieveUncreatedVendorReturnDelivery(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_VendorReturn
                        where i.IsDeliveryFulfilled == false && i.Status == (int)MPL.DocumentStatus.Approved
                        && i.IsDeliverable == true
                        select i;

            ApplySorting<v_VendorReturn>(ref query, "Code DESC", sortParameter);
            ApplyFilter<v_VendorReturn>(ref query, selectFilters);

            return ObjectHelper.CopyList<v_VendorReturn, VendorReturnModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public int RetrieveUncreatedVendorReturnDeliveryCount(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_VendorReturn
                        where i.IsDeliveryFulfilled == false && i.Status == (int)MPL.DocumentStatus.Approved
                        && i.IsDeliverable == true
                        select i;

            ApplyFilter<v_VendorReturn>(ref query, selectFilters);
            return query.Count();
        }

        public string RetrieveVendorReturnMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.VendorReturn
                        where i.Code.StartsWith(prefix) //&& i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "000000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public void CreateVendorReturnDetails(long poID, List<VendorReturnDetailModel> poDetails)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            foreach (var poDetail in poDetails)
            {
                poDetail.VendorReturnID = poID;
                var itemNo = poDetail.ItemNo;
                poDetail.ItemNo = itemNo;

                VendorReturnDetail obj = new VendorReturnDetail();
                ObjectHelper.CopyProperties(poDetail, obj);
                ent.AddToVendorReturnDetail(obj);
            }

            ent.SaveChanges();
        }


        #endregion

        #region Vendor Return Delivery
        public string RetrieveVendorReturnDeliveryMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.VendorReturnDelivery
                        where i.Code.StartsWith(prefix) && i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "000000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public List<VendorReturnDeliveryModel> RetrieveVendorReturnDeliveryByVendorReturnID(long vendorReturnID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from j in ent.v_VendorReturnDelivery
                        where j.VendorReturnID == vendorReturnID && j.Status != (int)DeliveryOrderStatus.Void
                        select j;

            return ObjectHelper.CopyList<v_VendorReturnDelivery, VendorReturnDeliveryModel>(query.ToList());
        }

        public List<VendorReturnDeliveryModel> RetrieveVendorReturnDeliveryByVendorReturnID(long vendorReturnID, DeliveryOrderStatus deliveryStatus)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from j in ent.v_VendorReturnDelivery
                        where j.VendorReturnID == vendorReturnID && j.Status == (int)deliveryStatus
                        select j;

            return ObjectHelper.CopyList<v_VendorReturnDelivery, VendorReturnDeliveryModel>(query.ToList());
        }

        public List<VendorReturnDeliveryDetailModel> RetrieveVendorReturnDeliveryDetailByVendorReturnID(long vendorReturnID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.VendorReturnDeliveryDetail
                        from j in ent.VendorReturnDelivery
                        where i.VendorReturnDeliveryID == j.ID && j.VendorReturnID == vendorReturnID && j.Status != (int)MPL.DocumentStatus.Void
                        select i;

            return ObjectHelper.CopyList<VendorReturnDeliveryDetail, VendorReturnDeliveryDetailModel>(query.ToList());
        }

        #endregion

        #region Warehouse

        public string RetrieveWarehouseMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Warehouse
                        where i.Code.StartsWith(prefix) && i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "0000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;

            return maxCode;
        }

        public List<WarehouseModel> RetrieveWarehouse(bool isActive)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Warehouse
                        where i.IsActive == isActive
                        select i;

            return ObjectHelper.CopyList<Warehouse, WarehouseModel>(query.ToList());
        }

        #endregion

        #region WorkOrder
        /* penambahan by tiar for manufactur */
        public string RetreiveWorkOrderMaxCode(string prefix, int length)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.WorkOrder
                        where i.Code.StartsWith(prefix) && i.Code.Length == prefix.Length + length
                        select i.Code;

            var extMaxCode = query.Max();

            var maxCode = prefix;
            var code = 1;

            if (!string.IsNullOrEmpty(extMaxCode))
            {
                extMaxCode = extMaxCode.ToString().Replace(prefix, "");
                int.TryParse(extMaxCode, out code);

                code++;
            }

            var numericContent = "000000" + code.ToString();
            numericContent = numericContent.Substring(numericContent.Length - length);

            maxCode += numericContent;
            return maxCode;
        }

        public int RetreiveListPendingWorkOrderCount(List<SelectFilter> SelectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_IsAbleListWorkOrder
                        select i;

            ApplyFilter<v_IsAbleListWorkOrder>(ref query, SelectFilters);
            return query.Count();

        }

        public int RetreiveListPendingWorkOrderCountFinishGood(List<SelectFilter> SelectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_IsAbleListWorkOrder
                        where i.ItemTypeID == 4
                        select i;

            ApplyFilter<v_IsAbleListWorkOrder>(ref query, SelectFilters);
            return query.Count();
        }

        public List<ProductModel> RetrieveListPendingWorkOrder(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_IsAbleListWorkOrder
                        select i;

            ApplySorting<v_IsAbleListWorkOrder>(ref query, "Presentase", sortParameter);
            ApplyFilter<v_IsAbleListWorkOrder>(ref query, selectFilters);
            return ObjectHelper.CopyList<v_IsAbleListWorkOrder, ProductModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public List<ProductModel> RetreiveListPendingWorkOrderFinishGood(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_IsAbleListWorkOrder
                        where i.ItemTypeID == 4
                        select i;

            ApplySorting<v_IsAbleListWorkOrder>(ref query, "Presentase", sortParameter);
            ApplyFilter<v_IsAbleListWorkOrder>(ref query, selectFilters);
            return ObjectHelper.CopyList<v_IsAbleListWorkOrder, ProductModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public int RetreiveWorkOrderCount(List<SelectFilter> selectFilters, bool showVoidDocuments)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_WorkOrder
                        select i;

            if (!showVoidDocuments)
                query = from i in ent.v_WorkOrder
                        where i.Status == 3
                        select i;

            ApplyFilter<v_WorkOrder>(ref query, selectFilters);
            return query.Count();
        }

        public int RetreiveWorkOrderCountFG(List<SelectFilter> selectFilters, bool showVoidDocuments)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_WorkOrder
                        where i.ItemTypeID == 4
                        select i;

            if (!showVoidDocuments)
                query = from i in ent.v_WorkOrder
                        where i.Status == 3 && i.ItemTypeID == 4
                        select i;

            ApplyFilter<v_WorkOrder>(ref query, selectFilters);
            return query.Count();
        }

        public List<WorkOrderModel> RetreiveWorkOrder(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters, bool showVoidDocuments)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_WorkOrder
                        select i;

            if (!showVoidDocuments)
                query = from i in ent.v_WorkOrder
                        where i.Status == 3
                        select i;

            ApplySorting<v_WorkOrder>(ref query, "DATE DESC", sortParameter);
            ApplyFilter<v_WorkOrder>(ref query, selectFilters);
            return ObjectHelper.CopyList<v_WorkOrder, WorkOrderModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public List<WorkOrderModel> RetreiveWorkOrderFG(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters, bool showVoidDocuments)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_WorkOrder
                        where i.ItemTypeID == 4
                        select i;

            if (!showVoidDocuments)
                query = from i in ent.v_WorkOrder
                        where i.Status == 3 && i.ItemTypeID == 4
                        select i;


            ApplySorting<v_WorkOrder>(ref query, "DATE DESC", sortParameter);
            ApplyFilter<v_WorkOrder>(ref query, selectFilters);
            return ObjectHelper.CopyList<v_WorkOrder, WorkOrderModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        public WorkOrderDetailModel RetreiveQtyByID(long WorkOrderID,long productDetailID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.WorkOrderDetail
                        where i.WorkOrderID == WorkOrderID && i.ProductDetailID == productDetailID
                        select i;

            var obj = new WorkOrderDetailModel();
            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), obj);
            else
                return null;

            return obj;
        }

        public List<WorkOrderDetailModel> RetreiveVwWorkOrderByWoID(long workOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_WorkOrderDetail
                        where i.WorkOrderID == workOrderID
                        select i;
            return ObjectHelper.CopyList<v_WorkOrderDetail, WorkOrderDetailModel>(query.ToList());
        }

        
        //public List<ResiDetailModel> RetreiveVwResiByID(long ResiID)
        //{
        //    ABCAPOSEntities ent = new ABCAPOSEntities();
        //    var query = from i in ent.v_Resi
        //                where i.ID == ResiID 
        //                select i;
        //return ObjectHelper.CopyList <v_Resi, ResiDetailModel>(query.ToList());
        //}

        public List<WorkOrderModel> RetreiveWorkOrderByproductID(long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_WorkOrder
                        where i.ProductID == productID && i.Status != (int)WorkOrderStatus.Void
                        select i;

            return ObjectHelper.CopyList<v_WorkOrder, WorkOrderModel>(query.OrderByDescending(p => p.Date).ToList());
            //return ObjectHelper.CopyList<v_WorkOrder, WorkOrderModel>(query.OrderByDescending(p => p.Date).Take(SystemConstants.ItemPerPage).ToList());
        }
        /*end*/
        #endregion

       
        #region resi bill

        public List<ResiModel> RetrieveUncreatedResiBillResi(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Resi
                        where i.Status == (int)ResiStatus.PendingBilling && i.IsHasBill == false
                        select i;


            ApplySorting<v_Resi>(ref query, "Code DESC", sortParameter);
            ApplyFilter<v_Resi>(ref query, selectFilters);
            return ObjectHelper.CopyList<v_Resi, ResiModel>(query.Skip(startIndex).Take(amount).ToList());
        }

        

        public int RetrieveUncreatedResiBillResiCount(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Resi
                        where i.Status == (int)ResiStatus.PendingBilling && i.IsHasBill == false 
                        select i;

            ApplyFilter<v_Resi>(ref query, selectFilters);

            return query.Count();
        }

        #endregion
       
    }
}
