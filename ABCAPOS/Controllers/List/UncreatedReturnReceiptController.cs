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
    public class UncreatedReturnReceiptController : MasterDetailController<CustomerReturnModel, CustomerReturnDetailModel>
    {

        public override MPL.Business.IMasterDetailBFC<CustomerReturnModel, CustomerReturnDetailModel> GetBFC()
        {
            return new CustomerReturnBFC();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            var poCount = 0;
            var poList = new List<CustomerReturnModel>();

            if (startIndex == null)
                startIndex = 0;

            if (amount == null)
                amount = 20;

            if (string.IsNullOrEmpty(sortParameter))
                sortParameter = "";

            poCount = new CustomerReturnBFC().RetrieveUncreatedReturnReceiptCount(filter.GetSelectFilters());
            poList = new CustomerReturnBFC().RetrieveUncreatedReturnReceipt((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters());

            ViewBag.DataCount = poCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            ViewBag.PageSeriesSize = GetPageSeriesSize();

            return View(poList);
        }
    }
}
