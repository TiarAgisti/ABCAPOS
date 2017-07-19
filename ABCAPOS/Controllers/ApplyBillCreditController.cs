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
    public class ApplyBillCreditController : MasterDetailController<ApplyBillCreditModel, ApplyBillCreditDetailModel>
    {

        private string ModuleID
        {
            get
            {
                return "ApplyBillCredit";
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

        private void SetPreCreateViewBag(ApplyBillCreditModel header)
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
            ViewBag.StaffList = new StaffBFC().Retrieve(true);
        }

        private void SetPreEditViewBag(ApplyBillCreditModel header)
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
            ViewBag.StaffList = new StaffBFC().Retrieve(true);
        }

        private void SetPreDetailViewBag()
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
        }

        private void AssignValue(List<ApplyBillCreditDetailModel> details)
        {
            foreach (var detail in details)
            {
                detail.Amount = Convert.ToDecimal(detail.Amount);      
            }
        }

        private List<ApplyBillCreditDetailModel> RemoveBlankBills(List<ApplyBillCreditDetailModel> details)
        {
            var resultList = new List<ApplyBillCreditDetailModel>();
            foreach (var detail in details)
            {
                if (detail.Amount > 0)
                {
                    resultList.Add(detail);
                }
            }
            return resultList;
        }

        public override MPL.Business.IMasterDetailBFC<ApplyBillCreditModel, ApplyBillCreditDetailModel> GetBFC()
        {
            return new ApplyBillCreditBFC();
        }

        protected override List<Button> GetAdditionalButtons(ApplyBillCreditModel header, List<ApplyBillCreditDetailModel> details, UIMode mode)
        {
            var list = new List<Button>();
            return list;
        }

        protected override void PreCreateDisplay(ApplyBillCreditModel header, List<ApplyBillCreditDetailModel> details)
        {
            SetPreCreateViewBag(header);

            var vendorReturnID = Request.QueryString["billCreditID"];

            if (!string.IsNullOrEmpty(vendorReturnID))
                new ApplyBillCreditBFC().PreFillWithBillCreditData(header, Convert.ToInt64(vendorReturnID));
        
            base.PreCreateDisplay(header, details);
        }

        protected override void PreDetailDisplay(ApplyBillCreditModel header, List<ApplyBillCreditDetailModel> details)
        {
            SetPreDetailViewBag();
            SetViewBagNotification();
            SetViewBagPermission();
            base.PreDetailDisplay(header, details);
        }

        protected override void PreUpdateDisplay(ApplyBillCreditModel header, List<ApplyBillCreditDetailModel> details)
        {
            SetPreEditViewBag(header);

            base.PreUpdateDisplay(header, details);
        }

        public override void CreateData(ApplyBillCreditModel obj, List<ApplyBillCreditDetailModel> details)
        {
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    new ApplyBillCreditBFC().Validate(obj, details);
                    details = RemoveBlankBills(details);
                    obj.CeilingAmount = details.Sum(p => p.Amount);
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

        public override void UpdateData(ApplyBillCreditModel obj, List<ApplyBillCreditDetailModel> details, FormCollection formCollection)
        {
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    details = RemoveBlankBills(details);
                    this.AssignValue(details);
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
            var dataCount = 0;
            var dataList = new List<ApplyBillCreditModel>();
            if (startIndex == null)
                startIndex = 0;
            if (amount == null)
                amount = 20;
            if (string.IsNullOrEmpty(sortParameter))
                sortParameter = "";

            dataCount = new ApplyBillCreditBFC().RetrieveApplyBillCreditCount(filter.GetSelectFilters());
            dataList = new ApplyBillCreditBFC().RetrieveApplyBillCredit((int) startIndex, (int) amount,sortParameter, filter.GetSelectFilters());

            ViewBag.DataCount = dataCount;
            ViewBag.PageSize = amount;
            ViewBag.StartIndex = startIndex;
            ViewBag.FilterFields = filter.FilterFields;

            return View(dataList);
            //return base.Index(startIndex, amount, sortParameter, filter);
        }

        public ActionResult VoidRemarks(string key, bool voidFromIndex)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var vendorReturn = new ApplyBillCreditBFC().RetrieveByID(key);

                ViewBag.VoidFromIndex = voidFromIndex;

                return View(vendorReturn);
            }

            return View();
        }

        [HttpPost]
        public ActionResult VoidRemarks(ApplyBillCreditModel vendorReturn, FormCollection col)
        {
            var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);

            try
            {
                new ApplyBillCreditBFC().Void(vendorReturn.ID, vendorReturn.VoidRemarks, MembershipHelper.GetUserName());

                //new EmailHelper().SendVoidApplyBillCreditEmail(vendorReturn.ID, vendorReturn.VoidRemarks, MembershipHelper.GetUserName());

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
