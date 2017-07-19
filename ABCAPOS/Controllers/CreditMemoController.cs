using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.Models;
using MPL.MVC;
using ABCAPOS.BF;
using ABCAPOS.Util;

namespace ABCAPOS.Controllers
{
    public class CreditMemoController : MasterDetailController<CreditMemoModel, CreditMemoDetailModel>
    {
        private string ModuleID
        {
            get
            {
                return "CreditMemo";
            }
        }

        private void SetViewBagPermission()
        {
            var roleDetails = new RoleBFC().RetrieveActions(MembershipHelper.GetRoleID(), ModuleID);

            ViewBag.AllowEdit = roleDetails.Contains("Edit");
            ViewBag.AllowCreate = roleDetails.Contains("Create");
            ViewBag.AllowVoid = roleDetails.Contains("Void");
        }

        public override MPL.Business.IMasterDetailBFC<CreditMemoModel, CreditMemoDetailModel> GetBFC()
        {
            return new CreditMemoBFC();
        }

        private void SetupEditableDisplay(CreditMemoModel header)
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
            ViewBag.StaffList = new StaffBFC().Retrieve(true);
            ViewBag.TermsList = new PurchaseOrderBFC().RetrieveAllTerms();
            ViewBag.ExpedisiList = new ExpedisiBFC().RetrieveAll();
            ViewBag.ConversionList = new ProductBFC().RetrieveAllUnits();
            ViewBag.PriceLevelList = new PriceLevelBFC().Retrieve(true);

            SetViewBagNotification();
            var ExcludeCommisions = "";
            if (!header.ExcludeCommisions)
            {
                ExcludeCommisions += "<tr><td>EXCLUDE COMMISIONS</td><td>:</td><td colspan='5'><input type='checkbox' name='ExcludeCommisions' value='0'></td></tr>";
            }
            else
            {
                ExcludeCommisions += "<tr><td>EXCLUDE COMMISIONS</td><td>:</td><td colspan='5'><input type='checkbox' name='ExcludeCommisions' value='1' checked></td></tr>";
            }
            ViewBag.CommisionCheckboxList = ExcludeCommisions;

            var Bonus = "";
            if (!header.Bonus)
            {
                Bonus += "<tr><td>BONUS</td><td>:</td><td colspan='5'><input type='checkbox' name='Bonus' value='0'></td></tr>";
            }
            else
            {
                Bonus += "<tr><td>BONUS</td><td>:</td><td colspan='5'><input type='checkbox' name='Bonus' value='1' checked></td></tr>";
            }
            ViewBag.BonusCheckBoxList = Bonus;
        }

        protected override void PreCreateDisplay(CreditMemoModel header, List<CreditMemoDetailModel> details)
        {
            SetupEditableDisplay(header);

            var customerReturnID = Convert.ToInt64(Request.QueryString["customerReturnID"]);

            new CreditMemoBFC().CreateCreditMemoByCustomerReturn(header, customerReturnID);

            header.WarehouseID = new StaffBFC().RetrieveDefaultWarehouseID(MembershipHelper.GetUserName());
            if (header.WarehouseID != 0)
                header.WarehouseName = new WarehouseBFC().RetrieveByID(header.WarehouseID).Name;

            base.PreCreateDisplay(header, header.Details);
        }

        protected override void PreUpdateDisplay(CreditMemoModel header, List<CreditMemoDetailModel> details)
        {
            SetupEditableDisplay(header);
            header.PriceLevelID = new CustomerBFC().RetrieveByID(header.CustomerID).PriceLevelID;
            var prodBFC = new ProductBFC();
            foreach (CreditMemoDetailModel detail in details)
            {
                var product = prodBFC.RetrieveByID(detail.ProductID);
                if (product != null)
                {
                    detail.PriceHidden = (double)prodBFC.RetrieveByIDPriceLevelIDTaxType(detail.ProductID, detail.PriceLevelID, detail.TaxType).SellingPrice;
                    detail.SaleUnitRateHidden = prodBFC.GetUnitRate(product.SaleUnitID);
                }
              
            }

            base.PreUpdateDisplay(header, details);
        }

        protected override void PreDetailDisplay(CreditMemoModel header, List<CreditMemoDetailModel> details)
        {
            SetViewBagNotification();
            SetViewBagPermission();
            ViewBag.ApplyCredit = new ApplyCreditMemoBFC().RetrieveByCreditMemoID(header.ID);
            var ExcludeCommisions = "";
            if (!header.ExcludeCommisions)
            {
                ExcludeCommisions += "<tr><td>EXCLUDE COMMISIONS</td><td>:</td><td colspan='5'><input type='checkbox' name='ExcludeCommisions' value='0'  disabled ></td></tr>";
            }
            else
            {
                ExcludeCommisions += "<tr><td>EXCLUDE COMMISIONS</td><td>:</td><td colspan='5'><input type='checkbox' name='ExcludeCommisions' value='1' checked disabled ></td></tr>";
            }
            ViewBag.CommisionCheckboxList = ExcludeCommisions;

            var Bonus = "";
            if (!header.Bonus)
            {
                Bonus += "<tr><td>BONUS</td><td>:</td><td colspan='5'><input type='checkbox' name='Bonus' value='0'  disabled ></td></tr>";
            }
            else
            {
                Bonus += "<tr><td>BONUS</td><td>:</td><td colspan='5'><input type='checkbox' name='Bonus' value='1' checked disabled ></td></tr>";
            }
            ViewBag.BonusCheckboxList = Bonus;
            base.PreDetailDisplay(header, details);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateCreditMemo(CreditMemoModel obj, FormCollection col)
        {
            try
            {

                //new CreditMemoBFC().Validate(obj, obj.Details);
                if (Request.Form["Bonus"] != null)
                    obj.Bonus = true;
                else
                    obj.Bonus = false;

                if (Request.Form["ExcludeCommisions"] != null)
                    obj.ExcludeCommisions = true;
                else
                    obj.ExcludeCommisions = false;

                foreach (var detail in obj.Details)
                {

                    detail.TotalAmount = (decimal)detail.Quantity * (decimal)detail.Price;
                    if (detail.TaxType == 2)
                        detail.TotalPPN = (decimal)detail.TotalAmount * (decimal)0.1;
                    else
                        detail.TotalPPN = 0;
                    detail.Total = detail.TotalAmount + detail.TotalPPN;

                    detail.Total = Math.Round(detail.Total, 0);
                }

                base.Create(obj);

                return RedirectToAction("Detail", new { key = obj.ID });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;
                ViewBag.Mode = UIMode.Create;
                SetViewBagPermission();

                return RedirectToAction("Create", new { customerReturnID = obj.CustomerReturnID, errorMessage = ex.Message });
            }

        }
        //public override void CreateData(CreditMemoModel obj, List<CreditMemoDetailModel> details)
        //{
        //    try
        //    {
        //        new CreditMemoBFC().Validate(obj, details);
        //        if (Request.Form["Bonus"] != null)
        //            obj.Bonus = true;
        //        else
        //            obj.Bonus = false;

        //        if (Request.Form["ExcludeCommisions"] != null)
        //            obj.ExcludeCommisions = true;
        //        else
        //            obj.ExcludeCommisions = false;

        //        foreach (var detail in details)
        //        {

        //            detail.TotalAmount = (decimal)detail.Quantity * (decimal)detail.Price;
        //            if (detail.TaxType == 2)
        //                detail.TotalPPN = (decimal)detail.TotalAmount * (decimal)0.1;
        //            else
        //                detail.TotalPPN = 0;
        //            detail.Total = detail.TotalAmount + detail.TotalPPN;

        //            detail.Total = Math.Round(detail.Total, 0);
        //        }

        //        base.CreateData(obj, details);

        //        TempData["SuccessNotification"] = "Document has been saved";
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.ErrorNotification = ex.Message;

        //        throw;
        //    }
        //}
        [HttpPost, ValidateInput(false)]
        public ActionResult UpdateCreditMemo(CreditMemoModel obj, FormCollection formCollection)
        {
            try
            {
                //new CreditMemoBFC().Validate(obj, obj.Details);
                if (Request.Form["Bonus"] != null)
                    obj.Bonus = true;
                else
                    obj.Bonus = false;

                if (Request.Form["ExcludeCommisions"] != null)
                    obj.ExcludeCommisions = true;
                else
                    obj.ExcludeCommisions = false;

                foreach (var detail in obj.Details)
                {

                    detail.TotalAmount = (decimal)detail.Quantity * (decimal)detail.Price;
                    if (detail.TaxType == 2)
                        detail.TotalPPN = (decimal)detail.TotalAmount * (decimal)0.1;
                    else
                        detail.TotalPPN = 0;
                    detail.Total = detail.TotalAmount + detail.TotalPPN;

                    detail.Total = Math.Round(detail.Total, 0);
                }

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

        protected override List<Button> GetAdditionalButtons(CreditMemoModel obj, List<CreditMemoDetailModel> details, UIMode mode)
        {
            var list = new List<Button>();

            if (mode == UIMode.Detail && obj.Status != (int) MPL.DocumentStatus.Void)
            {
                var creditMemoPrint = new Button();
                creditMemoPrint.Text = "Print Credit Memo";
                creditMemoPrint.CssClass = "button";
                creditMemoPrint.ID = "btnPrint";
                creditMemoPrint.OnClick = "if (confirm('Are you sure you want to print this document?')) { " + String.Format("window.open('{0}');", Url.Action("PopUp", "ReportViewer",
                    new { type = ReportViewerController.PrintOutType.Invoice, queryString = SystemConstants.str_CreditMemoID + "=" + obj.ID })) + " } ";
                creditMemoPrint.Href = "#";
                list.Add(creditMemoPrint);

            }
            
            return list;
        }

        public override ActionResult Detail(string key, string errorMessage)
        {
            SetViewBagPermission();

            return base.Detail(key, errorMessage);
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetIndexViewBag();
            SetViewBagPermission();

            var creditMemoCount = 0;
            var creditMemoList = new List<CreditMemoModel>();

            if (startIndex == null)
                startIndex = 0;

            if (amount == null)
                amount = 20;

            if (string.IsNullOrEmpty(sortParameter))
                sortParameter = "";

            //if (filter == null || filter.FilterFields.Count == 0)
            //{
            //    creditMemoCount = new CreditMemoBFC().RetrieveUnvoidCount(filter.GetSelectFilters());
            //    creditMemoList = new CreditMemoBFC().RetrieveUnvoid((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters());
            //}
            //else
            //{
            creditMemoCount = new CreditMemoBFC().RetrieveCount(filter.GetSelectFilters());
            creditMemoList = new CreditMemoBFC().Retrieve((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters());
            //}

            ViewBag.DataCount = creditMemoCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;
            ViewBag.PageSeriesSize = GetPageSeriesSize();

            return View(creditMemoList);
        }

        public ActionResult VoidRemarks(string key, bool voidFromIndex)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var creditMemo = new CreditMemoBFC().RetrieveByID(key);

                ViewBag.VoidFromIndex = voidFromIndex;

                return View(creditMemo);
            }

            return View();
        }

        [HttpPost]
        public ActionResult VoidRemarks(CreditMemoModel creditMemo, FormCollection col)
        {
            var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);

            try
            {
                new CreditMemoBFC().Void(creditMemo.ID, creditMemo.VoidRemarks, MembershipHelper.GetUserName());

                //new EmailHelper().SendVoidApplyCreditMemoEmail(customerReturn.ID, customerReturn.VoidRemarks, MembershipHelper.GetUserName());

                TempData["SuccessNotification"] = "Document has been canceled";

                //if (voidFromIndex)
                return RedirectToAction("Index");
                //else
                //    return RedirectToAction("Detail", new { key = customerReturn.ID });
            }
            catch (Exception ex)
            {
                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = creditMemo.ID, errorMessage = ex.Message });
            }

        }
        //public ActionResult Void(long key)
        //{
        //    try
        //    {
        //        new CreditMemoBFC().Void(key, MembershipHelper.GetUserName());

        //        return RedirectToAction("Detail", new { key = key });
        //    }
        //    catch (Exception ex)
        //    {
        //        return RedirectToAction("Detail", new { key = key, errorMessage = ex.Message });
        //    }
        //}
    }
}
