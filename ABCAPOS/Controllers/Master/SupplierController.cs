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
    public class SupplierController : GenericController<SupplierModel>
    {
        private string ModuleID
        {
            get
            {
                return "Supplier";
            }
        }

        private void SetViewBagPermission()
        {
            var roleDetails = new RoleBFC().RetrieveActions(MembershipHelper.GetRoleID(), ModuleID);

            ViewBag.AllowEdit = roleDetails.Contains("Edit");
            ViewBag.AllowCreate = roleDetails.Contains("Create");
        }

        public override MPL.Business.IGenericBFC<SupplierModel> GetBFC()
        {
            return new SupplierBFC();
        }

        public override void PreCreateDisplay(SupplierModel obj)
        {
            obj.Code = new SupplierBFC().GetSupplierCode();

            SetPreEditViewBag();

            base.PreCreateDisplay(obj);
        }

        public override void PreDetailDisplay(SupplierModel obj)
        {
            SetViewBagNotification();

            SetViewBagPermission();

            base.PreDetailDisplay(obj);
        }

        public override void PreUpdateDisplay(SupplierModel obj)
        {
            SetPreEditViewBag();

            base.PreUpdateDisplay(obj);
        }

        public override void CreateData(SupplierModel obj)
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

        public override void UpdateData(SupplierModel obj, FormCollection formCollection)
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

        private void SetViewBagNotification()
        {
            if (TempData["SuccessNotification"] != null)
                ViewBag.SuccessNotification = Convert.ToString(TempData["SuccessNotification"]);

            if (!string.IsNullOrEmpty(Request.QueryString["errorMessage"]))
                ViewBag.ErrorNotification = Convert.ToString(Request.QueryString["errorMessage"]);
        }

        private void SetPreEditViewBag()
        {
            List<SupplierGroupModel> groupList = new List<SupplierGroupModel>();

            //groupList.Add(new CustomerGroupModel() { ID = 0 });
            groupList.AddRange(new SupplierGroupBFC().RetrieveAll());

            ViewBag.GroupList = groupList;
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();

            return base.Index(startIndex, amount, sortParameter, filter);
        }

    }
}
