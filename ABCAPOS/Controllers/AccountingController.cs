using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.Models;
using MPL.MVC;
using ABCAPOS.BF;
using System.Globalization;
using ABCAPOS.Util;
using ABCAPOS.Helpers;
using System.Transactions;

namespace ABCAPOS.Controllers
{
    public class AccountingController : MasterDetailController<AccountingModel, IncomeAccountingDetailModel>
    {
        private string ModuleID
        {
            get
            {
                return "Accounting";
            }
        }

        private void SetViewBagPermission()
        {
            var roleDetails = new RoleBFC().RetrieveActions(MembershipHelper.GetRoleID(), ModuleID);

            ViewBag.AllowEdit = roleDetails.Contains("Edit");
            ViewBag.AllowCreate = roleDetails.Contains("Create");
        }

        public override MPL.Business.IMasterDetailBFC<AccountingModel, IncomeAccountingDetailModel> GetBFC()
        {
            return new AccountingBFC();
        }

        protected override void PreCreateDisplay(AccountingModel header, List<IncomeAccountingDetailModel> details)
        {
            header.Code = new AccountingBFC().GetAccountingCode();
            header.Month = DateTime.Now.Month;
            header.Year = DateTime.Now.Year;

            SetPreEditViewBag(header);

            base.PreCreateDisplay(header, details);
        }

        protected override void PreDetailDisplay(AccountingModel header, List<IncomeAccountingDetailModel> details)
        {
            SetViewBagNotification();

            SetViewBagPermission();

            header.Details = header.Details.OrderBy(p => p.Date).ToList();
            details = details.OrderBy(p => p.Date).ToList();
            header.Expenses = new AccountingBFC().RetrieveExpenses(header.ID).OrderBy(p => p.Date).ToList();

            base.PreDetailDisplay(header, details);
        }

        protected override void PreUpdateDisplay(AccountingModel header, List<IncomeAccountingDetailModel> details)
        {
            header.Month = header.Date.Month;
            header.Year = header.Date.Year;
            header.Expenses = new AccountingBFC().RetrieveExpenses(header.ID);

            SetPreEditViewBag(header);

            base.PreUpdateDisplay(header, details);
        }

        public void SetPreEditViewBag(AccountingModel header)
        {
            var template = "";

            template = "<tr><td>Tanggal</td><td>:</td><td><select id='Month' name='Month' >";

            for (int i = 1; i <= 12; i++)
            {
                if (header.Month == i)
                    template += "<option value='" + i + "' selected='true'>";
                else
                    template += "<option value='" + i + "' >";

                template += new IDNumericSayer().SayMonth(i) + "</option>";
            }

            template += "</select><select id='Year' name='Year' >";

            for (int i = 2013; i <= 2100; i++)
            {
                if (header.Year == i)
                    template += "<option value='" + i + "' selected='true'>" + i + "</option>";
                else
                    template += "<option value='" + i + "' >" + i + "</option>";
            }

            template += "</select></td></tr>";

            ViewBag.DateList = template;
        }

        private void SetViewBagNotification()
        {
            if (TempData["SuccessNotification"] != null)
                ViewBag.SuccessNotification = Convert.ToString(TempData["SuccessNotification"]);

            if (!string.IsNullOrEmpty(Request.QueryString["errorMessage"]))
                ViewBag.ErrorNotification = Convert.ToString(Request.QueryString["errorMessage"]);
        }

        private void SetIndexViewBag()
        {
            ViewBag.CustomerGroupList = new CustomerGroupBFC().RetrieveAll();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetIndexViewBag();
            SetViewBagPermission();

            if (startIndex == null)
                startIndex = 0;

            if (amount == null)
                amount = 20;

            if (string.IsNullOrEmpty(sortParameter))
                sortParameter = "";

            var accountingCount = new AccountingBFC().RetrieveCount(filter.GetSelectFilters(), false);
            var accountingList = new AccountingBFC().Retrieve((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters(), false);

            ResetBackToListUrl(filter);

            ViewBag.DataCount = accountingCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            ViewBag.PageSeriesSize = GetPageSeriesSize();

            return View(accountingList);
        }

        public ActionResult CreateAccounting(AccountingModel accounting, List<IncomeAccountingDetailModel> details, List<ExpenseAccountingDetailModel> expenses)
        {
            try
            {
                if (details == null)
                    details = new List<IncomeAccountingDetailModel>();

                if (expenses == null)
                    expenses = new List<ExpenseAccountingDetailModel>();

                accounting.Date = new DateTime(accounting.Year, accounting.Month, 1);
                accounting.Expenses = expenses;

                accounting.CreatedBy = accounting.ModifiedBy = MembershipHelper.GetUserName();
                accounting.CreatedDate = accounting.ModifiedDate = DateTime.Now;
                accounting.ApprovedDate = SystemConstants.UnsetDateTime;

                new AccountingBFC().Create(accounting, details);

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";

                return RedirectToAction("Detail", new { key = accounting.ID });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;
                ViewBag.Mode = UIMode.Create;

                throw;
            }
        }

        public ActionResult UpdateAccounting(AccountingModel accounting, List<IncomeAccountingDetailModel> details, List<ExpenseAccountingDetailModel> expenses)
        {
            try
            {
                if (details == null)
                    details = new List<IncomeAccountingDetailModel>();

                if (expenses == null)
                    expenses = new List<ExpenseAccountingDetailModel>();

                accounting.Date = new DateTime(accounting.Year, accounting.Month, 1);
                accounting.Expenses = expenses;

                accounting.ModifiedBy = MembershipHelper.GetUserName();
                accounting.ModifiedDate = DateTime.Now;
                accounting.ApprovedDate = SystemConstants.UnsetDateTime;

                new AccountingBFC().Update(accounting, details);

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";

                return RedirectToAction("Detail", new { key = accounting.ID });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;
                ViewBag.Mode = UIMode.Create;

                throw;
            }
        }

        public ActionResult VoidRemarks(string key, bool voidFromIndex)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var accounting = new AccountingBFC().RetrieveByID(key);

                ViewBag.VoidFromIndex = voidFromIndex;

                return View(accounting);
            }

            return View();
        }

        [HttpPost]
        public ActionResult VoidRemarks(AccountingModel accounting, FormCollection col)
        {
            var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);

            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    new AccountingBFC().Void(accounting.ID, accounting.VoidRemarks, MembershipHelper.GetUserName());

                    new EmailHelper().SendVoidAccountingEmail(accounting.ID, accounting.VoidRemarks, MembershipHelper.GetUserName());

                    trans.Complete();
                }
                TempData["SuccessNotification"] = "Dokumen berhasil dibatalkan";

                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = accounting.ID });
            }
            catch (Exception ex)
            {
                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = accounting.ID, errorMessage = ex.Message });
            }

        }
    }
}
