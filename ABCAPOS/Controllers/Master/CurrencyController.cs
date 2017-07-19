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
    public class CurrencyController : GenericController<CurrencyModel>
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

        public override MPL.Business.IGenericBFC<CurrencyModel> GetBFC()
        {
            return new CurrencyBFC();
        }

        public override void PreCreateDisplay(CurrencyModel obj)
        {
            SetViewBagPermission();

            base.PreCreateDisplay(obj);
        }

        public override void PreDetailDisplay(CurrencyModel obj)
        {
            ViewBag.Details = new CurrencyDateBFC().Retrieve(obj.ID);

            SetViewBagPermission();

            base.PreDetailDisplay(obj);
        }

        public override void PreUpdateDisplay(CurrencyModel obj)
        {
            SetViewBagPermission();

            base.PreUpdateDisplay(obj);
        }

        public override void CreateData(CurrencyModel obj)
        {
            try
            {
                base.CreateData(obj);

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

        public override void UpdateData(CurrencyModel obj, FormCollection formCollection)
        {
            try
            {
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
