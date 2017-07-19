using System;
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

namespace ABCAPOS.Controllers
{
    public class CustomerReturnController : MasterDetailController<CustomerReturnModel, CustomerReturnDetailModel>
    {
        private void AssignItemNo(CustomerReturnModel header, List<CustomerReturnDetailModel> details)
        {
            int maxItemNo = details.Max(p => p.ItemNo);

            foreach (CustomerReturnDetailModel detail in details)
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
                return "CustomerReturn";
            }
        }

        private void SetViewBagPermission()
        {
            var roleDetails = new RoleBFC().RetrieveActions(MembershipHelper.GetRoleID(), ModuleID);

            ViewBag.AllowEdit = roleDetails.Contains("Edit");
            ViewBag.AllowCreate = roleDetails.Contains("Create");
            ViewBag.AllowVoid = roleDetails.Contains("Void");
        }

        private void SetDetail(CustomerReturnModel header, List<CustomerReturnDetailModel> details)
        {
            foreach (var detail in details)
            {
                double unitRate = 0;

                if (detail.ConversionID == 0)
                    unitRate = 1;
                else
                    unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);

                detail.StockQty = detail.StockQtyHidden / unitRate;
                detail.StockAvailable = detail.StockAvailableHidden / unitRate;
            }
        }

        public override MPL.Business.IMasterDetailBFC<CustomerReturnModel, CustomerReturnDetailModel> GetBFC()
        {
            return new CustomerReturnBFC();
        }

        private void SetEditableViewBag(CustomerReturnModel header)
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
            ViewBag.StaffList = new StaffBFC().Retrieve(true);
            ViewBag.TermsList = new PurchaseOrderBFC().RetrieveAllTerms();
            ViewBag.ExpedisiList = new ExpedisiBFC().RetrieveAll();
            ViewBag.ConversionList = new ProductBFC().RetrieveAllUnits();
            ViewBag.PriceLevelList = new PriceLevelBFC().Retrieve(true);
            ViewBag.SalesmanList = new SalesmanBFC().Retrieve(true);
            ViewBag.PaymentMethodList = new PaymentMethodBFC().Retrieve(true).OrderBy(p => p.Name);

            var ExcludeCommisions = "<tr><td>Exclude Commisions</td><td>:</td><td colspan='5'><input type='checkbox' name='ExcludeCommisions' value='" + header.ExcludeCommisions + "' ></td></tr>";
            ViewBag.CommisionCheckboxList = ExcludeCommisions;
        }

        protected override void PreCreateDisplay(CustomerReturnModel header, List<CustomerReturnDetailModel> details)
        {
            SetEditableViewBag(header);

            var salesOrderID = Request.QueryString["salesOrderID"];

            if (!string.IsNullOrEmpty(salesOrderID))
                new CustomerReturnBFC().PreFillWithSalesOrderData(header, Convert.ToInt64(salesOrderID));

            header.Code = new CustomerReturnBFC().GetCustomerReturnCode();
            base.PreCreateDisplay(header, details);
        }

        protected override void PreDetailDisplay(CustomerReturnModel header, List<CustomerReturnDetailModel> details)
        {
            SetViewBagNotification();

            SetViewBagPermission();

            var ExcludeCommisions = "";
            if (!header.ExcludeCommisions)
            {
                ExcludeCommisions += "<tr><td>Exclude Commisions</td><td>:</td><td colspan='5'><input type='checkbox' name='ExcludeCommisions' value='0'  disabled ></td></tr>";
            }
            else
            {
                ExcludeCommisions += "<tr><td>Exclude Commisions</td><td>:</td><td colspan='5'><input type='checkbox' name='ExcludeCommisions' value='1' checked disabled ></td></tr>";
            }
            ViewBag.CommisionCheckboxList = ExcludeCommisions;

            this.SetDetail(header, details);


            ViewBag.DOList = new ReturnReceiptBFC().RetrieveByCustomerReturnID(header.ID);
            ViewBag.InvoiceList = new CreditMemoBFC().RetrieveByCustomerReturnID(header.ID);

            base.PreDetailDisplay(header, details);
        }

        protected override void PreUpdateDisplay(CustomerReturnModel header, List<CustomerReturnDetailModel> details)
        {
            SetEditableViewBag(header);
            header.PriceLevelID = new CustomerBFC().RetrieveByID(header.CustomerID).PriceLevelID;
            var prodBFC = new ProductBFC();
            foreach (CustomerReturnDetailModel detail in details)
            {
                var product = prodBFC.RetrieveByID(detail.ProductID);
                detail.PriceHidden = (double)
                    prodBFC.RetrieveByIDPriceLevelIDTaxType(
                    detail.ProductID, detail.PriceLevelID, detail.TaxType).SellingPrice;
                detail.SaleUnitRateHidden = prodBFC.GetUnitRate(product.SaleUnitID);
            }

            this.SetDetail(header, details);

            base.PreUpdateDisplay(header, details);
        }

        public override void CreateData(CustomerReturnModel obj, List<CustomerReturnDetailModel> details)
        {
            var statusOnCreate = obj.Status;
            try
            {
                //new CustomerReturnBFC().Validate(obj, details);
                AssignItemNo(obj, details);
                base.CreateData(obj, details);
                if (statusOnCreate == (int)MPL.DocumentStatus.Approved)
                    ApproveData(obj.ID.ToString());

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;
                throw;
            }
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult UpdateCustomerReturn(CustomerReturnModel obj, FormCollection formCollection)
        {
            try
            {
                //new CustomerReturnBFC().Validate(obj, obj.Details);
                AssignItemNo(obj, obj.Details);

                if (Request.Form["ExcludeCommisions"] != null)
                    obj.ExcludeCommisions = true;
                else
                    obj.ExcludeCommisions = false;

                base.Update(obj, formCollection);
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

        //public override void UpdateData(CustomerReturnModel obj, List<CustomerReturnDetailModel> details, FormCollection formCollection)
        //{
        //    try
        //    {
        //        new CustomerReturnBFC().Validate(obj, details);
        //        AssignItemNo(obj, details);
        //        base.UpdateData(obj, details, formCollection);

        //        TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.ErrorNotification = ex.Message;

        //        throw;
        //    }
        //}

        protected override List<Button> GetAdditionalButtons(CustomerReturnModel header, List<CustomerReturnDetailModel> details, UIMode mode)
        {
            var list = new List<Button>();

            //if (mode == UIMode.Detail && header.Status >= (int)MPL.DocumentStatus.Approved)
            //{
            //    var print = new Button();
            //    print.Text = "Print Picking Ticket";
            //    print.CssClass = "button";
            //    print.ID = "btnPrintPickingTicket";
            //    print.OnClick = String.Format("window.open('{0}');", Url.Action("PopUp", "ReportViewer",
            //        new { type = ReportViewerController.PrintOutType.PickingTicket, queryString = SystemConstants.str_CustomerReturnID + "=" + header.ID }));
            //    print.Href = "#";
            //    list.Add(print);

            //}

            return list;
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
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();
            SetIndexViewBag();

            return base.Index(startIndex, amount, sortParameter, filter);
        }

        public ActionResult ApproveFromIndex(string key)
        {
            ApproveData(key);

            return RedirectToAction("Index");
        }

        public override void ApproveData(string key)
        {
            base.ApproveData(key);
            //new CustomerReturnBFC().UpdateInventory(key);
            //new CustomerReturnBFC().UpdateStatus(Convert.ToInt64(key));

            //EmailPrintOut(Convert.ToInt64(key));
        }

        //public void EmailPrintOut(long customerReturnID)
        //{
        //    var setting = new CompanySettingBFC().Retrieve();
        //    var customerReturn = new CustomerReturnBFC().RetrieveByID(customerReturnID);
        //    var customer = new CustomerBFC().RetrieveByID(customerReturn.CustomerID);

        //    var subject = setting.Name + ":" + customerReturn.Code;

        //    var body = "";
        //    using (var sr = new StreamReader(Server.MapPath("\\App_Data\\Templates\\CustomerReturnHeaderTemplate.txt")))
        //    {
        //        body = sr.ReadToEnd();
        //    }
        //    //4
        //    string emailMessageHeader = string.Format(body, setting.Address, setting.City + " " + setting.PostCode, "ID", 
        //        customer.Name, customer.Address, "", "", "", customerReturn.Date.ToString("dd/MM/yyyy"), customerReturn.Code, customerReturn.POCustomerNo, customer.Name, "" );

        //    var footer = "";
        //    using (var sFooter = new StreamReader(Server.MapPath("\\App_Data\\Templates\\CustomerReturnFooterTemplate.txt")))
        //    {
        //        footer = sFooter.ReadToEnd();
        //    }
        //    string emailMessageFooter = string.Format(footer, customerReturn.SubTotal.ToString("N2"), 
        //        customerReturn.TaxValue.ToString("N2"), customerReturn.GrandTotal.ToString("N2"));

        //    var content = "<tr><td valign=\"top\">{0}</td><td valign=\"top\" align=\"right\" nowrap>{1}</td><td valign=\"top\">{2}</td><td valign=\"top\" align=\"right\" nowrap>{3}</td><td valign=\"top\" align=\"right\" nowrap>{4}</td><td valign=\"top\" align=\"right\" nowrap>{5}</td></tr>";

        //    var customerReturnDetails = new CustomerReturnBFC().RetrieveDetails(customerReturnID);
        //    string contentMessage = "";
        //    foreach (var soDetail in customerReturnDetails)
        //    {
        //        contentMessage += string.Format(content, soDetail.ProductName, soDetail.Quantity.ToString("N0"), soDetail.ConversionName, soDetail.Price.ToString("N2"), soDetail.TotalAmount.ToString("N2"), soDetail.Total.ToString("N2"));
        //    }

        //    //new EmailHelper().SendSOEmail(setting.OwnerEmail, subject, emailMessageHeader + contentMessage + emailMessageFooter);

        //}

        public ActionResult VoidFromIndex(string key)
        {
            base.VoidData(key);

            return RedirectToAction("Index");
        }

        public ActionResult VoidRemarks(string key, bool voidFromIndex)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var customerReturn = new CustomerReturnBFC().RetrieveByID(key);

                ViewBag.VoidFromIndex = voidFromIndex;

                return View(customerReturn);
            }

            return View();
        }

        [HttpPost]
        public ActionResult VoidRemarks(CustomerReturnModel customerReturn, FormCollection col)
        {
            var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);

            try
            {
                new CustomerReturnBFC().Void(customerReturn.ID, customerReturn.VoidRemarks, MembershipHelper.GetUserName());

                TempData["SuccessNotification"] = "Document has been canceled";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = customerReturn.ID, errorMessage = ex.Message });
            }

        }
    }
}
