using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ABCAPOS
{
    public class SettingViewEngine : RazorViewEngine
    {
        public SettingViewEngine()
            : base()
        {
            base.ViewLocationFormats = base.ViewLocationFormats.Concat(new[] {
            "~/Views/Setting/{1}/{0}.cshtml",
            "~/Views/Setting/{1}/{0}.vbhtml"
        }).ToArray();

            base.PartialViewLocationFormats = base.PartialViewLocationFormats.Concat(new[] {
            "~/Views/Setting/{1}/{0}.cshtml",
            "~/Views/Setting/{1}/{0}.vbhtml"
        }).ToArray();
        }
    }
}