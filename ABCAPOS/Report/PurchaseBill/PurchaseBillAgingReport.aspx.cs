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

namespace ABCAPOS.Report.PurchaseBill
{
    public partial class PurchaseBillAgingReport : System.Web.UI.Page
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

            var vendorName = "";
            int agingType = 0;

            var paramDR = paramDT.NewParamDTRow();
            paramDR.Label = "Jatuh Tempo";
            paramDR.Value = dueDate.ToString("dd/MM/yyyy");
            paramDT.LoadDataRow(paramDR.ItemArray, false);

            if (gffVendorName.Selected == true)
            {
                vendorName = gffVendorName.DropDownList.Text;

                paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Vendor";
                paramDR.Value = vendorName;
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

            if (gffAgingType.Selected)
            {
                //var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Jatuh Tempo";
                paramDR.Value = gffAgingType.DropDownList.SelectedItem.Text;
                paramDT.LoadDataRow(paramDR.ItemArray, false);
                agingType = Convert.ToInt32(gffAgingType.DropDownList.SelectedItem.Value);
            }

            var list = new ABCAPOSReportDAC().RetrievePurchaseBillAgingReport(startDate, endDate, dueDate, vendorName, agingType);
            var dt = ObjectHelper.CreateDynamicDataTable(list);

            ReportViewer1.LocalReport.DataSources.Clear();
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("PurchaseBillAgingDS", dt as DataTable));
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

            DropDownList ddlAgingType = gffAgingType.DropDownList as DropDownList;
            ddlAgingType.Items.Clear();

            ListItem t = new ListItem();
            t = new ListItem("1 - 30 Days", Convert.ToString(1));
            ddlAgingType.Items.Add(t);

            t = new ListItem("31 - 60 Days", Convert.ToString(2));
            ddlAgingType.Items.Add(t);

            t = new ListItem("61 - 90 Days", Convert.ToString(3));
            ddlAgingType.Items.Add(t);

            t = new ListItem("91 - 120 Days", Convert.ToString(4));
            ddlAgingType.Items.Add(t);

            t = new ListItem("Above 120 Days", Convert.ToString(5));
            ddlAgingType.Items.Add(t);
        }
    }
}