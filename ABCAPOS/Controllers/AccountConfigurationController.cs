using ABCAPOS.BF;
using ABCAPOS.Models;
using ABCAPOS.Util;
using MPL.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ABCAPOS.Controllers
{
    public class AccountConfigurationController : MasterDetailController<AccountConfigurationModel,AccountConfigurationDetailModel>
    {
        private string ModuleID
        {
            get
            {
                return "AccountConfiguration";
            }
        }

        private void SetViewBagPermission()
        {
            var roleDetails = new RoleBFC().RetrieveActions(MembershipHelper.GetRoleID(), ModuleID);

            ViewBag.AllowEdit = roleDetails.Contains("Edit");
            ViewBag.AllowCreate = roleDetails.Contains("Create");
        }

        private void SetDetails(AccountConfigurationModel header, List<AccountConfigurationDetailModel> details)
        {
            header.Details = (from i in details
                                    where i.Type == (int)AccountType.Debit
                                    select i).ToList();

            header.CreditDetails = (from i in details
                                    where i.Type == (int)AccountType.Credit
                                    select i).ToList();
        }

        public override MPL.Business.IMasterDetailBFC<AccountConfigurationModel, AccountConfigurationDetailModel> GetBFC()
        {
            return new AccountConfigurationBFC();
        }

        protected override void PreCreateDisplay(AccountConfigurationModel header, List<AccountConfigurationDetailModel> details)
        {
            header.Code = new AccountConfigurationBFC().GetCode();
            
            base.PreCreateDisplay(header, details);
        }

        protected override void PreDetailDisplay(AccountConfigurationModel header, List<AccountConfigurationDetailModel> details)
        {
            SetDetails(header, details);

            SetViewBagPermission();
            
            base.PreDetailDisplay(header, header.Details);
        }

        protected override void PreUpdateDisplay(AccountConfigurationModel header, List<AccountConfigurationDetailModel> details)
        {
            SetDetails(header, details);

            base.PreUpdateDisplay(header, header.Details);
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();
            
            return base.Index(startIndex, amount, sortParameter, filter);
        }

        public ActionResult CreateAccountConfiguration(AccountConfigurationModel accountConfiguration, List<AccountConfigurationDetailModel> details, List<AccountConfigurationDetailModel> creditDetails)
        {
            if (details == null)
                details = new List<AccountConfigurationDetailModel>();

            if (creditDetails == null)
                creditDetails = new List<AccountConfigurationDetailModel>();

            accountConfiguration.CreditDetails = creditDetails;

            base.CreateData(accountConfiguration, details);

            TempData["SuccessNotification"] = "Dokumen berhasil disimpan";

            return RedirectToAction("Detail", new { key = accountConfiguration.ID });
        }

        public ActionResult UpdateAccountConfiguration(AccountConfigurationModel accountConfiguration, List<AccountConfigurationDetailModel> details, List<AccountConfigurationDetailModel> creditDetails)
        {
            if (details == null)
                details = new List<AccountConfigurationDetailModel>();

            if (creditDetails == null)
                creditDetails = new List<AccountConfigurationDetailModel>();
            
            accountConfiguration.ModifiedBy = MembershipHelper.GetUserName();
            accountConfiguration.ModifiedDate = DateTime.Now;
            accountConfiguration.CreditDetails= creditDetails;

            new AccountConfigurationBFC().Update(accountConfiguration, details);

            TempData["SuccessNotification"] = "Dokumen berhasil disimpan";

            return RedirectToAction("Detail", new { key = accountConfiguration.ID });
        }
    }
}
