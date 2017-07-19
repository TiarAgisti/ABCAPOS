using ABCAPOS.BF;
using ABCAPOS.Models;
using MPL.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ABCAPOS.Controllers.Master
{
    public class AccountCategoryController : GenericController<AccountCategoryModel>
    {
        private string ModuleID
        {
            get
            {
                return "AccountCategory";
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

        public override MPL.Business.IGenericBFC<AccountCategoryModel> GetBFC()
        {
            return new AccountCategoryBFC();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();
            
            return base.Index(startIndex, amount, sortParameter, filter);
        }

        public override void PreDetailDisplay(AccountCategoryModel obj)
        {
            SetViewBagPermission();
            
            base.PreDetailDisplay(obj);
        }
    }
}
