using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.BF;
using ABCAPOS.Util;
using ABCAPOS.Models;
using MPL.MVC;

namespace ABCAPOS.Controllers.List
{
    public class UncreatedInvoiceController : MasterDetailController<SalesOrderModel, SalesOrderDetailModel>
    {

        public override MPL.Business.IMasterDetailBFC<SalesOrderModel, SalesOrderDetailModel> GetBFC()
        {
            return new SalesOrderBFC();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            var soCount = 0;
            var soList = new List<SalesOrderModel>();

            if (startIndex == null)
                startIndex = 0;

            if (amount == null)
                amount = 20;

            if (string.IsNullOrEmpty(sortParameter))
                sortParameter = "";

            soCount = new SalesOrderBFC().RetrieveUncreatedInvoiceCount(filter.GetSelectFilters());
            soList = new SalesOrderBFC().RetrieveUncreatedInvoice((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters());

            ViewBag.DataCount = soCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            ViewBag.PageSeriesSize = GetPageSeriesSize();

            return View(soList);
        }
    }
}
