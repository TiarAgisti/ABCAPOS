using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.Models;
using MPL.MVC;
using ABCAPOS.BF;
using ABCAPOS.Util;

namespace ABCAPOS.Controllers.Master
{
    public class ProductController : MasterDetailController<ProductModel, ProductDetailModel>
    {
        private string ModuleID
        {
            get
            {
                return "Product";
            }
        }
        private void AddIsSavedAttribute(List<ProductDetailModel> details)
        {
            foreach (var detail in details)
                detail.IsSaved = true;
        }
        private void SetDetails(ProductModel header)
        {
            header.basePriceDetails = new BasePriceLocationBFC().RetrieveByProductID(header.ID);
            header.LimitStockDetails = new LimitStockBFC().RetrieveByProductID(header.ID);
            header.FormulasiDetails = new FormulasiBFC().RetreiveByProductID(header.ID);
            foreach (var detail in header.FormulasiDetails)
            {
                detail.ConversionIDTemp = detail.ConversionID;
            }

            //if (MembershipHelper.GetRoleID() == (int)PermissionStatus.root || MembershipHelper.GetRoleID() == (int)PermissionStatus.production)
            //{
            //    header.FormulasiDetails = new FormulasiBFC().RetreiveByProductID(header.ID);
            //}
           
        }
        private void SetViewBagPermission()
        {
            var roleDetails = new RoleBFC().RetrieveActions(MembershipHelper.GetRoleID(), ModuleID);

            ViewBag.AllowEdit = roleDetails.Contains("Edit");
            ViewBag.AllowCreate = roleDetails.Contains("Create");
            ViewBag.AllowFinishGood = roleDetails.Contains("ViewFinishGood");
            ViewBag.AllowALL = roleDetails.Contains("ViewALL");
        }
        private void SetViewBagNotification()
        {
            if (TempData["SuccessNotification"] != null)
                ViewBag.SuccessNotification = Convert.ToString(TempData["SuccessNotification"]);

            if (!string.IsNullOrEmpty(Request.QueryString["errorMessage"]))
                ViewBag.ErrorNotification = Convert.ToString(Request.QueryString["errorMessage"]);
        }
        private void SetPreDetailViewBag()
        {
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
            ViewBag.UnitList = new UnitBFC().Retrieve(true);
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
        }
        private void SetPreDetailDisplayViewBag(ProductModel header)
        {
            ViewBag.inventoryList = new ItemLocationBFC().RetrieveByProductID(header.ID);
            ViewBag.POList = new ProductBFC().RetrievePurchaseOrderDetailByProductID(header.ID);
            ViewBag.PDList = new ProductBFC().RetrievePurchaseDeliveryDetailByProductID(header.ID);
            ViewBag.BillList = new ProductBFC().RetrievePurchaseBillDetailByProductID(header.ID);
            ViewBag.SOList = new ProductBFC().RetrieveSalesOrderDetailByProductID(header.ID);
            ViewBag.DOList = new ProductBFC().RetrieveDeliveryOrderDetailByProductID(header.ID);
            ViewBag.InvoiceList = new ProductBFC().RetrieveInvoiceDetailByProductID(header.ID);
            ViewBag.itemAdjustList = new ProductBFC().RetrieveInventoryAdjustmentDetailByProductID(header.ID);
            ViewBag.WorkOrder = new WorkOrderBFC().RetreiveWorkOrderByproductID(header.ID);
            ViewBag.Build = new AssemblyBuildBFC().RetreiveBuildByproductID(header.ID);
        }
        private void VendorDetails()
        {
            List<VendorModel> vendorList = new List<VendorModel>();
            vendorList.AddRange(new VendorBFC().RetrieveAll());
            ViewBag.VendorList = vendorList;
        }
        public override MPL.Business.IMasterDetailBFC<ProductModel, ProductDetailModel> GetBFC()
        {
            return new ProductBFC();
        }
        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();

            return base.Index(startIndex, amount, sortParameter, filter);
        }
        public override void CreateData(ProductModel obj, List<ProductDetailModel> details)
        {
            try
            {
                SetViewBagPermission();
                AddIsSavedAttribute(details);

                obj.StockQty = details.Sum(p => p.Quantity);
                //obj.StockUnitID = obj.SaleUnitID = obj.PurchaseUnitID = obj.UnitTypeID;

               
                base.CreateData(obj, details);
              
                new ProductBFC().UpdateBasePrice(obj.ID, obj.basePriceDetails);
                new ProductBFC().UpdateLimitStock(obj.ID, obj.LimitStockDetails,obj);
                new ProductBFC().UpdateFormulasi(obj.ID, obj.FormulasiDetails, obj);
                //if (MembershipHelper.GetRoleID() == (int)PermissionStatus.root || MembershipHelper.GetRoleID() == (int)PermissionStatus.production)
                //{
                //    new ProductBFC().UpdateFormulasi(obj.ID, obj.FormulasiDetails, obj);
                //}
               
              
                TempData["SuccessNotification"] = "Dokumen berhasil  disimpan";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }
        public override void UpdateData(ProductModel obj, List<ProductDetailModel> details, FormCollection formCollection)
        {
            try
            {
                SetViewBagPermission();
                var oldProduct = new ProductBFC().RetrieveByID(obj.ID);

                AddIsSavedAttribute(details);

                obj.StockQty = oldProduct.StockQty + details.Where(p => p.ItemNo == 0).Sum(p => p.Quantity);
                //obj.StockUnitID = obj.SaleUnitID = obj.PurchaseUnitID = obj.UnitTypeID;

                formCollection["StockQty"] = obj.StockQty.ToString();

                base.UpdateData(obj, details, formCollection);
             
                new ProductBFC().UpdateBasePrice(obj.ID, obj.basePriceDetails);
                new ProductBFC().UpdateLimitStock(obj.ID, obj.LimitStockDetails,obj);
                new ProductBFC().UpdateFormulasi(obj.ID, obj.FormulasiDetails, obj);
                //if (MembershipHelper.GetRoleID() == (int)PermissionStatus.root || MembershipHelper.GetRoleID() == (int)PermissionStatus.production)
                //{
                //    new ProductBFC().UpdateFormulasi(obj.ID, obj.FormulasiDetails, obj);
                //}
             
                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }
        protected override void PreCreateDisplay(ProductModel header, List<ProductDetailModel> details)
        {
            SetViewBagPermission();
            header.Code = new ProductBFC().GetProductCode();
            VendorDetails();
            SetPreDetailViewBag();
            SetDetails(header);

            ViewBag.UnitLimitStock = new ProductBFC().RetreiveDetailUnit(header.UnitTypeID);
            //ViewBag.UnitFormulasi = new ProductBFC().RetreiveDetailUnit(header.UnitTypeID);
            ViewBag.StockUnit = new ProductBFC().RetreiveDetailUnit(header.UnitTypeID);
            ViewBag.SalesUnit = new ProductBFC().RetreiveDetailUnit(header.UnitTypeID);
            ViewBag.PurchaseUnit = new ProductBFC().RetreiveDetailUnit(header.UnitTypeID);
            
            base.PreCreateDisplay(header, details);
        }
        protected override void PreUpdateDisplay(ProductModel header, List<ProductDetailModel> details)
        {
            SetViewBagPermission();
            VendorDetails();
            SetPreDetailViewBag();

            ViewBag.inventoryList = new ItemLocationBFC().RetrieveByProductID(header.ID);
            
            //var fo = new FormulasiBFC().RetreiveByProductID(header.ID);
            
            SetDetails(header);

            ViewBag.UnitLimitStock = new ProductBFC().RetreiveDetailUnit(header.UnitTypeID);
            //ViewBag.UnitFormulasi = new ProductBFC().RetreiveDetailUnit(header.UnitTypeID);
            ViewBag.StockUnit = new ProductBFC().RetreiveDetailUnit(header.UnitTypeID);
            ViewBag.SalesUnit = new ProductBFC().RetreiveDetailUnit(header.UnitTypeID);
            ViewBag.PurchaseUnit = new ProductBFC().RetreiveDetailUnit(header.UnitTypeID);

            base.PreUpdateDisplay(header, details);
        }
        protected override void PreDetailDisplay(ProductModel header, List<ProductDetailModel> details)
        {

            SetViewBagNotification();

            SetViewBagPermission();

           
            SetPreDetailViewBag();
            ViewBag.inventoryList = new ItemLocationBFC().RetrieveByProductID(header.ID);

            //ViewBag.StockMovement = new LogBFC().RetrieveStockMovement(header.ID, 0).OrderBy(p => p.LogID);
           

            SetDetails(header);

            ViewBag.UnitLimitStock = new ProductBFC().RetreiveDetailUnit(header.UnitTypeID);
            //ViewBag.UnitFormulasi = new ProductBFC().RetreiveDetailUnit(header.UnitTypeID);
            ViewBag.StockUnit = new ProductBFC().RetreiveDetailUnit(header.UnitTypeID);
            ViewBag.SalesUnit = new ProductBFC().RetreiveDetailUnit(header.UnitTypeID);
            ViewBag.PurchaseUnit = new ProductBFC().RetreiveDetailUnit(header.UnitTypeID);

            //details.OrderByDescending(p => p.ItemNo).ToList();
            var details2 = new ProductBFC().RetrieveDetails(header.ID).OrderByDescending(p => p.PurchaseOrderCode);
            SetPreDetailDisplayViewBag(header);
            base.PreDetailDisplay(header, details2.ToList());
        }

    }
}
