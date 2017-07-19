using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.BF;
using ABCAPOS.Util;
using ABCAPOS.Models;
using MPL.MVC;
using ABCAPOS.Helpers;
using System.Transactions;
using ABCAPOS.Controllers.GenericController;

namespace ABCAPOS.Controllers
{
    public class ResiBillController : PSIMasterDetailController<ResiBillModel,ResiBillDetailModel>
    {
        private void SetViewBagPermission()
        {
            var roleDetails = new RoleBFC().RetrieveActions(MembershipHelper.GetRoleID(), ModuleID);
            ViewBag.AllowEdit = roleDetails.Contains("Edit");
            ViewBag.AllowCreate = roleDetails.Contains("Create");
            ViewBag.AllowVoid = roleDetails.Contains("Void");
            ViewBag.AllowViewFG = roleDetails.Contains("ViewFinishGood");
            ViewBag.AllowViewALL = roleDetails.Contains("ViewALL");
        }

       public ResiBillController()
        {
            base.ModuleID = "ResiBill";
        }

        public override MPL.Business.IMasterDetailBFC<ResiBillModel, ResiBillDetailModel> GetBFC()
        {
            return new ResiBillBFC();
        }

        protected override void PreCreateDisplay(ResiBillModel header, List<ResiBillDetailModel> details)
        {
            var resiID = Convert.ToInt64(Request.QueryString["ResiID"]);
            new ResiBillBFC().PrepareByResi(header, resiID);
            header.Code = "";
            base.PreCreateDisplay(header, details);
        }

        protected override void PreDetailDisplay(ResiBillModel header, List<ResiBillDetailModel> details)
        {
            header.TotalPriceAmount = details.Sum(e => e.Amount);
            base.PreDetailDisplay(header, details);
        }

        protected override void SetPreview(ResiBillModel header, List<ResiBillDetailModel> details)
        {
            ViewBag.TermsList = new PurchaseOrderBFC().RetrieveAllTerms();
            ViewBag.CurrencyList = new CurrencyBFC().RetrieveAll();

            var paymentList = new ResiPaymentBFC().RetrieveResiPaymentDetailByResiBillID(header.ID);
            header.BeforePaymentAmount = paymentList.Sum(p => p.Amount);
            header.DiscountTaken = paymentList.Sum(p => p.DiscountTaken);
            ViewBag.ResiPayment = paymentList;

            base.SetPreview(header, details);
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();
           

            if (startIndex == null)
                startIndex = 0;

            if (amount == null)
                amount = 20;

            if (string.IsNullOrEmpty(sortParameter))
                sortParameter = "";

            var Count = new ResiBillBFC().RetrieveCount(filter.GetSelectFilters(), false);
            var List = new ResiBillBFC().Retrieve((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters(), false);

            ResetBackToListUrl(filter);

            ViewBag.DataCount = Count;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            ViewBag.PageSeriesSize = GetPageSeriesSize();

            return View(List);
        }


        public override void ApproveData(string key)
        {

            try
            {
                new ResiBillBFC().Approve(Convert.ToInt32(key), MembershipHelper.GetUserName());

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
            try
            {
                ApproveData(key);
                //UIMode mode;
                //TempData["SuccessNotification"] = "Document has been approved";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

        public ActionResult VoidRemarks(string key, bool voidFromIndex)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var AssemblyBuild = new ResiBillBFC().RetrieveByID(key);

                ViewBag.VoidFromIndex = voidFromIndex;

                return View(AssemblyBuild);
            }
            return View();
        }

        [HttpPost]
        public ActionResult VoidRemarks(ResiBillModel header, FormCollection col)
        {
            var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);

            try
            {
                //new AssemblyBuildBFC().VoidWorkOrder(header.WorkOrderID);

                new ResiBillBFC().Void(header.ID, header.VoidRemarks, MembershipHelper.GetUserName());

                //TempData["SuccessNotification"] = "Document has been canceled";

                return RedirectToAction("Index");
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
