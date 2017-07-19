using ABCAPOS.BF;
using ABCAPOS.ReportEDS;
using ABCAPOS.Util;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ABCAPOS.PrintOut.WorkOrder
{
    public partial class WorkOrderPrintOutStruk : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString[SystemConstants.str_WorkOrderID] != null)
                {
                    long workOrderID = Convert.ToInt64(Request.QueryString[SystemConstants.str_WorkOrderID]);

                    var workOrderDT = new ABCAPOSReportEDSC.WorkOrderDTDataTable();
                    var workOrderDR = new WorkOrderBFC().RetrievePrintOutStruk(workOrderID);
                    workOrderDT.LoadDataRow(workOrderDR.ItemArray, false);

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("EDS", workOrderDT as DataTable));
                  
                    this.ReportViewer1.LocalReport.Refresh();
                }
            }
        }
    }
}