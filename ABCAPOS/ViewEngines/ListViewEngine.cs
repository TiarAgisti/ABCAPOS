using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ABCAPOS
{
    public class ListViewEngine : RazorViewEngine
    {
        public ListViewEngine()
            : base()
        {
            base.ViewLocationFormats = base.ViewLocationFormats.Concat(new[] {
            "~/Views/List/{1}/{0}.cshtml",
            "~/Views/List/{1}/{0}.vbhtml"
        }).ToArray();

            base.PartialViewLocationFormats = base.PartialViewLocationFormats.Concat(new[] {
            "~/Views/List/{1}/{0}.cshtml",
            "~/Views/List/{1}/{0}.vbhtml"
        }).ToArray();
        }
    }
}