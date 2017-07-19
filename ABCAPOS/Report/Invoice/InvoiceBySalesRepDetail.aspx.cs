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

namespace ABCAPOS.Report.Invoice
{
    public partial class InvoiceBySalesRepDetail : System.Web.UI.Page
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
            if (gffDate.TextBoxDateFrom.Text != "")
            {
                var startDate = Convert.ToDateTime(gffDate.TextBoxDateFrom.Text);
                var endDate = Convert.ToDateTime(gffDate.TextBoxDateTo.Text);
                var customer = gffCustomerName.TextBoxString.Text;

                var list = new ABCAPOSReportDAC().RetrieveInvoiceBySalesRepDetailReport(startDate, endDate, customer);

                var dt = ObjectHelper.CreateDynamicDataTable(list);

                List<ReportParameter> param = new List<ReportParameter>();
                param.Add(new ReportParameter("Period", startDate.ToString("dd/MM/yyyy") + " s/d " + endDate.ToString("dd/MM/yyyy")));
                ReportViewer1.LocalReport.SetParameters(param);
                ReportViewer1.LocalReport.DataSources.Clear();
                ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("InvoiceBySalesRepDetailDT", dt as DataTable));
            }
            
            this.ReportViewer1.LocalReport.Refresh();
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