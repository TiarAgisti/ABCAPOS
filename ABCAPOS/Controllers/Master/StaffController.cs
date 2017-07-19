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
    public class StaffController : GenericController<StaffModel>
    {
        private string ModuleID
        {
            get
            {
                return "Staff";
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

        private List<UserModel> filterUserList()
        {
            var assignedNames = new StaffBFC().RetrieveAssignedUserNames();
            var allUserModels = MembershipHelper.GetAllUsers();
            var filteredUserModel = new List<UserModel>();
            foreach (var userModel in allUserModels)
            {
                if (!assignedNames.Contains(userModel.UserID))
                    filteredUserModel.Add(userModel);
            }
            return filteredUserModel;
        }

        public override MPL.Business.IGenericBFC<StaffModel> GetBFC()
        {
            return new StaffBFC();
        }

        public override void PreCreateDisplay(StaffModel obj)
        {
            ViewBag.SupervisorList = new StaffBFC().Retrieve(true);
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
            ViewBag.LocationList = new WarehouseBFC().Retrieve(true);
            ViewBag.UserList = filterUserList();
            
            base.PreCreateDisplay(obj);
        }

        public override void PreUpdateDisplay(StaffModel obj)
        {
            ViewBag.SupervisorList = new StaffBFC().Retrieve(true);
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
            ViewBag.LocationList = new WarehouseBFC().Retrieve(true);
            ViewBag.UserList = filterUserList();
            
            base.PreCreateDisplay(obj);
        }

        public override void PreDetailDisplay(StaffModel obj)
        {
            ViewBag.SupervisorList = new StaffBFC().Retrieve(true);
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
            ViewBag.LocationList = new WarehouseBFC().Retrieve(true);
            
            SetViewBagNotification();

            SetViewBagPermission();

            base.PreDetailDisplay(obj);
        }

        public override void CreateData(StaffModel obj)
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

        public override void UpdateData(StaffModel obj, FormCollection formCollection)
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
