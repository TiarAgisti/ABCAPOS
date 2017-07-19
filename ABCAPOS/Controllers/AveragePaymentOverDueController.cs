using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.Models;
using MPL.MVC;
using ABCAPOS.BF;
using ABCAPOS.Helpers;
using ABCAPOS.Util;
using ABCAPOS.Controllers.GenericController;

namespace ABCAPOS.Controllers
{
    public class AveragePaymentOverDueController : Controller
    {
        private string ModuleID
        {
            get
            {
                return "AveragePaymentOverDue";
            }
        }

        private void SetIndexViewBag()
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
        }

        private void SetViewBagPermission()
        {
            var roleDetails = new RoleBFC().RetrieveActions(MembershipHelper.GetRoleID(), ModuleID);

            ViewBag.AllowView = roleDetails.Contains("View");
        }

        public ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();
            SetIndexViewBag();

            var invoiceCount = 0;
            var invoiceList = new List<InvoiceModel>();

            if (startIndex == null)
                startIndex = 0;

            if (amount == null)
                amount = 20;

            if (string.IsNullOrEmpty(sortParameter))
                sortParameter = "";

            invoiceCount = new InvoiceBFC().RetrieveAveragePaymentOverDueCount(filter.GetSelectFilters());
            invoiceList = new InvoiceBFC().RetrieveAveragePaymentOverDue((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters());

            ViewBag.DataCount = invoiceCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            //ViewBag.PageSeriesSize = GetPageSeriesSize();

            return View(invoiceList);
        }

    }
}
