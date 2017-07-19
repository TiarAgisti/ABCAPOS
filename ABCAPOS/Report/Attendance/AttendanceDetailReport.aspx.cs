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
using ABCAPOS.ReportEDS;

namespace ABCAPOS.Report.Attendance
{
    public partial class AttendanceDetailReport : System.Web.UI.Page
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
            var list = new ABCAPOSReportDAC().RetrieveAttendanceDetailReport(GenericFilter1.SelectFilters);
            var dt = ObjectHelper.CreateDynamicDataTable(list);

            var paramDT = new ABCAPOSReportEDSC.ParamDTDataTable();

            var id = 1;

            if (gffDate.Selected)
            {
                var startDate = Convert.ToDateTime(gffDate.TextBoxDateFrom.Text);
                var endDate = Convert.ToDateTime(gffDate.TextBoxDateTo.Text);

                var paramDR = paramDT.NewParamDTRow();
                paramDR.ID = id++;
                paramDR.Label = "Tanggal Transaksi";
                paramDR.Value = startDate.ToString("dd/MM/yyyy") + " s/d " + endDate.ToString("dd/MM/yyyy");
                paramDT.LoadDataRow(paramDR.ItemArray, true);
            }

            ReportViewer1.LocalReport.DataSources.Clear();
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("ParamDT", paramDT as DataTable));
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AttendanceDetailDT", dt as DataTable));
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
            rpt.DataSourceItemType = typeof(v_Rpt_AttendanceDetail);
            ReportHelper.ReadFromXML(rpt, SystemConstants.ReportXMLFolder + "Attendance/ReportXML/AttendanceDetail.xml");

            rpt.Filters = ReportHelper.GetReportFilters(GenericFilter1);
            return ReportHelper.GenerateReport(rpt);
        }
    }
}