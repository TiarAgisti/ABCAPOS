using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.Models;
using ABCAPOS.BF;
using ABCAPOS.Util;
using MPL.MVC;
using System.Web.Security;
using ABCAPOS.Helpers;

namespace ABCAPOS.Controllers.Master
{
    public class RoleController : GenericController<RoleModel>
    {
        private string ModuleID
        {
            get
            {
                return "Role";
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

        private void PopulateDetails(RoleModel role, FormCollection col)
        {
            var roleDetails = new List<RoleDetailModel>();

            foreach (var obj in ModuleHelper.ModuleList())
            {
                if (col["chkView" + obj.Key] != null)
                {
                    var roleDetail = new RoleDetailModel();
                    roleDetail.ModuleID = obj.Key;
                    roleDetail.Action = "View";

                    roleDetails.Add(roleDetail);
                }

                if (col["chkCreate" + obj.Key] != null)
                {
                    var roleDetail = new RoleDetailModel();
                    roleDetail.ModuleID = obj.Key;
                    roleDetail.Action = "Create";

                    roleDetails.Add(roleDetail);
                }

                if (col["chkEdit" + obj.Key] != null)
                {
                    var roleDetail = new RoleDetailModel();
                    roleDetail.ModuleID = obj.Key;
                    roleDetail.Action = "Edit";

                    roleDetails.Add(roleDetail);
                }

                if (col["chkVoid" + obj.Key] != null)
                {
                    var roleDetail = new RoleDetailModel();
                    roleDetail.ModuleID = obj.Key;
                    roleDetail.Action = "Void";

                    roleDetails.Add(roleDetail);
                }

                if (col["chkFG" + obj.Key] != null)
                {
                    var roleDetail = new RoleDetailModel();
                    roleDetail.ModuleID = obj.Key;
                    roleDetail.Action = "ViewFinishGood";

                    roleDetails.Add(roleDetail);
                }

                if (col["chkALL" + obj.Key] != null)
                {
                    var roleDetail = new RoleDetailModel();
                    roleDetail.ModuleID = obj.Key;
                    roleDetail.Action = "ViewALL";

                    roleDetails.Add(roleDetail);
                }
            }

            role.Details = roleDetails;
        }

        public override MPL.Business.IGenericBFC<RoleModel> GetBFC()
        {
            return new RoleBFC();
        }

        public override void PreCreateDisplay(RoleModel obj)
        {
            SetViewBagPermission();

            base.PreCreateDisplay(obj);
        }

        public override void PreDetailDisplay(RoleModel obj)
        {
            obj.Details = new RoleBFC().RetrieveDetails(obj.ID);

            SetViewBagNotification();
            SetViewBagPermission();

            base.PreDetailDisplay(obj);
        }

        public override void PreUpdateDisplay(RoleModel obj)
        {
            obj.Details = new RoleBFC().RetrieveDetails(obj.ID);

            SetViewBagPermission();

            base.PreUpdateDisplay(obj);
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();

            var roleList = new List<RoleModel>();
            if (startIndex == null)
                startIndex = 0;

            if (amount == null)
                amount = 20;

            if (string.IsNullOrEmpty(sortParameter))
                sortParameter = "";

            if ((MembershipHelper.GetRoleID() == (int)PermissionStatus.root))
            {
                roleList = new RoleBFC().RetreiveListRoleRoot((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters());
            }
            else
            {
                roleList = new RoleBFC().RetreiveListRoleAdministrator((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters());
            }

            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            ViewBag.PageSeriesSize = GetPageSeriesSize();
            return View(roleList);
            //return base.Index(startIndex, amount, sortParameter, filter);
        }

        [HttpPost]
        public ActionResult CreateRole(RoleModel role, FormCollection col)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    PopulateDetails(role, col);
                    new RoleBFC().Create(role, role.Details, MembershipHelper.GetUserName());

                    TempData["SuccessNotification"] = "Dokumen berhasil disimpan";

                    return RedirectToAction("Detail", new { key = role.ID });
                }
                catch (Exception ex)
                {
                    ViewBag.Mode = EditMode.Create;
                    ViewBag.ErrorNotification = ex.Message;

                    return View(role);
                }
            }
            else
                ViewBag.ErrorMessage = "Object is invalid";

            ViewBag.Mode = EditMode.Create;

            return View(role);
        }

        [HttpPost]
        public ActionResult UpdateRole(RoleModel role, FormCollection col)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    PopulateDetails(role, col);
                    new RoleBFC().Update(role, role.Details, MembershipHelper.GetUserName());

                    foreach (MembershipUser user in Membership.GetAllUsers())
                        MenuHelper.ResetMenu(user.UserName);

                    TempData["SuccessNotification"] = "Dokumen berhasil disimpan";

                    return RedirectToAction("Detail", new { key = role.ID });
                }
                catch (Exception ex)
                {
                    ViewBag.Mode = EditMode.Update;
                    ViewBag.ErrorNotification = ex.Message;

                    return View(role);
                }
            }
            else
                ViewBag.ErrorMessage = "Object is invalid";

            ViewBag.Mode = EditMode.Update;

            return View(role);
        }

    }
}
