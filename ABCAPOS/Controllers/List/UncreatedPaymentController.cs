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
    public class UncreatedPaymentController : GenericController<InvoiceModel>
    {
        private void SetIndexViewBag()
        {
            ViewBag.CustomerGroupList = new CustomerGroupBFC().RetrieveAll();
        }

        public override MPL.Business.IGenericBFC<InvoiceModel> GetBFC()
        {
            throw new NotImplementedException();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetIndexViewBag();

            var invoiceCount = 0;
            var invoiceList = new List<InvoiceModel>();

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

            invoiceCount = new InvoiceBFC().RetrieveUncreatedPaymentCount(filter.GetSelectFilters());
            invoiceList = new InvoiceBFC().RetrieveUncreatedPayment((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters());

            ViewBag.DataCount = invoiceCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            ViewBag.PageSeriesSize = GetPageSeriesSize();

            return View(invoiceList);
        }
    }
}
