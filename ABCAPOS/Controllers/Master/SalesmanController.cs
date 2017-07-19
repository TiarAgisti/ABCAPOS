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
    public class SalesmanController : GenericController<SalesmanModel>
    {
        private string ModuleID
        {
            get
            {
                return "Salesman";
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
            List<UserModel> userList = new List<UserModel>();

            userList.Add(new UserModel() { UserID = "" });

            foreach (MembershipUser user in Membership.GetAllUsers())
            {
                userList.Add(new UserModel() { UserID = user.UserName });
            }

            ViewBag.UserList = userList;
        }

        private void SetViewBagNotification()
        {
            if (TempData["SuccessNotification"] != null)
                ViewBag.SuccessNotification = Convert.ToString(TempData["SuccessNotification"]);

            if (!string.IsNullOrEmpty(Request.QueryString["errorMessage"]))
                ViewBag.ErrorNotification = Convert.ToString(Request.QueryString["errorMessage"]);
        }

        public override MPL.Business.IGenericBFC<SalesmanModel> GetBFC()
        {
            return new SalesmanBFC();
        }

        public override void PreCreateDisplay(SalesmanModel obj)
        {
            obj.Code = new SalesmanBFC().GetSalesmanCode();

            SetPreEditViewBag();
            SetViewBagPermission();

            base.PreCreateDisplay(obj);
        }

        public override void PreDetailDisplay(SalesmanModel obj)
        {
            SetPreEditViewBag();

            SetViewBagPermission();

            base.PreDetailDisplay(obj);
        }

        public override void PreUpdateDisplay(SalesmanModel obj)
        {
            SetPreEditViewBag();
            SetViewBagPermission();

            base.PreUpdateDisplay(obj);
        }

        public override void CreateData(SalesmanModel obj)
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

        public override void UpdateData(SalesmanModel obj, FormCollection formCollection)
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
