using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.BF;
using ABCAPOS.Util;
using ABCAPOS.Models;
using MPL.MVC;

namespace ABCAPOS.Controllers.Master
{
    public class DepartmentController : GenericController<DepartmentModel>
    {
        private string ModuleID
        {
            get
            {
                return "Department";
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

        public override MPL.Business.IGenericBFC<DepartmentModel> GetBFC()
        {
            return new DepartmentBFC();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();

            return base.Index(startIndex, amount, sortParameter, filter);
        }

        public override void PreCreateDisplay(DepartmentModel obj)
        {
            obj.Code = new DepartmentBFC().GetDepartmentCode();

            SetViewBagPermission();

            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);

            base.PreCreateDisplay(obj);
        }

        public override void PreDetailDisplay(DepartmentModel obj)
        {
            SetViewBagNotification();

            SetViewBagPermission();

            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);

            base.PreDetailDisplay(obj);
        }

        public override void PreUpdateDisplay(DepartmentModel obj)
        {
            SetViewBagPermission();

            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);

            base.PreUpdateDisplay(obj);
        }

        public override void CreateData(DepartmentModel obj)
        {
            try
            {
                base.CreateData(obj);

                TempData["SuccessNotification"] = "Document has been saved";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

        public override void UpdateData(DepartmentModel obj, FormCollection formCollection)
        {
            try
            {
                base.UpdateData(obj, formCollection);

                TempData["SuccessNotification"] = "Document has been saved";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

    }
}
