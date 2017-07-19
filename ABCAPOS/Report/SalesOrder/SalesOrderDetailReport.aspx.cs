using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ABCAPOS.DA;
using MPL;
using Microsoft.Reporting.WebForms;
using MPL.Reporting;
using ABCAPOS.Util;
using ABCAPOS.EDM;
using System.Data;
using System.Reflection;
using ABCAPOS.ReportEDS;
using ABCAPOS.BF;

namespace ABCAPOS.Report.SalesOrder
{
    public partial class SalesOrderDetailReport : System.Web.UI.Page
    {
        private void SetDataSource()
        {
            var list = new ABCAPOSReportDAC().RetrieveSalesOrderDetailReport(GenericFilter1.SelectFilters);
            var dt = ObjectHelper.CreateDynamicDataTable(list);

            //string filePath = GenerateFromXML();

            var paramDT = new ABCAPOSReportEDSC.ParamDTDataTable();

            if (gffDate.Selected)
            {
                var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Tanggal Transaksi";
                paramDR.Value = Convert.ToDateTime(gffDate.TextBoxDateFrom.Text).ToString("dd/MM/yyyy") + " s/d " + Convert.ToDateTime(gffDate.TextBoxDateTo.Text).ToString("dd/MM/yyyy");
                paramDT.LoadDataRow(paramDR.ItemArray, false);
            }

            if (gffCode.Selected)
            {
                var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Kode Sales Order";
                paramDR.Value = gffCode.TextBoxString.Text;
                paramDT.LoadDataRow(paramDR.ItemArray, false);
            }

            if (gffCustomerName.Selected)
            {
                var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Nama Pelanggan";
                paramDR.Value = gffCustomerName.TextBoxString.Text;
                paramDT.LoadDataRow(paramDR.ItemArray, false);
            }

            ReportViewer1.LocalReport.DataSources.Clear();
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("ParamDT", paramDT as DataTable));
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("EDS", dt));
            ReportViewer1.DataBind();
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
            rpt.DataSourceItemType = typeof(v_Rpt_SalesOrderDetail);

            ReportHelper.ReadFromXML(rpt, SystemConstants.ReportXMLFolder + "SalesOrder/ReportXML/SalesOrderDetail.xml");

            rpt.Filters = ReportHelper.GetReportFilters(GenericFilter1);
            return ReportHelper.GenerateReport(rpt);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SetFilter();
                SetDataSource();
            }
        }

        protected void GenericFilter1_Filter()
        {
            SetDataSource();
        }
    }
}