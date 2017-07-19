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
    public class UncreatedMultipleInvoicingController : MasterDetailController<InvoiceModel, InvoiceDetailModel>
    {

        public override MPL.Business.IMasterDetailBFC<InvoiceModel, InvoiceDetailModel> GetBFC()
        {
            return new InvoiceBFC();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            var invCount = 0;
            var invList = new List<InvoiceModel>();

            if (startIndex == null)
                startIndex = 0;

            if (amount == null)
                amount = 20;

            if (string.IsNullOrEmpty(sortParameter))
                sortParameter = "";

            invCount = new InvoiceBFC().RetrieveUncreatedMultipleInvoicingCount(filter.GetSelectFilters());
            invList = new InvoiceBFC().RetrieveUncreatedMultipleInvoicing((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters());

            ViewBag.DataCount = invCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            ViewBag.PageSeriesSize = GetPageSeriesSize();

            return View(invList);
        }

    }
}
