using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.Models;
using MPL.MVC;
using ABCAPOS.BF;
using System.Globalization;

namespace ABCAPOS.Controllers
{
    public class AllowanceController : MasterDetailController<AllowanceModel, AllowanceDetailModel>
    {
        private string ModuleID
        {
            get
            {
                return "Allowance";
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

        private void SetIndexViewBag()
        {
            ViewBag.CustomerGroupList = new CustomerGroupBFC().RetrieveAll();
        }

        public void SetPreEditViewBag(AllowanceModel header)
        {
            var template = "";

            template = "<tr><td>Tanggal</td><td>:</td><td><select id='Month' name='Month' >";

            for (int i = 1; i <= 12; i++)
            {
                if (header.Month == i)
                    template += "<option value='" + i + "' selected='true'>";
                else
                    template += "<option value='" + i + "' >";

                template += CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i) + "</option>";
            }

            template += "</select><select id='Year' name='Year' >";

            for (int i = 2013; i <= 2100; i++)
            {
                if (header.Year == i)
                    template += "<option value='" + i + "' selected='true'>" + i + "</option>";
                else
                    template += "<option value='" + i + "' >" + i + "</option>";
            }

            template += "</select></td></tr>";

            ViewBag.DateList = template;
        }

        public override MPL.Business.IMasterDetailBFC<AllowanceModel, AllowanceDetailModel> GetBFC()
        {
            return new AllowanceBFC();
        }

        public override void CreateData(AllowanceModel obj, List<AllowanceDetailModel> details)
        {
            try
            {
                obj.Date = new DateTime(obj.Year, obj.Month, 1);
                new AllowanceBFC().Validate(obj, details);

                base.CreateData(obj, details);

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

        public override void UpdateData(AllowanceModel obj, List<AllowanceDetailModel> details, FormCollection formCollection)
        {
            try
            {
                obj.Date = new DateTime(obj.Year, obj.Month, 1);
                new AllowanceBFC().Validate(obj, details);

                base.UpdateData(obj, details, formCollection);

                TempData["SuccessNotification"] = "Dokumen berhasil disimpan";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorNotification = ex.Message;

                throw;
            }
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetIndexViewBag();
            SetViewBagPermission();

            return base.Index(startIndex, amount, sortParameter, filter);
        }

        protected override void PreCreateDisplay(AllowanceModel header, List<AllowanceDetailModel> details)
        {
            header.Code = new AllowanceBFC().GetAllowanceCode();
            header.Month = DateTime.Now.Month;
            header.Year = DateTime.Now.Year;

            SetPreEditViewBag(header);

            new AllowanceBFC().CreateFirstAllowance(header);

            base.PreCreateDisplay(header, header.Details);
        }

        protected override void PreDetailDisplay(AllowanceModel header, List<AllowanceDetailModel> details)
        {
            SetViewBagNotification();

            SetViewBagPermission();

            base.PreDetailDisplay(header, details);
        }

        protected override void PreUpdateDisplay(AllowanceModel header, List<AllowanceDetailModel> details)
        {
            header.Month = header.Date.Month;
            header.Year = header.Date.Year;

            SetPreEditViewBag(header);

            base.PreUpdateDisplay(header, details);
        }

        protected override List<Button> GetAdditionalButtons(AllowanceModel header, List<AllowanceDetailModel> details, UIMode mode)
        {
            var list = new List<Button>();

            if (mode == UIMode.Detail && header.Status != (int)MPL.DocumentStatus.Void)
            {
                var print = new Button();
                print.Text = "Cetak";
                print.CssClass = "button";
                print.ID = "btnPrint";
                print.OnClick = String.Format("window.open('{0}');", Url.Action("PopUp", "ReportViewer",
                    new { type = ReportViewerController.PrintOutType.Allowance, queryString = "allowanceID=" + header.ID }));
                print.Href = "#";
                list.Add(print);
            }

            return list;
        }


    }
}
