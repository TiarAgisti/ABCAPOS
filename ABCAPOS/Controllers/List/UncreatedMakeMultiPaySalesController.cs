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
    public class UncreatedMakeMultiPaySalesController : GenericController<MakeMultiPaySalesModel>
    {
        public override MPL.Business.IGenericBFC<MakeMultiPaySalesModel> GetBFC()
        {
            throw new NotImplementedException();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {

            var poCount = 0;
            var poList = new List<InvoiceModel>();

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

            poCount = new InvoiceBFC().RetrieveUncreatedPaymentCount(filter.GetSelectFilters());
            poList = new InvoiceBFC().RetrieveUncreatedPayment((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters());

            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            ViewBag.DataCount = poCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            ViewBag.PageSeriesSize = GetPageSeriesSize();

            return View(poList);
        }
    }
}
