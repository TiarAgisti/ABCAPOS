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
    public class CustomerController : GenericController<CustomerModel>
    {
        private string ModuleID
        {
            get
            {
                return "Customer";
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

        private void SetPreEditViewBag()
        {
            List<CustomerGroupModel> groupList = new List<CustomerGroupModel>();
            ViewBag.TermList = new PurchaseOrderBFC().RetrieveAllTerms();
            ViewBag.PriceLevelList = new PriceLevelBFC().Retrieve(true);
            //groupList.Add(new CustomerGroupModel() { ID = 0 });
            groupList.AddRange(new CustomerGroupBFC().RetrieveAll());

            ViewBag.GroupList = groupList;
            ViewBag.WarehouseList = new WarehouseBFC().RetrieveAll();
        }

        private void SetDisplayDetailViewBag(CustomerModel header)
        {
            ViewBag.SalesOrder = new SalesOrderBFC().RetreiveByCustomerID(header.ID);
            ViewBag.DeliveryOrder = new DeliveryOrderBFC().RetreiveDOByCustomerID(header.ID);
            ViewBag.Invoice = new InvoiceBFC().RetreiveInvByCustomerID(header.ID);
            ViewBag.InvPayment = new MakeMultiPaySalesBFC().RetreiveInvPaymentByCustomerID(header.ID);
        }

        public override MPL.Business.IGenericBFC<CustomerModel> GetBFC()
        {
            return new CustomerBFC();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            ViewBag.WarehouseList = new WarehouseBFC().RetrieveAll();

            SetViewBagPermission();

            return base.Index(startIndex, amount, sortParameter, filter);
        }

        public override void PreCreateDisplay(CustomerModel obj)
        {
            obj.Code = new CustomerBFC().GetCustomerCode();
            SetPreEditViewBag();
            
            base.PreCreateDisplay(obj);
        }

        public override void PreDetailDisplay(CustomerModel obj)
        {
            SetViewBagNotification();

            SetViewBagPermission();

            this.SetDisplayDetailViewBag(obj);
        
            base.PreDetailDisplay(obj);
        }

        public override void PreUpdateDisplay(CustomerModel obj)
        {
            SetPreEditViewBag();
            
            base.PreUpdateDisplay(obj);
        }

        public override void CreateData(CustomerModel obj)
        {
            try
            {
                new CustomerBFC().Validate(obj);

                base.CreateData(obj);

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

        public override void UpdateData(CustomerModel obj, FormCollection formCollection)
        {
            try
            {
                new CustomerBFC().Validate(obj);

                base.UpdateData(obj, formCollection);

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

    }
}
