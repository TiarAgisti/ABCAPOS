using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ABCAPOS.BF;
using ABCAPOS.Helpers;
using ABCAPOS.Util;
using ABCAPOS.ReportEDS;
using Microsoft.Reporting.WebForms;
using System.Data;

namespace ABCAPOS.PrintOut.Invoice
{
    public partial class InvoicePrintOut : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString[SystemConstants.str_InvoiceID] != null)
                {
                    #region Invoice

                    long invoiceID = Convert.ToInt64(Request.QueryString[SystemConstants.str_InvoiceID]);

                    var invoice = new InvoiceBFC().RetrieveByID(invoiceID);
                    invoice.Flaging += 1;
                    invoice.ModifiedBy = MembershipHelper.GetUserName();
                    invoice.ModifiedDate = DateTime.Now;
                    new InvoiceBFC().Update(invoice);

                    var invoiceDT = new ABCAPOSReportEDSC.InvoiceDTDataTable();
                    var invoiceDR = new InvoiceBFC().RetrievePrintOut(invoiceID);
                    var applyCreditDR = new ApplyCreditMemoBFC().RetrieveByInvoiceID(invoiceID);
                    decimal applyCreditValue = 0;
                    if (applyCreditDR.Count() > 0)
                    {
                        invoiceDR.CreditMemoCode = applyCreditDR[0].CreditMemoCode;
                        foreach(var item in applyCreditDR)
                        {
                            applyCreditValue += item.Amount;
                        }
                    }
                    var salesOrder = new SalesOrderBFC().RetrieveByID(invoiceDR.SalesOrderID);
                    var customer = new CustomerBFC().RetrieveByID(salesOrder.CustomerID);
                    invoiceDR.CustomerAddress = "";// customer.CompanyName;
                    if (!string.IsNullOrWhiteSpace(customer.BillingAddress1))
                        invoiceDR.CustomerAddress += "" + customer.BillingAddress1.Replace("\r", "<BR>");
                    if (!string.IsNullOrWhiteSpace(customer.City))
                        invoiceDR.CustomerAddress += " <BR>" + customer.City;
                    invoiceDR.Remarks = salesOrder.Title;
                    invoiceDT.LoadDataRow(invoiceDR.ItemArray, true);

                    var companySettingDT = new ABCAPOSReportEDSC.CompanySettingDTDataTable();
                    var companySettingDR = new CompanySettingBFC().RetrieveForPrintOut();

                    companySettingDT.LoadDataRow(companySettingDR.ItemArray, false);

                    var invoiceDetailDT = new InvoiceBFC().RetrieveDetailPrintOut(invoiceID);
                    new AccountingHelper().CalculateGrossAmount(invoiceDetailDT);
                    
                    ReportViewer1.LocalReport.EnableExternalImages = true;

                    //string path = @"file:\" + this.MapPath("~/Uploads/Logo/logo.png");

                    List<ReportParameter> param = new List<ReportParameter>();
                    //param.Add(new ReportParameter("ImagePath", path));
                    param.Add(new ReportParameter("Title", "Invoice"));
                    param.Add(new ReportParameter("CreditMemoValue", applyCreditValue.ToString()));
                    param.Add(new ReportParameter("ShippingAmountValue", invoice.ShippingAmount.ToString()));
                    //param.Add(new ReportParameter("Alamat", ""));
                    //param.Add(new ReportParameter("Kota", ""));
                    ReportViewer1.LocalReport.SetParameters(param);

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("InvoiceDT", invoiceDT as DataTable));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("InvoiceDetailDT", invoiceDetailDT as DataTable));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CompanySettingDT", companySettingDT as DataTable));
                    ReportViewer1.LocalReport.DisplayName = invoiceDR.Code;
                    this.ReportViewer1.LocalReport.Refresh();

                    #endregion
                }
                else if (Request.QueryString[SystemConstants.str_CashSalesID] != null)
                {
                    #region Cash Sales

                    long cashSalesID = Convert.ToInt64(Request.QueryString[SystemConstants.str_CashSalesID]);

                    var cashSales = new CashSalesBFC().RetrieveByID(cashSalesID);
                    cashSales.Flaging += 1;
                    cashSales.ModifiedBy = MembershipHelper.GetUserName();
                    cashSales.ModifiedDate = DateTime.Now;
                    new CashSalesBFC().Update(cashSales);

                    var cashSalesDT = new ABCAPOSReportEDSC.InvoiceDTDataTable();
                    var cashSalesDR = new CashSalesBFC().RetrievePrintOut(cashSalesID);

                    //var salesOrder = new SalesOrderBFC().RetrieveByID(invoiceDR.SalesOrderID);
                    var customer = new CustomerBFC().RetrieveByID(cashSales.CustomerID);
                    cashSalesDR.CustomerAddress = "";//customer.CompanyName;
                    if (!string.IsNullOrWhiteSpace(customer.BillingAddress1))
                        cashSalesDR.CustomerAddress += "" + customer.BillingAddress1.Replace("\r", "<BR>");
                        //cashSalesDR.CustomerAddress += " <BR>" + customer.BillingAddress1.Replace("\r", "<BR>");
                    if (!string.IsNullOrWhiteSpace(customer.City))
                        cashSalesDR.CustomerAddress += " <BR>" + customer.City;

                    cashSalesDT.LoadDataRow(cashSalesDR.ItemArray, true);

                    var companySettingDT = new ABCAPOSReportEDSC.CompanySettingDTDataTable();
                    var companySettingDR = new CompanySettingBFC().RetrieveForPrintOut();

                    companySettingDT.LoadDataRow(companySettingDR.ItemArray, false);

                    var cashDetailDT = new CashSalesBFC().RetrieveDetailPrintOut(cashSalesID);
                    foreach (var cashDetailDR in cashDetailDT)
                    {
                        cashDetailDR.GrossAmount = cashDetailDR.TotalAmount + cashDetailDR.TotalPPN;
                    }
                    ReportViewer1.LocalReport.EnableExternalImages = true;

                    //string path = @"file:\" + this.MapPath("~/Uploads/Logo/logo.png");

                    List<ReportParameter> param = new List<ReportParameter>();
                    //param.Add(new ReportParameter("ImagePath", path));
                    param.Add(new ReportParameter("Title", "Cash Sales"));
                    //param.Add(new ReportParameter("Alamat", ""));
                    //param.Add(new ReportParameter("Kota", ""));
                    ReportViewer1.LocalReport.SetParameters(param);

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("InvoiceDT", cashSalesDT as DataTable));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("InvoiceDetailDT", cashDetailDT as DataTable));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CompanySettingDT", companySettingDT as DataTable));
                    ReportViewer1.LocalReport.DisplayName = cashSalesDR.Code;
                    this.ReportViewer1.LocalReport.Refresh();

                    #endregion
                }
                else if (Request.QueryString[SystemConstants.str_MultipleInvoicingID] != null)
                {
                    #region Multiple Invoicing

                    long multipleInvoiceID = Convert.ToInt64(Request.QueryString[SystemConstants.str_MultipleInvoicingID]);

                    var multipleInvoiceDT = new ABCAPOSReportEDSC.InvoiceDTDataTable();
                    var multipleInvoiceDR = new MultipleInvoicingBFC().RetrieveMultipleInvoicePrintOut(multipleInvoiceID);
                    var details = new MultipleInvoicingBFC().RetrieveDetails(multipleInvoiceID);

                    int intCount = 0;
                    foreach (var detail in details)
                    {
                        var setComma = "";
                        if (intCount > 0)
                            setComma = ", ";

                        if (!multipleInvoiceDR.SalesOrderCodeList.Contains(detail.SalesOrderCode))
                            multipleInvoiceDR.SalesOrderCodeList += setComma + detail.SalesOrderCode;
                        if (!multipleInvoiceDR.DeliveryOrderCodeList.Contains(detail.DeliveryOrderCodeList))
                            multipleInvoiceDR.DeliveryOrderCodeList += setComma + detail.DeliveryOrderCodeList;
                        intCount += 1;
                    }
                    multipleInvoiceDR.SalesOrderCodeList = new IDNumericSayer().CutLastSevenDigit(multipleInvoiceDR.SalesOrderCodeList);
                    multipleInvoiceDR.DeliveryOrderCodeList = new IDNumericSayer().CutLastSevenDigit(multipleInvoiceDR.DeliveryOrderCodeList);
                    //var salesOrder = new SalesOrderBFC().RetrieveByID(multipleInvoiceDR.SalesOrderID);
                    var customer = new CustomerBFC().RetrieveByID(multipleInvoiceDR.CustomerID);
                    multipleInvoiceDR.CustomerAddress = multipleInvoiceDR.BillingAddress1.Replace("\r", "<BR>");// customer.CompanyName;
                    //if (!string.IsNullOrWhiteSpace(customer.BillingAddress1))
                    //    multipleInvoiceDR.CustomerAddress += "" + customer.BillingAddress1.Replace("\r", "<BR>");
                    
                    if (string.IsNullOrEmpty(multipleInvoiceDR.BillingAddress1) && !string.IsNullOrWhiteSpace(customer.City))
                        multipleInvoiceDR.CustomerAddress += " <BR>" + customer.City;
                    
                    multipleInvoiceDT.LoadDataRow(multipleInvoiceDR.ItemArray, true);

                    var companySettingDT = new ABCAPOSReportEDSC.CompanySettingDTDataTable();
                    var companySettingDR = new CompanySettingBFC().RetrieveForPrintOut();

                    companySettingDT.LoadDataRow(companySettingDR.ItemArray, false);

                    var multipleInvoiceItemDT = new MultipleInvoicingBFC().RetrieveDetailPrintOut(multipleInvoiceDR.ID);
                    new AccountingHelper().CalculateGrossAmount(multipleInvoiceItemDT);

                    ReportViewer1.LocalReport.EnableExternalImages = true;

                    //string path = @"file:\" + this.MapPath("~/Uploads/Logo/logo.png");

                    List<ReportParameter> param = new List<ReportParameter>();
                    //param.Add(new ReportParameter("ImagePath", path));
                    param.Add(new ReportParameter("Title", "Invoice"));
                    //param.Add(new ReportParameter("Alamat", "Jl. Pariwisata No. 19 Dadap"));
                    //param.Add(new ReportParameter("Kota", "Tangerang INDONESIA 15211"));
                    ReportViewer1.LocalReport.SetParameters(param);

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("InvoiceDT", multipleInvoiceDT as DataTable));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("InvoiceDetailDT", multipleInvoiceItemDT as DataTable));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CompanySettingDT", companySettingDT as DataTable));
                    ReportViewer1.LocalReport.DisplayName = multipleInvoiceDR.Code;
                    this.ReportViewer1.LocalReport.Refresh();

                    #endregion
                }
                else if (Request.QueryString[SystemConstants.str_CreditMemoID] != null)
                {
                    #region Credit Memo

                    long creditMemoID = Convert.ToInt64(Request.QueryString[SystemConstants.str_CreditMemoID]);

                    var creditMemo = new CreditMemoBFC().RetrieveByID(creditMemoID);
                    //creditMemo.Flaging += 1;
                    //creditMemo.ModifiedBy = MembershipHelper.GetUserName();
                    //creditMemo.ModifiedDate = DateTime.Now;
                    //new CashSalesBFC().Update(creditMemo);

                    var creditMemoDT = new ABCAPOSReportEDSC.InvoiceDTDataTable();
                    var creditMemoDR = new CreditMemoBFC().RetrievePrintOut(creditMemoID);

                    //var salesOrder = new SalesOrderBFC().RetrieveByID(invoiceDR.SalesOrderID);
                    var customer = new CustomerBFC().RetrieveByID(creditMemo.CustomerID);
                    //creditMemoDR.CustomerAddress = customer.CompanyName;
                    if (!string.IsNullOrWhiteSpace(customer.BillingAddress1))
                        creditMemoDR.CustomerAddress += "" + customer.BillingAddress1.Replace("\r", "<BR>");
                    if (!string.IsNullOrWhiteSpace(customer.City))
                        creditMemoDR.CustomerAddress += " <BR>" + customer.City;

                    creditMemoDT.LoadDataRow(creditMemoDR.ItemArray, true);

                    var companySettingDT = new ABCAPOSReportEDSC.CompanySettingDTDataTable();
                    var companySettingDR = new CompanySettingBFC().RetrieveForPrintOut();

                    companySettingDT.LoadDataRow(companySettingDR.ItemArray, false);

                    var creditDetailDT = new CreditMemoBFC().RetrieveDetailPrintOut(creditMemoID);
                    new AccountingHelper().CalculateGrossAmount(creditDetailDT);
                    
                    ReportViewer1.LocalReport.EnableExternalImages = true;

                    //string path = @"file:\" + this.MapPath("~/Uploads/Logo/logo.png");

                    List<ReportParameter> param = new List<ReportParameter>();
                    //param.Add(new ReportParameter("ImagePath", path));
                    param.Add(new ReportParameter("Title", "Credit Memo"));
                    //param.Add(new ReportParameter("Alamat", "Jl. Pariwisata No. 19 Dadap"));
                    //param.Add(new ReportParameter("Kota", "Tangerang INDONESIA 15211"));
                    ReportViewer1.LocalReport.SetParameters(param);

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("InvoiceDT", creditMemoDT as DataTable));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("InvoiceDetailDT", creditDetailDT as DataTable));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CompanySettingDT", companySettingDT as DataTable));
                    ReportViewer1.LocalReport.DisplayName = creditMemoDR.Code;
                    this.ReportViewer1.LocalReport.Refresh();

                    #endregion 
                }
            }
        }

    }
}