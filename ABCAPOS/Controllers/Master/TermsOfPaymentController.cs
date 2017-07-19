using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.Models;
using MPL.MVC;
using ABCAPOS.BF;

namespace ABCAPOS.Controllers.Master
{
    public class TermsOfPaymentController : GenericController<TermsOfPaymentModel>
    {
        private string ModuleID
        {
            get
            {
                return "TermsOfPayment";
            }
        }

        private void SetViewBagPermission()
        {
            var roleDetails = new RoleBFC().RetrieveActions(MembershipHelper.GetRoleID(), ModuleID);

            ViewBag.AllowEdit = roleDetails.Contains("Edit");
            ViewBag.AllowCreate = roleDetails.Contains("Create");
        }

        private void SetViewBagNotification()
        {
            if (TempData["SuccessNotification"] != null)
                ViewBag.SuccessNotification = Convert.ToString(TempData["SuccessNotification"]);

            if (!string.IsNullOrEmpty(Request.QueryString["errorMessage"]))
                ViewBag.ErrorNotification = Convert.ToString(Request.QueryString["errorMessage"]);
        }

        public override MPL.Business.IGenericBFC<TermsOfPaymentModel> GetBFC()
        {
            return new TermsOfPaymentBFC();
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {

            SetViewBagPermission();

            return base.Index(startIndex, amount, sortParameter, filter);
        }

        public override void PreCreateDisplay(TermsOfPaymentModel obj)
        {
            base.PreCreateDisplay(obj);
        }

        public override void PreDetailDisplay(TermsOfPaymentModel obj)
        {
            SetViewBagNotification();

            SetViewBagPermission();

            base.PreDetailDisplay(obj);
        }

        public override void PreUpdateDisplay(TermsOfPaymentModel obj)
        {
            base.PreUpdateDisplay(obj);
        }

        public override void CreateData(TermsOfPaymentModel obj)
        {
            try
            {
                base.CreateData(obj);

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

        public override void UpdateData(TermsOfPaymentModel obj, FormCollection formCollection)
        {
            try
            {

                base.UpdateData(obj, formCollection);

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

    }
}
