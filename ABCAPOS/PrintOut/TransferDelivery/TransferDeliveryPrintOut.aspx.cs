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

namespace ABCAPOS.PrintOut.TransferDelivery
{
    public partial class TransferDeliveryPrintOut : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString[SystemConstants.str_TransferDeliveryID] != null)
                {
                    long transferDeliveryID = Convert.ToInt64(Request.QueryString[SystemConstants.str_TransferDeliveryID]);

                    var transferDelivery = new TransferDeliveryBFC().RetrieveByID(transferDeliveryID);
                    transferDelivery.Flaging += 1;
                    transferDelivery.ModifiedBy = MembershipHelper.GetUserName();
                    transferDelivery.ModifiedDate = DateTime.Now;
                    new TransferDeliveryBFC().Update(transferDelivery);

                    var transferDeliveryDT = new ABCAPOSReportEDSC.DeliveryOrderDTDataTable();
                    var transferDeliveryDR = new TransferDeliveryBFC().RetrievePrintOut(transferDeliveryID);
                    var transferDeliveryDetailDT = new TransferDeliveryBFC().RetrieveDetailPrintOut(transferDeliveryID);

                    var transferOrder = new TransferOrderBFC().RetrieveByID(transferDeliveryDR.TransferOrderID);
                    var warehouse = new WarehouseBFC().RetrieveByID(transferOrder.ToWarehouseID);
                    transferDeliveryDR.CustomerAddress = warehouse.Address;
                    transferDeliveryDR.CustomerCity = warehouse.City;
                    transferDeliveryDR.CustomerPhone = warehouse.Phone;
                    transferDeliveryDT.LoadDataRow(transferDeliveryDR.ItemArray, true);

                    var companySettingDT = new ABCAPOSReportEDSC.CompanySettingDTDataTable();
                    var companySettingDR = new CompanySettingBFC().RetrieveForPrintOut();

                    companySettingDT.LoadDataRow(companySettingDR.ItemArray, false);

                    ReportViewer1.LocalReport.EnableExternalImages = true;

                    string path = @"file:\" + this.MapPath("~/Uploads/Logo/logo.png");

                    List<ReportParameter> param = new List<ReportParameter>();
                    param.Add(new ReportParameter("ImagePath", path));
                    param.Add(new ReportParameter("Flaging", Convert.ToString(transferDelivery.Flaging)));
                    ReportViewer1.LocalReport.SetParameters(param);

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DeliveryOrderDT", transferDeliveryDT as DataTable));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DeliveryOrderDetailDT", transferDeliveryDetailDT as DataTable));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CompanySettingDT", companySettingDT as DataTable));
                    ReportViewer1.LocalReport.DisplayName = transferDeliveryDR.Code;
                    //ReportViewer1.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(LocalReport_SubreportProcessing);
                    this.ReportViewer1.LocalReport.Refresh();
                }
            }
        }
    }
}