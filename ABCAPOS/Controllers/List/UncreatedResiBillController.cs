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
    public class UncreatedResiBillController : MasterDetailController<ResiModel, ResiDetailModel>
    {

        public override MPL.Business.IMasterDetailBFC<ResiModel, ResiDetailModel> GetBFC()
        {
            return new ResiBFC();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            var poCount = 0;
            var poList = new List<ResiModel>();

            if (startIndex == null)
                startIndex = 0;

            if (amount == null)
                amount = 20;

            if (string.IsNullOrEmpty(sortParameter))
                sortParameter = "";

           poCount = new ResiBFC().RetrieveUncreatedResiBillCount(filter.GetSelectFilters());
            poList = new ResiBFC().RetrieveUncreatedResiBill((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters());

            ViewBag.DataCount = poCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            ViewBag.PageSeriesSize = GetPageSeriesSize();

            return View(poList);
        }
    }
}
