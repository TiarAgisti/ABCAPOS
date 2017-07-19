using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.Models;
using MPL.MVC;
using ABCAPOS.BF;
using ABCAPOS.DA;
using ABCAPOS.Util;
using ABCAPOS.Helpers;

namespace ABCAPOS.Controllers
{
    public class UnitController : MasterDetailController<UnitModel, UnitDetailModel>
    {

        private string ModuleID
        {
            get
            {
                return "Unit";
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

        public override MPL.Business.IMasterDetailBFC<UnitModel, UnitDetailModel> GetBFC()
        {
            return new UnitBFC();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();

            return base.Index(startIndex, amount, sortParameter, filter);
        }

        protected override void PreDetailDisplay(UnitModel obj, List<UnitDetailModel> details)
        {
            SetViewBagNotification();
            SetViewBagPermission();
            base.PreDetailDisplay(obj, details);
        }

        public override void CreateData(UnitModel obj, List<UnitDetailModel> details)
        {
            try
            {
                new UnitBFC().Validate(obj, details);
                base.CreateData(obj, details);
                TempData["SuccessNotification"] = "Document has been saved";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;
                throw;
            }
        }

        public override void UpdateData(UnitModel obj, List<UnitDetailModel> details, FormCollection formCollection)
        {
            try
            {
                new UnitBFC().Validate(obj, details);
                base.UpdateData(obj, details, formCollection);
                TempData["SuccessNotification"] = "Document has been saved";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;
                throw;
            }
        }

        #region commented

        //private void SetIndexViewBag()
        //{
        //    //ViewBag.SupplierGroupList = new SupplierGroupBFC().RetrieveAll();
        //}



        //private void SetPreCreateViewBag(UnitModel header)
        //{
        //    ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
        //    ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
        //    ViewBag.StaffList = new StaffBFC().Retrieve(true);
        //    ViewBag.ConversionList = new ProductBFC().RetrieveAllConversion();
        //}

        //private void SetPreEditViewBag(UnitModel header)
        //{
        //    ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
        //    ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
        //    ViewBag.StaffList = new StaffBFC().Retrieve(true);
        //    ViewBag.ConversionList = new ProductBFC().RetrieveAllConversion();
        //}

        //private void SetPreDetailViewBag()
        //{
        //    ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
        //    ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
        //}

        //public override void PreCreateDisplay(CustomerModel obj)
        //{
        //    obj.Code = new CustomerBFC().GetCustomerCode();
        //    base.PreCreateDisplay(obj);
        //}

        //protected override void PreUpdateDisplay(UnitModel header, List<UnitDetailModel> details)
        //{
        //    SetPreEditViewBag(header);

        //    base.PreUpdateDisplay(header, details);
        //}

        #endregion

       
    }
}
