using MPL.MVC;
using ABCAPOS.BF;
using ABCAPOS.Helpers;
using ABCAPOS.Util;
using System;
using System.Web.Mvc;
using System.Transactions;

namespace ABCAPOS.Controllers.GenericController
{
    public abstract class PSIGenericController<ModelType> : GenericController<ModelType>
    {
        public string ModuleID { get; set; }
        protected virtual void SetPreview(ModelType obj) { }
        protected void SetViewBag(string ModuleID)
        {
            /*ViewBag Permission*/
            ViewBag.AllowCreate = new RoleBFC().CheckIsAllowed(MembershipHelper.GetRoleID(), ModuleID, SystemConstants.str_permission_Create);
            ViewBag.AllowEdit = new RoleBFC().CheckIsAllowed(MembershipHelper.GetRoleID(), ModuleID, SystemConstants.str_permission_Edit);
            ViewBag.AllowView = new RoleBFC().CheckIsAllowed(MembershipHelper.GetRoleID(), ModuleID, SystemConstants.str_permission_View);

            /*ViewBag Notification*/
            if (TempData["SuccessNotification"] != null)
                ViewBag.SuccessNotification = Convert.ToString(TempData["SuccessNotification"]);

            if (!string.IsNullOrEmpty(Request.QueryString["errorMessage"]))
                ViewBag.ErrorNotification = Convert.ToString(Request.QueryString["errorMessage"]);
        }
        public Boolean checkPageAuthority(string moduleID, string moduleAction)
        {
            return new RoleBFC().RetrieveActions(MembershipHelper.GetRoleID(), moduleID).Contains(moduleAction);
        }
        public void RedirectToHome()
        {
            Response.Redirect("~/Home");
        }
        public ActionResult ActionResultRedirectToHome()
        {
            return View();
        }
        public void PreCreateDisplay(ModelType obj, string ModuleID)
        {
            this.SetViewBag(ModuleID);
            this.SetPreview(obj);
            if (this.checkPageAuthority(ModuleID, SystemConstants.str_permission_Create))
                base.PreCreateDisplay(obj);
            else
                this.RedirectToHome();
        }
        public void PreDetailDisplay(ModelType obj, string ModuleID)
        {
            this.SetViewBag(ModuleID);
            this.SetPreview(obj);
            if (this.checkPageAuthority(ModuleID, SystemConstants.str_permission_View))
                base.PreDetailDisplay(obj);
            else
                this.RedirectToHome();
        }
        public void PreUpdateDisplay(ModelType obj, string ModuleID)
        {
            this.SetViewBag(ModuleID);
            this.SetPreview(obj);
            if (this.checkPageAuthority(ModuleID, SystemConstants.str_permission_Edit))
                base.PreUpdateDisplay(obj);
            else
                this.RedirectToHome();
        }
        public ActionResult IndexHardened(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter, string ModuleID)
        {
            //base.ResetBackToListUrl(filter);
            this.SetViewBag(ModuleID);
            if (this.checkPageAuthority(ModuleID, SystemConstants.str_permission_View))
                return base.Index(startIndex, amount, sortParameter, filter);
            else
                return this.ActionResultRedirectToHome();
        }

        public virtual void CreateDataFunction(ModelType obj, string ModuleID)
        {
            base.CreateData(obj);
        }

        public virtual void UpdateDataFunction(ModelType obj, FormCollection formCollection, string ModuleID)
        {
            base.UpdateData(obj, formCollection);
        }
        public void CreateData(ModelType obj, string ModuleID)
        {
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    this.CreateDataFunction(obj, ModuleID);
                    trans.Complete();
                }
                TempData["SuccessNotification"] = SystemConstants.str_notif_success;
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;
                throw;
            }
        }
        public void UpdateData(ModelType obj, FormCollection formCollection, string ModuleID)
        {
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    this.UpdateDataFunction(obj, formCollection, ModuleID);
                    trans.Complete();
                }
                TempData["SuccessNotification"] = SystemConstants.str_notif_success;
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;
                throw;
            }
        }

        /*Auto Override MPL Controller*/
        public override void PreCreateDisplay(ModelType obj)
        {
            this.PreCreateDisplay(obj, this.ModuleID);
        }
        public override void PreDetailDisplay(ModelType obj)
        {
            this.PreDetailDisplay(obj, this.ModuleID);
        }
        public override void PreUpdateDisplay(ModelType obj)
        {
            this.PreUpdateDisplay(obj, this.ModuleID);
        }
        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            return this.IndexHardened(startIndex, amount, sortParameter, filter, this.ModuleID);
        }
        public override void CreateData(ModelType obj)
        {
            this.CreateData(obj, this.ModuleID);
        }
        public override void UpdateData(ModelType obj, FormCollection formCollection)
        {
            this.UpdateData(obj, formCollection, this.ModuleID);
        }
    }
}