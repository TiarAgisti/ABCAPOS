﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.Models;
using MPL.MVC;
using ABCAPOS.BF;
using ABCAPOS.DA;
using ABCAPOS.Util;
using ABCAPOS.Helpers;
using System.Text;
using System.IO;
using System.Transactions;

namespace ABCAPOS.Controllers
{
    public class SalesOrderController : MasterDetailController<SalesOrderModel, SalesOrderDetailModel>
    {
        private string ModuleID
        {
            get
            {
                return "SalesOrder";
            }
        }
        private string recalculateEmail(string emailTo, string ownerEmail)
        {
            var collEmail = emailTo.Split(';');
            return (collEmail.Count() > 0) ? emailTo.Replace(';', ',') : ownerEmail;
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
        private void SetPreEditViewBag()
        {
            ViewBag.SalesmanList = new SalesmanBFC().Retrieve(true);

            ViewBag.PaymentMethodList = new PaymentMethodBFC().Retrieve(true).OrderBy(p => p.Name);

            ViewBag.PriceLevelList = new PriceLevelBFC().Retrieve(true);
        }
        private void SetIndexViewBag()
        {
            ViewBag.CustomerGroupList = new CustomerGroupBFC().RetrieveAll();
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
        }
        private void SetPreCreateViewBag(SalesOrderModel header)
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
            ViewBag.StaffList = new StaffBFC().Retrieve(true);
            ViewBag.TermsList = new PurchaseOrderBFC().RetrieveAllTerms();
            ViewBag.ExpedisiList = new ExpedisiBFC().RetrieveAll();
            ViewBag.ConversionList = new List<UnitDetailModel>();
            ViewBag.PriceLevelList = new PriceLevelBFC().Retrieve(true);
        }
        private void AssignItemNo(SalesOrderModel header, List<SalesOrderDetailModel> details)
        {
            int maxItemNo = details.Max(p => p.ItemNo);

            foreach (SalesOrderDetailModel detail in details)
            {
                if (detail.ItemNo == 0)
                {
                    detail.ItemNo = maxItemNo + 1;
                    maxItemNo++;
                }
            }
        }
        private List<SalesOrderDetailModel> removeEmptyLines(List<SalesOrderDetailModel> details)
        {
            List<SalesOrderDetailModel> returnList = new List<SalesOrderDetailModel>();
            foreach (var detail in details)
            {
                if (detail.ProductID != 0)
                {
                    returnList.Add(detail);
                }
            }
            return returnList;
        }
        protected override void PreCreateDisplay(SalesOrderModel header, List<SalesOrderDetailModel> details)
        {
            SetViewBagNotification();
            SetPreCreateViewBag(header);

            var salesOrderID = Request.QueryString["salesOrderID"];

            // shipping tax code should be Non PPN by default
            header.ShippingTaxCode = 1;

            if (!string.IsNullOrEmpty(salesOrderID))
                new SalesOrderBFC().CopyTransaction(header, Convert.ToInt64(salesOrderID));

            if (ViewBag.ErrorNotification != null)
            {
                new SalesOrderBFC().ErrorTransaction(header, details);
                //ViewBag.ErrorNotification = null;
            }
            else
            {
                header.WarehouseID = new StaffBFC().RetrieveDefaultWarehouseID(MembershipHelper.GetUserName());
                if (header.WarehouseID != 0)
                {
                    var warehouse = new WarehouseBFC().RetrieveByID(header.WarehouseID);
                    header.WarehouseName = warehouse.Name;
                    header.EmailTo = warehouse.Email;
                }
            }
            SetPreEditViewBag();

            var bookingSalesID = Request.QueryString["bookingSalesID"];

            if (!string.IsNullOrEmpty(bookingSalesID))
                new SalesOrderBFC().CopyTransactionBooking(header, Convert.ToInt64(bookingSalesID));

            header.Code = SystemConstants.autoGenerated; //new SalesOrderBFC().GetSalesOrderCode();
            header.DepartmentID = 6;


            base.PreCreateDisplay(header, details);
        }
        protected override void PreDetailDisplay(SalesOrderModel header, List<SalesOrderDetailModel> details)
        {
            SetViewBagNotification();
            SetViewBagPermission();
            foreach (var detail in details)
            {
                detail.HPP = detail.AssetPrice;
                double unitRate = 0;
                if (detail.ConversionID == 0)
                    unitRate = 1;
                else
                    unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);

                detail.StockQty = detail.StockQtyHidden / unitRate;
                detail.StockAvailable = detail.StockAvailableHidden / unitRate;
            }

            ViewBag.DOList = new DeliveryOrderBFC().RetrieveBySalesOrderID(header.ID);

            ViewBag.InvoiceList = new InvoiceBFC().RetrieveBySalesOrder(header.ID);

            ViewBag.CustomerReturnList = new CustomerReturnBFC().RetrieveBySalesOrder(header.ID);
            //var newDetails = details.OrderBy(p => p.LineSequenceNumber).ToList();
            base.PreDetailDisplay(header, details);
        }
        protected void SetPreEditDetailViewBag(SalesOrderModel header, List<SalesOrderDetailModel> details)
        {
            foreach (var detail in details)
            {
                detail.ConversionIDTemp = detail.ConversionID;
                detail.StockAvailable = detail.StockAvailableHidden;
                detail.StockQty = detail.StockQtyHidden;

                var product = new ProductBFC().RetrieveByID(detail.ProductID);
                var customer = new CustomerBFC().RetrieveByID(header.CustomerID);

                product = new SalesOrderBFC().GetSellingPrice(product, customer);
                detail.SaleUnitRateHidden = product.SaleUnitID;
                detail.PriceHidden = Convert.ToDouble(product.SellingPrice);
            }
        }
        protected override void PreUpdateDisplay(SalesOrderModel header, List<SalesOrderDetailModel> details)
        {
            SetViewBagNotification();
            SetPreCreateViewBag(header);
            SetPreEditViewBag();
            SetPreEditDetailViewBag(header, details);

            //var newDetails = details.OrderBy(p => p.LineSequenceNumber).ToList();
            base.PreUpdateDisplay(header, details);

            if (ViewBag.ErrorNotification != null)
            {
                RedirectToAction("Update", new { key = header.ID, ErrorMessage = ViewBag.ErrorNotification });
                //new SalesOrderBFC().ErrorTransaction(header, details);
                //ViewBag.ErrorNotification = null;
            }
        }
        protected override List<Button> GetAdditionalButtons(SalesOrderModel header, List<SalesOrderDetailModel> details, UIMode mode)
        {
            var list = new List<Button>();
            if (mode == UIMode.Detail && header.Status >= (int)MPL.DocumentStatus.Approved)
            {
                var print = new Button();
                print.Text = "Print Picking Ticket";
                print.CssClass = "button";
                print.ID = "btnPrintPickingTicket";
                print.OnClick = String.Format("window.open('{0}');", Url.Action("PopUp", "ReportViewer",
                    new { type = ReportViewerController.PrintOutType.PickingTicket, queryString = SystemConstants.str_SalesOrderID + "=" + header.ID }));
                print.Href = "#";
                list.Add(print);

            }

            return list;
        }
        public override MPL.Business.IMasterDetailBFC<SalesOrderModel, SalesOrderDetailModel> GetBFC()
        {
            return new SalesOrderBFC();
        }
        public override void CreateData(SalesOrderModel obj, List<SalesOrderDetailModel> details)
        {
            try
            {
                // disable validation for now 
                //new SalesOrderBFC().Validate(obj, details);
                details = removeEmptyLines(details);
                AssignItemNo(obj, details);
                new SalesOrderBFC().ValidasiCreditLimit(obj,details);
                base.CreateData(obj, details);

                //ApproveData(obj.ID.ToString());

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;
                var bookID = obj.BookingSalesID;

                if (bookID != 0)
                    RedirectToAction("Create", new { bookingSalesID = bookID, errorMessage = ex.Message });

                throw;
            }
        }
        public override void ApproveData(string key)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                base.ApproveData(key);
                new SalesOrderBFC().Approve(Convert.ToInt32(key), MembershipHelper.GetUserName());
                //new SalesOrderBFC().UpdateInventory(key);
                new SalesOrderBFC().UpdateStatus(Convert.ToInt64(key));

                EmailPrintOut(Convert.ToInt64(key));

                trans.Complete();
            }
            TempData["SuccessNotification"] = "Document has been approved";
        }
        public void RetrieveRatesFromExcel()
        {
            const string fromCurrency = "USD";
            const string toCurrency = "IDR";
            const double amount = 1;

            // Construct URL to query the Yahoo! Finance API
            const string urlPattern = "http://finance.yahoo.com/d/quotes.csv?s={0}{1}=X&f=l1";
            string url = String.Format(urlPattern, fromCurrency, toCurrency);
            string response = new System.Net.WebClient().DownloadString(url);
            double exchangeRate = double.Parse(response, System.Globalization.CultureInfo.InvariantCulture);

            RateModel rate = new RateModel();
            rate.ID = 1;
            rate.Name = "USD";
            rate.Value = Convert.ToDecimal(exchangeRate);
            new ABCAPOSDAC().UpdateRate(rate);
        }
        public void EmailPrintOut(long salesOrderID)
        {
            var setting = new CompanySettingBFC().Retrieve();
            var salesOrder = new SalesOrderBFC().RetrieveByID(salesOrderID);
            var customer = new CustomerBFC().RetrieveByID(salesOrder.CustomerID);

            var subject = salesOrder.Code + ":" + customer.Name;

            var body = "";
            using (var sr = new StreamReader(Server.MapPath("\\App_Data\\Templates\\SalesOrderHeaderTemplate.txt")))
            {
                body = sr.ReadToEnd();
            }
            //4
            string emailMessageHeader = string.Format(body, setting.Address, setting.City + " " + setting.PostCode, "ID",
                customer.Name, customer.Address, "", "", "", salesOrder.Date.ToString("dd/MM/yyyy"), salesOrder.Code, salesOrder.POCustomerNo, customer.Name, "",salesOrder.SalesReference, salesOrder.CreatedBy);

            var footer = "";
            using (var sFooter = new StreamReader(Server.MapPath("\\App_Data\\Templates\\SalesOrderFooterTemplate.txt")))
            {
                footer = sFooter.ReadToEnd();
            }
            string emailMessageFooter = string.Format(footer, salesOrder.SubTotal.ToString("N2"),
                salesOrder.TaxValue.ToString("N2"), salesOrder.GrandTotal.ToString("N2"));

            var content = "<tr><td valign=\"top\">{0}</td><td valign=\"top\" align=\"right\" nowrap>{1}</td><td valign=\"top\">{2}</td><td valign=\"top\" align=\"right\" nowrap>{3}</td><td valign=\"top\" align=\"right\" nowrap>{4}</td><td valign=\"top\" align=\"right\" nowrap>{5}</td></tr>";

            var salesOrderDetails = new SalesOrderBFC().RetrieveDetails(salesOrderID);
            string contentMessage = "";
            foreach (var soDetail in salesOrderDetails)
            {
                contentMessage += string.Format(content, soDetail.ProductName, soDetail.Quantity.ToString("N0"), soDetail.ConversionName, soDetail.Price.ToString("N2"), soDetail.TotalAmount.ToString("N2"), soDetail.Total.ToString("N2"));
            }
            var recalcEmail = recalculateEmail(salesOrder.EmailTo, setting.OwnerEmail);
            new EmailHelper().SendSOEmail(recalcEmail.ToString(), subject, emailMessageHeader + contentMessage + emailMessageFooter);

        }
        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();
            SetIndexViewBag();

            if (filter == null || filter.FilterFields.Count == 0)
            {
                //filter.FilterFields[0].Value = DateTime.Now.ToString("mm/dd/yyyy");
                //filter.FilterFields[0].Value1 = DateTime.Now.ToString("mm/dd/yyyy");
            }

            return base.Index(startIndex, amount, sortParameter, filter);
        }
        public ActionResult ApproveFromIndex(string key)
        {
            ApproveData(key);

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
                var salesOrder = new SalesOrderBFC().RetrieveByID(key);

                ViewBag.VoidFromIndex = voidFromIndex;

                return View(salesOrder);
            }

            return View();
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult UpdateSalesOrder(SalesOrderModel obj, FormCollection col)
        {
            try
            {
                new SalesOrderBFC().UpdateValidation(obj, obj.Details);
                AssignItemNo(obj, obj.Details);

                new SalesOrderBFC().ValidasiCreditLimit(obj, obj.Details);
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
        [HttpPost]
        public ActionResult VoidRemarks(SalesOrderModel salesOrder, FormCollection col)
        {
            var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);

            try
            {
                new SalesOrderBFC().Void(salesOrder.ID, salesOrder.VoidRemarks, MembershipHelper.GetUserName());
                new SalesOrderBFC().UpdateStatus(Convert.ToInt64(salesOrder.ID));
                //new EmailHelper().SendVoidSalesOrderEmail(salesOrder.ID, salesOrder.VoidRemarks, MembershipHelper.GetUserName());

                TempData["SuccessNotification"] = "Dokumen berhasil dibatalkan";

                //if (voidFromIndex)
                return RedirectToAction("Index");
                //else
                //    return RedirectToAction("Detail", new { key = salesOrder.ID });
            }
            catch (Exception ex)
            {
                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = salesOrder.ID, errorMessage = ex.Message });
            }

        }

    }
}
