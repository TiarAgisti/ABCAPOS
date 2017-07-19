using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.Models;
using MPL.MVC;
using ABCAPOS.BF;
using ABCAPOS.Util;
using ABCAPOS.Helpers;

namespace ABCAPOS.Controllers
{
    public class PurchaseReturnController : MasterDetailController<PurchaseReturnModel, PurchaseReturnDetailModel>
    {
        private string ModuleID
        {
            get
            {
                return "PurchaseReturn";
            }
        }

        private void SetViewBagPermission()
        {
            var roleDetails = new RoleBFC().RetrieveActions(MembershipHelper.GetRoleID(), ModuleID);

            ViewBag.AllowEdit = roleDetails.Contains("Edit");
            ViewBag.AllowCreate = roleDetails.Contains("Create");
            ViewBag.AllowApprove = roleDetails.Contains("Approve");
            ViewBag.AllowVoid = roleDetails.Contains("Void");
        }

        private void SetViewBagNotification()
        {
            if (TempData["SuccessNotification"] != null)
                ViewBag.SuccessNotification = Convert.ToString(TempData["SuccessNotification"]);

            if (!string.IsNullOrEmpty(Request.QueryString["errorMessage"]))
                ViewBag.ErrorNotification = Convert.ToString(Request.QueryString["errorMessage"]);
        }

        public override MPL.Business.IMasterDetailBFC<PurchaseReturnModel, PurchaseReturnDetailModel> GetBFC()
        {
            return new PurchaseReturnBFC();
        }

        private void SetPreEditViewBag()
        {
            //ViewBag.ConversionList = new ProductBFC().RetrieveAllConversion();
        }

        protected override void PreCreateDisplay(PurchaseReturnModel header, List<PurchaseReturnDetailModel> details)
        {
            var purchaseOrderID = Request.QueryString["purchaseOrderID"];

            header.PurchaseOrderID = Convert.ToInt64(purchaseOrderID);

            if (!string.IsNullOrEmpty(purchaseOrderID))
                new PurchaseReturnBFC().CreateByPurchaseOrder(header, Convert.ToInt64(purchaseOrderID));

            header.Code = new PurchaseReturnBFC().GetPurchaseReturnCode();

            SetPreEditViewBag();

            base.PreCreateDisplay(header, header.Details);
        }

        protected override void PreDetailDisplay(PurchaseReturnModel header, List<PurchaseReturnDetailModel> details)
        {
            SetViewBagNotification();

            SetViewBagPermission();

            base.PreDetailDisplay(header, details);
        }

        protected override void PreUpdateDisplay(PurchaseReturnModel header, List<PurchaseReturnDetailModel> details)
        {
            SetPreEditViewBag();

            base.PreUpdateDisplay(header, details);
        }

        public override void CreateData(PurchaseReturnModel obj, List<PurchaseReturnDetailModel> details)
        {
            try
            {
                new PurchaseReturnBFC().Validate(obj, details);

                base.CreateData(obj, details);

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

        public override void UpdateData(PurchaseReturnModel obj, List<PurchaseReturnDetailModel> details, FormCollection formCollection)
        {
            try
            {
                new PurchaseReturnBFC().Validate(obj, details);
                obj.Details = details;

                base.UpdateData(obj, obj.Details, formCollection);

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

        protected override List<Button> GetAdditionalButtons(PurchaseReturnModel header, List<PurchaseReturnDetailModel> details, UIMode mode)
        {
            var list = new List<Button>();

            //if (mode == UIMode.Detail && header.Status == (int)MPL.DocumentStatus.Approved)
            //{
            //    var print = new Button();
            //    print.Text = "Cetak";
            //    print.CssClass = "button";
            //    print.ID = "btnPrint";
            //    print.OnClick = String.Format("window.open('{0}');", Url.Action("PopUp", "ReportViewer",
            //        new { type = ReportViewerController.PrintOutType.PurchaseReturn, queryString = SystemConstants.str_PurchaseReturnID + "=" + header.ID }));
            //    print.Href = "#";
            //    list.Add(print);

            //}

            return list;
        }
        public void SetIndexViewBag()
        {
            //ViewBag.SupplierGroupList = new SupplierGroupBFC().RetrieveAll();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();
            SetIndexViewBag();

            return base.Index(startIndex, amount, sortParameter, filter);
        }

        public override void ApproveData(string key)
        {
            new PurchaseReturnBFC().Approve(Convert.ToInt64(key), MembershipHelper.GetUserName());
        }

        public ActionResult ApproveFromIndex(string key)
        {
            base.ApproveData(key);

            return RedirectToAction("Index");
        }

        public ActionResult VoidFromIndex(string key)
        {
            base.VoidData(key);

            return RedirectToAction("Index");
        }

        public ActionResult VoidRemarks(string key, bool voidFromIndex)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var purchasereturn = new PurchaseReturnBFC().RetrieveByID(key);

                ViewBag.VoidFromIndex = voidFromIndex;

                return View(purchasereturn);
            }

            return View();
        }

        [HttpPost]
        public ActionResult VoidRemarks(PurchaseReturnModel purchaseReturn, FormCollection col)
        {
            var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);

            try
            {
                new PurchaseReturnBFC().Void(purchaseReturn.ID, purchaseReturn.VoidRemarks, MembershipHelper.GetUserName());

                //new EmailHelper().SendVoidPurchaseReturnEmail(purchaseReturn.ID, purchaseReturn.VoidRemarks, MembershipHelper.GetUserName());

                TempData["SuccessNotification"] = "Dokumen berhasil dibatalkan";

                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = purchaseReturn.ID });
            }
            catch (Exception ex)
            {
                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = purchaseReturn.ID, errorMessage = ex.Message });
            }

        }
    }
}
