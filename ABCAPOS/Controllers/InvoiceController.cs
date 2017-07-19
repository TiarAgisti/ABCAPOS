using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.Models;
using MPL.MVC;
using ABCAPOS.BF;
using ABCAPOS.Helpers;
using ABCAPOS.Util;
using System.Transactions;

namespace ABCAPOS.Controllers
{
    public class InvoiceController : MasterDetailController<InvoiceModel, InvoiceDetailModel>
    {
        private void AssignItemNo(InvoiceModel header, List<InvoiceDetailModel> details)
        {
            int maxItemNo = details.Max(p => p.ItemNo);

            foreach (InvoiceDetailModel detail in details)
            {
                if (detail.ItemNo == 0)
                {
                    detail.ItemNo = maxItemNo + 1;
                    maxItemNo++;
                }
            }
        }
        private void SetPreCreateViewBag(InvoiceModel header)
        {
            ViewBag.OwnerList = new StaffBFC().Retrieve(StaffType.Owner, true);
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
        private void SetIndexViewBag()
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
        }
        private string ModuleID
        {
            get
            {
                return "Invoice";
            }
        }
        public override MPL.Business.IMasterDetailBFC<InvoiceModel, InvoiceDetailModel> GetBFC()
        {
            return new InvoiceBFC();
        }
        protected override void PreCreateDisplay(InvoiceModel header, List<InvoiceDetailModel> details)
        {
            SetViewBagNotification();
            SetPreCreateViewBag(header);
            var salesOrderID = Convert.ToInt64(Request.QueryString["salesOrderID"]);
            new InvoiceBFC().CreateInvoiceBySalesOrder(header, salesOrderID);
            base.PreCreateDisplay(header, header.Details);
        }
        protected void SetPreEditDetailViewBag(InvoiceModel header, List<InvoiceDetailModel> details)
        {
            foreach (var detail in details)
            {
                detail.StrQuantity = Convert.ToDouble(detail.Quantity).ToString();
                detail.CreatedInvQuantity = detail.CreatedInvQuantity - detail.Quantity;
                //harga
                detail.TotalAmount = Convert.ToDecimal(Convert.ToDecimal(Convert.ToDecimal(detail.Price) * Convert.ToDecimal(detail.Quantity)).ToString("N2"));
                if (detail.TaxType == 2)
                    detail.TotalPPN = Convert.ToDecimal(Convert.ToDecimal(Convert.ToDouble(detail.TotalAmount) * 0.1).ToString("N2"));
                else
                    detail.TotalPPN = 0;

                detail.GrossAmount = detail.TotalAmount + detail.TotalPPN;
            }
        }
        protected override void PreUpdateDisplay(InvoiceModel header, List<InvoiceDetailModel> details)
        {
            SetViewBagNotification();
            SetPreCreateViewBag(header);
            SetPreEditDetailViewBag(header, details);
            header.ResiDetails = new InvoiceBFC().RetrieveInvoiceResiDetails(header.ID);
            base.PreUpdateDisplay(header, details);
        }
        protected void SetPreDetailViewBag(InvoiceModel header, List<InvoiceDetailModel> details)
        {
            foreach (var detail in details)
            {
                new InvoiceBFC().CalculateGrossAmountInvDetail(detail);
            }
        }
        protected override void PreDetailDisplay(InvoiceModel header, List<InvoiceDetailModel> details)
        {
            SetViewBagNotification();
            SetViewBagPermission();
            SetPreDetailViewBag(header, details);

            ViewBag.PaymentList = new MakeMultiPaySalesBFC().RetrieveByInvoiceID(header.ID);
            ViewBag.ApplyCreditList = new ApplyCreditMemoBFC().RetrieveByInvoiceID(header.ID);

            header.ResiDetails = new InvoiceBFC().RetrieveInvoiceResiDetails(header.ID);

            base.PreDetailDisplay(header, details);
        }
        protected override List<Button> GetAdditionalButtons(InvoiceModel obj, List<InvoiceDetailModel> details, UIMode mode)
        {
            var list = new List<Button>();

            if (mode == UIMode.Detail && obj.Status != (int)MPL.DocumentStatus.Void)
            {
                //if (obj.TaxAmount > 0)
                //{
                var invoicePrint = new Button();
                invoicePrint.Text = "Print Invoice";
                invoicePrint.CssClass = "button";
                invoicePrint.ID = "btnPrint";
                invoicePrint.OnClick = "if (confirm('Are you sure you want to print this document?')) { " + String.Format("window.open('{0}');", Url.Action("PopUp", "ReportViewer",
                    new { type = ReportViewerController.PrintOutType.Invoice, queryString = "InvoiceID=" + obj.ID })) + " } ";
                invoicePrint.Href = "#";
                list.Add(invoicePrint);

                var taxInvoicePrint = new Button();
                taxInvoicePrint.Text = "Print Faktur Pajak";
                taxInvoicePrint.CssClass = "button";
                taxInvoicePrint.ID = "btnPrintFakturPajak";
                taxInvoicePrint.OnClick = String.Format("window.open('{0}');window.close();", Url.Action("PopUp", "ReportViewer",
                    new { type = ReportViewerController.PrintOutType.TaxInvoice, queryString = "InvoiceID=" + obj.ID }));
                taxInvoicePrint.Href = "#";
                list.Add(taxInvoicePrint);
                //}


            }

            return list;
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult CreateInvoice(InvoiceModel obj, FormCollection col)
        {
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    new InvoiceBFC().Validate(obj, obj.Details);
                    AssignItemNo(obj, obj.Details);
                    //new InvoiceBFC().Create(obj, obj.Details);
                    base.Create(obj);
                    trans.Complete();
                }
                TempData["SuccessNotification"] = "Document has been saved";

                return RedirectToAction("Detail", new { key = obj.ID});
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;
                ViewBag.Mode = UIMode.Create;
                SetViewBagPermission();

                return RedirectToAction("Create", new { salesOrderID = obj.SalesOrderID, errorMessage = ex.Message });
            }

        }
        [HttpPost, ValidateInput(false)]
        public ActionResult UpdateInvoice(InvoiceModel obj, FormCollection col)
        {
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    new InvoiceBFC().Validate(obj, obj.Details);
                    AssignItemNo(obj, obj.Details);
                    base.Update(obj, col);

                    trans.Complete();
                }
                TempData["SuccessNotification"] = "Document has been Updated";

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
        public override ActionResult Detail(string key, string errorMessage)
        {
            return base.Detail(key, errorMessage);
        }
        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();
            SetIndexViewBag();

            var invoiceCount = 0;
            var invoiceList = new List<InvoiceModel>();

            if (startIndex == null)
                startIndex = 0;

            if (amount == null)
                amount = 20;

            if (string.IsNullOrEmpty(sortParameter))
                sortParameter = "";

            //if (filter == null || filter.FilterFields.Count == 0)
            //{
            //    invoiceCount = new InvoiceBFC().RetrieveUnvoidCount(filter.GetSelectFilters());
            //    invoiceList = new InvoiceBFC().RetrieveUnvoid((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters());
            //}
            //else
            //{
                invoiceCount = new InvoiceBFC().RetrieveInvoiceCount(filter.GetSelectFilters());
                invoiceList = new InvoiceBFC().RetrieveInvoice((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters());
            //}

            ViewBag.DataCount = invoiceCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            ViewBag.PageSeriesSize = GetPageSeriesSize();

            return View(invoiceList);
        }
        public ActionResult VoidRemarks(string key, bool voidFromIndex)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var invoice = new InvoiceBFC().RetrieveByID(key);

                ViewBag.VoidFromIndex = voidFromIndex;

                return View(invoice);
            }

            return View();
        }
        [HttpPost]
        public ActionResult VoidRemarks(InvoiceModel invoice, FormCollection col)
        {
            var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);

            try
            {
                using (TransactionScope trans = new TransactionScope())
                {

                    new InvoiceBFC().Void(invoice.ID, invoice.VoidRemarks, MembershipHelper.GetUserName());

                    //new EmailHelper().SendVoidDOEmail(deliveryOrder.ID, deliveryOrder.VoidRemarks, MembershipHelper.GetUserName());
                    trans.Complete();
                }
                TempData["SuccessNotification"] = "Dokumen berhasil dibatalkan";

                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = invoice.ID });
            }
            catch (Exception ex)
            {
                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = invoice.ID, errorMessage = ex.Message });
            }

        }
    }
}
