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
    public class UncreatedPurchasePaymentController : GenericController<PurchaseOrderModel>
    {
        
        public override MPL.Business.IGenericBFC<PurchaseOrderModel> GetBFC()
        {
            throw new NotImplementedException();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            //SetIndexViewBag();

            var poCount = 0;
            var poList = new List<PurchaseBillModel>();

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

            poCount = new PurchaseBillBFC().RetrieveUncreatedPurchasePaymentBillCount(filter.GetSelectFilters());
            poList = new PurchaseBillBFC().RetrieveUncreatedPurchasePaymentBill((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters());

            ViewBag.DataCount = poCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            ViewBag.PageSeriesSize = GetPageSeriesSize();

            return View(poList);
        }
    }
}
