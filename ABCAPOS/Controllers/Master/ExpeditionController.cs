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

namespace ABCAPOS.Controllers.Master
{
    public class ExpeditionController : PSIMasterDetailController<ExpeditionModel,ExpeditionDetailModel>
    {
        public ExpeditionController()
        {
            base.ModuleID = "Expedition";
        }

        public override IMasterDetailBFC<ExpeditionModel, ExpeditionDetailModel> GetBFC()
        {
            return new ExpeditionBFC();
        }

        public override void CreateDataFunction(ExpeditionModel header, List<ExpeditionDetailModel> details, string ModuleID)
        {
            base.CreateDataFunction(header, details, ModuleID);
        }
    }
}
