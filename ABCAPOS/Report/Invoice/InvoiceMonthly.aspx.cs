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
using ABCAPOS.Models;
using ABCAPOS.ReportEDS;
using System.Data;

namespace ABCAPOS.Report.Invoice
{
    public partial class InvoiceMonthly : System.Web.UI.Page
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
            var startDate = SystemConstants.UnsetDateTime;
            var endDate = SystemConstants.UnsetDateTime;
            var startDueDate = SystemConstants.UnsetDateTime;
            var endDueDate = SystemConstants.UnsetDateTime;

            List<ReportParameter> param = new List<ReportParameter>();
            var paramDT = new ABCAPOSReportEDSC.ParamDTDataTable();

            if (gffDate.Selected)
            {
                startDate = Convert.ToDateTime(gffDate.TextBoxDateFrom.Text);
                endDate = Convert.ToDateTime(gffDate.TextBoxDateTo.Text);

                var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Tanggal Transaksi";
                paramDR.Value = Convert.ToDateTime(gffDate.TextBoxDateFrom.Text).ToString("dd/MM/yyyy") + " s/d " + Convert.ToDateTime(gffDate.TextBoxDateTo.Text).ToString("dd/MM/yyyy");
                paramDT.LoadDataRow(paramDR.ItemArray, false);

                //param.Add(new ReportParameter("Period", startDate.ToString("dd/MM/yyyy") + " s/d " + endDate.ToString("dd/MM/yyyy")));
                //ReportViewer1.LocalReport.SetParameters(param);
            }

            if (gffDueDate.Selected)
            {
                startDueDate = Convert.ToDateTime(gffDueDate.TextBoxDateFrom.Text);
                endDueDate = Convert.ToDateTime(gffDueDate.TextBoxDateTo.Text);

                var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Tanggal Due Date";
                paramDR.Value = Convert.ToDateTime(gffDueDate.TextBoxDateFrom.Text).ToString("dd/MM/yyyy") + " s/d " + Convert.ToDateTime(gffDueDate.TextBoxDateTo.Text).ToString("dd/MM/yyyy");
                paramDT.LoadDataRow(paramDR.ItemArray, false);

                //param.Add(new ReportParameter("DueDate", startDueDate.ToString("dd/MM/yyyy") + " s/d " + endDueDate.ToString("dd/MM/yyyy")));
                //ReportViewer1.LocalReport.SetParameters(param);
            }

            var warehouseName = "";
            if (gffWarehouse.Selected == true)
            {
                var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Location";
                paramDR.Value = gffWarehouse.DropDownList.SelectedItem.Text;
                paramDT.LoadDataRow(paramDR.ItemArray, false);

                warehouseName = gffWarehouse.DropDownList.Text;
            }
               

            var customer = "";
            if (gffCustomerName.Selected)
            {
                var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Customer";
                paramDR.Value = gffCustomerName.TextBoxString.Text;
                paramDT.LoadDataRow(paramDR.ItemArray, false);

                customer = gffCustomerName.TextBoxString.Text;
            }
              

            var status = "";
            if (gffStatus.Selected)
            {
                var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Status";
                paramDR.Value = gffStatus.DropDownList.SelectedItem.Text;
                paramDT.LoadDataRow(paramDR.ItemArray, false);

                status = gffStatus.DropDownList.SelectedValue;
            }

            var list = new ABCAPOSReportDAC().RetrieveInvoiceMonthly(startDate, endDate, startDueDate, endDueDate, warehouseName, customer, status);
            var dt = ObjectHelper.CreateDynamicDataTable(list);

            ReportViewer1.LocalReport.DataSources.Clear();
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("ParamDT", paramDT as DataTable));
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("EDS", dt as DataTable));
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

            gffDate.TextBoxDateFrom.Text = gffDueDate.TextBoxDateFrom.Text = startDate.ToString("MM/dd/yyyy");
            gffDate.TextBoxDateTo.Text = gffDueDate.TextBoxDateTo.Text = enddate.ToString("MM/dd/yyyy");

            DropDownList ddlWarehouse = gffWarehouse.DropDownList as DropDownList;
            ddlWarehouse.Items.Clear();

            var warehouses = new WarehouseBFC().Retrieve(true);
            foreach (var detail in warehouses)
            {
                ListItem t = new ListItem();
                t = new ListItem(detail.Name, detail.Name);
                ddlWarehouse.Items.Add(t);
            }

            DropDownList ddlStatus = gffStatus.DropDownList as DropDownList;
            ddlStatus.Items.Clear();

            ListItem t2 = new ListItem();
            t2 = new ListItem("Void", MPL.DocumentStatus.Void.ToString());
            ddlStatus.Items.Add(t2);

            t2 = new ListItem();
            t2 = new ListItem("Open", "Open");
            ddlStatus.Items.Add(t2);

            t2 = new ListItem();
            t2 = new ListItem("Lunas", "Lunas");
            ddlStatus.Items.Add(t2);
        }
    }
}