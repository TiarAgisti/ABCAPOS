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
    public class VendorController : GenericController<VendorModel>
    {
        private string ModuleID
        {
            get
            {
                return "Vendor";
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

        public override MPL.Business.IGenericBFC<VendorModel> GetBFC()
        {
            return new VendorBFC();
        }

        public override void PreCreateDisplay(VendorModel obj)
        {
            obj.Code = new VendorBFC().GetVendorCode();
            
            SetViewBagPermission();

            ViewBag.TermsList = new PurchaseOrderBFC().RetrieveAllTerms();
            //ViewBag.RateList = new RateBFC().Retrieve(true);

            base.PreCreateDisplay(obj);
        }

        public override void PreDetailDisplay(VendorModel obj)
        {
            SetViewBagNotification();

            SetViewBagPermission();

            ViewBag.TermsList = new PurchaseOrderBFC().RetrieveAllTerms();
            //ViewBag.RateList = new RateBFC().Retrieve(true);

            base.PreDetailDisplay(obj);
        }

        public override void PreUpdateDisplay(VendorModel obj)
        {
            SetViewBagPermission();

            ViewBag.TermsList = new PurchaseOrderBFC().RetrieveAllTerms();
            //ViewBag.RateList = new RateBFC().Retrieve(true);

            base.PreUpdateDisplay(obj);
        }

        public override void CreateData(VendorModel obj)
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

        public override void UpdateData(VendorModel obj, FormCollection formCollection)
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

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();

            return base.Index(startIndex, amount, sortParameter, filter);
        }
    }
}
