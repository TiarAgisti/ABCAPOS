using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.BF;
using ABCAPOS.Util;
using ABCAPOS.Models;
using System.IO;
using MPL.MVC;
using System.Web.Security;

namespace ABCAPOS.Controllers.Master
{
    public class CurrencyDateController : GenericController<CurrencyDateModel>
    {
        private string ModuleID
        {
            get
            {
                return "Currency";
            }
        }

        private void SetViewBagPermission()
        {
            var roleDetails = new RoleBFC().RetrieveActions(MembershipHelper.GetRoleID(), ModuleID);

            ViewBag.AllowEdit = roleDetails.Contains("Edit");
            ViewBag.AllowCreate = roleDetails.Contains("Create");
        }

        private void SetViewBagNotification()
        {
            if (TempData["SuccessNotification"] != null)
                ViewBag.SuccessNotification = Convert.ToString(TempData["SuccessNotification"]);

            if (!string.IsNullOrEmpty(Request.QueryString["errorMessage"]))
                ViewBag.ErrorNotification = Convert.ToString(Request.QueryString["errorMessage"]);
        }

        public override MPL.Business.IGenericBFC<CurrencyDateModel> GetBFC()
        {
            return new CurrencyDateBFC();
        }

        public override void PreCreateDisplay(CurrencyDateModel obj)
        {
            var currencyID = Request.QueryString["currencyID"];

            if (!string.IsNullOrEmpty(currencyID))
            {
                var currency = new CurrencyBFC().RetrieveByID(currencyID);

                obj.CurrencyID = currency.ID;
                obj.CurrencyName = currency.Name;
            }

            SetViewBagPermission();

            base.PreCreateDisplay(obj);
        }

        public override void PreDetailDisplay(CurrencyDateModel obj)
        {
            SetViewBagPermission();

            base.PreDetailDisplay(obj);
        }

        public override void PreUpdateDisplay(CurrencyDateModel obj)
        {
            SetViewBagPermission();

            base.PreUpdateDisplay(obj);
        }

        public override void CreateData(CurrencyDateModel obj)
        {
            try
            {
                new CurrencyDateBFC().Validate(obj);

                base.CreateData(obj);

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

        public override void UpdateData(CurrencyDateModel obj, FormCollection formCollection)
        {
            try
            {
                new CurrencyDateBFC().Validate(obj);

                base.UpdateData(obj, formCollection);

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();

            return base.Index(startIndex, amount, sortParameter, filter);
        }

    }
}
