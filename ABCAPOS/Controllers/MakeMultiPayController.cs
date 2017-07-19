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
    public class MakeMultiPayController : MasterDetailController<MakeMultiPayModel, MakeMultiPayDetailModel>
    {

        private string ModuleID
        {
            get
            {
                return "MakeMultiPay";
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

        private void readValues(MakeMultiPayModel header, List<MakeMultiPayDetailModel> details)
        {
            header.AmountHelp = header.TotalAmount;
            foreach (var detail in details)
            {
                detail.AmountStr = detail.Amount.ToString("N2");
            }
        }

        public override MPL.Business.IMasterDetailBFC<MakeMultiPayModel, MakeMultiPayDetailModel> GetBFC()
        {
            return new MakeMultiPayBFC();
        }

        private void SetPreCreateViewBag(MakeMultiPayModel header)
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
            ViewBag.StaffList = new StaffBFC().Retrieve(true);
            ViewBag.PaymentMethodList = new PaymentMethodBFC().Retrieve(true);
            ViewBag.AccountList = new AccountBFC().RetrieveInvoicePaymentAutoComplete();
            //ViewBag.AccountList = new AccountBFC().Retrieve(true);
        }

        private void SetPreEditViewBag(MakeMultiPayModel header)
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
            ViewBag.StaffList = new StaffBFC().Retrieve(true);
            ViewBag.PaymentMethodList = new PaymentMethodBFC().Retrieve(true);
            //ViewBag.AccountList = new AccountBFC().Retrieve(true);
            ViewBag.AccountList = new AccountBFC().RetrieveInvoicePaymentAutoComplete();
        }

        protected override List<Button> GetAdditionalButtons(MakeMultiPayModel header, List<MakeMultiPayDetailModel> details, UIMode mode)
        {
            var list = new List<Button>();
            return list;
        }

        private void AddIsFPCheckbox(MakeMultiPayModel header)
        {
            var IsFPCheckbox = "";
            if (header.IsFP)
            {
                IsFPCheckbox += "<tr><td>FAKTUR PAJAK PEMBELIAN</td><td>:</td><td colspan='5'><input type='checkbox' name='IsFakturPajak' value='0'></td></tr>";
            }
            else
            {
                IsFPCheckbox += "<tr><td>FAKTUR PAJAK PEMBELIAN</td><td>:</td><td colspan='5'><input type='checkbox' name='IsFakturPajak' value='1' checked></td></tr>";
            }
            ViewBag.IsFPCheckbox = IsFPCheckbox;
        }

        protected override void PreCreateDisplay(MakeMultiPayModel header, List<MakeMultiPayDetailModel> details)
        {
            SetPreCreateViewBag(header);
            AddIsFPCheckbox(header);

            var purchasebillID = Request.QueryString["purchaseBillID"];

            if (!string.IsNullOrEmpty(purchasebillID))
                new MakeMultiPayBFC().PreFillWithPurchaseBillData(header, Convert.ToInt64(purchasebillID));
        
            base.PreCreateDisplay(header, header.Details);
        }

        private void SetPreDetailViewBag()
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
        }

       

        protected override void PreDetailDisplay(MakeMultiPayModel header, List<MakeMultiPayDetailModel> details)
        {
            SetPreDetailViewBag();
            readValues(header, details);
            AddIsFPCheckbox(header);
            SetViewBagNotification();
            SetViewBagPermission();
            header.LastCode = header.Code;
            base.PreDetailDisplay(header, details);
        }

        protected override void PreUpdateDisplay(MakeMultiPayModel header, List<MakeMultiPayDetailModel> details)
        {
            SetPreEditViewBag(header);
            AddIsFPCheckbox(header);
            readValues(header, details);
            
            foreach (var detail in details)
            {
                // show amount due if this transaction didn't exist
                detail.AmountDue += detail.DiscountTaken + detail.Amount;
            }

            header.LastCode = header.Code;
            base.PreUpdateDisplay(header, details);
        }

        public override void UpdateData(MakeMultiPayModel obj, List<MakeMultiPayDetailModel> details, FormCollection formCollection)
        {
            try
            {
                assignValues(obj, details);
                base.UpdateData(obj, details, formCollection);
                TempData["SuccessNotification"] = "Document has been Updated";

            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;
                throw;
            }
           
        }

        private void assignValues(MakeMultiPayModel header, List<MakeMultiPayDetailModel> details)
        {
            foreach (var detail in details)
            {
                if (detail.AmountStr == null)
                    detail.AmountStr = "0";

                if (detail.AmountStr.Length > 0)
                    detail.Amount = Convert.ToDecimal(detail.AmountStr);

            }
        }


        private List<MakeMultiPayDetailModel> RemoveBlankBills(List<MakeMultiPayDetailModel> details)
        {
            var resultList = new List<MakeMultiPayDetailModel>();
            foreach (var detail in details)
            {
                if (detail.Amount > 0 || detail.DiscountTaken > 0)
                {
                    resultList.Add(detail);
                }
            }
            return resultList;
        }


        public override void CreateData(MakeMultiPayModel obj, List<MakeMultiPayDetailModel> details)
        {
            assignValues(obj, details);
            details = RemoveBlankBills(details);
            try
            {
                //new MakeMultiPayBFC().Validate(obj, details); remarks krn terjadi error,dalam proses perbaikan validasi(Tiar)
                // read Faktur Pajak Checkbox value
                if (Request.Form["IsFakturPajak"] != null)
                    obj.IsFP = true;
                else
                    obj.IsFP = false;
                base.CreateData(obj, details);

                TempData["SuccessNotification"] = "Document has been saved";
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
            return base.Index(startIndex, amount, sortParameter, filter);
        }

        public ActionResult VoidRemarks(string key, bool voidFromIndex)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var vendorReturn = new MakeMultiPayBFC().RetrieveByID(key);

                ViewBag.VoidFromIndex = voidFromIndex;

                return View(vendorReturn);
            }

            return View();
        }

        [HttpPost]
        public ActionResult VoidRemarks(MakeMultiPayModel vendorReturn, FormCollection col)
        {
            var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);

            try
            {
                new MakeMultiPayBFC().Void(vendorReturn.ID, vendorReturn.VoidRemarks, MembershipHelper.GetUserName());

                //new EmailHelper().SendVoidMakeMultiPayEmail(vendorReturn.ID, vendorReturn.VoidRemarks, MembershipHelper.GetUserName());

                //TempData["SuccessNotification"] = "Document has been canceled";

                return RedirectToAction("Index");

                //if (voidFromIndex)
                //    return RedirectToAction("Index");
                //else
                //    return RedirectToAction("Detail", new { key = vendorReturn.ID });
            }
            catch (Exception ex)
            {
                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = vendorReturn.ID, errorMessage = ex.Message });
            }

        }
    }
}
