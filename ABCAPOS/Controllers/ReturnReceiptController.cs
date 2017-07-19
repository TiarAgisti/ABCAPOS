using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.BF;
using ABCAPOS.Util;
using ABCAPOS.Models;
using MPL.MVC;
using ABCAPOS.Helpers;

namespace ABCAPOS.Controllers
{
    public class ReturnReceiptController : MasterDetailController<ReturnReceiptModel, ReturnReceiptDetailModel>
    {
        private string ModuleID
        {
            get
            {
                return "ReturnReceipt";
            }
        }

        private List<ReturnReceiptDetailModel> RemoveEmptyLines(List<ReturnReceiptDetailModel> details)
        {
            var resultList = new List<ReturnReceiptDetailModel>();
            foreach (var detail in details)
            {
                if (detail.ProductID != 0)
                {
                    resultList.Add(detail);
                }
            }
            return resultList;
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

        private void SetPreEditViewBag(ReturnReceiptModel obj, long customerReturnID)
        {
            var template = "";

            ViewBag.RowEditor = template;
        }

        private void SetEditableDisplay()
        {
            ViewBag.BinList = new BinBFC().RetrieveAll();
        }

        private void SetDetail(ReturnReceiptModel header, List<ReturnReceiptDetailModel> details)
        {
            foreach (var detail in details)
            {
                var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);

                if (unitRate != 0)
                {
                    detail.StockQty = detail.StockQty / unitRate;
                    detail.StockQtyAvailable = detail.StockQtyAvailable / unitRate;
                }
                else
                {
                    unitRate = 1;
                    detail.StockQty = detail.StockQty / unitRate;
                    detail.StockQtyAvailable = detail.StockQtyAvailable / unitRate;
                }
              
            }
        }

        public override MPL.Business.IMasterDetailBFC<ReturnReceiptModel, ReturnReceiptDetailModel> GetBFC()
        {
            return new ReturnReceiptBFC();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();

            return base.Index(startIndex, amount, sortParameter, filter);
        }

        protected override void PreCreateDisplay(ReturnReceiptModel header, List<ReturnReceiptDetailModel> details)
        {
            SetViewBagNotification();
            SetEditableDisplay();
            var customerReturnID = Convert.ToInt64(Request.QueryString["customerReturnID"]);

            new ReturnReceiptBFC().CreateByCustomerReturn(header, customerReturnID);

            base.PreCreateDisplay(header, header.Details);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateReturnReceipt(ReturnReceiptModel obj, FormCollection col)
        {
            try
            {
                //new ReturnReceiptBFC().Validate(obj, obj.Details);
                obj.Details = RemoveEmptyLines(obj.Details);

                base.Create(obj);

                TempData["SuccessNotification"] = "Document has been saved";

                return RedirectToAction("Detail", new { key = obj.ID });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;
                SetPreEditViewBag(obj, 0);
                ViewBag.Mode = UIMode.Create;
                SetViewBagPermission();

                return RedirectToAction("Create", new { key = obj.ID, errorMessage = ex.Message });
            }

        }

        [HttpPost, ValidateInput(false)]
        public ActionResult UpdateReturnReceipt(ReturnReceiptModel obj, FormCollection col)
        {
            try
            {
                obj.Details = RemoveEmptyLines(obj.Details);

                base.Update(obj, col);

                TempData["SuccessNotification"] = "Document has been updated";

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

        protected void SetPreEditDetailViewBag(ReturnReceiptModel header, List<ReturnReceiptDetailModel> details)
        {
            foreach (var detail in details)
            {
                detail.StrQuantity = detail.Quantity.ToString("N0");

            }
        }

        protected override void PreUpdateDisplay(ReturnReceiptModel header, List<ReturnReceiptDetailModel> details)
        {
            SetPreEditDetailViewBag(header, details);
            SetEditableDisplay();
            this.SetDetail(header, details);
            base.PreUpdateDisplay(header, details);
        }

        protected override void PreDetailDisplay(ReturnReceiptModel header, List<ReturnReceiptDetailModel> details)
        {
            
            SetViewBagNotification();

            SetViewBagPermission();

            this.SetDetail(header, details);
           
            base.PreDetailDisplay(header, details);
        }

        public ActionResult VoidRemarks(string key, bool voidFromIndex)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var returnReceipt = new ReturnReceiptBFC().RetrieveByID(key);

                ViewBag.VoidFromIndex = voidFromIndex;

                return View(returnReceipt);
            }

            return View();
        }

        [HttpPost]
        public ActionResult VoidRemarks(ReturnReceiptModel returnReceipt, FormCollection col)
        {
            var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);

            try
            {
                new ReturnReceiptBFC().Void(returnReceipt.ID, returnReceipt.VoidRemarks, MembershipHelper.GetUserName());

                TempData["SuccessNotification"] = "Document has been canceled";


                //if (voidFromIndex)
                //    return RedirectToAction("Index");
                //else
                //return RedirectToAction("Detail", new { key = returnReceipt.ID });
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = returnReceipt.ID, errorMessage = ex.Message });
            }

        }
    }
}
