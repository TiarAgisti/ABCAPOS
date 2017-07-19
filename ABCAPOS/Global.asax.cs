using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ABCAPOS.EDM;
using System.Configuration;
using ABCAPOS.Util;
using System.IO;
using System.Globalization;
using System.Threading;

namespace ABCAPOS
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private bool _licenseSubmitted = false;

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

            ViewEngines.Engines.Add(new MasterViewEngine());
            ViewEngines.Engines.Add(new SettingViewEngine());
            ViewEngines.Engines.Add(new ListViewEngine());
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            AppDomain.CurrentDomain.SetData("SQLServerCompactEditionUnderWebHosting", true);
            MPL.Configuration.DefaultPageSeriesSize = 10;

            MPL.Business.Configuration.DefaultObjectContextType = typeof(ABCAPOSEntities);
            MPL.Configuration.ConnectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;

            MPL.Configuration.AllowDeleteTransaction = false;

            MPL.Configuration.Language = MPL.Language.English;
            MPL.Configuration.GetCurrentDateTimeFunction = new MPL.Configuration.GetCurrentDateTimeDelegate(GetCurrentDateTime);

            SystemConstants.ReportXMLFolder = Server.MapPath("~/Report/");
            SystemConstants.TempReportFolder = Server.MapPath("~/TempReport/");
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            if (!_licenseSubmitted)
                SubmitLicense();
        }

        public void SubmitLicense()
        {
            string domainName = HttpContext.Current.Request.ServerVariables["SERVER_NAME"].ToLower();

            if (domainName.StartsWith("www."))
                domainName = domainName.Replace("www.", "");

            string directory = Server.MapPath("~/License/");

            bool validLicense = false;
            foreach (string filePath in Directory.GetFiles(directory, "*.lic"))
            {
                string lic = File.ReadAllText(filePath);
                validLicense = MPL.Configuration.SubmitWebLicense("Smartec", domainName, lic);

                if (validLicense)
                    break;
            }

            _licenseSubmitted = validLicense;
        }

        protected void Application_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            CultureInfo objCultureInfo = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = objCultureInfo;
            Thread.CurrentThread.CurrentUICulture = objCultureInfo;
        }

        public static DateTime GetCurrentDateTime()
        {
            return DateTime.Now;
        }
    }
}