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
    public class UncreatedVendorReturnDeliveryController : MasterDetailController<VendorReturnModel, VendorReturnDetailModel>
    {
        private void SetIndexViewBag()
        {
            ViewBag.CustomerGroupList = new CustomerGroupBFC().RetrieveAll();
        }

        public override MPL.Business.IMasterDetailBFC<VendorReturnModel, VendorReturnDetailModel> GetBFC()
        {
            return new VendorReturnBFC();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetIndexViewBag();

            var soCount = 0;
            var soList = new List<VendorReturnModel>();

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

            soCount = new VendorReturnBFC().RetrieveUncreatedVendorReturnDeliveryCount(filter.GetSelectFilters());
            soList = new VendorReturnBFC().RetrieveUncreatedVendorReturnDelivery((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters());

            ViewBag.DataCount = soCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            ViewBag.PageSeriesSize = GetPageSeriesSize();

            return View(soList);
        }
    }
}
