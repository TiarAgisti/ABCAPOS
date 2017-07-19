using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;
using ABCAPOS.ReportEDS;
using ABCAPOS.Util;
using ABCAPOS.BF;
using System.Data;

namespace ABCAPOS.PrintOut.VendorReturn
{
    public partial class VendorReturnPrintOut : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString[SystemConstants.str_VendorReturnID] != null)
                {
                    long vendorReturnID = Convert.ToInt64(Request.QueryString[SystemConstants.str_VendorReturnID]);

                    //var vendorReturn = new VendorReturnBFC().RetrieveByID(vendorReturnID);
                    //vendorReturn.Flaging += 1;
                    //vendorReturn.ModifiedBy = MembershipHelper.GetUserName();
                    //vendorReturn.ModifiedDate = DateTime.Now;
                    //new VendorReturnBFC().Update(vendorReturn);

                    var vendorReturnDT = new ABCAPOSReportEDSC.DeliveryOrderDTDataTable();
                    var vendorReturnDR = new VendorReturnBFC().RetrievePrintOut(vendorReturnID);
                    var vendorReturnDetailDT = new VendorReturnBFC().RetrieveDetailPrintOut(vendorReturnID);

                    var vendorReturn = new VendorReturnBFC().RetrieveByID(vendorReturnDR.ID);
                    var vendor = new VendorBFC().RetrieveByID(vendorReturn.SupplierID);
                    vendorReturnDR.CustomerAddress = vendor.Address;
                    vendorReturnDR.CustomerCity = vendor.City;
                    vendorReturnDR.CustomerContactPerson = vendor.ContactPerson;
                    vendorReturnDR.CustomerPhone = vendor.Phone;
                    vendorReturnDT.LoadDataRow(vendorReturnDR.ItemArray, true);

                    var companySettingDT = new ABCAPOSReportEDSC.CompanySettingDTDataTable();
                    var companySettingDR = new CompanySettingBFC().RetrieveForPrintOut();

                    //if (salesOrder.TaxType == (int)TaxType.NonTax)
                    //    companySettingDR.Name = companySettingDR.OwnerName;

                    companySettingDT.LoadDataRow(companySettingDR.ItemArray, false);

                    ReportViewer1.LocalReport.EnableExternalImages = true;

                    string path = @"file:\" + this.MapPath("~/Uploads/Logo/logo.png");

                    List<ReportParameter> param = new List<ReportParameter>();
                    param.Add(new ReportParameter("ImagePath", path));
                    //param.Add(new ReportParameter("Flaging", Convert.ToString(vendorReturn.Flaging)));
                    ReportViewer1.LocalReport.SetParameters(param);

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DeliveryOrderDT", vendorReturnDT as DataTable));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DeliveryOrderDetailDT", vendorReturnDetailDT as DataTable));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CompanySettingDT", companySettingDT as DataTable));
                    ReportViewer1.LocalReport.DisplayName = vendorReturnDR.Code;
                    //ReportViewer1.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(LocalReport_SubreportProcessing);
                    this.ReportViewer1.LocalReport.Refresh();
                }
            }
        }

    }
}