using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ABCAPOS.DA;
using ABCAPOS.BF;
using ABCAPOS.Util;
using Microsoft.Reporting.WebForms;
using MPL;
using System.Data;
using ABCAPOS.ReportEDS;

namespace ABCAPOS.PrintOut.TransferOrder
{
    public partial class PickingTicketPrintOut : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString[SystemConstants.str_TransferOrderID] != null)
                {
                    long transferOrderID = Convert.ToInt64(Request.QueryString[SystemConstants.str_TransferOrderID]);

                    var transferOrderDT = new ABCAPOSReportEDSC.SalesOrderDTDataTable();
                    var transferOrderDR = new TransferOrderBFC().RetrievePrintOut(transferOrderID);

                    var warehouse = new WarehouseBFC().RetrieveByID(transferOrderDR.ToWarehouseID);
                    transferOrderDR.CustomerAddress = warehouse.Address;
                    transferOrderDR.CustomerCity = warehouse.City;
                    transferOrderDR.CustomerPhone = warehouse.Phone;

                    var transferOrderDetailDT = new TransferOrderBFC().RetrieveDetailPrintOut(transferOrderID);
                    transferOrderDT.LoadDataRow(transferOrderDR.ItemArray, true);

                    ReportViewer1.LocalReport.EnableExternalImages = true;

                    //string path = @"file:\" + this.MapPath("~/Uploads/Logo/logo.png");

                    //List<ReportParameter> param = new List<ReportParameter>();
                    //param.Add(new ReportParameter("ImagePath", path));
                    //ReportViewer1.LocalReport.SetParameters(param);

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("SalesOrderDT", transferOrderDT as DataTable));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("SalesOrderDetailDT", transferOrderDetailDT as DataTable));
                    ReportViewer1.LocalReport.DisplayName = transferOrderDR.Code;
                    this.ReportViewer1.LocalReport.Refresh();
                }
            }
        }

    }
}