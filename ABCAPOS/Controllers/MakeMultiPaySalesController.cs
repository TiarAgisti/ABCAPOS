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

namespace ABCAPOS.Controllers
{
    public class MakeMultiPaySalesController : MasterDetailController<MakeMultiPaySalesModel, MakeMultiPaySalesDetailModel>
    {
        private string ModuleID
        {
            get
            {
                return "MakeMultiPaySales";
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

        private void SetPreCreateViewBag(MakeMultiPaySalesModel header)
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
            ViewBag.StaffList = new StaffBFC().Retrieve(true);
            ViewBag.AccountList = new AccountBFC().Retrieve(true);
        }

        private void SetPreEditViewBag(MakeMultiPaySalesModel header)
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
            ViewBag.StaffList = new StaffBFC().Retrieve(true);
            ViewBag.AccountList = new AccountBFC().Retrieve(true);
        }

        private void AddIsBonusCheckbox(MakeMultiPaySalesModel header)
        {
            var IsBonusCheckbox = "";
            if (header.IsBonus)
            {
                IsBonusCheckbox += "<tr><td>BONUS</td><td>:</td><td><input type='checkbox' name='IsBonus' value='0'></td></tr>";
            }
            else
            {
                IsBonusCheckbox += "<tr><td>BONUS</td><td>:</td><td><input type='checkbox' name='IsBonus' value='1' checked></td></tr>";
            }

            ViewBag.IsBonusCheckbox = IsBonusCheckbox; //+ IsDepositCheckbox;
        }

        private void AddIsDepositCheckbox(MakeMultiPaySalesModel header)
        {
            var IsDepositCheckbox = "";
            if (header.IsDeposit)
            {
                IsDepositCheckbox += "<tr><td>DEPOSIT</td><td>:</td><td><input type='checkbox' name='IsDeposit' value='0'></td></tr>";
            }
            else
            {
                IsDepositCheckbox += "<tr><td>DEPOSIT</td><td>:</td><td><input type='checkbox' name='IsDeposit' value='1' checked></td></tr>";
            }
            ViewBag.IsDepositCheckbox = IsDepositCheckbox;
        }

        private void SetIndexViewBag()
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
        }

        private List<MakeMultiPaySalesDetailModel> RemoveBlankBills(List<MakeMultiPaySalesDetailModel> details)
        {
            var resultList = new List<MakeMultiPaySalesDetailModel>();
            foreach (var detail in details)
            {
                if (detail.Amount > 0 || detail.DiscountTaken > 0)
                {
                    resultList.Add(detail);
                }
            }
            return resultList;
        }

        private void SetPreDetailViewBag()
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
            ViewBag.AccountList = new AccountBFC().Retrieve(true);
        }

        public override MPL.Business.IMasterDetailBFC<MakeMultiPaySalesModel, MakeMultiPaySalesDetailModel> GetBFC()
        {
            return new MakeMultiPaySalesBFC();
        }

        protected override List<Button> GetAdditionalButtons(MakeMultiPaySalesModel header, List<MakeMultiPaySalesDetailModel> details, UIMode mode)
        {
            var list = new List<Button>();
            return list;
        }

        protected override void PreCreateDisplay(MakeMultiPaySalesModel header, List<MakeMultiPaySalesDetailModel> details)
        {
            SetPreCreateViewBag(header);
            AddIsBonusCheckbox(header);
            AddIsDepositCheckbox(header);
            //AddIsAutoApplyCheckbox(header);

            var invoiceID = Request.QueryString["invoiceID"];

            if (!string.IsNullOrEmpty(invoiceID))
                new MakeMultiPaySalesBFC().PreFillWithInvoiceData(header, Convert.ToInt64(invoiceID));

            base.PreCreateDisplay(header, details);
        }

        protected override void PreDetailDisplay(MakeMultiPaySalesModel header, List<MakeMultiPaySalesDetailModel> details)
        {
            SetPreDetailViewBag();
            AddIsBonusCheckbox(header);
            AddIsDepositCheckbox(header);

            header.AmountHelp = header.TotalPayment;

            SetViewBagNotification();
            SetViewBagPermission();
            base.PreDetailDisplay(header, details);
        }

        protected override void PreUpdateDisplay(MakeMultiPaySalesModel header, List<MakeMultiPaySalesDetailModel> details)
        {
            SetPreEditViewBag(header);
            AddIsBonusCheckbox(header);
            AddIsDepositCheckbox(header);

            new MakeMultiPaySalesBFC().AddOtherPayableInvoices(header, details);

            base.PreUpdateDisplay(header, details);
        }

        public override void CreateData(MakeMultiPaySalesModel obj, List<MakeMultiPaySalesDetailModel> details)
        {
            try
            {
                //new MakeMultiPaySalesBFC().Validate(obj, details);
                details = RemoveBlankBills(details);
                // read Faktur Pajak Checkbox value
                if (Request.Form["IsBonus"] != null)
                    obj.IsBonus = true;
                else
                    obj.IsBonus = false;
                if (Request.Form["IsDeposit"] != null)
                    obj.IsDeposit = true;
                else
                    obj.IsDeposit = false;

                base.CreateData(obj, details);

                TempData["SuccessNotification"] = "Document has been saved";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;
                throw;
            }
        }

        public override void UpdateData(MakeMultiPaySalesModel obj, List<MakeMultiPaySalesDetailModel> details, FormCollection formCollection)
        {
            try
            {
                details = RemoveBlankBills(details);
                // read Faktur Pajak Checkbox value
                if (Request.Form["IsBonus"] != null)
                    obj.IsBonus = true;
                else
                    obj.IsBonus = false;
                if (Request.Form["IsDeposit"] != null)
                    obj.IsDeposit = true;
                else
                    obj.IsDeposit = false;

                var header = new MakeMultiPaySalesBFC().RetrieveByID(obj.ID);
                obj.CreatedBy = header.CreatedBy;
                obj.CreatedDate = header.CreatedDate;
                obj.ModifiedBy = MembershipHelper.GetUserName();
                obj.ModifiedDate = DateTime.Now;
                obj.ApprovedBy = MembershipHelper.GetUserName();
                obj.ApprovedDate = DateTime.Now;

                //this.ValidationPayment(header, details);

                base.UpdateData(obj, details, formCollection);
                TempData["SuccessNotification"] = "Document has been updated";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;
                throw;
            }
          
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();
            SetIndexViewBag();

            return base.Index(startIndex, amount, sortParameter, filter);
        }

        public ActionResult VoidRemarks(string key, bool voidFromIndex)
        {
            var invoice = new MakeMultiPaySalesBFC().RetrieveByID(key);
            if (!string.IsNullOrEmpty(key))
            {
                new MakeMultiPaySalesBFC().Void(invoice.ID, invoice.VoidRemarks, MembershipHelper.GetUserName());
                ViewBag.VoidFromIndex = voidFromIndex;
            }

            return RedirectToAction("Index");

        }

        //private void AddIsAutoApplyCheckbox(MakeMultiPaySalesModel header)
        //{
        //    var IsAutoApplyCheckbox = "";

        //    IsAutoApplyCheckbox += "<tr><td>AUTO APPLY</td><td>:</td><td><input type='checkbox' name='IsAutoApply' value='0' onchange='onchangePayment()' ></td></tr>";

        //    ViewBag.IsAutoApplyCheckbox = IsAutoApplyCheckbox;
        //}

        //[HttpPost]
        //public ActionResult VoidRemarks(MakeMultiPaySalesModel invoice, FormCollection col)
        //{
        //    var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);

        //    try
        //    {
        //        new MakeMultiPaySalesBFC().Void(invoice.ID, invoice.VoidRemarks, MembershipHelper.GetUserName());

        //        //new EmailHelper().SendVoidMakeMultiPayEmail(vendorReturn.ID, vendorReturn.VoidRemarks, MembershipHelper.GetUserName());

        //        TempData["SuccessNotification"] = "Document has been canceled";

        //        if (voidFromIndex)
        //            return RedirectToAction("Index");
        //        else
        //            return RedirectToAction("Detail", new { key = invoice.ID });
        //    }
        //    catch (Exception ex)
        //    {
        //        if (voidFromIndex)
        //            return RedirectToAction("Index");
        //        else
        //            return RedirectToAction("Detail", new { key = invoice.ID, errorMessage = ex.Message });
        //    }

        //}
    }
}
