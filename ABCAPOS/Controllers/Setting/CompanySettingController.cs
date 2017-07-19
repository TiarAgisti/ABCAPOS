using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.BF;
using ABCAPOS.Models;
using ABCAPOS.Util;
using System.IO;
using MPL.MVC;

namespace ABCAPOS.Controllers.Setting
{
    public class CompanySettingController : GenericController<CompanySettingModel>
    {

        public override MPL.Business.IGenericBFC<CompanySettingModel> GetBFC()
        {
            return new CompanySettingBFC();
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

        [HttpPost]
        public ActionResult UpdateSetting(CompanySettingModel setting, FormCollection col, HttpPostedFileBase file)
        {
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = file.FileName;
                    var fileExtension = "";

                    if (fileName.Contains("."))
                        fileExtension = fileName.Substring(fileName.LastIndexOf("."));

                    var directory = "~/Uploads/CompanySetting/";

                    if (!Directory.Exists(Server.MapPath(directory)))
                        Directory.CreateDirectory(Server.MapPath(directory));

                    var path = Path.Combine(directory, "logo") + fileExtension;
                    file.SaveAs(Server.MapPath(path));

                    setting.ImageName = "logo" + fileExtension;

                    col.Add("ImageName", setting.ImageName);
                }

                base.Update(setting, col);

                return RedirectToAction("Detail");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                ViewBag.Mode = UIMode.Update;

                return View(setting);
            }
        }

    }
}
