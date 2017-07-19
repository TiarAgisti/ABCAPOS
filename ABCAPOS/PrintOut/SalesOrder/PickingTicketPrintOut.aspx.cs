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

namespace ABCAPOS.PrintOut.SalesOrder
{
    public partial class PickingTicketPrintOut : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString[SystemConstants.str_SalesOrderID] != null)
                {
                    long salesOrderID = Convert.ToInt64(Request.QueryString[SystemConstants.str_SalesOrderID]);

                    var salesOrderDT = new ABCAPOSReportEDSC.SalesOrderDTDataTable();
                    var salesOrderDR = new SalesOrderBFC().RetrievePrintOut(salesOrderID);

                    var customer = new CustomerBFC().RetrieveByID(salesOrderDR.CustomerID);
                    salesOrderDR.CustomerAddress = customer.Address;
                    salesOrderDR.CustomerCity = customer.City;
                    salesOrderDR.CustomerContactPerson = customer.ContactPerson;
                    salesOrderDR.CustomerPhone = customer.Phone;

                    var salesOrderDetailDT = new SalesOrderBFC().RetrieveDetailPrintOut(salesOrderID);
                    //var dt = ObjectHelper.CreateDynamicDataTable(salesOrderDetailDT);
                    //salesOrderDR.ItemAmount = salesOrderDetailDT.Sum(p => p.Quantity);
                    
                    salesOrderDT.LoadDataRow(salesOrderDR.ItemArray, true);

                    ReportViewer1.LocalReport.EnableExternalImages = true;

                    //string path = @"file:\" + this.MapPath("~/Uploads/Logo/logo.png");

                    //List<ReportParameter> param = new List<ReportParameter>();
                    //param.Add(new ReportParameter("ImagePath", path));
                    //ReportViewer1.LocalReport.SetParameters(param);

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("SalesOrderDT", salesOrderDT as DataTable));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("SalesOrderDetailDT", salesOrderDetailDT as DataTable));
                    ReportViewer1.LocalReport.DisplayName = salesOrderDR.Code;
                    this.ReportViewer1.LocalReport.Refresh();
                }
            }
        }

    }
}