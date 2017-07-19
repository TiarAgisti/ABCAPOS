using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ABCAPOS.Models;
using ABCAPOS.Util;
using ABCAPOS.BF;
using System.Transactions;
using MPL.MVC;
using ABCAPOS;
using ABCAPOS.Helpers;

namespace ABCAPOS.Controllers
{
    public class UserController : Controller
    {
        private string ModuleID
        {
            get
            {
                return "User";
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
            if ((MembershipHelper.GetRoleID() == (int)PermissionStatus.root))
            {
                ViewBag.RoleList = new RoleBFC().Retrieve(true).OrderBy(p => p.Name);
            }
            else
            {
                ViewBag.RoleList = new RoleBFC().RetreiveRoleAdminstrator(true).OrderBy(p => p.Name);
            }
            //ViewBag.RoleList = new RoleBFC().Retrieve(true).OrderBy(p => p.Name);
          
            SetViewBagPermission();
        }

        private void SetViewBagNotification()
        {
            if (TempData["SuccessNotification"] != null)
                ViewBag.SuccessNotification = Convert.ToString(TempData["SuccessNotification"]);

            if (!string.IsNullOrEmpty(Request.QueryString["errorMessage"]))
                ViewBag.ErrorNotification = Convert.ToString(Request.QueryString["errorMessage"]);
        }

        private void Validate(UserModel user)
        {
            if (string.IsNullOrEmpty(user.Password))
                throw new Exception("Kata kunci harus diisi");

            if (string.IsNullOrEmpty(user.ConfirmPassword))
                throw new Exception("Pengulangan kata kunci harus diisi");

            if (user.Password != user.ConfirmPassword)
                throw new Exception("Kata kunci dan pengulangan kata kunci tidak sama");

        }

        public ActionResult Index(int? pageIndex, int? amount, string sortParameter, string filterBy, string filterKey)
        {
            if (pageIndex == null)
                pageIndex = 1;

            int startIndex = Convert.ToInt32(pageIndex * amount);

            var userList = new List<UserModel>();
            var membershipList = Membership.GetAllUsers();

            var roleList = new RoleBFC().RetrieveAll();
   
            foreach (MembershipUser membershipUser in membershipList)
            {
                var user = new UserModel();
                user.UserID = membershipUser.UserName;

                var profile = ProfileCommon.GetProfile(membershipUser.UserName);

                var query = from i in roleList
                            where i.ID == profile.RoleID
                            select i;

                if (query.FirstOrDefault() != null)
                {
                    user.RoleID = query.FirstOrDefault().ID;
                    user.RoleName = query.FirstOrDefault().Name;
                }

                if (profile.IsActive == true)
                {
                    if ((MembershipHelper.GetRoleID() == (int)PermissionStatus.root))
                    {
                        userList.Add(user);
                    }
                    else
                    {
                        if (user.RoleID != 23 && user.RoleID != 25)
                            userList.Add(user);
                    }
                }
                    //userList.Add(user);

               
              
               
            }

            ViewBag.ControllerName = "User";
            ViewBag.PageIndex = pageIndex;
            ViewBag.PageCount = Math.Floor(Convert.ToDecimal(membershipList.Count / SystemConstants.ItemPerPage)) + 1;
            ViewBag.SortParameter = sortParameter;
            ViewBag.FilterKey = filterKey;

            SetViewBagPermission();

            return View(userList);
        }

        public ActionResult Create()
        {
            var obj = new UserModel();

            ViewBag.Mode = UIMode.Create;
            SetPreEditViewBag();

            return View(obj);
        }

        [HttpPost]
        public ActionResult Create(UserModel user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Validate(user);

                    using (TransactionScope trans = new TransactionScope())
                    {
                        Membership.CreateUser(user.UserID, user.Password);

                        var profile = ProfileCommon.GetProfile(user.UserID);
                        profile.RoleID = user.RoleID;
                        profile.IsActive = user.IsActive;

                        trans.Complete();
                    }

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Mode = UIMode.Create;
                    ViewBag.ErrorNotification = ex.Message;
                    SetPreEditViewBag();

                    return View(user);
                }
            }
            else
                ViewBag.ErrorNotification = "Object is invalid";

            ViewBag.Mode = UIMode.Create;
            SetPreEditViewBag();

            return View(user);
        }

        public ActionResult Detail(string key)
        {
            var obj = new UserModel();
            var membership = Membership.GetUser(key);
            var profile = ProfileCommon.GetProfile(key);

            obj.UserID = membership.UserName;

            if (profile != null)
            {
                obj.RoleID = profile.RoleID;
                obj.IsActive = profile.IsActive;
            }

            if (obj.RoleID != 0)
            {
                var role = new RoleBFC().RetrieveByID(obj.RoleID);
                obj.RoleName = role.Name;
            }

            ViewBag.Mode = UIMode.Detail;

            SetViewBagNotification();
            SetViewBagPermission();

            return View(obj);
        }

        public ActionResult Update(string key)
        {
            var obj = new UserModel();
            var membership = Membership.GetUser(key);
            var profile = ProfileCommon.GetProfile(key);

            obj.UserID = membership.UserName;
            obj.RoleID = profile.RoleID;
            obj.IsActive = profile.IsActive;

            if (obj.RoleID != 0)
            {
                var role = new RoleBFC().RetrieveByID(obj.RoleID);
                obj.RoleName = role.Name;
            }

            ViewBag.Mode = UIMode.Update;
            SetPreEditViewBag();

            return View(obj);
        }

        [HttpPost]
        public ActionResult Update(UserModel user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (!string.IsNullOrEmpty(user.Password))
                    {
                        Validate(user);

                        var membershipUser = Membership.GetUser(user.UserID);

                        membershipUser.ChangePassword(membershipUser.ResetPassword(), user.Password);
                    }

                    var profile = ProfileCommon.GetProfile(user.UserID);
                    profile.RoleID = user.RoleID;
                    profile.IsActive = user.IsActive;

                    MenuHelper.ResetMenu(user.UserID);

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Mode = UIMode.Update;
                    ViewBag.ErrorNotification = ex.Message;
                    SetPreEditViewBag();

                    return View(user);
                }
            }
            else
                ViewBag.ErrorNotification = "Object is invalid";

            ViewBag.Mode = UIMode.Update;
            SetPreEditViewBag();

            return View(user);
        }

        public ActionResult Delete(string userID)
        {
            try
            {
                Membership.DeleteUser(userID);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Mode = UIMode.Update;
                ViewBag.ErrorMessage = ex.Message;

                return RedirectToAction("Index");
            }
        }

        public ActionResult LogOn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogOn(UserModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (Membership.ValidateUser(model.UserID, model.Password))
                {
                    //var profile = ProfileCommon.GetProfile(model.UserID);

                    //if (profile.IsActive)
                    //{
                        FormsAuthentication.SetAuthCookie(model.UserID, false);
                        if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                            && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    //}
                    //else
                    //{
                    //    ModelState.AddModelError("", "The user is not active.");
                    //}
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

    }
}
