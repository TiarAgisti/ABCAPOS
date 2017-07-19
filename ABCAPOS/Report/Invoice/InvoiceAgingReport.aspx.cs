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

namespace ABCAPOS.Report.Invoice
{
    public partial class InvoiceAgingReport : System.Web.UI.Page
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
            var paramDT = new ABCAPOSReportEDSC.ParamDTDataTable();
            var dueDate = Convert.ToDateTime(gffDueDate.TextBoxDate.Text);
            var startDate = SystemConstants.UnsetDateTime;
            var endDate = SystemConstants.UnsetDateTime;

            var customerName = "";

            var paramDR = paramDT.NewParamDTRow();
            paramDR.Label = "Jatuh Tempo";
            paramDR.Value = dueDate.ToString("dd/MM/yyyy");
            paramDT.LoadDataRow(paramDR.ItemArray, false);

            if (gffCustomerName.Selected == true)
            {
                customerName = gffCustomerName.DropDownList.Text;

                paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Customer";
                paramDR.Value = customerName;
                paramDT.LoadDataRow(paramDR.ItemArray, false);
            }

            if (gffDate.Selected)
            {
                startDate = Convert.ToDateTime(gffDate.TextBoxDateFrom.Text);
                endDate = Convert.ToDateTime(gffDate.TextBoxDateTo.Text);

                paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Periode";
                paramDR.Value = startDate.ToString("dd/MM/yyyy") + " s/d " + endDate.ToString("dd/MM/yyyy");
                paramDT.LoadDataRow(paramDR.ItemArray, false);
            }

            var list = new ABCAPOSReportDAC().RetrieveInvoiceAgingReport(startDate, endDate, dueDate, customerName);
            var dt = ObjectHelper.CreateDynamicDataTable(list);

            ReportViewer1.LocalReport.DataSources.Clear();
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("InvoiceAgingDT", dt as DataTable));
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("ParamDT", paramDT as DataTable));
            this.ReportViewer1.LocalReport.Refresh();
        }

        protected void GenericFilter1_Filter()
        {
            SetDataSource();
        }

        private void SetFilter()
        {
            gffDueDate.TextBoxDate.Text = DateTime.Now.ToString("MM/dd/yyyy");

            string month = DateTime.Now.Month.ToString();
            DateTime startDate = Convert.ToDateTime(month + "/1/" + DateTime.Now.Year);
            DateTime enddate = (startDate.AddMonths(1)).AddDays(-1);

            gffDate.TextBoxDateFrom.Text = gffDueDate.TextBoxDateFrom.Text = startDate.ToString("MM/dd/yyyy");
            gffDate.TextBoxDateTo.Text = gffDueDate.TextBoxDateTo.Text = enddate.ToString("MM/dd/yyyy");
        }
    }
}