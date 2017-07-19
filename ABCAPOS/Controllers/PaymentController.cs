using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.Models;
using MPL.MVC;
using ABCAPOS.BF;
using ABCAPOS.Helpers;

namespace ABCAPOS.Controllers
{
    public class PaymentController : GenericTransactionController<PaymentModel>
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

        public override MPL.Business.IGenericTransactionBFC<PaymentModel> GetBFC()
        {
            return new PaymentBFC();
        }

        protected override void PreCreateDisplay(PaymentModel header)
        {
            var invoiceID = Convert.ToInt64(Request.QueryString["invoiceID"]);

            new PaymentBFC().CreateByInvoice(header, invoiceID);

            SetPreEditViewBag();

            base.PreCreateDisplay(header);
        }

        protected override void PreDetailDisplay(PaymentModel header)
        {
            SetViewBagNotification();

            SetViewBagPermission();

            var paymentList = new PaymentBFC().RetrieveByInvoice(header.InvoiceID);
            header.SisaAmount = header.InvoiceAmount - paymentList.Sum(p => p.Amount);
            base.PreDetailDisplay(header);
        }

        protected override void PreUpdateDisplay(PaymentModel header)
        {
            SetPreEditViewBag();

            base.PreUpdateDisplay(header);
        }

        public override void CreateData(PaymentModel obj)
        {
            try
            {
                new PaymentBFC().Validate(obj);

                base.CreateData(obj);

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

        public override void UpdateData(PaymentModel obj, FormCollection formCollection)
        {
            try
            {
                new PaymentBFC().Validate(obj);

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
            new PaymentBFC().Approve(Convert.ToInt64(key), MembershipHelper.GetUserName());
        }

        public override void VoidData(string key)
        {
            new PaymentBFC().Void(Convert.ToInt64(key), "", MembershipHelper.GetUserName());
        }

        private void SetPreEditViewBag()
        {
            ViewBag.PaymentMethodList = new PaymentMethodBFC().Retrieve(true).OrderBy(p => p.Name);
        }

        private void SetViewBagNotification()
        {
            if (TempData["SuccessNotification"] != null)
                ViewBag.SuccessNotification = Convert.ToString(TempData["SuccessNotification"]);

            if (!string.IsNullOrEmpty(Request.QueryString["errorMessage"]))
                ViewBag.ErrorNotification = Convert.ToString(Request.QueryString["errorMessage"]);
        }

        private void SetIndexViewBag()
        {
            ViewBag.CustomerGroupList = new CustomerGroupBFC().RetrieveAll();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetIndexViewBag();
            SetViewBagPermission();

            var paymentCount = 0;
            var paymentList = new List<PaymentModel>();

            if (startIndex == null)
                startIndex = 0;

            if (amount == null)
                amount = 20;

            if (string.IsNullOrEmpty(sortParameter))
                sortParameter = "";

            //if (filter == null || filter.FilterFields.Count == 0)
            //{
            //    paymentCount = new PaymentBFC().RetrieveUnvoidCount(filter.GetSelectFilters());
            //    paymentList = new PaymentBFC().RetrieveUnvoid((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters());
            //}
            //else
            //{
            paymentCount = new PaymentBFC().RetrieveCount(filter.GetSelectFilters());
            paymentList = new PaymentBFC().Retrieve((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters());
            //}

            ViewBag.DataCount = paymentCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            ViewBag.PageSeriesSize = GetPageSeriesSize();

            return View(paymentList);
        }

        public ActionResult VoidRemarks(string key, bool voidFromIndex)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var payment = new PaymentBFC().RetrieveByID(key);

                ViewBag.VoidFromIndex = voidFromIndex;

                return View(payment);
            }

            return View();
        }

        [HttpPost]
        public ActionResult VoidRemarks(PaymentModel payment, FormCollection col)
        {
            var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);

            try
            {
                new PaymentBFC().Void(payment.ID, payment.VoidRemarks, MembershipHelper.GetUserName());

                new EmailHelper().SendVoidPaymentEmail(payment.ID, payment.VoidRemarks, MembershipHelper.GetUserName());

                TempData["SuccessNotification"] = "Dokumen berhasil dibatalkan";

                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = payment.ID });
            }
            catch (Exception ex)
            {
                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = payment.ID, errorMessage = ex.Message });
            }

        }
    }
}
