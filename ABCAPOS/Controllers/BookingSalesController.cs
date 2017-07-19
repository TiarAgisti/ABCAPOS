using ABCAPOS.BF;
using ABCAPOS.Models;
using ABCAPOS.Util;
using ABCAPOS.Helpers;
using MPL.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.IO;
using System.Transactions;

namespace ABCAPOS.Controllers
{
    public class BookingSalesController : MasterDetailController<BookingSalesModel, BookingSalesDetailModel>
    {
        private void AssignItemNo(BookingSalesModel header, List<BookingSalesDetailModel> details)
        {
            int maxItemNo = details.Max(p => p.ItemNo);

            foreach (BookingSalesDetailModel detail in details)
            {
                if (detail.ItemNo == 0)
                {
                    detail.ItemNo = maxItemNo + 1;
                    maxItemNo++;
                }
            }
        }
        
        private string ModuleID
        {
            get
            {
                return "BookingSales";
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

        private void SetIndexViewBag()
        {
            ViewBag.WarehouseList = new WarehouseBFC().RetrieveAll();
        }

        private string recalculateEmail(string emailTo, string ownerEmail)
        {
            var collEmail = emailTo.Split(';');
            return (collEmail.Count() > 0) ? emailTo.Replace(';', ',') : ownerEmail;
        }

        public override MPL.Business.IMasterDetailBFC<BookingSalesModel, BookingSalesDetailModel> GetBFC()
        {
            return new BookingSalesBFC();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();
            SetIndexViewBag();

            if (startIndex == null)
                startIndex = 0;

            if (amount == null)
                amount = 20;

            if (string.IsNullOrEmpty(sortParameter))
                sortParameter = "";

            var bookingSalesCount = new BookingSalesBFC().RetrieveCount(filter.GetSelectFilters(), false);
            var bookingSalesList = new BookingSalesBFC().Retrieve((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters(), false);

            ResetBackToListUrl(filter);

            ViewBag.DataCount = bookingSalesCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            ViewBag.PageSeriesSize = GetPageSeriesSize();

            return View(bookingSalesList);
        }

        public void SetPreDetailViewBag()
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            ViewBag.PriceLevelList = new PriceLevelBFC().Retrieve(true);
        }

        public void EmailPrintOut(long bookingSalesID)
        {
            var setting = new CompanySettingBFC().Retrieve();
            var bookingSales = new BookingSalesBFC().RetrieveByID(bookingSalesID);
            var customer = new CustomerBFC().RetrieveByID(bookingSales.CustomerID);

            var subject = bookingSales.Code + ":" + customer.Name;

            var body = "";
            using (var sr = new StreamReader(Server.MapPath("\\App_Data\\Templates\\BookingSalesHeaderTemplate.txt")))
            {
                body = sr.ReadToEnd();
            }
            string emailMessageHeader = string.Format(body, setting.Address, setting.City + " " + setting.PostCode, "ID",
                customer.Name, customer.Address, "", "", "", bookingSales.Date.ToString("dd/MM/yyyy"), bookingSales.Code, customer.Name, bookingSales.CreatedBy);

            var footer = "";
            using (var sFooter = new StreamReader(Server.MapPath("\\App_Data\\Templates\\BookingSalesFooterTemplate.txt")))
            {
                footer = sFooter.ReadToEnd();
            }
            string emailMessageFooter = string.Format(footer, bookingSales.SubTotal.ToString("N0"),
                bookingSales.TaxValue.ToString("N0"), bookingSales.GrandTotal.ToString("N0"));

            var content = "<tr><td valign=\"top\">{0}</td><td valign=\"top\" align=\"right\" nowrap>{1}</td><td valign=\"top\">{2}</td><td valign=\"top\" align=\"right\" nowrap>{3}</td><td valign=\"top\" align=\"right\" nowrap>{4}</td><td valign=\"top\" align=\"right\" nowrap>{5}</td></tr>";
            var bookingSalesDetails = new BookingSalesBFC().RetrieveDetails(bookingSalesID);
            string contentMessage = "";
            foreach (var boDetail in bookingSalesDetails)
            {
                contentMessage += string.Format(content, boDetail.ProductName, boDetail.Quantity.ToString("N0"), boDetail.ConversionName, boDetail.Price.ToString("N2"), boDetail.TotalAmount.ToString("N2"), boDetail.Total.ToString("N2"));
            }
            var recalcEmail = recalculateEmail(bookingSales.EmailTo, setting.OwnerEmail);
            new EmailHelper().SendSOEmail(recalcEmail.ToString(), subject, emailMessageHeader + contentMessage + emailMessageFooter);

        }

        protected void SetPreEditDetailViewBag(BookingSalesModel header, List<BookingSalesDetailModel> details)
        {
            foreach (var detail in details)
            {
                detail.ConversionIDTemp = detail.ConversionID;
                var product = new WebService().RetrieveProductOnSalesOrder(detail.ProductCode, header.CustomerID, header.WarehouseID);
                detail.SaleUnitRateHidden = new WebService().GetUnitRateByID(product.SaleUnitID);
                detail.PriceHidden = Convert.ToDouble(product.SellingPrice);
            }
        }

        protected override void PreCreateDisplay(BookingSalesModel header, List<BookingSalesDetailModel> details)
        {
            SetPreDetailViewBag();

            if (ViewBag.ErrorNotification != null)
            {
                new BookingSalesBFC().ErrorTransaction(header, details);
                //ViewBag.ErrorNotification = null;
            }

            header.Code = SystemConstants.autoGenerated;
            header.EmailTo = "hkasmara@abca-indonesia.com";
            header.WarehouseID = new StaffBFC().RetrieveDefaultWarehouseID(MembershipHelper.GetUserName());

            base.PreCreateDisplay(header, details);
        }

        protected override void PreUpdateDisplay(BookingSalesModel header, List<BookingSalesDetailModel> details)
        {
            SetViewBagNotification();
            SetPreDetailViewBag();
            SetPreEditDetailViewBag(header, details);

            base.PreUpdateDisplay(header, details);
        }

        protected override void PreDetailDisplay(BookingSalesModel header, List<BookingSalesDetailModel> details)
        {
            SetPreDetailViewBag();
            SetViewBagPermission();
            SetViewBagNotification();

            ViewBag.soList = new SalesOrderBFC().RetrieveByBSID(header.ID);
            ViewBag.soDeliveryList = new DeliveryOrderBFC().RetrieveByBSID(header.ID);
            ViewBag.soInvList = new InvoiceBFC().RetrieveByBSID(header.ID);

            base.PreDetailDisplay(header, details);
        }

        #region Create Update Booking

        public override void CreateData(BookingSalesModel obj, List<BookingSalesDetailModel> details)
        {
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    new BookingSalesBFC().Validate(obj, details);
                    AssignItemNo(obj, details);
                    //obj.CurrencyID = 2;
                    obj.EmailTo = "hkasmara@abca-indonesia.com";

                    base.CreateData(obj, details);

                    EmailPrintOut(obj.ID);

                    trans.Complete();
                }
              

                TempData["SuccessNotification"] = "Document has been saved";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }


        [HttpPost, ValidateInput(false)]
        public ActionResult UpdateBookingSales(BookingSalesModel obj, FormCollection col)
        {
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    new BookingSalesBFC().Validate(obj, obj.Details);
                    new BookingSalesBFC().UpdateValidation(obj, obj.Details);
                    AssignItemNo(obj, obj.Details);
                    base.Update(obj, col);
                    EmailPrintOut(obj.ID);

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

        #endregion

        #region Void

        public ActionResult VoidFromIndex(string key)
        {
            base.VoidData(key);

            return RedirectToAction("Index");
        }

        public ActionResult VoidRemarks(string key, bool voidFromIndex)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var BookingSales = new BookingSalesBFC().RetrieveByID(key);

                ViewBag.VoidFromIndex = voidFromIndex;

                return View(BookingSales);
            }

            return View();
        }

        [HttpPost]
        public ActionResult VoidRemarks(BookingSalesModel BookingSales, FormCollection col)
        {
            var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);

            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    new BookingSalesBFC().Void(BookingSales.ID, BookingSales.VoidRemarks, MembershipHelper.GetUserName());

                    trans.Complete();
                }
               

                TempData["SuccessNotification"] = "Document has been canceled";

                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = BookingSales.ID });
            }
            catch (Exception ex)
            {
                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = BookingSales.ID, errorMessage = ex.Message });
            }

        }

        #endregion

        #region Print

        protected override List<Button> GetAdditionalButtons(BookingSalesModel header, List<BookingSalesDetailModel> details, UIMode mode)
        {
            var list = new List<Button>();

            if (mode == UIMode.Detail)
            {
                var print = new Button();
                print.Text = "Print Kontrak";
                print.CssClass = "button";
                print.ID = "btnPrint";
                print.OnClick = "if (confirm('Are you sure you want to print this document?')) { " + String.Format("window.open('{0}');", Url.Action("PopUp", "ReportViewer",
                    new { type = ReportViewerController.PrintOutType.BookingSales, queryString = SystemConstants.str_BookingSalesID + "=" + header.ID })) + " } ";
                print.Href = "#";
                list.Add(print);

            }

            return list;
        }

        #endregion 
    }
}
