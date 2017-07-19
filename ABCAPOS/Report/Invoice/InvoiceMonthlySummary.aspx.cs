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
using System.Data;

namespace ABCAPOS.Report.Invoice
{
    public partial class InvoiceMonthlySummary : System.Web.UI.Page
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

            var warehouseID = 0;
            if (gffWarehouse.Selected == true)
                warehouseID = Convert.ToInt32(gffWarehouse.DropDownList.SelectedValue);

            var customer = "";
            if (gffCustomerName.Selected)
                customer = gffCustomerName.TextBoxString.Text;

            var status = "";
            if (gffStatus.Selected)
            {
                status = gffStatus.DropDownList.SelectedValue;
            }

            //var status = 0;
            //if (gffStatus.Selected)
            //{
            //    status = Convert.ToInt32(gffStatus.DropDownList.SelectedValue);
            //}
            
            var list = new ABCAPOSReportDAC().RetrieveInvoiceMonthlySummary(startDate, endDate, warehouseID, customer, status );
            var dt = ObjectHelper.CreateDynamicDataTable(list);
            
            List<ReportParameter> param = new List<ReportParameter>();
            param.Add(new ReportParameter("Period", startDate.ToString("dd/MM/yyyy") + " s/d " + endDate.ToString("dd/MM/yyyy")));
            ReportViewer1.LocalReport.SetParameters(param);

            ReportViewer1.LocalReport.DataSources.Clear();
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

            gffDate.TextBoxDateFrom.Text = startDate.ToString("MM/dd/yyyy");
            gffDate.TextBoxDateTo.Text = enddate.ToString("MM/dd/yyyy");

            DropDownList ddlWarehouse = gffWarehouse.DropDownList as DropDownList;
            ddlWarehouse.Items.Clear();

            var warehouses = new WarehouseBFC().Retrieve(true);
            foreach (var detail in warehouses)
            {
                ListItem t = new ListItem();
                t = new ListItem(detail.Name, Convert.ToString(detail.ID));
                ddlWarehouse.Items.Add(t);
            }
            //var warehouses = new WarehouseBFC().Retrieve(true);
            //foreach (var detail in warehouses)
            //{
            //    ListItem t = new ListItem();
            //    t = new ListItem(detail.Name, detail.Name);
            //    ddlWarehouse.Items.Add(t);
            //}

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

            //DropDownList ddlStatus = gffStatus.DropDownList as DropDownList;
            //ddlStatus.Items.Clear();

            //ListItem t2 = new ListItem();
            //t2 = new ListItem("Void", Convert.ToString((int)InvoiceStatus.Void));
            //ddlStatus.Items.Add(t2);

            //t2 = new ListItem();
            //t2 = new ListItem("Open", Convert.ToString((int)InvoiceStatus.New));
            //ddlStatus.Items.Add(t2);

            //t2 = new ListItem();
            //t2 = new ListItem("Lunas", Convert.ToString((int)InvoiceStatus.Paid));
            //ddlStatus.Items.Add(t2);
        }
    }
}