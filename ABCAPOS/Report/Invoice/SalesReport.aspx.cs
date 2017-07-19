using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ABCAPOS.BF;
using ABCAPOS.DA;
using MPL;
using Microsoft.Reporting.WebForms;
using MPL.Reporting;
using ABCAPOS.Util;
using ABCAPOS.EDM;

namespace ABCAPOS.Report.Invoice
{
    public partial class SalesReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SetFilter();
                SetDataSource();
            }
        }

        private void SetDataSource()
        {
            var list = new ABCAPOSReportDAC().RetrieveSalesReport(GenericFilter1.SelectFilters);

            foreach (var dat in list)
            {
                var prodDate = dat.ProductDate.Split(';');
                var purchaseDateStr = prodDate[0].Split(':');
                var product = new ProductBFC().RetrieveByID(dat.ProductID);
                var productID = dat.ProductID;
                var purchaseDate = new DateTime(Convert.ToInt32(purchaseDateStr[1].ToString().Substring(0, 4)), Convert.ToInt32(purchaseDateStr[1].ToString().Substring(5, 2)), Convert.ToInt32(purchaseDateStr[1].ToString().Substring(8, 2)));
                var productDetail = new ProductBFC().RetrieveDetails(productID);
                var selProductDet = from i in productDetail
                                    where i.Date == purchaseDate
                                    select i;
                var sellingPrice = Math.Round(selProductDet.ToList()[0].Price, 0);
                var assetPrice = Math.Round(selProductDet.ToList()[0].AssetPrice, 0);
                if (sellingPrice > 0 && sellingPrice > assetPrice)
                    dat.CostExpedition = (sellingPrice - assetPrice) * Convert.ToDecimal(dat.Quantity);
                
                dat.PurchasePrice = assetPrice * Convert.ToDecimal(dat.Quantity);
                //dat.PurchaseDateDesc = selProductDet.ToList()[0].PurchaseOrderCode;
                //foreach (var pDate in prodDate)
                //{
                //    var purchaseDatesStr = pDate.Split(':');
                //    //dat.PurchaseDate += purchaseDatesStr[1] + ",";
                //}
            }

            var dt = ObjectHelper.CreateDynamicDataTable(list);

            string filePath = GenerateFromXML();

            ReportViewer1.LocalReport.DataSources.Clear();
            ReportViewer1.LocalReport.ReportPath = filePath;
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("EDS", dt));
            ReportViewer1.DataBind();
        }

        protected void GenericFilter1_Filter()
        {
            SetDataSource();
        }

        private void SetFilter()
        {
            string month = DateTime.Now.Month.ToString();
            DateTime startDate = Convert.ToDateTime("1/" + month + "/" + DateTime.Now.Year);
            DateTime enddate = (startDate.AddMonths(1)).AddDays(-1);

            gffDate.TextBoxDateFrom.Text = startDate.ToString("dd/MM/yyyy");
            gffDate.TextBoxDateTo.Text = enddate.ToString("dd/MM/yyyy");
        }

        private string GenerateFromXML()
        {
            var rpt = new TableReport();

            rpt.PageWidth = 15f;
            rpt.DataSourceItemType = typeof(v_Rpt_Sales);

            ReportHelper.ReadFromXML(rpt, SystemConstants.ReportXMLFolder + "Invoice/ReportXML/SalesReport.xml");

            rpt.Filters = ReportHelper.GetReportFilters(GenericFilter1);
            return ReportHelper.GenerateReport(rpt);
        }
    }
}