using ABCAPOS.BF;
using ABCAPOS.DA;
using ABCAPOS.ReportEDS;
using ABCAPOS.Util;
using MPL;
using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ABCAPOS.Report.GoodsReturnedByCustomer
{
    public partial class GoodsReturnedByCustomer : System.Web.UI.Page
    {
        private void setFilter()
        {
            gffDate.TextBoxDateFrom.Text = DateTime.Now.AddMonths(-1).ToString("MM/dd/yyyy");
            gffDate.TextBoxDateTo.Text = DateTime.Now.ToString("MM/dd/yyyy");
        }
        private void setDataSource()
        {
            var paramDT = new ABCAPOSReportEDSC.ParamDTDataTable();
            var startDate = SystemConstants.UnsetDateTime;
            var endDate = SystemConstants.UnsetDateTime;
            var productCode = "";
            var productName = "";
            var customerName = "";
            if (gffDate.Selected)
            {
                var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Tanggal";
                paramDR.Value = Convert.ToDateTime(gffDate.TextBoxDateFrom.Text).ToString("MM/dd/yyyy") + " s/d " + Convert.ToDateTime(gffDate.TextBoxDateTo.Text).ToString("MM/dd/yyyy");
                paramDT.LoadDataRow(paramDR.ItemArray, false);

                startDate = Convert.ToDateTime(gffDate.TextBoxDateFrom.Text);
                endDate = Convert.ToDateTime(gffDate.TextBoxDateTo.Text);
            }
            if (gffProductCode.Selected)
            {
                var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Kode Product";
                paramDR.Value = gffProductCode.TextBoxString.Text;

                productCode = gffProductCode.TextBoxString.Text;
            }
            if (gffProductName.Selected)
            {
                var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Nama Produk";
                paramDR.Value = gffProductName.TextBoxString.Text;

                productName = gffProductName.TextBoxString.Text;
            }
            if (gffcustomerName.Selected)
            {
                var paramDR = paramDT.NewParamDTRow();
                paramDR.Label = "Nama Customer";
                paramDR.Value = gffcustomerName.TextBoxString.Text;

                customerName = gffcustomerName.TextBoxString.Text;
            }

            var list = new LogBFC().RetrieveProductReturnedByCustomer(startDate, endDate, productCode, productName, customerName);
            var dt = ObjectHelper.CreateDynamicDataTable(list);

            ReportViewer1.LocalReport.DataSources.Clear();
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("ParamDT", paramDT as DataTable));
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("EDS", dt));
            ReportViewer1.DataBind();

        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                setFilter();
                setDataSource();
            }
        }
        protected void GenericFilter1_Filter()
        {
            setDataSource();
        }
    }
}