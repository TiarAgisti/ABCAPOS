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
using ABCAPOS.EDM;
using ABCAPOS.ReportEDS;
using System.Data;

namespace ABCAPOS.Report.DeliveryOrder
{
    public partial class DeliveryOrderReport : System.Web.UI.Page
    {
        private void SetFilter()
        {
            string month = DateTime.Now.Month.ToString();
            DateTime startDate = Convert.ToDateTime(month + "/1/" + DateTime.Now.Year);
            DateTime enddate = (startDate.AddMonths(1)).AddDays(-1);

            gffDate.TextBoxDateFrom.Text = startDate.ToString("MM/dd/yyyy");
            gffDate.TextBoxDateTo.Text = enddate.ToString("MM/dd/yyyy");

            DropDownList ddlStatus = gffStatus.DropDownList as DropDownList;
            ddlStatus.Items.Clear();

            ListItem t = new ListItem();
            t = new ListItem("Void", Convert.ToString((int)MPL.DocumentStatus.Void));
            ddlStatus.Items.Add(t);

            t = new ListItem();
            t = new ListItem("Picked", Convert.ToString((int)DeliveryOrderStatus.New));
            ddlStatus.Items.Add(t);

            t = new ListItem();
            t = new ListItem("Packed", Convert.ToString((int)DeliveryOrderStatus.Packed));
            ddlStatus.Items.Add(t);

            t = new ListItem();
            t = new ListItem("Shipped", Convert.ToString((int)DeliveryOrderStatus.Shipped));
            ddlStatus.Items.Add(t);

            DropDownList ddlWarehouse = gffWarehouse.DropDownList as DropDownList;
            ddlWarehouse.Items.Clear();

            var warehouses = new WarehouseBFC().Retrieve(true);
            foreach (var detail in warehouses)
            {
                ListItem t2 = new ListItem();
                t2 = new ListItem(detail.Name, Convert.ToString(detail.ID));
                ddlWarehouse.Items.Add(t2);
            }

        }

        private void SetDataSource()
        {
            var list = new ABCAPOSReportDAC().RetrieveDeliveryOrderReport(GenericFilter1.SelectFilters);
            var dt = ObjectHelper.CreateDynamicDataTable(list);

            var paramDT = new ABCAPOSReportEDSC.ParamDTDataTable();

            if (gffDate.Selected)
            {
                var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Tanggal SJ";
                paramDR.Value = Convert.ToDateTime(gffDate.TextBoxDateFrom.Text).ToString("dd/MM/yyyy") + " s/d " + Convert.ToDateTime(gffDate.TextBoxDateTo.Text).ToString("dd/MM/yyyy");
                paramDT.LoadDataRow(paramDR.ItemArray, false);
            }

            if (gffCode.Selected)
            {
                var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "SJ NO";
                paramDR.Value = gffCode.TextBoxString.Text;
                paramDT.LoadDataRow(paramDR.ItemArray, false);
            }

            if (gffCustomerName.Selected)
            {
                var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "CUSTOMER";
                paramDR.Value = gffCustomerName.TextBoxString.Text;
                paramDT.LoadDataRow(paramDR.ItemArray, false);
            }

            if (gffStatus.Selected)
            {
                var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Status";
                paramDR.Value = gffStatus.DropDownList.SelectedItem.Text;
                paramDT.LoadDataRow(paramDR.ItemArray, false);
            }

            ReportViewer1.LocalReport.DataSources.Clear();
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("ParamDT", paramDT as DataTable));
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("EDS", dt));
            ReportViewer1.DataBind();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SetFilter();
                SetDataSource();
            }
        }

       

        protected void GenericFilter1_Filter()
        {
            SetDataSource();
        }

    }  
}