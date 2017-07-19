using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.BF;
using ABCAPOS.Util;
using ABCAPOS.Models;
using System.Web.Security;
using MPL.MVC;

namespace ABCAPOS.Controllers.Master
{
    public class AccountController : GenericController<AccountModel>
    {
        private string ModuleID
        {
            get
            {
                return "Account";
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
            ViewBag.CategoryList = new AccountCategoryBFC().RetrieveAll();
        }

        private void SetViewBagNotification()
        {
            if (TempData["SuccessNotification"] != null)
                ViewBag.SuccessNotification = Convert.ToString(TempData["SuccessNotification"]);

            if (!string.IsNullOrEmpty(Request.QueryString["errorMessage"]))
                ViewBag.ErrorNotification = Convert.ToString(Request.QueryString["errorMessage"]);
        }

        public override MPL.Business.IGenericBFC<AccountModel> GetBFC()
        {
            return new AccountBFC();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();

            return base.Index(startIndex, amount, sortParameter, filter);
        }

        public override void PreCreateDisplay(AccountModel obj)
        {
            obj.Code = new AccountBFC().GetAccountCode();

            SetViewBagPermission();
            SetPreEditViewBag();

            base.PreCreateDisplay(obj);
        }

        public override void PreDetailDisplay(AccountModel obj)
        {
            SetViewBagNotification();
            SetViewBagPermission();

            base.PreDetailDisplay(obj);
        }

        public override void PreUpdateDisplay(AccountModel obj)
        {
            SetViewBagPermission();
            SetPreEditViewBag();

            base.PreUpdateDisplay(obj);
        }

        public override void CreateData(AccountModel obj)
        {
            try
            {
                base.CreateData(obj);

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;
                SetPreEditViewBag();

                throw;
            }
        }

        public override void UpdateData(AccountModel obj, FormCollection formCollection)
        {
            try
            {
                base.UpdateData(obj, formCollection);

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;
                SetPreEditViewBag();

                throw;
            }
        }

    }
}
