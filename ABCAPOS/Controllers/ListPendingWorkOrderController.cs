using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.Models;
using MPL.MVC;
using ABCAPOS.BF;
using ABCAPOS.Util;
using ABCAPOS.Helpers;

namespace ABCAPOS.Controllers
{
    public class ListPendingWorkOrderController : MasterDetailController<ProductModel,ProductDetailModel>
    {
        private string ModuleID
        {
            get
            {
                return "WorkOrder";
            }
        }

        private void SetViewBagPermission()
        {
            var roleDetails = new RoleBFC().RetrieveActions(MembershipHelper.GetRoleID(), ModuleID);
            ViewBag.AllowViewFG = roleDetails.Contains("ViewFinishGood");
            ViewBag.AllowViewALL = roleDetails.Contains("ViewALL");
        }

        public override MPL.Business.IMasterDetailBFC<ProductModel, ProductDetailModel>GetBFC()
        {
            return new ProductBFC();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            this.SetViewBagPermission();
            var woCount = 0;
            var woList = new List<ProductModel>();


            if (startIndex == null)
                startIndex = 0;

            if (amount == null)
                amount = 20;

            if (string.IsNullOrEmpty(sortParameter))
                sortParameter = "";

            woCount = new ProductBFC().RetreiveListPendingWorkOrderCount(filter.GetSelectFilters(), ViewBag);
            woList = new ProductBFC().RetrieveListPendingWorkOrder((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters(), ViewBag);

            //if (MembershipHelper.GetRoleID() == (int)PermissionStatus.AdminProduksi)
            //{
            //    woCount = new ProductBFC().RetreiveListPendingWorkOrderCountFinishGood(filter.GetSelectFilters());
            //    woList = new ProductBFC().RetreiveListPendingWorkOrderFinishGood((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters());
            //}
            //else
            //{
            //    woCount = new ProductBFC().RetreiveListPendingWorkOrderCount(filter.GetSelectFilters(),ViewBag);
            //    woList = new ProductBFC().RetrieveListPendingWorkOrder((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters(),ViewBag);
            //}
         

            ViewBag.DataCount = woCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            ViewBag.PageSeriesSize = GetPageSeriesSize();
            ViewBag.WarehouseList = new WarehouseBFC().RetrieveAll();

            return View(woList);
        }

    }
}
