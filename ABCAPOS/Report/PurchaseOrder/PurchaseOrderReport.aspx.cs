using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MPL.Reporting;
using ABCAPOS.DA;
using MPL;
using Microsoft.Reporting.WebForms;
using ABCAPOS.Util;
using ABCAPOS.EDM;
using ABCAPOS.ReportEDS;
using System.Data;

namespace ABCAPOS.Report.PurchaseOrder
{
    public partial class PurchaseOrderReport : System.Web.UI.Page
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
            var list = new ABCAPOSReportDAC().RetrievePurchaseOrderReport(GenericFilter1.SelectFilters);
            var dt = ObjectHelper.CreateDynamicDataTable(list);

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
                paramDR.Label = "Transaction No";
                paramDR.Value = gffCode.TextBoxString.Text;
                paramDT.LoadDataRow(paramDR.ItemArray, false);
            }

            if (gffVendorName.Selected)
            {
                var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Vendor";
                paramDR.Value = gffVendorName.TextBoxString.Text;
                paramDT.LoadDataRow(paramDR.ItemArray, false);
            }

            if (gffStatus.Selected)
            {
                var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Status";
                paramDR.Value = gffStatus.TextBoxString.Text;
                paramDT.LoadDataRow(paramDR.ItemArray, false);
            }

            ReportViewer1.LocalReport.DataSources.Clear();
            //ReportViewer1.LocalReport.ReportPath = filePath;
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("ParamDT", paramDT as DataTable));
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
            DateTime startDate = Convert.ToDateTime(month + "/1/" + DateTime.Now.Year);
            DateTime enddate = (startDate.AddMonths(1)).AddDays(-1);

            gffDate.TextBoxDateFrom.Text = startDate.ToString("MM/dd/yyyy");
            gffDate.TextBoxDateTo.Text = enddate.ToString("MM/dd/yyyy");

        }

    }
}