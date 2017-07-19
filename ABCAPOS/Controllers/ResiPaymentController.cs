using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.BF;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using ABCAPOS.Controllers.GenericController;
using MPL.Business;
using System.Transactions;


namespace ABCAPOS.Controllers
{
    public class ResiPaymentController : PSIMasterDetailController<ResiPaymentModel,ResiPaymentDetailModel>
    {
        private void readValues(ResiPaymentModel header)
        {
            header.AmountHelp = header.TotalAmount;
            foreach (var detail in header.Details)
            {
                detail.AmountStr = detail.Amount.ToString("N2");
            }
        }

        private void readValuesResi(ResiPaymentModel header)
        {
            foreach (var detail in header.ResiDetails)
            {
                detail.AmountStr = detail.Amount.ToString("N2");
            }
        }

        private List<ResiPaymentDetailModel> RemoveBlankBills(ResiPaymentModel header,List<ResiPaymentDetailModel> details)
        {
            var resultList = new List<ResiPaymentDetailModel>();
            foreach (var detail in details)
            {
                if (detail.Amount > 0 || detail.DiscountTaken > 0)
                {
                    resultList.Add(detail);
                }
            }

            foreach (var detailResi in header.ResiDetails)
            {
                if (detailResi.Amount > 0 || detailResi.DiscountTaken > 0)
                {
                    resultList.Add(detailResi);
                }
            }


            return resultList;
        }

        private void assignValues(ResiPaymentModel header, List<ResiPaymentDetailModel> details)
        {
            if (header.ExchangeRate == 0)
                header.ExchangeRate = 1;

            foreach (var detail in details)
            {
                if (detail.AmountStr == null)
                    detail.AmountStr = "0";

                if (detail.AmountStr.Length > 0)
                    detail.Amount = Convert.ToDouble(detail.AmountStr);

            }

            foreach (var detailResi in header.ResiDetails)
            {
                if (detailResi.AmountStr == null)
                    detailResi.AmountStr = "0";

                if (detailResi.AmountStr.Length > 0)
                    detailResi.Amount = Convert.ToDouble(detailResi.AmountStr);
            }
        }

        public ResiPaymentController()
        {
            base.ModuleID = "ResiPayment";
        }

        public override IMasterDetailBFC<ResiPaymentModel, ResiPaymentDetailModel> GetBFC()
        {
            return new ResiPaymentBFC();
        }

        protected override void SetPreview(ResiPaymentModel header, List<ResiPaymentDetailModel> details)
        {
            ViewBag.WarehouseList = new WarehouseBFC().Retrieve(true);
            ViewBag.PaymentMethodList = new PaymentMethodBFC().Retrieve(true);
            ViewBag.AccountList = new AccountBFC().RetrieveInvoicePaymentAutoComplete();

            base.SetPreview(header, details);
        }

        protected override void PreCreateDisplay(ResiPaymentModel header, List<ResiPaymentDetailModel> details)
        {
            var resiBillID = Request.QueryString["resiBillID"];

            if (!string.IsNullOrEmpty(resiBillID))
                new ResiPaymentBFC().PrepareResiBillByID(header, Convert.ToInt64(resiBillID));

            header.Code = "";

            base.PreCreateDisplay(header, details);
        }

        protected override void PreDetailDisplay(ResiPaymentModel header, List<ResiPaymentDetailModel> details)
        {
            //header.Details = new ResiPaymentBFC().RetrieveResiPaymentByIsCover(header.ID, true);
            //header.ResiDetails = new ResiPaymentBFC().RetrieveResiPaymentByIsCover(header.ID, false);
            this.readValues(header);
            this.readValuesResi(header);
            base.PreDetailDisplay(header, header.Details);
        }

        protected override void PreUpdateDisplay(ResiPaymentModel header, List<ResiPaymentDetailModel> details)
        {
            //header.Details = new ResiPaymentBFC().RetrieveResiPaymentByIsCover(header.ID, true);
            //header.ResiDetails = new ResiPaymentBFC().RetrieveResiPaymentByIsCover(header.ID, false);
            this.readValues(header);
            this.readValuesResi(header);
            base.PreUpdateDisplay(header, details);
        }

        public override void CreateDataFunction(ResiPaymentModel header, List<ResiPaymentDetailModel> details, string ModuleID)
        {
            this.assignValues(header, details);
            details = this.RemoveBlankBills(header,details);

            base.CreateDataFunction(header, details, ModuleID);
        }

        public override void UpdateDataFunction(ResiPaymentModel header, List<ResiPaymentDetailModel> details, FormCollection formCollection, string ModuleID)
        {
            this.assignValues(header, details);
            details = this.RemoveBlankBills(header, details);
            base.UpdateDataFunction(header, details, formCollection, ModuleID);
        }

        public override void ApproveData(string key)
        {
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    new ResiPaymentBFC().ApproveResiPayment(key, MembershipHelper.GetUserName(), MembershipHelper.GetRoleID());
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

        public ActionResult ApproveFromIndex(string key)
        {
            this.ApproveData(key);
            return base.RedirectToAction("Index", new { key = key});
        }

        public ActionResult VoidRemarks(string key, bool voidFromIndex)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var obj = new ResiPaymentBFC().RetrieveByID(Convert.ToInt64(key));
                ViewBag.VoidFromIndex = voidFromIndex;
                return View(obj);
            }
            return View();
        }

        [HttpPost]
        public ActionResult VoidRemarks(ResiPaymentModel header, FormCollection col)
        {
            var voidFromIndex = Convert.ToBoolean(col["hdnVoidFromIndex"]);
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    new ResiPaymentBFC().VoidResiPayment(header, MembershipHelper.GetUserName());
                    trans.Complete();
                }
                TempData["SuccesNotification"] = "Document has been canceled";

                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = header.ID});
            }
            catch (Exception ex)
            {
                if (voidFromIndex)
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Detail", new { key = header.ID, errorMesage = ex.Message});
            }
        }
    }
}
