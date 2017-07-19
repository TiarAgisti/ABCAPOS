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

namespace ABCAPOS.Controllers
{
    public class PurchaseBillController : MasterDetailController<PurchaseBillModel, PurchaseBillDetailModel>
    {
        private string ModuleID
        {
            get
            {
                return "PurchaseBill";
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

        private void SetPreEditViewBag(PurchaseBillModel obj, long purchaseBillID)
        {
            var template = "";

            ViewBag.RowEditor = template;
        }

        private void SetPreCreateViewBag(PurchaseBillModel header)
        {
            ViewBag.TermsList = new PurchaseOrderBFC().RetrieveAllTerms();
            ViewBag.CurrencyList = new CurrencyBFC().RetrieveAll();
        }

        public override MPL.Business.IMasterDetailBFC<PurchaseBillModel, PurchaseBillDetailModel> GetBFC()
        {
            return new PurchaseBillBFC();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();

            return base.Index(startIndex, amount, sortParameter, filter);
        }

        protected override void PreCreateDisplay(PurchaseBillModel header, List<PurchaseBillDetailModel> details)
        {
            if (!string.IsNullOrEmpty(Request.QueryString["errorMessage"]))
                SetViewBagNotification();

            SetPreCreateViewBag(header);

            var purchaseOrderID = Convert.ToInt64(Request.QueryString["purchaseOrderID"]);

            new PurchaseBillBFC().CreateByPurchaseOrder(header, purchaseOrderID);
            
            base.PreCreateDisplay(header, header.Details);
        }

        protected void SetPreEditDetailViewBag(PurchaseBillModel header, List<PurchaseBillDetailModel> details)
        {
            ViewBag.CurrencyList = new CurrencyBFC().RetrieveAll();
            foreach (var detail in details)
            {
                detail.StrQuantity = detail.Quantity.ToString("N2");
                detail.StrTotal = detail.Total.ToString("N2");
            }
        }

        protected override void PreUpdateDisplay(PurchaseBillModel header, List<PurchaseBillDetailModel> details)
        {
            SetPreCreateViewBag(header);
            SetViewBagNotification();
            SetPreEditDetailViewBag(header, details);

            header.GrandTotal = Convert.ToDouble(Math.Round(details.Sum(p => p.Total), 2));

            base.PreUpdateDisplay(header, details);
        }

        protected override void PreDetailDisplay(PurchaseBillModel header, List<PurchaseBillDetailModel> details)
        {

            SetViewBagNotification();

            SetViewBagPermission();
            var paymentList = new MakeMultiPayBFC().RetrieveByBillID(header.ID);
            header.BeforePaymentAmount = paymentList.Sum(p => p.Amount);
            header.DiscountTaken = paymentList.Sum(p => p.DiscountTaken);

            ViewBag.PaymentList = paymentList;

            ViewBag.ApplyCreditList = new ApplyBillCreditBFC().RetrieveByBillID(header.ID);

            //foreach (var detail in details)
            //{
            //    detail.Total = detail.TotalAmount - Convert.ToDecimal(detail.Discount) + detail.TotalPPN;
            //}
            header.GrandTotal = Convert.ToDouble(Math.Round(details.Sum(p => p.Total), 2));

            base.PreDetailDisplay(header, details);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreatePurchaseBill(PurchaseBillModel obj, FormCollection col)
        {
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    var bfc = new PurchaseBillBFC();
                    //bfc.AssignValues(obj, obj.Details);
                    //bfc.Validate(obj, obj.Details);

                    base.Create(obj);
                    trans.Complete();
                }
                TempData["SuccessNotification"] = "Document successfully saved";

                return RedirectToAction("Detail", new { key = obj.ID });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;
                SetPreEditViewBag(obj, 0);
                ViewBag.Mode = UIMode.Create;
                SetViewBagPermission();

                return RedirectToAction("Create", new { purchaseOrderID = obj.PurchaseOrderID, errorMessage = ex.Message });
            }

        }

        [HttpPost, ValidateInput(false)]
        public ActionResult UpdatePurchaseBill(PurchaseBillModel obj, FormCollection col)
        {
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    var bfc = new PurchaseBillBFC();
                    //bfc.AssignValues(obj, obj.Details);
                    //bfc.Validate(obj, obj.Details);

                    base.Update(obj, col);

                    trans.Complete();
                }

                TempData["SuccessNotification"] = "Document successfully updated";

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

        public ActionResult VoidRemarks(string key, bool voidFromIndex)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var pBill = new PurchaseBillBFC().RetrieveByID(key);

                ViewBag.VoidFromIndex = voidFromIndex;

                return View(pBill);
            }

            return View();
        }

        [HttpPost]
        public ActionResult VoidRemarks(PurchaseBillModel pBill, FormCollection col)
        {
            var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);

            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    new PurchaseBillBFC().Void(pBill.ID, pBill.VoidRemarks, MembershipHelper.GetUserName());

                    trans.Complete();
                }
                //TempData["SuccessNotification"] = "Dokumen berhasil dibatalkan";

                return RedirectToAction("Index");
               
            }
            catch (Exception ex)
            {
                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = pBill.ID, errorMessage = ex.Message });
            }

        }
    }
}
