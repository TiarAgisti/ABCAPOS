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

namespace ABCAPOS.PrintOut.DeliveryOrder
{
    public partial class DeliveryOrderPrintOut : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString[SystemConstants.str_DeliveryOrderID] != null)
                {
                    long deliveryOrderID = Convert.ToInt64(Request.QueryString[SystemConstants.str_DeliveryOrderID]);

                    var deliveryOrder = new DeliveryOrderBFC().RetrieveByID(deliveryOrderID);
                    deliveryOrder.Flaging += 1;
                    deliveryOrder.ModifiedBy = MembershipHelper.GetUserName();
                    deliveryOrder.ModifiedDate = DateTime.Now;
                    new DeliveryOrderBFC().Update(deliveryOrder);

                    var deliveryOrderDT = new ABCAPOSReportEDSC.DeliveryOrderDTDataTable();
                    var deliveryOrderDR = new DeliveryOrderBFC().RetrievePrintOut(deliveryOrderID);
                    var deliveryOrderDetailDT = new DeliveryOrderBFC().RetrieveDetailPrintOut(deliveryOrderID);

                    var salesOrder = new SalesOrderBFC().RetrieveByID(deliveryOrderDR.SalesOrderID);
                    var customer = new CustomerBFC().RetrieveByID(salesOrder.CustomerID);
                    deliveryOrderDR.CustomerAddress = customer.CompanyName;
                    if (!string.IsNullOrWhiteSpace(customer.ShippingAddress))
                        deliveryOrderDR.CustomerAddress += " <BR>" + customer.ShippingAddress.Replace("\r", "<BR>");
                    if (!string.IsNullOrWhiteSpace(customer.City))
                        deliveryOrderDR.CustomerAddress += " <BR>" + customer.City;

                    deliveryOrderDR.SalesOrderDate = salesOrder.Date;
                    deliveryOrderDR.CustomerCity = customer.City;
                    deliveryOrderDR.CustomerContactPerson = customer.ContactPerson;
                    deliveryOrderDR.CustomerPhone = customer.Phone;
                    deliveryOrderDT.LoadDataRow(deliveryOrderDR.ItemArray, true);

                    var companySettingDT = new ABCAPOSReportEDSC.CompanySettingDTDataTable();
                    var companySettingDR = new CompanySettingBFC().RetrieveForPrintOut();

                    //if (salesOrder.TaxType == (int)TaxType.NonTax)
                    //    companySettingDR.Name = companySettingDR.OwnerName;

                    companySettingDT.LoadDataRow(companySettingDR.ItemArray, false);

                    ReportViewer1.LocalReport.EnableExternalImages = true;

                    //string path = @"file:\" + this.MapPath("~/Uploads/Logo/logo.png");

                    List<ReportParameter> param = new List<ReportParameter>();
                    //param.Add(new ReportParameter("ImagePath", path));
                    param.Add(new ReportParameter("Flaging", Convert.ToString(deliveryOrder.Flaging)));
                    ReportViewer1.LocalReport.SetParameters(param);

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DeliveryOrderDT", deliveryOrderDT as DataTable));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DeliveryOrderDetailDT", deliveryOrderDetailDT as DataTable));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CompanySettingDT", companySettingDT as DataTable));
                    ReportViewer1.LocalReport.DisplayName = deliveryOrderDR.Code;
                    //ReportViewer1.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(LocalReport_SubreportProcessing);
                    this.ReportViewer1.LocalReport.Refresh();
                }
            }
        }

        void LocalReport_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            if (Request.QueryString[SystemConstants.str_DeliveryOrderID] != null)
            {
                long deliveryOrderID = Convert.ToInt64(Request.QueryString[SystemConstants.str_DeliveryOrderID]);

                var deliveryOrderDetailDT = new DeliveryOrderBFC().RetrieveDetailPrintOut(deliveryOrderID);

                e.DataSources.Add(new ReportDataSource("DeliveryOrderDetailDT", deliveryOrderDetailDT as DataTable));
            }
        }
    }
}