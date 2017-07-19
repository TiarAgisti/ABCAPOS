using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ABCAPOS
{
    public class MasterViewEngine : RazorViewEngine
    {
        public MasterViewEngine()
            : base()
        {
            base.ViewLocationFormats = base.ViewLocationFormats.Concat(new[] {
            "~/Views/Master/{1}/{0}.cshtml",
            "~/Views/Master/{1}/{0}.vbhtml"
        }).ToArray();

            base.PartialViewLocationFormats = base.PartialViewLocationFormats.Concat(new[] {
            "~/Views/Master/{1}/{0}.cshtml",
            "~/Views/Master/{1}/{0}.vbhtml"
        }).ToArray();
        }
    }
}