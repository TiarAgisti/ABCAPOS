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

namespace ABCAPOS.Controllers
{
    public class PurchaseDeliveryController : MasterDetailController<PurchaseDeliveryModel, PurchaseDeliveryDetailModel>
    {
        private string ModuleID
        {
            get
            {
                return "PurchaseDelivery";
            }
        }

        private void SetViewBagPermission()
        {
            var roleDetails = new RoleBFC().RetrieveActions(MembershipHelper.GetRoleID(), ModuleID);

            ViewBag.AllowEdit = roleDetails.Contains("Edit");
            ViewBag.AllowCreate = roleDetails.Contains("Create");
            ViewBag.AllowVoid = roleDetails.Contains("Void");
        }

        private void SetViewBagNotification()
        {
            if (TempData["SuccessNotification"] != null)
                ViewBag.SuccessNotification = Convert.ToString(TempData["SuccessNotification"]);

            if (!string.IsNullOrEmpty(Request.QueryString["errorMessage"]))
                ViewBag.ErrorNotification = Convert.ToString(Request.QueryString["errorMessage"]);
        }

        private void SetPreEditViewBag(PurchaseDeliveryModel obj, long purchaseOrderID)
        {
            var template = "";

            ViewBag.RowEditor = template;
        }

        private void SetPreEditDetailViewBag(PurchaseDeliveryModel header, List<PurchaseDeliveryDetailModel> details)
        {
            foreach (var detail in details)
            {
                detail.StrQuantity = detail.Quantity.ToString("N2");
                detail.QtyReceive = detail.QtyReceive - detail.Quantity;
            }
        }

        public override MPL.Business.IMasterDetailBFC<PurchaseDeliveryModel, PurchaseDeliveryDetailModel> GetBFC()
        {
            return new PurchaseDeliveryBFC();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();

            return base.Index(startIndex, amount, sortParameter, filter);
        }

        protected override void PreCreateDisplay(PurchaseDeliveryModel header, List<PurchaseDeliveryDetailModel> details)
        {
            SetViewBagNotification();
            ViewBag.BinList = new BinBFC().RetrieveAll();
            var purchaseOrderID = Convert.ToInt64(Request.QueryString["purchaseOrderID"]);

            new PurchaseDeliveryBFC().CreateByPurchaseOrder(header, purchaseOrderID);

            base.PreCreateDisplay(header, header.Details);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreatePurchaseDelivery(PurchaseDeliveryModel obj, FormCollection col)
        {
            try
            {

                new PurchaseDeliveryBFC().Validate(obj, obj.Details);

                base.Create(obj);

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";

                return RedirectToAction("Detail", new { key = obj.ID });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;
                ViewBag.Mode = UIMode.Create;
                SetViewBagPermission();

                return RedirectToAction("Create", new { purchaseOrderID = obj.PurchaseOrderID, errorMessage = ex.Message });
            }

        }

        [HttpPost, ValidateInput(false)]
        public ActionResult UpdatePurchaseDelivery(PurchaseDeliveryModel obj, List<PurchaseDeliveryDetailModel> details, FormCollection col)
        {
            try
            {
                // different validation logic when updating
                new PurchaseDeliveryBFC().UpdateValidation(obj, obj.Details);

                base.Update(obj, col);
                TempData["SuccessNotification"] = "Dokumen berhasil diupdate";
                return RedirectToAction("Detail", new { key = obj.ID });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;
                ViewBag.Mode = UIMode.Update;
                SetViewBagPermission();

                return RedirectToAction("Update", new { key = obj.ID, errorMessage = ex.Message });
            }
        }

        //public override void UpdateData(PurchaseDeliveryModel obj, List<PurchaseDeliveryDetailModel> details, FormCollection formCollection)
        //{
        //    try
        //    {
        //        new PurchaseDeliveryBFC().UpdateValidation(obj, obj.Details);
        //        base.UpdateData(obj, details, formCollection);

        //        TempData["SuccessNotification"] = "Dokumen berhasil diupdate";
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.ErrorNotification = ex.Message;
        //        ViewBag.Mode = UIMode.Update;
        //        SetViewBagPermission();
        //        throw;

        //        //return RedirectToAction("Update", new { key = obj.ID, errorMessage = ex.Message });
        //    }
            
        //}
       
        protected override void PreUpdateDisplay(PurchaseDeliveryModel header, List<PurchaseDeliveryDetailModel> details)
        {
            SetViewBagNotification();
            SetPreEditDetailViewBag(header, details);
            foreach (var detail in details)
            {
                var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                detail.StockQty = detail.StockQty / unitRate;
            }

            //header.LogStockDetails = new LogStockBFC().RetreiveByLogStockID(header.LogStockID);
            base.PreUpdateDisplay(header, details);
        }

        protected override void PreDetailDisplay(PurchaseDeliveryModel header, List<PurchaseDeliveryDetailModel> details)
        {
            
            SetViewBagNotification();

            SetViewBagPermission();
            foreach (var detail in details)
            {
                var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                detail.StockQty = detail.StockQty / unitRate;
            }
            base.PreDetailDisplay(header, details);
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
                var pd = new PurchaseDeliveryBFC().RetrieveByID(key);

                ViewBag.VoidFromIndex = voidFromIndex;

                return View(pd);
            }

            return View();
        }

        [HttpPost]
        public ActionResult VoidRemarks(PurchaseDeliveryModel purchaseDelivery, FormCollection col)
        {
            var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);

            try
            {
                new PurchaseDeliveryBFC().Void(purchaseDelivery.ID, purchaseDelivery.VoidRemarks, MembershipHelper.GetUserName());

                //TempData["SuccessNotification"] = "Document has been canceled";

                return RedirectToAction("Index");

                //if (voidFromIndex)
                //    return RedirectToAction("Index");
                //else
                //    return RedirectToAction("Detail", new { key = purchaseDelivery.ID });
            }
            catch (Exception ex)
            {
                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = purchaseDelivery.ID, errorMessage = ex.Message });
            }

        }
    }
}
