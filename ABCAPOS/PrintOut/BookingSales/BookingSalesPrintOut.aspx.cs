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

namespace ABCAPOS.PrintOut.BookingSales
{
    public partial class BookingSalesPrintOut : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString[SystemConstants.str_BookingSalesID] != null)
                {
                    long bookingSalesID = Convert.ToInt64(Request.QueryString[SystemConstants.str_BookingSalesID]);

                    var bookingSalesDT = new ABCAPOSReportEDSC.SalesOrderDTDataTable();
                    var bookingSalesDR = new BookingSalesBFC().RetrievePrintOut(bookingSalesID);

                    var warehouse = new WarehouseBFC().RetrieveByID(bookingSalesDR.WarehouseID);

                    var customer = new CustomerBFC().RetrieveByID(bookingSalesDR.CustomerID);
                    bookingSalesDR.CustomerAddress = customer.Address;
                    bookingSalesDR.CustomerCity = customer.City;
                    bookingSalesDR.CustomerContactPerson = customer.ContactPerson;
                    bookingSalesDR.CustomerPhone = customer.Phone;

                    var bookingSalesDetailDT = new BookingSalesBFC().RetrieveDetailPrintOut(bookingSalesID);
                    decimal total = 0;
                    foreach (var bookingSalesDetailDR in bookingSalesDetailDT)
                    {
                        total += bookingSalesDetailDR.Price * Convert.ToDecimal(bookingSalesDetailDR.Quantity);
                    }

                    bookingSalesDT.LoadDataRow(bookingSalesDR.ItemArray, true);

                    ReportViewer1.LocalReport.EnableExternalImages = true;

                    string path = @"file:\" + this.MapPath("~/Uploads/Logo/header.png");
                    string sayChunk = new IDNumericSayer().Say(total, "rupiah");
                    List<ReportParameter> param = new List<ReportParameter>();
                    param.Add(new ReportParameter("ImagePath", path));
                    param.Add(new ReportParameter("CurrentLocationAndDate", "Jakarta, " + new IDNumericSayer().DateFormatingInd(bookingSalesDR.Date)));
                    param.Add(new ReportParameter("SayChunk", sayChunk));
                    ReportViewer1.LocalReport.SetParameters(param);

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BookingSalesDT", bookingSalesDT as DataTable));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BookingSalesDetailDT", bookingSalesDetailDT as DataTable));
                    ReportViewer1.LocalReport.DisplayName = bookingSalesDR.Code;
                    this.ReportViewer1.LocalReport.Refresh();
                }
            }
        }
    }
}