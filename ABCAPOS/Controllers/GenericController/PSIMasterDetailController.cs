using MPL.MVC;
using ABCAPOS.BF;
using ABCAPOS.Helpers;
using ABCAPOS.Util;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Transactions;

namespace ABCAPOS.Controllers.GenericController
{
    public abstract class PSIMasterDetailController<MasterModelType, DetailModelType> : MasterDetailController<MasterModelType, DetailModelType>
    {
        public string ModuleID { get; set; }
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
        private void DetailHandler(MasterModelType header, List<DetailModelType> details)
        {
            details = (details == null) ? new List<DetailModelType>() : details;
        }
        /*PRESET AND OVERRIDABLE PRESET*/
        protected void SetViewBag(string ModuleID)
        {
            /*ViewBag Permission*/
            ViewBag.AllowCreate = new RoleBFC().CheckIsAllowed(MembershipHelper.GetRoleID(), ModuleID, SystemConstants.str_permission_Create);
            ViewBag.AllowEdit = new RoleBFC().CheckIsAllowed(MembershipHelper.GetRoleID(), ModuleID, SystemConstants.str_permission_Edit);
            ViewBag.AllowView = new RoleBFC().CheckIsAllowed(MembershipHelper.GetRoleID(), ModuleID, SystemConstants.str_permission_View);
            ViewBag.AllowVoid = new RoleBFC().CheckIsAllowed(MembershipHelper.GetRoleID(), ModuleID, SystemConstants.str_permission_Void);

            /*ViewBag Notification*/
            if (TempData["SuccessNotification"] != null)
                ViewBag.SuccessNotification = Convert.ToString(TempData["SuccessNotification"]);

            if (!string.IsNullOrEmpty(Request.QueryString["errorMessage"]))
                ViewBag.ErrorNotification = Convert.ToString(Request.QueryString["errorMessage"]);
        }
        protected virtual void SetPreview(MasterModelType header, List<DetailModelType> details) { }
        protected virtual void SetPreview(){ }
        private void PreSet(MasterModelType header, List<DetailModelType> details)
        {
            this.SetViewBag(ModuleID);
            this.SetPreview(header, details);
        }
        private void PreSet()
        {
            this.SetViewBag(ModuleID);
            this.SetPreview();
        }
        /*OVERRIDABLE TRANSACTION*/
        public virtual void CreateDataFunction(MasterModelType header, List<DetailModelType> details, string ModuleID)
        {
            this.DetailHandler(header, details);
            base.CreateData(header, details);
        }
        public virtual void UpdateDataFunction(MasterModelType header, List<DetailModelType> details, FormCollection formCollection, string ModuleID)
        {
            this.DetailHandler(header, details);
            base.UpdateData(header, details, formCollection);
        }
        public virtual ActionResult IndexFunction(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter, string ModuleID)
        {
            return base.Index(startIndex, amount, sortParameter, filter);
        }

        /*Auto Override MPL Controller*/
        protected override void PreCreateDisplay(MasterModelType header, List<DetailModelType> details)
        {
            if (this.checkPageAuthority(ModuleID, SystemConstants.str_permission_Create))
            {
                this.PreSet(header, details);
                base.PreCreateDisplay(header, details);
            }
            else
                this.RedirectToHome();
        }
        protected override void PreDetailDisplay(MasterModelType header, List<DetailModelType> details)
        {
            if (this.checkPageAuthority(ModuleID, SystemConstants.str_permission_View))
            {
                this.PreSet(header, details);
                base.PreDetailDisplay(header, details);
            }
            else
                this.RedirectToHome();
        }
        protected override void PreUpdateDisplay(MasterModelType header, List<DetailModelType> details)
        {
            if (this.checkPageAuthority(ModuleID, SystemConstants.str_permission_Edit))
            {
                this.PreSet(header, details);
                base.PreUpdateDisplay(header, details);
            }
            else
                this.RedirectToHome();
        }
        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            this.PreSet();
            if (this.checkPageAuthority(ModuleID, SystemConstants.str_permission_View))
                return IndexFunction(startIndex, amount, sortParameter, filter, ModuleID);
            else
                return this.ActionResultRedirectToHome();
        }
        public override void CreateData(MasterModelType header, List<DetailModelType> details)
        {
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    CreateDataFunction(header, details, ModuleID);
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
        public override void UpdateData(MasterModelType header, List<DetailModelType> details, FormCollection formCollection)
        {
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    UpdateDataFunction(header, details, formCollection, ModuleID);
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
    }
}

