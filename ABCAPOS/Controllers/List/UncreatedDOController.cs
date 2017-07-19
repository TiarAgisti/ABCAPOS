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
    public class UncreatedDOController : MasterDetailController<SalesOrderModel, SalesOrderDetailModel>
    {
        private void SetIndexViewBag()
        {
            ViewBag.CustomerGroupList = new CustomerGroupBFC().RetrieveAll();
        }

        public override MPL.Business.IMasterDetailBFC<SalesOrderModel, SalesOrderDetailModel> GetBFC()
        {
            return new SalesOrderBFC();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetIndexViewBag();

            var soCount = 0;
            var soList = new List<SalesOrderModel>();


            if (startIndex == null)
                startIndex = 0;

            if (amount == null)
                amount = 20;

            if (string.IsNullOrEmpty(sortParameter))
                sortParameter = "";


            soCount = new SalesOrderBFC().RetrieveUncreatedDeliveryOrderCount(filter.GetSelectFilters());
            soList = new SalesOrderBFC().RetrieveUncreatedDeliveryOrder((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters());

            ViewBag.DataCount = soCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            ViewBag.PageSeriesSize = GetPageSeriesSize();

            return View(soList);
        }
    }
}
