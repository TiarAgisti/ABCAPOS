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
    public partial class ReportOverdue : System.Web.UI.Page
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
            var endDate = Convert.ToDateTime(gffEndDate.TextBoxDate.Text);
            var customer = "";

            if (gffCustomerName.Selected)
                customer = gffCustomerName.TextBoxString.Text;

            var warehouseName = "";
            if (gffWarehouse.Selected == true)
                warehouseName = gffWarehouse.DropDownList.Text;

            var salesRef = "";
            if (gffSalesRef.Selected == true)
                salesRef = gffSalesRef.TextBoxString.Text;

            var list = new ABCAPOSReportDAC().RetrieveReportOverdue(endDate, customer, warehouseName, salesRef);
            var dt = ObjectHelper.CreateDynamicDataTable(list);

            List<ReportParameter> param = new List<ReportParameter>();
            param.Add(new ReportParameter("Period", " s/d " + endDate.ToString("dd/MM/yyyy")));
            ReportViewer1.LocalReport.SetParameters(param);

            ReportViewer1.LocalReport.DataSources.Clear();
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("ReportOverdueDT", dt as DataTable));
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
            DateTime enddate = DateTime.Now;

            //gffDate.TextBoxDateFrom.Text = startDate.ToString("MM/dd/yyyy");
            gffEndDate.TextBoxDate.Text = enddate.ToString("MM/dd/yyyy");

            DropDownList ddlWarehouse = gffWarehouse.DropDownList as DropDownList;
            ddlWarehouse.Items.Clear();

            var warehouses = new WarehouseBFC().Retrieve(true);
            foreach (var detail in warehouses)
            {
                ListItem t = new ListItem();
                t = new ListItem(detail.Name, detail.Name);
                ddlWarehouse.Items.Add(t);
            }
        }
    }
}