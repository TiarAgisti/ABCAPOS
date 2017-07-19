using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ABCAPOS.Util;
using ABCAPOS.BF;
using ABCAPOS.ReportEDS;
using Microsoft.Reporting.WebForms;
using System.Data;

namespace ABCAPOS.PrintOut.MultipleInvoicing
{
    public partial class MultipleInvoicingPrintOut : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString[SystemConstants.str_MultipleInvoicingID] != null)
                {
                    long multipleInvoicingID = Convert.ToInt64(Request.QueryString[SystemConstants.str_MultipleInvoicingID]);
                    
                    MultipleInvoicingBFC bf = new MultipleInvoicingBFC();
                    ABCAPOSReportEDSC.TaxInvoiceDTDataTable multipleInvoicingDT = new ABCAPOSReportEDSC.TaxInvoiceDTDataTable();
                    ABCAPOSReportEDSC.TaxInvoiceDTRow taxInvoiceDR = bf.RetrieveMultipleInvoiceTaxInvoicePrintOut(multipleInvoicingID);
                    
                    var customer = new CustomerBFC().RetrieveByCode(taxInvoiceDR.CustomerCode);
                    taxInvoiceDR.CustomerAddress1 = customer.Address;
                    taxInvoiceDR.CustomerAddress2 = customer.City;
                    taxInvoiceDR.CustomerTaxFileNumber = customer.TaxFileNumber;

                    //var invoiceDR = new InvoiceBFC().RetrievePrintOut(multipleInvoicingID);
                    //var multipleInvoicingDR = new MultipleInvoicingBFC().RetrieveMultipleInvoicingPrintOut(multipleInvoicingID);
                    //var multipleInvoicingDetailDR = new MultipleInvoicingBFC().RetrieveDetails(multipleInvoicingID);
                    
                    //foreach (var detail in multipleInvoicingDetailDR)
                    //{
                    //    //detail.InvoiceID
                    //    var invoiceDetails = new InvoiceBFC().RetrieveDetails(detail.InvoiceID);


                    //}

                    //var invoice = new InvoiceBFC().RetrieveByID(taxInvoiceDR.);

                    //if (salesOrder.Currency == (int)Currency.IDR)
                    //{
                    //    invoiceDR.ExchangeRate = 1;
                    //}
                    //else
                    //{
                    //    invoiceDR.ExchangeRate = salesOrder.ExchangeRate;
                    //}

                    //var salesOrderDetailDT = new SalesOrderBFC().RetrieveDetailPrintOut(taxInvoiceDR.SalesOrderID);
                    //var deliveryOrderDetailDT = new DeliveryOrderBFC().RetrieveDetailPrintOut(taxInvoiceDR.DeliveryOrderID);

                    //taxInvoiceDR.SOTotal = 0;

                    //foreach (var deliveryOrderDetailDR in deliveryOrderDetailDT)
                    //{
                    //    var Price = salesOrderDetailDT.Where(p => p.ItemNo == deliveryOrderDetailDR.SalesOrderItemNo).FirstOrDefault().Price;
                    //    deliveryOrderDetailDR.Price = Price / invoiceDR.ExchangeRate;
                    //    taxInvoiceDR.SOTotal += deliveryOrderDetailDR.Price * Convert.ToDecimal(deliveryOrderDetailDR.Quantity);
                    //}
                    multipleInvoicingDT.LoadDataRow(taxInvoiceDR.ItemArray, false);

                    var companySettingDT = new ABCAPOSReportEDSC.CompanySettingDTDataTable();
                    var companySettingDR = new CompanySettingBFC().RetrieveForPrintOut();
                    companySettingDT.LoadDataRow(companySettingDR.ItemArray, false);

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("TaxInvoiceDT", multipleInvoicingDT as DataTable));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CompanySettingDT", companySettingDT as DataTable));
                    ReportViewer1.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(LocalReport_SubreportProcessing);
                    this.ReportViewer1.LocalReport.Refresh();
                }
            }
        }

        void LocalReport_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            if (Request.QueryString[SystemConstants.str_MultipleInvoicingID] != null)
            {
                long multipleInvoicingID = Convert.ToInt64(Request.QueryString[SystemConstants.str_MultipleInvoicingID]);

                MultipleInvoicingBFC bf = new MultipleInvoicingBFC();
                var multipleInvoicingDT = bf.RetrieveByID(multipleInvoicingID);
                var multipleInvoicingDetailDT = bf.RetrieveDetailPrintOut(multipleInvoicingID);
                

                //foreach (var multipleInvoicingDetail in multipleInvoicingDetailDT)
                //{
                //    var invoiceDetailDT = new InvoiceBFC().RetrieveDetails(
                //    //var deliveryOrderDetailDT = new DeliveryOrderBFC().RetrieveDetailPrintOut(invoice.DeliveryOrderID);
                //    //var salesOrderDetailDT = new SalesOrderBFC().RetrieveDetailPrintOut(invoice.SalesOrderID);

                //    foreach (var invoiceDetailDR in invoiceDetailDT)
                //    {
                //        //var Price = invoiceDetailDR.Where(p => p.ItemNo == deliveryOrderDetailDR.SalesOrderItemNo).FirstOrDefault().Price;
                //        //invoiceDetailDR.Price = Price;
                //    }
                //}
                e.DataSources.Add(new ReportDataSource("DeliveryOrderDetailDT", multipleInvoicingDetailDT as DataTable));
            }
        }
    }
}