using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.Models;
using MPL.MVC;
using ABCAPOS.BF;
using ABCAPOS.Util;
using ABCAPOS.Helpers;

namespace ABCAPOS.Controllers
{
    public class ExpenseBankController : MasterDetailController<IncomeExpenseModel, IncomeExpenseDetailModel>
    {
        private string ModuleID
        {
            get
            {
                return "IncomeExpense";
            }
        }

        private void SetViewBagPermission()
        {
            var roleDetails = new RoleBFC().RetrieveActions(MembershipHelper.GetRoleID(), ModuleID);

            ViewBag.AllowEdit = roleDetails.Contains("Edit");
            ViewBag.AllowCreate = roleDetails.Contains("Create");
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            //SetIndexViewBag();
            SetViewBagPermission();

            var incomeExpenseCount = 0;
            var incomeExpenseList = new List<IncomeExpenseModel>();

            if (startIndex == null)
                startIndex = 0;

            if (amount == null)
                amount = 20;

            if (string.IsNullOrEmpty(sortParameter))
                sortParameter = "";

            incomeExpenseCount = new IncomeExpenseBFC().RetrieveIncomeExpenseByCategoryCount(1, filter.GetSelectFilters());
            incomeExpenseList = new IncomeExpenseBFC().RetrieveIncomeExpenseByCategory(1, (int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters());

            ViewBag.DataCount = incomeExpenseCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            ViewBag.PageSeriesSize = GetPageSeriesSize();

            return View(incomeExpenseList);
        }

        public override MPL.Business.IMasterDetailBFC<IncomeExpenseModel, IncomeExpenseDetailModel> GetBFC()
        {
            return new IncomeExpenseBFC();
        }

        private void SetViewBagNotification()
        {
            if (TempData["SuccessNotification"] != null)
                ViewBag.SuccessNotification = Convert.ToString(TempData["SuccessNotification"]);

            if (!string.IsNullOrEmpty(Request.QueryString["errorMessage"]))
                ViewBag.ErrorNotification = Convert.ToString(Request.QueryString["errorMessage"]);
        }

        protected override void PreCreateDisplay(IncomeExpenseModel header, List<IncomeExpenseDetailModel> details)
        {
            var incomeExpenseID = Request.QueryString["incomeExpenseID"];

            if (!string.IsNullOrEmpty(incomeExpenseID))
                new IncomeExpenseBFC().CopyTransaction(header, Convert.ToInt64(incomeExpenseID));

            header.Code = new IncomeExpenseBFC().GetIncomeExpenseCode(1);

            //SetPreEditViewBag();

            base.PreCreateDisplay(header, details);
        }

        protected override void PreDetailDisplay(IncomeExpenseModel header, List<IncomeExpenseDetailModel> details)
        {
            SetViewBagNotification();

            SetViewBagPermission();

            base.PreDetailDisplay(header, details);
        }

        protected override void PreUpdateDisplay(IncomeExpenseModel header, List<IncomeExpenseDetailModel> details)
        {
            //SetPreEditViewBag();

            base.PreUpdateDisplay(header, details);
        }

        public override void CreateData(IncomeExpenseModel obj, List<IncomeExpenseDetailModel> details)
        {
            try
            {
                obj.CategoryID = 1;

                new IncomeExpenseBFC().Validate(obj, details);

                base.CreateData(obj, details);

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

        public override void UpdateData(IncomeExpenseModel obj, List<IncomeExpenseDetailModel> details, FormCollection formCollection)
        {
            try
            {
                new IncomeExpenseBFC().Validate(obj, details);

                base.UpdateData(obj, details, formCollection);

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

        protected override List<Button> GetAdditionalButtons(IncomeExpenseModel header, List<IncomeExpenseDetailModel> details, UIMode mode)
        {
            var list = new List<Button>();

            if (mode == UIMode.Detail && header.Status >= (int)IncomeExpenseStatus.New)
            {
                var print = new Button();
                print.Text = "Cetak";
                print.CssClass = "button";
                print.ID = "btnPrint";
                print.OnClick = String.Format("window.open('{0}');", Url.Action("PopUp", "ReportViewer",
                    new { type = ReportViewerController.PrintOutType.ExpenseBank, queryString = SystemConstants.str_IncomeExpenseID + "=" + header.ID }));
                print.Href = "#";
                list.Add(print);

            }

            return list;
        }

        public ActionResult ApproveFromIndex(string key)
        {
            base.ApproveData(key);

            return RedirectToAction("Index");
        }

        public ActionResult VoidFromIndex(string key)
        {
            base.VoidData(key);

            return RedirectToAction("Index");
        }

        public ActionResult VoidRemarks(string key, bool voidFromIndex)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var incomeExpense = new IncomeExpenseBFC().RetrieveByID(key);

                ViewBag.VoidFromIndex = voidFromIndex;

                return View(incomeExpense);
            }

            return View();
        }

        [HttpPost]
        public ActionResult VoidRemarks(IncomeExpenseModel incomeExpense, FormCollection col)
        {
            var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);

            try
            {
                new IncomeExpenseBFC().Void(incomeExpense.ID, incomeExpense.VoidRemarks, MembershipHelper.GetUserName());

                new EmailHelper().SendVoidIncomeExpenseEmail(incomeExpense.ID, incomeExpense.VoidRemarks, MembershipHelper.GetUserName());

                TempData["SuccessNotification"] = "Dokumen berhasil dibatalkan";

                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = incomeExpense.ID });
            }
            catch (Exception ex)
            {
                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = incomeExpense.ID, errorMessage = ex.Message });
            }

        }

    }
}
