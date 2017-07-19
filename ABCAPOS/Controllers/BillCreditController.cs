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
using System.Transactions;

namespace ABCAPOS.Controllers
{
    public class BillCreditController : MasterDetailController<BillCreditModel, BillCreditDetailModel>
    {

        private string ModuleID
        {
            get
            {
                return "BillCredit";
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
            //ViewBag.SupplierGroupList = new SupplierGroupBFC().RetrieveAll();
        }

        private void SetPreCreateViewBag(BillCreditModel header)
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
            ViewBag.StaffList = new StaffBFC().Retrieve(true);
            ViewBag.ConversionList = new ProductBFC().RetrieveAllUnits();
            header.CurrencyID = 1;
            header.CurrencyName = "Rupiah";
            var IsFakturPajakCheckboxList = "<tr><td>Faktur Pajak</td><td>:</td><td colspan='5'><input type='checkbox' name='IsFakturPajak' value='" + header.IsFakturPajak + "' ></td></tr>";
            ViewBag.FPembelianCheckboxList = IsFakturPajakCheckboxList;
        }

        private void SetPreEditViewBag(BillCreditModel header)
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
            ViewBag.ConversionList = new ProductBFC().RetrieveAllUnits();
            ViewBag.StaffList = new StaffBFC().Retrieve(true);
        }

        public override MPL.Business.IMasterDetailBFC<BillCreditModel, BillCreditDetailModel> GetBFC()
        {
            return new BillCreditBFC();
        }

        protected override List<Button> GetAdditionalButtons(BillCreditModel header, List<BillCreditDetailModel> details, UIMode mode)
        {
            var list = new List<Button>();

            return list;
        }

        protected override void PreCreateDisplay(BillCreditModel header, List<BillCreditDetailModel> details)
        {
            SetPreCreateViewBag(header);

            var vendorReturnID = Request.QueryString["vendorReturnID"];

            if (!string.IsNullOrEmpty(vendorReturnID))
                new BillCreditBFC().PreFillWithReturnData(header, Convert.ToInt64(vendorReturnID));
        
            header.Code = new BillCreditBFC().GetBillCreditCode();
        
            base.PreCreateDisplay(header, details);
        }

        protected override void PreDetailDisplay(BillCreditModel header, List<BillCreditDetailModel> details)
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
            ViewBag.ApplyCredit = new ApplyBillCreditBFC().RetrieveByBillCreditID(header.ID);

            var SJReturnCheckboxList = "";

            if (!header.IsFakturPajak)
            {
                SJReturnCheckboxList += "<tr><td>Faktur Pajak</td><td>:</td><td colspan='5'><input type='checkbox' name='IsFakturPajak' value='0'  disabled ></td></tr>";
            }
            else
            {
                SJReturnCheckboxList += "<tr><td>Faktur Pajak</td><td>:</td><td colspan='5'><input type='checkbox' name='IsFakturPajak' value='1' checked disabled ></td></tr>";
            }
            ViewBag.FPembelianCheckboxList = SJReturnCheckboxList;

            SetViewBagNotification();
            SetViewBagPermission();
            base.PreDetailDisplay(header, details);
        }

        protected override void PreUpdateDisplay(BillCreditModel header, List<BillCreditDetailModel> details)
        {
            SetPreEditViewBag(header);

            base.PreUpdateDisplay(header, details);
        }

        public override void CreateData(BillCreditModel obj, List<BillCreditDetailModel> details)
        {
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    new BillCreditBFC().Validate(obj, details);
                    if (Request.Form["IsFakturPajak"] != null)
                        obj.IsFakturPajak = true;
                    else
                        obj.IsFakturPajak = false;
                    base.CreateData(obj, details);

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

        public override void UpdateData(BillCreditModel obj, List<BillCreditDetailModel> details, FormCollection formCollection)
        {
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    new BillCreditBFC().Validate(obj, details);

                    base.UpdateData(obj, details, formCollection);

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

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();
            SetIndexViewBag();

            var billCount = 0;
            var billList = new List<BillCreditModel>();

            if (startIndex == null)
                startIndex = 0;

            if (amount == null)
                amount = 20;

            if (string.IsNullOrEmpty(sortParameter))
                sortParameter = "";

            billCount = new BillCreditBFC().RetrieveBillCreditCount(filter.GetSelectFilters());
            billList = new BillCreditBFC().RetrieveBillCredit((int)startIndex, (int)amount, sortParameter, filter.GetSelectFilters());

            ViewBag.DataCount = billCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;

            return View(billList);

            //return base.Index(startIndex, amount, sortParameter, filter);
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
                    new BillCreditBFC().Approve(Convert.ToInt64(key), MembershipHelper.GetUserName());
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
                var vendorReturn = new BillCreditBFC().RetrieveByID(key);

                ViewBag.VoidFromIndex = voidFromIndex;

                return View(vendorReturn);
            }

            return View();
        }

        [HttpPost]
        public ActionResult VoidRemarks(BillCreditModel vendorReturn, FormCollection col)
        {
            var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);

            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    new BillCreditBFC().Void(vendorReturn.ID, vendorReturn.VoidRemarks, MembershipHelper.GetUserName());

                    //new EmailHelper().SendVoidBillCreditEmail(vendorReturn.ID, vendorReturn.VoidRemarks, MembershipHelper.GetUserName());

                    trans.Complete();
                }
                TempData["SuccessNotification"] = "Document has been canceled";

                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = vendorReturn.ID });
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
