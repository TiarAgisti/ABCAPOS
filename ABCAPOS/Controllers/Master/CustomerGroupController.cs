using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.Models;
using ABCAPOS.BF;
using MPL.MVC;

namespace ABCAPOS.Controllers.Master
{
    public class CustomerGroupController : GenericController<CustomerGroupModel>
    {
        private string ModuleID
        {
            get
            {
                return "Customer";
            }
        }

        private void SetViewBagPermission()
        {
            var roleDetails = new RoleBFC().RetrieveActions(MembershipHelper.GetRoleID(), ModuleID);

            ViewBag.AllowEdit = roleDetails.Contains("Edit");
            ViewBag.AllowCreate = roleDetails.Contains("Create");
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();

            return base.Index(startIndex, amount, sortParameter, filter);
        }

        public override MPL.Business.IGenericBFC<CustomerGroupModel> GetBFC()
        {
            return new CustomerGroupBFC();
        }

        public override void PreDetailDisplay(CustomerGroupModel obj)
        {
            SetViewBagPermission();

            base.PreDetailDisplay(obj);
        }

    }
}