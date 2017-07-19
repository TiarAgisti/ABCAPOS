using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.Models;
using ABCAPOS.BF;
using MPL.MVC;

namespace ABCAPOS.Controllers.Master
{
    public class PPh21ExpenseController : GenericController<PPh21ExpenseModel>
    {
        private string ModuleID
        {
            get
            {
                return "PPh21Expense";
            }
        }

        private void SetViewBagPermission()
        {
            var roleDetails = new RoleBFC().RetrieveActions(MembershipHelper.GetRoleID(), ModuleID);

            ViewBag.AllowEdit = roleDetails.Contains("Edit");
            ViewBag.AllowCreate = roleDetails.Contains("Create");
        }

        private void SetPreEditViewBag()
        {
            List<object> yearList = new List<object>();

            for (int i = 2013; i <= 2100; i++)
                yearList.Add(new { Value = i, Text = i });

            ViewBag.YearList = yearList;
        }

        private void SetViewBagNotification()
        {
            if (TempData["SuccessNotification"] != null)
                ViewBag.SuccessNotification = Convert.ToString(TempData["SuccessNotification"]);

            if (!string.IsNullOrEmpty(Request.QueryString["errorMessage"]))
                ViewBag.ErrorNotification = Convert.ToString(Request.QueryString["errorMessage"]);
        }

        public override MPL.Business.IGenericBFC<PPh21ExpenseModel> GetBFC()
        {
            return new PPh21ExpenseBFC();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();

            return base.Index(startIndex, amount, sortParameter, filter);
        }

        public override void PreCreateDisplay(PPh21ExpenseModel obj)
        {
            SetPreEditViewBag();
            SetViewBagPermission();

            obj.StrYear = obj.Date.Year.ToString();

            base.PreCreateDisplay(obj);
        }

        public override void PreDetailDisplay(PPh21ExpenseModel obj)
        {
            SetPreEditViewBag();

            SetViewBagPermission();

            base.PreDetailDisplay(obj);
        }

        public override void PreUpdateDisplay(PPh21ExpenseModel obj)
        {
            SetPreEditViewBag();
            SetViewBagPermission();

            obj.StrYear = obj.Date.Year.ToString();

            base.PreUpdateDisplay(obj);
        }

        public override void CreateData(PPh21ExpenseModel obj)
        {
            try
            {
                obj.Date = new DateTime(Convert.ToInt32(obj.StrYear), 1, 1);

                base.CreateData(obj);

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

        public override void UpdateData(PPh21ExpenseModel obj, FormCollection formCollection)
        {
            try
            {
                obj.Date = new DateTime(Convert.ToInt32(obj.StrYear), 1, 1);

                formCollection["Date"] = obj.Date.ToString();

                base.UpdateData(obj, formCollection);

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

        
    }
}
