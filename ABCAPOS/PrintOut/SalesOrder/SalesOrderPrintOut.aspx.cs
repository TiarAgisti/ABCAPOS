using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ABCAPOS.Util;
using Microsoft.Reporting.WebForms;
using ABCAPOS.DA;
using ABCAPOS.BF;
using System.Data;
using ABCAPOS.ReportEDS;

namespace ABCAPOS.PrintOut.SalesOrder
{
    public partial class SalesOrderPrintOut : System.Web.UI.Page
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

                    if (salesOrderDR.Currency == (int)Currency.IDR)
                    {
                        salesOrderDR.CurrencyDescription = "Rupiah (IDR)";
                        salesOrderDR.ExchangeRate = 1;
                    }
                    else
                    {
                        salesOrderDR.CurrencyDescription = "Dollar (USD)";
                        salesOrderDR.ExchangeRate = salesOrderDR.ExchangeRate;
                    }

                    var salesOrderDetailDT = new SalesOrderBFC().RetrieveDetailPrintOut(salesOrderID);
                    //salesOrderDR.ItemAmount = salesOrderDetailDT.Sum(p => p.Quantity);

                    salesOrderDT.LoadDataRow(salesOrderDR.ItemArray, true);

                    var companySettingDT = new ABCAPOSReportEDSC.CompanySettingDTDataTable();
                    var companySettingDR = new CompanySettingBFC().RetrieveForPrintOut();

                    //if (salesOrderDR.TaxType == (int)TaxType.NonTax)
                    //    companySettingDR.Name = companySettingDR.OwnerName;

                    companySettingDT.LoadDataRow(companySettingDR.ItemArray, false);

                    //var companySetting = new CompanySettingBFC().Retrieve();

                    ReportViewer1.LocalReport.EnableExternalImages = true;

                    string path = @"file:\" + this.MapPath("~/Uploads/Logo/logo.png");

                    List<ReportParameter> param = new List<ReportParameter>();
                    param.Add(new ReportParameter("ImagePath", path));
                    ReportViewer1.LocalReport.SetParameters(param);

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("SalesOrderDT", salesOrderDT as DataTable));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CompanySettingDT", companySettingDT as DataTable));
                    ReportViewer1.LocalReport.DisplayName = salesOrderDR.Code;
                    ReportViewer1.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(LocalReport_SubreportProcessing);
                    this.ReportViewer1.LocalReport.Refresh();
                }
            }
        }

        void LocalReport_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            if (Request.QueryString[SystemConstants.str_SalesOrderID] != null)
            {
                long salesOrderID = Convert.ToInt64(Request.QueryString[SystemConstants.str_SalesOrderID]);

                var salesOrder = new SalesOrderBFC().RetrieveByID(salesOrderID);
                var salesOrderDetailDT = new SalesOrderBFC().RetrieveDetailPrintOut(salesOrderID);

                foreach (var salesOrderDetailDR in salesOrderDetailDT)
                    salesOrderDetailDR.Price = Convert.ToDecimal(((salesOrderDetailDR.Price - salesOrderDetailDR.Discount) / salesOrder.ExchangeRate).ToString("N0"));

                e.DataSources.Add(new ReportDataSource("SalesOrderDetailDT", salesOrderDetailDT as DataTable));
            }
        }
    }
}