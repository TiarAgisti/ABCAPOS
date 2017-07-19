using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.Models;
using MPL.MVC;
using ABCAPOS.BF;

namespace ABCAPOS.Controllers.Master
{
    public class AnnualVATController : GenericController<AnnualVATModel>
    {
        private string ModuleID
        {
            get
            {
                return "AnnualVAT";
            }
        }

        private void SetViewBagPermission()
        {
            var roleDetails = new RoleBFC().RetrieveActions(MembershipHelper.GetRoleID(), ModuleID);

            ViewBag.AllowEdit = roleDetails.Contains("Edit");
            ViewBag.AllowCreate = roleDetails.Contains("Create");
        }

        public override MPL.Business.IGenericBFC<AnnualVATModel> GetBFC()
        {
            return new AnnualVATBFC();
        }

        public override void PreCreateDisplay(AnnualVATModel obj)
        {
            SetPreEditViewBag();
            SetViewBagPermission();

            base.PreCreateDisplay(obj);
        }

        public override void PreDetailDisplay(AnnualVATModel obj)
        {
            SetPreEditViewBag();

            SetViewBagPermission();

            base.PreDetailDisplay(obj);
        }

        public override void PreUpdateDisplay(AnnualVATModel obj)
        {
            SetPreEditViewBag();
            SetViewBagPermission();

            base.PreUpdateDisplay(obj);
        }

        public override void CreateData(AnnualVATModel obj)
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

        public override void UpdateData(AnnualVATModel obj, FormCollection formCollection)
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

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();

            return base.Index(startIndex, amount, sortParameter, filter);
        }
    }
}
