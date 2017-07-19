using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MPL.Reporting;
using ABCAPOS.BF;
using ABCAPOS.DA;
using MPL;
using Microsoft.Reporting.WebForms;
using ABCAPOS.Util;
using ABCAPOS.EDM;
using ABCAPOS.ReportEDS;
using System.Data;

namespace ABCAPOS.Report.Product
{
    public partial class ProductReport : System.Web.UI.Page
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
            var list = new ABCAPOSReportDAC().RetrieveProductReport(GenericFilter1.SelectFilters);
            var dt = ObjectHelper.CreateDynamicDataTable(list);

            ReportViewer1.LocalReport.DataSources.Clear();
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("ProductDT", dt));
            ReportViewer1.DataBind();
        }

        protected void GenericFilter1_Filter()
        {
            SetDataSource();
        }

        private void SetFilter()
        {
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
    }
}