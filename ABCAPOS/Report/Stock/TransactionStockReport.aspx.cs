using ABCAPOS.BF;
using ABCAPOS.DA;
using ABCAPOS.ReportEDS;
using ABCAPOS.Util;
using Microsoft.Reporting.WebForms;
using MPL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ABCAPOS.Report.Stock
{
    public partial class TransactionStockReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SetFilter();
                SetDataSource();
            }
        }

        private void SetFilter()
        {
            gffDate.TextBoxDateFrom.Text = DateTime.Now.AddMonths(-1).ToString("MM/dd/yyyy");
            gffDate.TextBoxDateTo.Text = DateTime.Now.ToString("MM/dd/yyyy");

            DropDownList ddlWarehouse = gffWarehouse.DropDownList as DropDownList;
            ddlWarehouse.Items.Clear();

            var warehouses = new WarehouseBFC().Retrieve(true);
            foreach (var detail in warehouses)
            {
                ListItem t = new ListItem();
                t = new ListItem(detail.Name, Convert.ToString(detail.ID));
                ddlWarehouse.Items.Add(t);
            }
        }

        private void SetDataSource()
        {
            var paramDT = new ABCAPOSReportEDSC.ParamDTDataTable();

            var startDate = SystemConstants.UnsetDateTime;
            var endDate = SystemConstants.UnsetDateTime;
            var productCode = "";
            var productName = "";
            long warehouseID = 0;

            if (gffDate.Selected)
            {
                var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Tanggal";
                paramDR.Value = Convert.ToDateTime(gffDate.TextBoxDateFrom.Text).ToString("dd/MM/yyyy") + " s/d " + Convert.ToDateTime(gffDate.TextBoxDateTo.Text).ToString("dd/MM/yyyy");
                paramDT.LoadDataRow(paramDR.ItemArray, false);

                startDate = Convert.ToDateTime(gffDate.TextBoxDateFrom.Text);
                endDate = Convert.ToDateTime(gffDate.TextBoxDateTo.Text);
            }

            if (gffCode.Selected)
            {
                var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Kode Produk";
                paramDR.Value = gffCode.TextBoxString.Text;
                paramDT.LoadDataRow(paramDR.ItemArray, false);

                productCode = gffCode.TextBoxString.Text;
            }

            if (gffProductName.Selected)
            {
                var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Nama Produk";
                paramDR.Value = gffProductName.TextBoxString.Text;
                paramDT.LoadDataRow(paramDR.ItemArray, false);

                productName = gffProductName.TextBoxString.Text;
            }

            if (gffWarehouse.Selected)
            {
                var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Location";
                paramDR.Value = gffWarehouse.DropDownList.SelectedItem.Text;
                paramDT.LoadDataRow(paramDR.ItemArray, false);

                warehouseID = Convert.ToInt64(gffWarehouse.DropDownList.SelectedItem.Value);
            }

            var list = new ABCAPOSReportDAC().RetrieveStockReport(startDate, endDate, productCode, productName, warehouseID);
            var dt = ObjectHelper.CreateDynamicDataTable(list);

            ReportViewer1.LocalReport.DataSources.Clear();
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("ParamDT", paramDT as DataTable));
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("EDS", dt));
            ReportViewer1.DataBind();
        }

        protected void GenericFilter1_Filter()
        {
            SetDataSource();
        }

      
    }
}