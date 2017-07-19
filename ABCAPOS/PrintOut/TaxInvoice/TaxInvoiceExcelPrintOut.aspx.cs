using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ABCAPOS.BF;
using ABCAPOS.Helpers;
using ABCAPOS.Models;
using ABCAPOS.ReportEDS;
using ABCAPOS.Util;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.HSSF.Util;
using NPOI.POIFS.FileSystem;
using NPOI.HPSF;
using System.Data;

namespace ABCAPOS.PrintOut.TaxInvoice
{
    public partial class TaxInvoiceExcelPrintOut : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString[SystemConstants.str_InvoiceID] != null)
                {
                    long invoiceID = Convert.ToInt64(Request.QueryString[SystemConstants.str_InvoiceID]);
                    
                    var filename = "Template Faktur Pajak";
                    if (File.Exists(Server.MapPath("~/App_Data/Templates/Template Faktur Pajak.xls")))
                    {
                        var invoice = new InvoiceBFC().RetrieveByID(invoiceID);
                        var invoiceDetails = new InvoiceBFC().RetrieveDetails(invoiceID);
                        var customer = new CustomerBFC().RetrieveByID(invoice.CustomerID);
                        

                        var workbook = new HSSFWorkbook(File.OpenRead(Server.MapPath("~/App_Data/Templates/" + filename + ".xls")));
                        int sheetIndex = 0;
                        ISheet sheet = workbook.GetSheetAt(sheetIndex);
                        //sethere
                        SetHeaderTaxInvoice(sheet, invoice, customer);
                        
                        int countData = 0;
                        int row = 23;
                        foreach (var invDet in invoiceDetails)
                        {
                            if (countData > 0)
                            {
                                if (countData % 20 == 0)
                                {
                                    sheetIndex ++;
                                    sheet = workbook.GetSheetAt(sheetIndex);
                                    //sethere too
                                    SetHeaderTaxInvoice(sheet, invoice, customer);
                                    row = 23;
                                }
                            }
                            sheet.GetRow(row).GetCell(0).SetCellValue(countData+1);
                            sheet.GetRow(row).GetCell(2).SetCellValue(invDet.ProductName);
                            sheet.GetRow(row).GetCell(16).SetCellValue(invDet.Quantity.ToString("N0") + " " + invDet.ConversionName);
                            new InvoiceBFC().CalculateGrossAmountInvDetail(invDet);
                            sheet.GetRow(row).GetCell(22).SetCellValue(invDet.Price.ToString("N2"));
                            sheet.GetRow(row).GetCell(25).SetCellValue(Convert.ToDouble(invDet.TotalAmount.ToString("N0")));
                            countData++;
                            row++;
                        }
                        
                        HSSFFormulaEvaluator.EvaluateAllFormulaCells(workbook);
                        
                        Response.Clear();
                        Response.Buffer = true;
                        Response.Charset = "";
                        Response.ContentType = "application/vnd.ms-excel";
                        Response.AddHeader("content-disposition", "attachment;filename=Faktur_Pajak_" + invoice.Code + "_" + DateTime.Now.ToString("ddMMyyhhmmss") + ".xls");
                        using (MemoryStream MyMemoryStream = new MemoryStream())
                        {
                            workbook.Write(MyMemoryStream);
                            //wb.SaveAs(MyMemoryStream);
                            MyMemoryStream.WriteTo(Response.OutputStream);
                            Response.Flush();
                            Response.End();
                        }
                    }
                }
                else if (Request.QueryString[SystemConstants.str_MultipleInvoicingID] != null)
                {
                    long multipleInvoiceID = Convert.ToInt64(Request.QueryString[SystemConstants.str_MultipleInvoicingID]);

                    var filename = "Template Faktur Pajak";
                    if (File.Exists(Server.MapPath("~/App_Data/Templates/Template Faktur Pajak.xls")))
                    {
                        var multipleInvoice = new MultipleInvoicingBFC().RetrieveByID(multipleInvoiceID);
                        var details = new MultipleInvoicingBFC().RetrieveItemDetailsGroup(multipleInvoiceID);
                        var customer = new CustomerBFC().RetrieveByID(multipleInvoice.CustomerID);

                        var workbook = new HSSFWorkbook(File.OpenRead(Server.MapPath("~/App_Data/Templates/" + filename + ".xls")));
                        int sheetIndex = 0;
                        ISheet sheet = workbook.GetSheetAt(sheetIndex);
                        
                        SetHeaderTaxMultipleInvoice(sheet, multipleInvoice, customer);
                        
                        int countData = 0;
                        int row = 23;
                        foreach (var invDet in details)
                        {
                            if (countData > 0)
                            {
                                if (countData % 20 == 0)
                                {
                                    sheetIndex++;
                                    sheet = workbook.GetSheetAt(sheetIndex);
                                    //sethere too
                                    SetHeaderTaxMultipleInvoice(sheet, multipleInvoice, customer);
                                    row = 23;
                                }
                            }
                            sheet.GetRow(row).GetCell(0).SetCellValue(countData + 1);
                            sheet.GetRow(row).GetCell(2).SetCellValue(invDet.ProductName);
                            sheet.GetRow(row).GetCell(16).SetCellValue(invDet.Quantity.ToString("N0") + " " + invDet.ConversionName);
                            new AccountingHelper().CalculateGrossAmountMultiInvDetail(invDet);
                            sheet.GetRow(row).GetCell(22).SetCellValue(invDet.Price.ToString("N2"));
                            sheet.GetRow(row).GetCell(25).SetCellValue(Convert.ToDouble(invDet.TotalAmount.ToString("N0")));
                            //cell.SetCellValue(countData+1);
                            //sheet.GetRow(row).CreateCell(0).NumericCellValue = 0;
                            countData++;
                            row++;
                        }

                        HSSFFormulaEvaluator.EvaluateAllFormulaCells(workbook);
                        //var 
                        //ISheet sheet = workbook.GetSheetAt(0);

                        Response.Clear();
                        Response.Buffer = true;
                        Response.Charset = "";
                        Response.ContentType = "application/vnd.ms-excel";
                        Response.AddHeader("content-disposition", "attachment;filename=Faktur_Pajak_" + multipleInvoice.Code + "_" + DateTime.Now.ToString("ddMMyyhhmmss") + ".xls");
                        using (MemoryStream MyMemoryStream = new MemoryStream())
                        {
                            workbook.Write(MyMemoryStream);
                            //wb.SaveAs(MyMemoryStream);
                            MyMemoryStream.WriteTo(Response.OutputStream);
                            Response.Flush();
                            Response.End();
                        }
                    }
                }
            }
        }

        private void SetHeaderTaxMultipleInvoice(ISheet sheet, MultipleInvoicingModel invoice, CustomerModel customer)
        {
            var invoiceDate = new IDNumericSayer().DateFormatingInd(invoice.Date);
            SetCustomerAddress(sheet, customer);
            sheet.GetRow(52).GetCell(26).SetCellValue(invoiceDate);
            //if (string.IsNullOrWhiteSpace(invoice.ReceiptNo))
                sheet.GetRow(4).GetCell(2).SetCellValue("Kode dan Nomor Seri Faktur Pajak : 010.000 - 12.00000000");
            //else
            //    sheet.GetRow(4).GetCell(2).SetCellValue("Kode dan Nomor Seri Faktur Pajak : " + invoice.ReceiptNo);

        }
        private void SetHeaderTaxInvoice(ISheet sheet, InvoiceModel invoice, CustomerModel customer)
        {
            var invoiceDate = new IDNumericSayer().DateFormatingInd(invoice.Date);
            SetCustomerAddress(sheet, customer);
            sheet.GetRow(52).GetCell(26).SetCellValue(invoiceDate);
            if (string.IsNullOrWhiteSpace(invoice.ReceiptNo))
                sheet.GetRow(4).GetCell(2).SetCellValue("Kode dan Nomor Seri Faktur Pajak : 010.000 - 12.00000000");
            else
                sheet.GetRow(4).GetCell(2).SetCellValue("Kode dan Nomor Seri Faktur Pajak : " + invoice.ReceiptNo);

        }

        private void SetCustomerAddress(ISheet sheet, CustomerModel customer)
        {
            sheet.GetRow(15).GetCell(10).SetCellValue(customer.Name);

            if (!string.IsNullOrWhiteSpace(customer.TaxFileNumber))
            {
                sheet.GetRow(16).GetCell(10).SetCellValue(customer.Address);
                sheet.GetRow(18).GetCell(10).SetCellValue(customer.TaxFileNumber);

            }
            else
            {
                sheet.GetRow(16).GetCell(10).SetCellValue(customer.City);
                sheet.GetRow(18).GetCell(10).SetCellValue("00.000.000.0-000.000");
            }
        }
    }
}