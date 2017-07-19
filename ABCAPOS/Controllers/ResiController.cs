using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.BF;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using ABCAPOS.Controllers.GenericController;
using MPL.Business;
using System.Transactions;

namespace ABCAPOS.Controllers
{
    public class ResiController : PSIMasterDetailController<ResiModel,ResiDetailModel>
    {
        private void OtherDetailFunction(ResiModel header, List<ResiDetailModel> details, string ModuleID)
        {
            List<ResiPriceDetailModel> ResiPriceDetail = header.ResiPriceDetails;
            header = new ResiBFC().RetrieveByID(header.ID);
            new ResiPriceDetailBFC().Update(header, ResiPriceDetail);
        }
        protected override void SetPreview(ResiModel header, List<ResiDetailModel> details)
        {
            if (header.ID != 0)
            {
                header.ResiPriceDetails = new ResiPriceDetailBFC().RetrieveDetails(header.ID);
            }
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            //var test = new ResiPaymentBFC().RetrieveResiPaymentDetailByResiID(header.ID);
            ViewBag.ResiBillDetail = new ResiBillBFC().RetrieveResiBillDetailByResiID(header.ID);
            ViewBag.InvoiceResi = new InvoiceBFC().RetrieveInvoiceResiByResiID(header.ID);
            base.SetPreview(header, details);
        }
        public ResiController()
        {
            base.ModuleID = "Resi";
        }
        public override IMasterDetailBFC<ResiModel, ResiDetailModel> GetBFC()
        {
            return new ResiBFC();
        }
        public override void CreateDataFunction(ResiModel header, List<ResiDetailModel> details, string ModuleID)
        {
            base.CreateDataFunction(header, details, ModuleID);
            this.OtherDetailFunction(header, details, ModuleID);
        }
        public override void UpdateDataFunction(ResiModel header, List<ResiDetailModel> details, FormCollection formCollection, string ModuleID)
        {
            base.UpdateDataFunction(header, details, formCollection, ModuleID);
            this.OtherDetailFunction(header, details, ModuleID);
        }
        protected override void PreCreateDisplay(ResiModel header, List<ResiDetailModel> details)
        {
            new ResiBFC().SetDateFromDateTo(header);

            var customerID = Convert.ToInt64(Request.QueryString["customerID"]);
            var expeditionID = Convert.ToInt64(Request.QueryString["expeditionID"]);
            var dateFrom = Request.QueryString["dateFrom"];
            var dateTo = Request.QueryString["dateTo"];
            var date = Request.QueryString["date"];
            var resiCode = Request.QueryString["resiCode"];

            new ResiBFC().PrepareByCustomerID(header, customerID, expeditionID, dateFrom, dateTo, date, resiCode);

            if (string.IsNullOrEmpty(resiCode))
                header.Code = "";

            base.PreCreateDisplay(header, details);
        }
        public override void ApproveData(string key)
        {
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    new ResiBFC().ApproveResi(key, MembershipHelper.GetUserName(), MembershipHelper.GetRoleID());
                    trans.Complete();
                }
                TempData["SuccessNotification"] = "Document has been approved";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }
        public ActionResult ApproveFromIndex(string key)
        {
            this.ApproveData(key);
            return base.RedirectToAction("Index", new { key = key });
        }
        public ActionResult VoidRemarks(string key, bool voidFromIndex)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var obj = new ResiBFC().RetrieveByID(Convert.ToUInt64(key));
                ViewBag.VoidFromIndex = voidFromIndex;
                return View(obj);
            }
            return View();
        }
        [HttpPost]
        public ActionResult VoidRemarks(ResiModel header,FormCollection col)
        {
            var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    new ResiBFC().VoidResi(header, MembershipHelper.GetUserName());
                    trans.Complete();
                }
                TempData["SuccessNotification"] = "Document has been canceled";

                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = header.ID });

            }
            catch (Exception ex)
            {
                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = header.ID, errorMessage = ex.Message });
            }
        }
    }
}
