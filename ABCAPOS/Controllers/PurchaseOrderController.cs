using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Transactions;
using ABCAPOS.Models;
using MPL.MVC;
using ABCAPOS.BF;
using ABCAPOS.DA;
using ABCAPOS.Util;
using ABCAPOS.Helpers;
using System.Text;
using System.IO;

namespace ABCAPOS.Controllers
{
    public class PurchaseOrderController : MasterDetailController<PurchaseOrderModel, PurchaseOrderDetailModel>
    {
        private void AssignItemNo(PurchaseOrderModel header, List<PurchaseOrderDetailModel> details)
        {
            int maxItemNo = details.Max(p => p.ItemNo);

            foreach (PurchaseOrderDetailModel detail in details)
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
                return "PurchaseOrder";
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
            //ViewBag.SupplierGroupList = new SupplierGroupBFC().RetrieveAll();
        }

        private void SetViewBagDetail(PurchaseOrderModel header,List<PurchaseOrderDetailModel>details)
        {
            var productID = Request.QueryString["productID"];
            if (!string.IsNullOrEmpty(productID))
                new PurchaseOrderBFC().PreparePOByWorkOrder(header,details,Convert.ToInt64(productID));
        }

        private void SetPreCreateViewBag(PurchaseOrderModel header)
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
            ViewBag.StaffList = new StaffBFC().Retrieve(true);
            ViewBag.ConversionList = new List<UnitDetailModel>();
            ViewBag.CurrencyList = new CurrencyBFC().RetrieveAll();

           
                //new WorkOrderBFC().CreatedByFormulasi(header, Convert.ToInt64(productID));
        }

        private void SetPreEditViewBag(PurchaseOrderModel header)
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
            ViewBag.StaffList = new StaffBFC().Retrieve(true);
            ViewBag.CurrencyList = new CurrencyBFC().RetrieveAll();
            ViewBag.ConversionList = new List<UnitDetailModel>();
        }

        private string recalculateEmail(string emailTo, string ownerEmail)
        {
            var collEmail = emailTo.Split(';');
            return (collEmail.Count() > 0) ? emailTo.Replace(';', ',') : ownerEmail;
        }

        public void EmailPrintOut(long purchaseOrderID)
        {
            var setting = new CompanySettingBFC().Retrieve();
            var purchaseOrder = new PurchaseOrderBFC().RetrieveByID(purchaseOrderID);
            var vendor = new VendorBFC().RetrieveByID(purchaseOrder.SupplierID);

            var subject = purchaseOrder.Code + ":" + vendor.Name;

            var body = "";
            using (var sr = new StreamReader(Server.MapPath("\\App_Data\\Templates\\PurchaseOrderHeaderTemplate.txt")))
            {
                body = sr.ReadToEnd();
            }
            string emailMessageHeader = string.Format(body, setting.Address, setting.City + " " + setting.PostCode, "ID",
                vendor.Name, vendor.Address, "", "", "", purchaseOrder.Date.ToString("dd/MM/yyyy"), purchaseOrder.Code, vendor.Name, purchaseOrder.CreatedBy);

            var footer = "";
            using (var sFooter = new StreamReader(Server.MapPath("\\App_Data\\Templates\\PurchaseOrderFooterTemplate.txt")))
            {
                footer = sFooter.ReadToEnd();
            }
            string emailMessageFooter = string.Format(footer, purchaseOrder.SubTotal.ToString("N0"),
                purchaseOrder.TaxValue.ToString("N0"), purchaseOrder.GrandTotal.ToString("N0"));

            var content = "<tr><td valign=\"top\">{0}</td><td valign=\"top\" align=\"right\" nowrap>{1}</td><td valign=\"top\">{2}</td><td valign=\"top\" align=\"right\" nowrap>{3}</td><td valign=\"top\" align=\"right\" nowrap>{4}</td><td valign=\"top\" align=\"right\" nowrap>{5}</td></tr>";
            var purchaseOrderDetails = new PurchaseOrderBFC().RetrieveDetails(purchaseOrderID);
            string contentMessage = "";
            foreach (var poDetail in purchaseOrderDetails)
            {
                contentMessage += string.Format(content, poDetail.ProductName, poDetail.Quantity.ToString("N0"), poDetail.ConversionName, poDetail.AssetPrice.ToString("N2"), poDetail.TotalAmount.ToString("N2"), poDetail.Total.ToString("N2"));
            }
            var recalcEmail = recalculateEmail(purchaseOrder.EmailTo, setting.OwnerEmail);
            new EmailHelper().SendSOEmail(recalcEmail.ToString(), subject, emailMessageHeader + contentMessage + emailMessageFooter);

        }

        protected override List<Button> GetAdditionalButtons(PurchaseOrderModel header, List<PurchaseOrderDetailModel> details, UIMode mode)
        {
            var list = new List<Button>();
            // TODO: implement print PurchaseOrder
            if (mode == UIMode.Detail && header.Status >= (int)MPL.DocumentStatus.Approved)
            {
                var print = new Button();
                print.Text = "Print";
                print.CssClass = "button";
                print.ID = "btnPrint";
                print.OnClick = String.Format("window.open('{0}');", Url.Action("PopUp", "ReportViewer",
                    new { type = ReportViewerController.PrintOutType.PurchaseOrder, queryString = SystemConstants.str_PurchaseOrderID + "=" + header.ID }));
                print.Href = "#";
                list.Add(print);

            }

            return list;
        }

        protected override void PreCreateDisplay(PurchaseOrderModel header, List<PurchaseOrderDetailModel> details)
        {
            SetPreCreateViewBag(header);

            header.CurrencyID = 1;
            header.CurrencyName = "Rupiah";

            header.EmailTo = "hkasmara@abca-indonesia.com";

            var purchaseOrderID = Request.QueryString["purchaseOrderID"];


            if (!string.IsNullOrEmpty(purchaseOrderID))
                new PurchaseOrderBFC().CopyTransaction(header, Convert.ToInt64(purchaseOrderID));

            if (ViewBag.ErrorNotification != null)
            {
                new PurchaseOrderBFC().ErrorTransaction(header, details);
                //ViewBag.ErrorNotification = null;
            }

            header.WarehouseID = new StaffBFC().RetrieveDefaultWarehouseID(MembershipHelper.GetUserName());

            var bookingOrderID = Request.QueryString["bookingOrderID"];

            if (!string.IsNullOrEmpty(bookingOrderID))
                new PurchaseOrderBFC().CopyTransactionBooking(header, Convert.ToInt64(bookingOrderID));
            //put the code after
            header.Code = SystemConstants.autoGenerated;
            SetViewBagDetail(header,details);
            header.DepartmentID = 5;
            base.PreCreateDisplay(header, details);
        }

        protected override void PreDetailDisplay(PurchaseOrderModel header, List<PurchaseOrderDetailModel> details)
        {
            SetPreDetailViewBag();

            SetViewBagNotification();

            SetViewBagPermission();

            ViewBag.poDeliveryList = new PurchaseDeliveryBFC().RetrieveByPOID(header.ID);
            ViewBag.poBillList = new PurchaseBillBFC().RetrieveByPOID(header.ID);
            foreach (var detail in details)
            {
                //detail.ConversionID = detail.ConversionIDTemp;

                var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                detail.StockQty = detail.StockQty / unitRate;
                detail.StockAvailable = detail.StockAvailable / unitRate;
            }
            base.PreDetailDisplay(header, details);
        }

        protected override void PreUpdateDisplay(PurchaseOrderModel header, List<PurchaseOrderDetailModel> details)
        {
            SetViewBagNotification();
            SetPreEditViewBag(header);
            SetPreEditDetailViewBag(header, details);

            base.PreUpdateDisplay(header, details);
        }

        protected void SetPreEditDetailViewBag(PurchaseOrderModel header, List<PurchaseOrderDetailModel> details)
        {
            foreach (var detail in details)
            {
                detail.ConversionIDTemp = detail.ConversionID;
                detail.StockAvailableHidden = detail.StockAvailable;
                detail.StockQtyHidden = detail.StockQty;
            }
        }

        public override MPL.Business.IMasterDetailBFC<PurchaseOrderModel, PurchaseOrderDetailModel> GetBFC()
        {
            return new PurchaseOrderBFC();
        }

        public override void CreateData(PurchaseOrderModel obj, List<PurchaseOrderDetailModel> details)
        {
            try
            {
                new PurchaseOrderBFC().Validate(obj, details);
                AssignItemNo(obj, details);
                //obj.CurrencyID = 2;
                obj.EmailTo = "hkasmara@abca-indonesia.com";
                base.CreateData(obj, details);

                TempData["SuccessNotification"] = "Document has been saved";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;
                var bookID = obj.BookingOrderID;
                
                if (bookID != 0)
                    RedirectToAction("Create", new { bookingOrderID = bookID, errorMessage = ex.Message });
                
                throw;
            }
        }

        public override void UpdateData(PurchaseOrderModel obj, List<PurchaseOrderDetailModel> details, FormCollection formCollection)
        {
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    new PurchaseOrderBFC().Validate(obj, details);
                    new PurchaseOrderBFC().UpdateValidation(obj, details);
                    base.UpdateData(obj, details, formCollection);
                    new PurchaseOrderBFC().UpdatePriceInContainerLogDetail(obj.ID);
                    new PurchaseOrderBFC().UpdatePriceInPurchaseBill(obj.ID);
                    new PurchaseOrderBFC().UpdatePriceInPurchaseBillHedaer(obj.ID);

                    if (obj.Status == (int)MPL.DocumentStatus.Approved)
                        new PurchaseOrderBFC().UpdateStatus(obj.ID);

                    trans.Complete();
                }
               

                TempData["SuccessNotification"] = "Document has been saved";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                //RedirectToAction("Update", new { key = obj.ID, ErrorMessage = ex.Message});
                throw;

            }
        }

        public void SetPreDetailViewBag()
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult UpdatePurchaseOrder(PurchaseOrderModel obj, FormCollection col)
        {
            try
            {
                new PurchaseOrderBFC().Validate(obj, obj.Details);
                new PurchaseOrderBFC().UpdateValidation(obj, obj.Details);
                AssignItemNo(obj, obj.Details);
                base.Update(obj, col);

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

            var poCount = new PurchaseOrderBFC().RetrieveCount(filter.GetSelectFilters(), false);
            var poList = new PurchaseOrderBFC().Retrieve((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters(), false);

            ResetBackToListUrl(filter);

            ViewBag.DataCount = poCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            ViewBag.PageSeriesSize = GetPageSeriesSize();

            return View(poList);
        }

        public ActionResult ApproveFromIndex(string key)
        {
            try
            {
                ApproveData(key);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

        public override void ApproveData(string key)
        {
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    new PurchaseOrderBFC().Approve(Convert.ToInt64(key), MembershipHelper.GetUserName());
                    new PurchaseOrderBFC().UpdateStatus(Convert.ToInt64(key));

                    var po = new PurchaseOrderBFC().RetrieveByID(Convert.ToInt64(key));
                    if (po.EmailTo == null || po.EmailTo =="")
                    {
                        throw new Exception("Masukan alamat email sebelum approve PO");
                    }
                    else
                    {
                        EmailPrintOut(Convert.ToInt64(key));
                    }
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

        public ActionResult VoidFromIndex(string key)
        {
            base.VoidData(key);

            return RedirectToAction("Index");
        }

        public ActionResult VoidRemarks(string key, bool voidFromIndex)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var purchaseOrder = new PurchaseOrderBFC().RetrieveByID(key);

                ViewBag.VoidFromIndex = voidFromIndex;

                return View(purchaseOrder);
            }

            return View();
        }

        [HttpPost]
        public ActionResult VoidRemarks(PurchaseOrderModel purchaseOrder, FormCollection col)
        {
            var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);

            try
            {
                new PurchaseOrderBFC().Void(purchaseOrder.ID, purchaseOrder.VoidRemarks, MembershipHelper.GetUserName());

                //new EmailHelper().SendVoidPurchaseOrderEmail(purchaseOrder.ID, purchaseOrder.VoidRemarks, MembershipHelper.GetUserName());

                TempData["SuccessNotification"] = "Document has been canceled";

                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = purchaseOrder.ID });
            }
            catch (Exception ex)
            {
                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = purchaseOrder.ID, errorMessage = ex.Message });
            }

        }

    }
}
