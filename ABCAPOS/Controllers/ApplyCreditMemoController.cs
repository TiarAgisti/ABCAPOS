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
    public class ApplyCreditMemoController : MasterDetailController<ApplyCreditMemoModel, ApplyCreditMemoDetailModel>
    {

        private string ModuleID
        {
            get
            {
                return "ApplyCreditMemo";
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

        private void SetPreCreateViewBag(ApplyCreditMemoModel header)
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            ViewBag.DepartmentList = new DepartmentBFC().Retrieve(true);
            ViewBag.StaffList = new StaffBFC().Retrieve(true);
        }

        private void SetPreEditViewBag(ApplyCreditMemoModel header)
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

        private List<ApplyCreditMemoDetailModel> RemoveBlankBills(List<ApplyCreditMemoDetailModel> details)
        {
            var resultList = new List<ApplyCreditMemoDetailModel>();
            foreach (var detail in details)
            {
                if (detail.Amount > 0)
                {
                    resultList.Add(detail);
                }
            }
            return resultList;
        }

        public override MPL.Business.IMasterDetailBFC<ApplyCreditMemoModel, ApplyCreditMemoDetailModel> GetBFC()
        {
            return new ApplyCreditMemoBFC();
        }

        protected override List<Button> GetAdditionalButtons(ApplyCreditMemoModel header, List<ApplyCreditMemoDetailModel> details, UIMode mode)
        {
            var list = new List<Button>();
            return list;
        }

        protected override void PreCreateDisplay(ApplyCreditMemoModel header, List<ApplyCreditMemoDetailModel> details)
        {
            SetPreCreateViewBag(header);

            var customerReturnID = Request.QueryString["creditMemoID"];

            if (!string.IsNullOrEmpty(customerReturnID))
                new ApplyCreditMemoBFC().PreFillWithCreditMemoData(header, Convert.ToInt64(customerReturnID));
        
            base.PreCreateDisplay(header, details);
        }

        protected override void PreDetailDisplay(ApplyCreditMemoModel header, List<ApplyCreditMemoDetailModel> details)
        {
            SetPreDetailViewBag();
            SetViewBagNotification();
            SetViewBagPermission();
            base.PreDetailDisplay(header, details);
        }

        protected override void PreUpdateDisplay(ApplyCreditMemoModel header, List<ApplyCreditMemoDetailModel> details)
        {
            SetPreEditViewBag(header);

            new ApplyCreditMemoBFC().AddOtherPayableInvoices(header, details);
            //header.CreditRemaining = header.CeilingAmount = details.Sum(p => p.Amount);

            var creditMemoBFC = new CreditMemoBFC();
            var source = creditMemoBFC.RetrieveByID(header.CreditMemoID);
            if (source == null)
                return;

            header.CeilingAmount = source.TotalUnapplied + details.Sum(p => p.Amount);
            header.CreditRemaining = source.TotalUnapplied;

            base.PreUpdateDisplay(header, details);
        }

        public override void UpdateData(ApplyCreditMemoModel obj, List<ApplyCreditMemoDetailModel> details, FormCollection formCollection)
        {
            details = RemoveBlankBills(details);
            obj.CeilingAmount = details.Sum(p => p.Amount);
            base.UpdateData(obj, details, formCollection);
        }

        public override void CreateData(ApplyCreditMemoModel obj, List<ApplyCreditMemoDetailModel> details)
        {
            try
            {
                // disable validations for now
                //new ApplyCreditMemoBFC().Validate(obj, details);
                details = RemoveBlankBills(details);
                obj.CeilingAmount = details.Sum(p=>p.Amount);
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
                var applyCredit = new ApplyCreditMemoBFC().RetrieveByID(key);

                ViewBag.VoidFromIndex = voidFromIndex;

                return View(applyCredit);
            }

            return View();
        }

        [HttpPost]
        public ActionResult VoidRemarks(ApplyCreditMemoModel customerReturn, FormCollection col)
        {
            var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);

            try
            {
                new ApplyCreditMemoBFC().Void(customerReturn.ID, customerReturn.VoidRemarks, MembershipHelper.GetUserName());

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
                    return RedirectToAction("Detail", new { key = customerReturn.ID, errorMessage = ex.Message });
            }

        }
    }
}
