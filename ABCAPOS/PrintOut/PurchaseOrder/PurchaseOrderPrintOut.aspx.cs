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
using MPL;

namespace ABCAPOS.PrintOut.PurchaseOrder
{
    public partial class PurchaseOrderPrintOut : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString[SystemConstants.str_PurchaseOrderID] != null)
                {
                    long purchaseOrderID = Convert.ToInt64(Request.QueryString[SystemConstants.str_PurchaseOrderID]);

                    var purchaseOrderDT = new ABCAPOSReportEDSC.PurchaseOrderDTDataTable();
                    var purchaseOrderDR = new PurchaseOrderBFC().RetrievePrintOut(purchaseOrderID);

                    var vendor = new VendorBFC().RetrieveByID(purchaseOrderDR.SupplierID);
                    purchaseOrderDR.ContactPerson = vendor.ContactPerson;
                    //var supplier = new SupplierBFC().RetrieveByID(purchaseOrderDR.SupplierID);
                    //purchaseOrderDR.SupplierAddress = supplier.Address;
                    //purchaseOrderDR.SupplierCity = supplier.City;
                    //purchaseOrderDR.SupplierContactPerson = supplier.ContactPerson;
                    //purchaseOrderDR.SupplierPhone = supplier.Phone;

                    //if (purchaseOrderDR.Currency == (int)Currency.IDR)
                    //{
                    //    purchaseOrderDR.CurrencyDescription = "Rupiah (IDR)";
                    //    purchaseOrderDR.ExchangeRate = 1;
                    //}
                    //else
                    //{
                    //    purchaseOrderDR.CurrencyDescription = "Dollar (USD)";
                    //    purchaseOrderDR.ExchangeRate = purchaseOrderDR.ExchangeRate;
                    //}

                    var purchaseOrderDetailDT = new PurchaseOrderBFC().RetrieveDetailPrintOut(purchaseOrderID);
                    var dt = ObjectHelper.CreateDynamicDataTable(purchaseOrderDetailDT);
                    //purchaseOrderDR.ItemAmount = purchaseOrderDetailDT.Sum(p => p.Quantity);

                    purchaseOrderDT.LoadDataRow(purchaseOrderDR.ItemArray, true);

                    var companySettingDT = new ABCAPOSReportEDSC.CompanySettingDTDataTable();
                    var companySettingDR = new CompanySettingBFC().RetrieveForPrintOut();

                    if (purchaseOrderDR.TaxType == (int)TaxType.NonTax)
                        companySettingDR.Name = companySettingDR.OwnerName;

                    companySettingDT.LoadDataRow(companySettingDR.ItemArray, false);

                    var companySetting = new CompanySettingBFC().Retrieve();

                    ReportViewer1.LocalReport.EnableExternalImages = true;

                    string path = @"file:\" + this.MapPath("~/Uploads/Logo/logo.png");
                    //string pathTommy = @"file:\" + this.MapPath("~/Uploads/Logo/Tommy.png");
                    //string pathHendraLukman = @"file:\" + this.MapPath("~/Uploads/Logo/HendraLukman.png");

                    List<ReportParameter> param = new List<ReportParameter>();
                    param.Add(new ReportParameter("ImagePath", path));
                    //param.Add(new ReportParameter("CurrentLocationAndDate", "Jakarta, " + new IDNumericSayer().DateFormatingInd(purchaseOrderDR.Date)));
                    ReportViewer1.LocalReport.SetParameters(param);

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("PurchaseOrderDT", purchaseOrderDT as DataTable));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CompanySettingDT", companySettingDT as DataTable));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("PurchaseOrderDetailDT", dt as DataTable));
                    ReportViewer1.LocalReport.DisplayName = purchaseOrderDR.Code;
                    //ReportViewer1.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(LocalReport_SubreportProcessing);
                    this.ReportViewer1.LocalReport.Refresh();
                }
            }
        }

        //void LocalReport_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        //{
        //    if (Request.QueryString[SystemConstants.str_PurchaseOrderID] != null)
        //    {
        //        long purchaseOrderID = Convert.ToInt64(Request.QueryString[SystemConstants.str_PurchaseOrderID]);

        //        var purchaseOrder = new PurchaseOrderBFC().RetrieveByID(purchaseOrderID);
        //        var purchaseOrderDetailDT = new PurchaseOrderBFC().RetrieveDetailPrintOut(purchaseOrderID);

        //        foreach (var purchaseOrderDetailDR in purchaseOrderDetailDT)
        //            purchaseOrderDetailDR.Price = Convert.ToDecimal((purchaseOrderDetailDR.Price - purchaseOrderDetailDR.Discount).ToString("N0"));

        //        e.DataSources.Add(new ReportDataSource("PurchaseOrderDetailDT", purchaseOrderDetailDT as DataTable));
        //    }
        //}
    }
}