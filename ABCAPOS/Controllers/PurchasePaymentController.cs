using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.Models;
using MPL.MVC;
using ABCAPOS.BF;
using ABCAPOS.DA;
using ABCAPOS.Helpers;

namespace ABCAPOS.Controllers
{
    public class PurchasePaymentController : GenericTransactionController<PurchasePaymentModel>
    {
        private string ModuleID
        {
            get
            {
                return "Payment";
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

        public override MPL.Business.IGenericTransactionBFC<PurchasePaymentModel> GetBFC()
        {
            return new PurchasePaymentBFC();
        }

        private void SetIndexViewBag()
        {
        }

        private void SetPreCreateViewBag(PurchasePaymentModel header)
        {
            //header.ExchangeRate = new ABCAPOSDAC().RetrieveRate();
        }

        protected override void PreCreateDisplay(PurchasePaymentModel header)
        {
            var billID = Convert.ToInt64(Request.QueryString["purchaseBillID"]);

            new PurchasePaymentBFC().CreateByBill(header, billID);

            SetPreCreateViewBag(header);

            base.PreCreateDisplay(header);
        }

        protected override void PreDetailDisplay(PurchasePaymentModel header)
        {
            SetViewBagNotification();

            SetViewBagPermission();
            var purchasePaymentList = new PurchasePaymentBFC().RetrieveByBill(header.PurchaseBillID);
            header.SisaAmount = header.PurchaseBillAmount - purchasePaymentList.Sum(p => p.Amount);
            base.PreDetailDisplay(header);
        }

        protected override void PreUpdateDisplay(PurchasePaymentModel header)
        {
            //SetPreEditViewBag();

            base.PreUpdateDisplay(header);
        }

        public override void CreateData(PurchasePaymentModel obj)
        {
            try
            {
                new PurchasePaymentBFC().Validate(obj);

                base.CreateData(obj);

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

        public override void UpdateData(PurchasePaymentModel obj, FormCollection formCollection)
        {
            try
            {
                new PurchasePaymentBFC().Validate(obj);

                base.UpdateData(obj, formCollection);

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

        public override void ApproveData(string key)
        {
            new PurchasePaymentBFC().Approve(Convert.ToInt64(key), MembershipHelper.GetUserName());
        }

        public override void VoidData(string key)
        {
            new PurchasePaymentBFC().Void(Convert.ToInt64(key), "", MembershipHelper.GetUserName());
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetIndexViewBag();
            SetViewBagPermission();

            var purchasePaymentCount = 0;
            var purchasePaymentList = new List<PurchasePaymentModel>();

            if (startIndex == null)
                startIndex = 0;

            if (amount == null)
                amount = 20;

            if (string.IsNullOrEmpty(sortParameter))
                sortParameter = "";

            purchasePaymentCount = new PurchasePaymentBFC().RetrieveCount(filter.GetSelectFilters());
            purchasePaymentList = new PurchasePaymentBFC().Retrieve((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters());

            ViewBag.DataCount = purchasePaymentCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            ViewBag.PageSeriesSize = GetPageSeriesSize();

            return View(purchasePaymentList);
        }

        public ActionResult VoidRemarks(string key, bool voidFromIndex)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var purchasePayment = new PurchasePaymentBFC().RetrieveByID(key);

                ViewBag.VoidFromIndex = voidFromIndex;

                return View(purchasePayment);
            }

            return View();
        }

        [HttpPost]
        public ActionResult VoidRemarks(PurchasePaymentModel purchasePayment, FormCollection col)
        {
            var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);

            try
            {
                new PurchasePaymentBFC().Void(purchasePayment.ID, purchasePayment.VoidRemarks, MembershipHelper.GetUserName());

                new EmailHelper().SendVoidPurchasePaymentEmail(purchasePayment.ID, purchasePayment.VoidRemarks, MembershipHelper.GetUserName());

                TempData["SuccessNotification"] = "Dokumen berhasil dibatalkan";

                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = purchasePayment.ID });
            }
            catch (Exception ex)
            {
                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = purchasePayment.ID, errorMessage = ex.Message });
            }

        }
    }
}
