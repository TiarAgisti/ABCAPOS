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
using ABCAPOS.Models;
using System.Data;

namespace ABCAPOS.Report.Accounting
{
    public partial class TrialBalance : System.Web.UI.Page
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
            var startDate = Convert.ToDateTime(gffDate.TextBoxDateFrom.Text);
            var endDate = Convert.ToDateTime(gffDate.TextBoxDateTo.Text);
            long accountID = 0;

            var list = new ABCAPOSReportDAC().RetrieveTrialBalanceReport(startDate, endDate,accountID);
            var dt = ObjectHelper.CreateDynamicDataTable(list);

            List<ReportParameter> param = new List<ReportParameter>();
            param.Add(new ReportParameter("Period", startDate.ToString(SystemConstants.str_dateFormat) + " s/d " + endDate.ToString(SystemConstants.str_dateFormat)));
            ReportViewer1.LocalReport.SetParameters(param);

            ReportViewer1.LocalReport.DataSources.Clear();
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("TrialBalanceDT", dt as DataTable));
            this.ReportViewer1.LocalReport.Refresh();
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

            gffDate.TextBoxDateFrom.Text = startDate.ToString(SystemConstants.str_dateFormat);
            gffDate.TextBoxDateTo.Text = enddate.ToString(SystemConstants.str_dateFormat);
        }

        private string GenerateFromXML()
        {
            var rpt = new TableReport();
            rpt.DataSourceItemType = typeof(TrialBalanceReportModel);
            ReportHelper.ReadFromXML(rpt, SystemConstants.ReportXMLFolder + "Accounting/ReportXML/TrialBalance.xml");

            rpt.Filters = ReportHelper.GetReportFilters(GenericFilter1);
            return ReportHelper.GenerateReport(rpt);
        }
    }
}