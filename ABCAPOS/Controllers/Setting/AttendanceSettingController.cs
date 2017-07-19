using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.Models;
using ABCAPOS.BF;
using MPL.MVC;

namespace ABCAPOS.Controllers.Setting
{
    public class AttendanceSettingController : GenericController<AttendanceSettingModel>
    {
        public override MPL.Business.IGenericBFC<AttendanceSettingModel> GetBFC()
        {
            return new AttendanceSettingBFC();
        }

        public override ActionResult Update(string key)
        {
            key = "1";

            return base.Update(key);
        }

        public override ActionResult Detail(string key, string errorMessage)
        {
            key = "1";

            return base.Detail(key, errorMessage);
        }

    }
}
