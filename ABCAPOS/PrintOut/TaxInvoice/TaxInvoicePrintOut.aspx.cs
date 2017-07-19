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

namespace ABCAPOS.PrintOut.TaxInvoice
{
    public partial class TaxInvoicePrintOut : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString[SystemConstants.str_InvoiceID] != null)
                {
                    long invoiceID = Convert.ToInt64(Request.QueryString[SystemConstants.str_InvoiceID]);
                    
                    InvoiceBFC bf = new InvoiceBFC();
                    ABCAPOSReportEDSC.TaxInvoiceDTDataTable taxInvoiceDT = new ABCAPOSReportEDSC.TaxInvoiceDTDataTable();
                    ABCAPOSReportEDSC.TaxInvoiceDTRow taxInvoiceDR = bf.RetrieveTaxInvoicePrintOut(invoiceID);
                    
                    var customer = new CustomerBFC().RetrieveByCodeOrName(taxInvoiceDR.CustomerCode);
                    if (!string.IsNullOrWhiteSpace(customer.TaxFileNumber))
                    {
                        taxInvoiceDR.CustomerAddress1 = customer.Address;
                        taxInvoiceDR.CustomerAddress2 = customer.City;
                        taxInvoiceDR.CustomerTaxFileNumber = customer.TaxFileNumber;
                    }
                    else
                    {
                        taxInvoiceDR.CustomerAddress1 = customer.CompanyName;
                        if (!string.IsNullOrWhiteSpace(customer.City))
                            taxInvoiceDR.CustomerAddress1 += ", " + customer.City;
                        taxInvoiceDR.CustomerTaxFileNumber = "00.000.000.0-000.000";
                    }
                    var invoiceDR = new InvoiceBFC().RetrievePrintOut(invoiceID);
                    invoiceDR.Amount = 0;
                    invoiceDR.TaxAmount = 0;

                    var invoiceDetailDT = new InvoiceBFC().RetrieveDetailPrintOut(invoiceID);
                    foreach (var invoiceDet in invoiceDetailDT)
                    {
                        invoiceDR.Amount += invoiceDet.Price * Convert.ToDecimal(invoiceDet.Quantity);
                        if (invoiceDet.TaxType == 2)
                            invoiceDR.TaxAmount += invoiceDet.Price * Convert.ToDecimal(invoiceDet.Quantity) * Convert.ToDecimal(0.1);
                    }
                    taxInvoiceDR.Amount = invoiceDR.Amount;
                    taxInvoiceDR.TaxAmount = invoiceDR.TaxAmount;
                    var salesOrder = new SalesOrderBFC().RetrieveByID(taxInvoiceDR.SalesOrderID);

                    if (salesOrder.Currency == (int)Currency.IDR)
                    {
                        invoiceDR.ExchangeRate = 1;
                    }
                    else
                    {
                        invoiceDR.ExchangeRate = salesOrder.ExchangeRate;
                    }

                    //var salesOrderDetailDT = new SalesOrderBFC().RetrieveDetailPrintOut(taxInvoiceDR.SalesOrderID);
                    
                    taxInvoiceDT.LoadDataRow(taxInvoiceDR.ItemArray, false);

                    var companySettingDT = new ABCAPOSReportEDSC.CompanySettingDTDataTable();
                    var companySettingDR = new CompanySettingBFC().RetrieveForPrintOut();
                    companySettingDT.LoadDataRow(companySettingDR.ItemArray, false);
                    
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("TaxInvoiceDT", taxInvoiceDT as DataTable));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CompanySettingDT", companySettingDT as DataTable));
                    ReportViewer1.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(LocalReport_SubreportProcessing);
                    this.ReportViewer1.LocalReport.Refresh();
                }
                else if (Request.QueryString[SystemConstants.str_MultipleInvoicingID] != null)
                {
                    long multipleInvoiceID = Convert.ToInt64(Request.QueryString[SystemConstants.str_MultipleInvoicingID]);

                    MultipleInvoicingBFC bf = new MultipleInvoicingBFC();
                    ABCAPOSReportEDSC.TaxInvoiceDTDataTable taxInvoiceDT = new ABCAPOSReportEDSC.TaxInvoiceDTDataTable();
                    ABCAPOSReportEDSC.TaxInvoiceDTRow taxInvoiceDR = bf.RetrieveMultipleInvoiceTaxInvoicePrintOut(multipleInvoiceID);

                    var customer = new CustomerBFC().RetrieveByCode(taxInvoiceDR.CustomerCode);
                    if (!string.IsNullOrWhiteSpace(customer.TaxFileNumber))
                    {
                        taxInvoiceDR.CustomerAddress1 = customer.Address;
                        taxInvoiceDR.CustomerAddress2 = customer.City;
                        taxInvoiceDR.CustomerTaxFileNumber = customer.TaxFileNumber;
                    }
                    else
                    {
                        taxInvoiceDR.CustomerAddress1 = customer.CompanyName;
                        if (!string.IsNullOrWhiteSpace(customer.City))
                            taxInvoiceDR.CustomerAddress1 += ", " + customer.City;
                        taxInvoiceDR.CustomerTaxFileNumber = "00.000-000.0-000.000";
                    }
                    var multipleInvoiceDR = new MultipleInvoicingBFC().RetrieveMultipleInvoicePrintOut(multipleInvoiceID);
                    multipleInvoiceDR.Amount = 0;
                    multipleInvoiceDR.TaxAmount = 0;

                    var multipleInvoiceDetailDT = new MultipleInvoicingBFC().RetrieveDetailPrintOut(multipleInvoiceID);
                    foreach (var multipleInvoiceDet in multipleInvoiceDetailDT)
                    {
                        multipleInvoiceDR.Amount += multipleInvoiceDet.Price * Convert.ToDecimal(multipleInvoiceDet.Quantity);
                        if (multipleInvoiceDet.TaxType == 2)
                            multipleInvoiceDR.TaxAmount += multipleInvoiceDet.Price * Convert.ToDecimal(multipleInvoiceDet.Quantity) * Convert.ToDecimal(0.1);
                    }
                    taxInvoiceDR.Amount = multipleInvoiceDR.Amount;
                    taxInvoiceDR.TaxAmount = multipleInvoiceDR.TaxAmount;
                    //var salesOrder = new SalesOrderBFC().RetrieveByID(taxInvoiceDR.SalesOrderID);

                    //if (salesOrder.Currency == (int)Currency.IDR)
                    //{
                    //    multipleInvoiceDR.ExchangeRate = 1;
                    //}
                    //else
                    //{
                    //    multipleInvoiceDR.ExchangeRate = salesOrder.ExchangeRate;
                    //}

                    //var salesOrderDetailDT = new SalesOrderBFC().RetrieveDetailPrintOut(taxInvoiceDR.SalesOrderID);

                    taxInvoiceDT.LoadDataRow(taxInvoiceDR.ItemArray, false);

                    var companySettingDT = new ABCAPOSReportEDSC.CompanySettingDTDataTable();
                    var companySettingDR = new CompanySettingBFC().RetrieveForPrintOut();
                    companySettingDT.LoadDataRow(companySettingDR.ItemArray, false);

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("TaxInvoiceDT", taxInvoiceDT as DataTable));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CompanySettingDT", companySettingDT as DataTable));
                    ReportViewer1.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(LocalReport_SubreportProcessing);
                    this.ReportViewer1.LocalReport.Refresh();
                }
            }
        }

        void LocalReport_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            if (Request.QueryString[SystemConstants.str_InvoiceID] != null)
            {
                long invoiceID = Convert.ToInt64(Request.QueryString[SystemConstants.str_InvoiceID]);

                InvoiceBFC bf = new InvoiceBFC();
                var invoice = bf.RetrieveByID(invoiceID);

                var invoiceDetailDT = new InvoiceBFC().RetrieveDetailPrintOut(invoiceID);
                
                // TODO: TaxInvoicePrintOut fix (don't retrieve data from Delivery Order)
                //var deliveryOrder = new DeliveryOrderBFC().RetrieveBySalesOrderID(invoice.SalesOrderID);
                //var deliveryOrderDetailDT = new DeliveryOrderBFC().RetrieveDetailPrintOut(deliveryOrder[0].ID);

                e.DataSources.Add(new ReportDataSource("DeliveryOrderDetailDT", invoiceDetailDT as DataTable));
            }
            else if (Request.QueryString[SystemConstants.str_MultipleInvoicingID] != null)
            {
                long multiInvoiceID = Convert.ToInt64(Request.QueryString[SystemConstants.str_MultipleInvoicingID]);

                MultipleInvoicingBFC bf = new MultipleInvoicingBFC();
                var multipleInvoice = bf.RetrieveByID(multiInvoiceID);

                var multiInvoiceDetailDT = new MultipleInvoicingBFC().RetrieveDetailPrintOut(multiInvoiceID);

                //var deliveryOrder = new DeliveryOrderBFC().RetrieveBySalesOrderID(invoice.SalesOrderID);
                //var deliveryOrderDetailDT = new DeliveryOrderBFC().RetrieveDetailPrintOut(deliveryOrder[0].ID);

                e.DataSources.Add(new ReportDataSource("DeliveryOrderDetailDT", multiInvoiceDetailDT as DataTable));
            }
        }
    }
}