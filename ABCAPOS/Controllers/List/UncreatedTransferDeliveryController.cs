using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.Models;
using MPL.MVC;
using ABCAPOS.BF;

namespace ABCAPOS.Controllers.List
{
    public class UncreatedTransferDeliveryController : MasterDetailController<TransferOrderModel, TransferOrderDetailModel>
    {
        private void SetIndexViewBag()
        {
            ViewBag.WarehouseList = new WarehouseBFC().RetrieveAll();
        }

        public override MPL.Business.IMasterDetailBFC<TransferOrderModel, TransferOrderDetailModel> GetBFC()
        {
            return new TransferOrderBFC();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetIndexViewBag();

            var toCount = 0;
            var toList = new List<TransferOrderModel>();

            //var moduleID = GetModuleID(type);

            //if (string.IsNullOrEmpty(moduleID))
            //    return Redirect("/");

            if (startIndex == null)
                startIndex = 0;

            if (amount == null)
                amount = 20;

            if (string.IsNullOrEmpty(sortParameter))
                sortParameter = "";

            //var userName = MembershipHelper.GetUserName();
            //var permission = AuthorizationUtil.GetUserPermission(userName, moduleID, "View");

            //if (permission == UserPermission.None)
            //    return Redirect("/");

            toCount = new TransferOrderBFC().RetrieveUncreatedTransferDeliveryCount(filter.GetSelectFilters());
            toList = new TransferOrderBFC().RetrieveUncreatedTransferDelivery((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters());

            ViewBag.DataCount = toCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            ViewBag.PageSeriesSize = GetPageSeriesSize();

            return View(toList);
        }
    }
}
