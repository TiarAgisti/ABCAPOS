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
using ABCAPOS.ReportEDS;

namespace ABCAPOS.Report.Invoice
{
    public partial class ReportBonus : System.Web.UI.Page
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
            var startDate = Convert.ToDateTime(gffDate.TextBoxDateFrom.Text);
            var endDate = Convert.ToDateTime(gffDate.TextBoxDateTo.Text);
            //var startDateInv = Convert.ToDateTime(gffDateInv.TextBoxDateFrom.Text);
            //var endDateInv = Convert.ToDateTime(gffDateInv.TextBoxDateTo.Text);

            var invoiceCode = "";
            if (gffInvoiceCode.Selected)
            {
                var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Transaction Number";
                paramDR.Value = gffInvoiceCode.TextBoxString.Text;
                paramDT.LoadDataRow(paramDR.ItemArray, false);

                invoiceCode = gffInvoiceCode.TextBoxString.Text;
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
              

            var warehouseName = "";
            if (gffWarehouse.Selected == true)
            {
                var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Location";
                paramDR.Value = gffWarehouse.DropDownList.SelectedItem.Text;
                paramDT.LoadDataRow(paramDR.ItemArray, false);

                warehouseName = gffWarehouse.DropDownList.Text;
            }

            var salesReference = "";
            if (gffSalesReference.Selected == true)
            {
                var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Sales Reference";
                paramDR.Value = gffSalesReference.TextBoxString.Text;
                paramDT.LoadDataRow(paramDR.ItemArray, false);

                salesReference = gffSalesReference.TextBoxString.Text;
            }

            var list = new ABCAPOSReportDAC().RetrieveBonusByInvoice(startDate, endDate, invoiceCode, customer,warehouseName,salesReference);
            var dt = ObjectHelper.CreateDynamicDataTable(list);

            List<ReportParameter> param = new List<ReportParameter>();
            param.Add(new ReportParameter("Period", startDate.ToString("dd/MM/yyyy") + " s/d " + endDate.ToString("dd/MM/yyyy")));
            ReportViewer1.LocalReport.SetParameters(param);

            ReportViewer1.LocalReport.DataSources.Clear();
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("ParamDT", paramDT as DataTable));
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("EDS", dt as DataTable));
            ReportViewer1.DataBind();
            //this.ReportViewer1.LocalReport.Refresh();
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

            //DateTime startDateInv = Convert.ToDateTime(month + "/1/" + DateTime.Now.Year);
            //DateTime enddateInv = (startDate.AddMonths(1)).AddDays(-1);

            gffDate.TextBoxDateFrom.Text = startDate.ToString("MM/dd/yyyy");
            gffDate.TextBoxDateTo.Text = enddate.ToString("MM/dd/yyyy");

            //gffDateInv.TextBoxDateFrom.Text = startDateInv.ToString("MM/dd/yyyy");
            //gffDateInv.TextBoxDateTo.Text = enddateInv.ToString("MM/dd/yyyy");


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