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

namespace ABCAPOS.Controllers.List
{
    public class UncreatedBuildController : MasterDetailController<WorkOrderModel,WorkOrderDetailModel>
    {
        private void SetIndexViewBag()
        {
            ViewBag.WarehouseList = new WarehouseBFC().RetrieveAll();
        }

        private string ModuleID
        {
            get
            {
                return "AssemblyBuild";
            }
        }

        private void SetViewBagPermission()
        {
            var roleDetails = new RoleBFC().RetrieveActions(MembershipHelper.GetRoleID(), ModuleID);
            ViewBag.AllowViewFG = roleDetails.Contains("ViewFinishGood");
            ViewBag.AllowViewALL = roleDetails.Contains("ViewALL");
        }

        public override MPL.Business.IMasterDetailBFC<WorkOrderModel, WorkOrderDetailModel> GetBFC()
        {
            return new WorkOrderBFC();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            this.SetViewBagPermission();
            SetIndexViewBag();

            var woCount = 0;
            var woList = new List<WorkOrderModel>();

            if (startIndex == null)
                startIndex = 0;

            if (amount == null)
                amount = 20;

            if (string.IsNullOrEmpty(sortParameter))
                sortParameter = "";

            woCount = new WorkOrderBFC().RetreiveCount(filter.GetSelectFilters(), false, ViewBag);
            woList = new WorkOrderBFC().Retreive((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters(), false, ViewBag);

            //if (MembershipHelper.GetRoleID() == (int)PermissionStatus.AdminProduksi)
            //{
            //    woCount = new WorkOrderBFC().RetreiveCountFG(filter.GetSelectFilters(), false);
            //    woList = new WorkOrderBFC().RetreiveFG((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters(), false);
            //}
            //else
            //{
            //    woCount = new WorkOrderBFC().RetreiveCount(filter.GetSelectFilters(), false);
            //    woList = new WorkOrderBFC().Retreive((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters(), false);
            //}
          

            ViewBag.DataCount = woCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            ViewBag.PageSeriesSize = GetPageSeriesSize();

            return View(woList);
        }

    }
}
