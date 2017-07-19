using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.Controllers.GenericController;
using ABCAPOS.Models;
using ABCAPOS.BF;
using MPL.Business;
using MPL.MVC;

namespace ABCAPOS.Controllers.List
{
    public class UncreatedResiPaymentController : PSIMasterDetailController<ResiBillModel,ResiBillDetailModel>
    {
        private string ModuleID
        {
            get
            {
                return "ResiPayment";
            }
        }

        public override IMasterDetailBFC<ResiBillModel, ResiBillDetailModel> GetBFC()
        {
            return new ResiBillBFC();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            var count = 0;
            var List = new List<ResiBillModel>();

            if (startIndex == null)
                startIndex = 0;

            if (amount == null)
                amount = 20;

            if (string.IsNullOrEmpty(sortParameter))
                sortParameter = "";

            count = new ResiBillBFC().RetrieveUncreatedResiPaymentCount(filter.GetSelectFilters());
            List = new ResiBillBFC().RetrieveUncreatedResiPayment((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters());

            ViewBag.DataCount = count;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;

            return View(List);
        }
    }
}
