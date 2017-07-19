using ABCAPOS.BF;
using ABCAPOS.Helpers;
using ABCAPOS.Models;
using ABCAPOS.EDM;
using ABCAPOS.DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using MPL.Business;
using ABCAPOS.Util;

namespace ABCAPOS.Controllers
{
    public class HomeController : Controller
    {
        private bool IsEmailSent(DateTime date)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.SentEmail
                        where i.Date == date.Date
                        select i;

            if (query.FirstOrDefault() != null)
                return true;
            else
                return false;
        }

        private void PostEmail(DateTime date)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            SentEmail obj = new SentEmail();
            obj.Date = new DateTime(date.Year, date.Month, 1);
            ent.AddToSentEmail(obj);
            ent.SaveChanges();
        }

        private void SendEmail()
        {
            var date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            if (IsEmailSent(date))
                return;

            var startDate = Convert.ToDateTime(date.Date.AddMonths(-1));
            var endDate = Convert.ToDateTime(date.Date.AddDays(-1));
            long accountID = 0;

            var list = new ABCAPOSReportDAC().RetrieveTrialBalanceReport(startDate, endDate,accountID);

            var helper = new AccountingHelper();

            var salesDT = helper.GetSalesDataTable(list);
            var modalDT = helper.GetModalDataTable(list);
            var businessDT = helper.GetBusinessDataTable(list);

            decimal profitLossAmount = salesDT.Sum(p => p.Amount) - modalDT.Sum(p => p.Amount) - businessDT.Sum(p => p.Amount);

            var invoiceGrandTotal = new InvoiceBFC().RetrieveUnpaidGrandTotal(startDate, endDate);

            StringBuilder emailBody = new StringBuilder();

            emailBody.Append("Laba (Rugi) Usaha : Rp." + profitLossAmount.ToString("N0"));
            emailBody.Append("\n");
            emailBody.Append("Total Invoice yang belum lunas : Rp." + invoiceGrandTotal.ToString("N0"));

            var setting = new CompanySettingBFC().Retrieve();

            new EmailHelper().SendEmail(setting.OwnerEmail, "ABCA Accounting Result " + DateTime.Now.ToString("MMMM yyyy"), emailBody.ToString());

            PostEmail(date);
        }

        public ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            var menu = MenuHelper.GetMenuList();

            string textFormat = "Anda mempunyai {0} {1} {2}";

            var customerGroupList = new CustomerGroupBFC().RetrieveAll();

            var count = 0;

            var notificationList = new List<HomeNotificationModel>();

            foreach (var customerGroup in customerGroupList)
            {
                if (menu.Contains("SalesOrder"))
                {
                    string moduleName = "Sales Order";

                    var notification = new HomeNotificationModel();
                    notification.Module = moduleName;
                    notification.CustomerGroupID = customerGroup.ID;

                    count = new SalesOrderBFC().RetrieveUnapprovedSalesOrderCount(customerGroup);

                    if (count > 0)
                    {
                        var notificationDetail = new HomeNotificationDetailModel();
                        notificationDetail.Count = count;
                        notificationDetail.Url = "/SalesOrder/Index?filter.FilterFields%5B0%5D.Value=7%2F7%2F2015&filter.FilterFields%5B0%5D.Value1=7%2F7%2F2015&filter.FilterFields%5B0%5D.FieldType=DateRange&filter.FilterFields%5B0%5D.PropertyName=Date&filter.FilterFields%5B0%5D.TargetDataType=System.DateTime&filter.FilterFields%5B0%5D.FilterOperator=Like&filter.FilterFields%5B1%5D.Value=&filter.FilterFields%5B1%5D.FieldType=TextBox&filter.FilterFields%5B1%5D.PropertyName=Code&filter.FilterFields%5B1%5D.TargetDataType=System.String&filter.FilterFields%5B1%5D.FilterOperator=Like&filter.FilterFields%5B2%5D.Value=&filter.FilterFields%5B2%5D.FieldType=TextBox&filter.FilterFields%5B2%5D.PropertyName=CustomerName&filter.FilterFields%5B2%5D.TargetDataType=System.String&filter.FilterFields%5B2%5D.FilterOperator=Like&filter.FilterFields%5B3%5D.Value=Head+Office&filter.FilterFields%5B3%5D.FieldType=DropDownList&filter.FilterFields%5B3%5D.PropertyName=WarehouseName&filter.FilterFields%5B3%5D.TargetDataType=System.String&filter.FilterFields%5B3%5D.FilterOperator=Equal&filter.FilterFields%5B4%5D.Value=&filter.FilterFields%5B4%5D.FieldType=TextBox&filter.FilterFields%5B4%5D.PropertyName=StatusDescription&filter.FilterFields%5B4%5D.TargetDataType=System.String&filter.FilterFields%5B4%5D.FilterOperator=Like&filter.FilterFields%5B5%5D.Selected=true&filter.FilterFields%5B5%5D.Value=1&filter.FilterFields%5B5%5D.FieldType=DropDownList&filter.FilterFields%5B5%5D.PropertyName=Status&filter.FilterFields%5B5%5D.TargetDataType=System.Int32&filter.FilterFields%5B5%5D.FilterOperator=Equal&filter.FilterFields%5B6%5D.Value=true&filter.FilterFields%5B6%5D.FieldType=DropDownList&filter.FilterFields%5B6%5D.PropertyName=IsDeliverable&filter.FilterFields%5B6%5D.TargetDataType=System.Boolean&filter.FilterFields%5B6%5D.FilterOperator=Equal";
                        notificationDetail.Text = String.Format(textFormat, count, moduleName, " yang belum disetujui");

                        notification.Details.Add(notificationDetail);
                    }

                    count = new SalesOrderBFC().RetrieveUnfulfillDeliveryOrderCount();
                    if (count > 0)
                    {
                        var notificationDetail = new HomeNotificationDetailModel();
                        notificationDetail.Count = count;
                        notificationDetail.Url = "/SalesOrder/Index?filter.FilterFields%5B0%5D.Value=7%2F7%2F2015&filter.FilterFields%5B0%5D.Value1=7%2F7%2F2015&filter.FilterFields%5B0%5D.FieldType=DateRange&filter.FilterFields%5B0%5D.PropertyName=Date&filter.FilterFields%5B0%5D.TargetDataType=System.DateTime&filter.FilterFields%5B0%5D.FilterOperator=Like&filter.FilterFields%5B1%5D.Value=&filter.FilterFields%5B1%5D.FieldType=TextBox&filter.FilterFields%5B1%5D.PropertyName=Code&filter.FilterFields%5B1%5D.TargetDataType=System.String&filter.FilterFields%5B1%5D.FilterOperator=Like&filter.FilterFields%5B2%5D.Value=&filter.FilterFields%5B2%5D.FieldType=TextBox&filter.FilterFields%5B2%5D.PropertyName=CustomerName&filter.FilterFields%5B2%5D.TargetDataType=System.String&filter.FilterFields%5B2%5D.FilterOperator=Like&filter.FilterFields%5B3%5D.Value=Head+Office&filter.FilterFields%5B3%5D.FieldType=DropDownList&filter.FilterFields%5B3%5D.PropertyName=WarehouseName&filter.FilterFields%5B3%5D.TargetDataType=System.String&filter.FilterFields%5B3%5D.FilterOperator=Equal&filter.FilterFields%5B4%5D.Value=&filter.FilterFields%5B4%5D.FieldType=TextBox&filter.FilterFields%5B4%5D.PropertyName=StatusDescription&filter.FilterFields%5B4%5D.TargetDataType=System.String&filter.FilterFields%5B4%5D.FilterOperator=Like&filter.FilterFields%5B5%5D.Selected=true&filter.FilterFields%5B5%5D.Value=3&filter.FilterFields%5B5%5D.FieldType=DropDownList&filter.FilterFields%5B5%5D.PropertyName=Status&filter.FilterFields%5B5%5D.TargetDataType=System.Int32&filter.FilterFields%5B5%5D.FilterOperator=Equal&filter.FilterFields%5B6%5D.Selected=true&filter.FilterFields%5B6%5D.Value=true&filter.FilterFields%5B6%5D.FieldType=DropDownList&filter.FilterFields%5B6%5D.PropertyName=IsDeliverable&filter.FilterFields%5B6%5D.TargetDataType=System.Boolean&filter.FilterFields%5B6%5D.FilterOperator=Equal";
                        notificationDetail.Text = String.Format(textFormat, count, moduleName, " yang perlu dibuatkan SJ");

                        notification.Details.Add(notificationDetail);
                    }

                    var invoiceStatusFilter = new SelectFilter()
                    {
                        CompareValue = "Open",
                        DataType = FilterDataType.String,
                        FieldName = "StatusDesc",
                        Operator = FilterOperator.Equal
                    };

                    count = new InvoiceBFC().RetreiveInvoiceNotification(new List<SelectFilter>() { invoiceStatusFilter });
                    if (count > 0)
                    {
                        var notificationDetail = new HomeNotificationDetailModel();
                        notificationDetail.Count = count;
                        notificationDetail.Url = "/Invoice/Index?filter.FilterFields%5B0%5D.Value=3%2F22%2F2016&filter.FilterFields%5B0%5D.Value1=2016-03-22&filter.FilterFields%5B0%5D.FieldType=DateRange&filter.FilterFields%5B0%5D.PropertyName=Date&filter.FilterFields%5B0%5D.TargetDataType=System.DateTime&filter.FilterFields%5B0%5D.FilterOperator=Like&filter.FilterFields%5B1%5D.Value=&filter.FilterFields%5B1%5D.FieldType=TextBox&filter.FilterFields%5B1%5D.PropertyName=Code&filter.FilterFields%5B1%5D.TargetDataType=System.String&filter.FilterFields%5B1%5D.FilterOperator=Like&filter.FilterFields%5B2%5D.Value=&filter.FilterFields%5B2%5D.FieldType=TextBox&filter.FilterFields%5B2%5D.PropertyName=DeliveryOrderCodeList&filter.FilterFields%5B2%5D.TargetDataType=System.String&filter.FilterFields%5B2%5D.FilterOperator=Like&filter.FilterFields%5B3%5D.Value=&filter.FilterFields%5B3%5D.FieldType=TextBox&filter.FilterFields%5B3%5D.PropertyName=SalesOrderCode&filter.FilterFields%5B3%5D.TargetDataType=System.String&filter.FilterFields%5B3%5D.FilterOperator=Like&filter.FilterFields%5B4%5D.Value=&filter.FilterFields%5B4%5D.FieldType=TextBox&filter.FilterFields%5B4%5D.PropertyName=CustomerName&filter.FilterFields%5B4%5D.TargetDataType=System.String&filter.FilterFields%5B4%5D.FilterOperator=Like&filter.FilterFields%5B5%5D.Value=Head+Office&filter.FilterFields%5B5%5D.FieldType=DropDownList&filter.FilterFields%5B5%5D.PropertyName=WarehouseName&filter.FilterFields%5B5%5D.TargetDataType=System.String&filter.FilterFields%5B5%5D.FilterOperator=Equal&filter.FilterFields%5B6%5D.Selected=true&filter.FilterFields%5B6%5D.Value=New&filter.FilterFields%5B6%5D.FieldType=DropDownList&filter.FilterFields%5B6%5D.PropertyName=StatusDesc&filter.FilterFields%5B6%5D.TargetDataType=System.String&filter.FilterFields%5B6%5D.FilterOperator=Equal";
                        notificationDetail.Text = String.Format(textFormat, count, "Invoice", " yang perlu di Invoice Payment");

                        notification.Details.Add(notificationDetail);
                    }

                    if (notification.Details.Any())
                        notificationList.Add(notification);
                }

                if (menu.Contains("PurchaseOrder"))
                {
                    string moduleName = "Purchase Order";

                    var notification = new HomeNotificationModel();
                    notification.Module = moduleName;
                    notification.CustomerGroupID = customerGroup.ID;

                    var newStatusFilter = new SelectFilter()
                    {
                        CompareValue = "Pending Approval",
                        DataType = FilterDataType.String,
                        FieldName = "StatusDescription",
                        Operator = FilterOperator.Equal
                    };

                    count = new PurchaseOrderBFC().RetrieveCount(new List<SelectFilter>() { newStatusFilter });

                    if (count > 0)
                    {
                        var notificationDetail = new HomeNotificationDetailModel();
                        notificationDetail.Count = count;
                        notificationDetail.Url = "/PurchaseOrder/Index?filter.FilterFields%5B0%5D.Value=11%2F1%2F2015&filter.FilterFields%5B0%5D.Value1=11%2F1%2F2015&filter.FilterFields%5B0%5D.FieldType=DateRange&filter.FilterFields%5B0%5D.PropertyName=Date&filter.FilterFields%5B0%5D.TargetDataType=System.DateTime&filter.FilterFields%5B0%5D.FilterOperator=Like&filter.FilterFields%5B1%5D.Value=&filter.FilterFields%5B1%5D.FieldType=TextBox&filter.FilterFields%5B1%5D.PropertyName=Code&filter.FilterFields%5B1%5D.TargetDataType=System.String&filter.FilterFields%5B1%5D.FilterOperator=Like&filter.FilterFields%5B2%5D.Value=&filter.FilterFields%5B2%5D.FieldType=TextBox&filter.FilterFields%5B2%5D.PropertyName=VendorName&filter.FilterFields%5B2%5D.TargetDataType=System.String&filter.FilterFields%5B2%5D.FilterOperator=Like&filter.FilterFields%5B3%5D.Value=&filter.FilterFields%5B3%5D.FieldType=TextBox&filter.FilterFields%5B3%5D.PropertyName=WarehouseName&filter.FilterFields%5B3%5D.TargetDataType=System.String&filter.FilterFields%5B3%5D.FilterOperator=Like&filter.FilterFields%5B4%5D.Selected=true&filter.FilterFields%5B4%5D.Value=Pending+Approval&filter.FilterFields%5B4%5D.FieldType=TextBox&filter.FilterFields%5B4%5D.PropertyName=StatusDescription&filter.FilterFields%5B4%5D.TargetDataType=System.String&filter.FilterFields%5B4%5D.FilterOperator=Like";
                        notificationDetail.Text = String.Format(textFormat, count, moduleName, " yang perlu di Approve");

                        notification.Details.Add(notificationDetail);
                    }

                    var pendingReceiptStatusFilter = new SelectFilter()
                    {
                        CompareValue = "Pending Receipt",
                        DataType = FilterDataType.String,
                        FieldName = "StatusDescription",
                        Operator = FilterOperator.Equal
                    };

                    count = new PurchaseOrderBFC().RetrieveCount(new List<SelectFilter>() { pendingReceiptStatusFilter });
                    if (count > 0)
                    {
                        var notificationDetail = new HomeNotificationDetailModel();
                        notificationDetail.Count = count;
                        notificationDetail.Url = "/PurchaseOrder/Index?filter.FilterFields%5B0%5D.Value=11%2F1%2F2015&filter.FilterFields%5B0%5D.Value1=11%2F1%2F2015&filter.FilterFields%5B0%5D.FieldType=DateRange&filter.FilterFields%5B0%5D.PropertyName=Date&filter.FilterFields%5B0%5D.TargetDataType=System.DateTime&filter.FilterFields%5B0%5D.FilterOperator=Like&filter.FilterFields%5B1%5D.Value=&filter.FilterFields%5B1%5D.FieldType=TextBox&filter.FilterFields%5B1%5D.PropertyName=Code&filter.FilterFields%5B1%5D.TargetDataType=System.String&filter.FilterFields%5B1%5D.FilterOperator=Like&filter.FilterFields%5B2%5D.Value=&filter.FilterFields%5B2%5D.FieldType=TextBox&filter.FilterFields%5B2%5D.PropertyName=VendorName&filter.FilterFields%5B2%5D.TargetDataType=System.String&filter.FilterFields%5B2%5D.FilterOperator=Like&filter.FilterFields%5B3%5D.Value=&filter.FilterFields%5B3%5D.FieldType=TextBox&filter.FilterFields%5B3%5D.PropertyName=WarehouseName&filter.FilterFields%5B3%5D.TargetDataType=System.String&filter.FilterFields%5B3%5D.FilterOperator=Like&filter.FilterFields%5B4%5D.Selected=true&filter.FilterFields%5B4%5D.Value=Pending+Receipt&filter.FilterFields%5B4%5D.FieldType=TextBox&filter.FilterFields%5B4%5D.PropertyName=StatusDescription&filter.FilterFields%5B4%5D.TargetDataType=System.String&filter.FilterFields%5B4%5D.FilterOperator=Like";
                        notificationDetail.Text = String.Format(textFormat, count, moduleName, " yang perlu di Receipt");

                        notification.Details.Add(notificationDetail);
                    }

                    var pendingBillingStatusFilter = new SelectFilter()
                    {
                        CompareValue = "Pending Billing",
                        DataType = FilterDataType.String,
                        FieldName = "StatusDescription",
                        Operator = FilterOperator.Equal
                    };

                    count = new PurchaseOrderBFC().RetrieveCount(new List<SelectFilter>() { pendingReceiptStatusFilter});
                    if (count > 0)
                    {
                        var notificationDetail = new HomeNotificationDetailModel();
                        notificationDetail.Count = count;
                        notificationDetail.Url = "/PurchaseOrder/Index?filter.FilterFields%5B0%5D.Value=11%2F1%2F2015&filter.FilterFields%5B0%5D.Value1=11%2F1%2F2015&filter.FilterFields%5B0%5D.FieldType=DateRange&filter.FilterFields%5B0%5D.PropertyName=Date&filter.FilterFields%5B0%5D.TargetDataType=System.DateTime&filter.FilterFields%5B0%5D.FilterOperator=Like&filter.FilterFields%5B1%5D.Value=&filter.FilterFields%5B1%5D.FieldType=TextBox&filter.FilterFields%5B1%5D.PropertyName=Code&filter.FilterFields%5B1%5D.TargetDataType=System.String&filter.FilterFields%5B1%5D.FilterOperator=Like&filter.FilterFields%5B2%5D.Value=&filter.FilterFields%5B2%5D.FieldType=TextBox&filter.FilterFields%5B2%5D.PropertyName=VendorName&filter.FilterFields%5B2%5D.TargetDataType=System.String&filter.FilterFields%5B2%5D.FilterOperator=Like&filter.FilterFields%5B3%5D.Value=&filter.FilterFields%5B3%5D.FieldType=TextBox&filter.FilterFields%5B3%5D.PropertyName=WarehouseName&filter.FilterFields%5B3%5D.TargetDataType=System.String&filter.FilterFields%5B3%5D.FilterOperator=Like&filter.FilterFields%5B4%5D.Selected=true&filter.FilterFields%5B4%5D.Value=Pending+Billing&filter.FilterFields%5B4%5D.FieldType=TextBox&filter.FilterFields%5B4%5D.PropertyName=StatusDescription&filter.FilterFields%5B4%5D.TargetDataType=System.String&filter.FilterFields%5B4%5D.FilterOperator=Like";
                        notificationDetail.Text = String.Format(textFormat, count, moduleName, " yang perlu di Bill");

                        notification.Details.Add(notificationDetail);
                    }

                    var pendingPaymentStatusFilter = new SelectFilter()
                    {
                        CompareValue = (int)PurchaseBillStatus.New,
                        DataType = FilterDataType.Integer,
                        FieldName = "Status",
                        Operator = FilterOperator.Equal
                    };

                    count = new PurchaseBillBFC().RetrieveCount(new List<SelectFilter>() { pendingPaymentStatusFilter});
                    if (count > 0)
                    {
                        var notificationDetail = new HomeNotificationDetailModel();
                        notificationDetail.Count = count;
                        notificationDetail.Url = "/PurchaseBill/Index";
                        notificationDetail.Text = String.Format(textFormat, count, "Purchase Bill", " yang perlu di Bill Payment");

                        notification.Details.Add(notificationDetail);
                    }

                    if (notification.Details.Any())
                        notificationList.Add(notification);
                }

                if (menu.Contains("TransferOrder"))
                {
                    string moduleName = "Transfer Order";

                    var notification = new HomeNotificationModel();
                    notification.Module = moduleName;
                    notification.CustomerGroupID = customerGroup.ID;

                    count = new TransferOrderBFC().RetrieveTransferOrderUnDelivery();
                    if (count > 0)
                    {
                        var notificationDetail = new HomeNotificationDetailModel();
                        notificationDetail.Count = count;
                        notificationDetail.Url = "/TransferOrder/Index?filter.FilterFields%5B0%5D.Value=4%2F28%2F2016&filter.FilterFields%5B0%5D.Value1=04%2F28%2F2016&filter.FilterFields%5B0%5D.FieldType=DateRange&filter.FilterFields%5B0%5D.PropertyName=Date&filter.FilterFields%5B0%5D.TargetDataType=System.DateTime&filter.FilterFields%5B0%5D.FilterOperator=Like&filter.FilterFields%5B1%5D.Value=&filter.FilterFields%5B1%5D.FieldType=TextBox&filter.FilterFields%5B1%5D.PropertyName=Code&filter.FilterFields%5B1%5D.TargetDataType=System.String&filter.FilterFields%5B1%5D.FilterOperator=Like&filter.FilterFields%5B2%5D.Selected=true&filter.FilterFields%5B2%5D.Value=3&filter.FilterFields%5B2%5D.FieldType=DropDownList&filter.FilterFields%5B2%5D.PropertyName=Status&filter.FilterFields%5B2%5D.TargetDataType=System.Int32&filter.FilterFields%5B2%5D.FilterOperator=Equal";
                        notificationDetail.Text = String.Format(textFormat, count, moduleName, " yang perlu dibuatkan SJ");

                        notification.Details.Add(notificationDetail);
                    }
                    

                    if (notification.Details.Any())
                        notificationList.Add(notification);
                }
            }

            ViewBag.NotificationList = notificationList;
            ViewBag.CustomerGroupList = customerGroupList;

            //ViewBag.SalesOrderThisMonthCount = new SalesOrderBFC().RetrieveThisMonthSalesOrderCount();
            //ViewBag.SalesOrderLastMonthCount = new SalesOrderBFC().RetrieveLastMonthSalesOrderCount();
            //ViewBag.SalesOrderThisYearCount = new SalesOrderBFC().RetrieveThisYearSalesOrderCount();
            //ViewBag.SalesOrderLastYearCount = new SalesOrderBFC().RetrieveLastYearSalesOrderCount();
            //ViewBag.AveragePaymentDays = new InvoiceBFC().RetrieveAveragePaymentDays();

            ViewBag.MonthlyInvoice = new ABCAPOSDAC().RetrieveMonthlyInvoice();
            ViewBag.MonthlyPO = new ABCAPOSDAC().RetrieveMonthlyPO();
            ViewBag.MonthlyInvoicePaymemt = new ABCAPOSDAC().RetrieveMonthlyInvoicePayment();
            ViewBag.MonthlyBilling = new ABCAPOSDAC().RetrieveMonthlyBillingPayment();

            List<SalesOrderModel> invoiceTop = new SalesOrderBFC().RetriveSalesOrderTopList();
            ViewBag.InvoiceTopList = invoiceTop;

            List<SalesOrderModel> invoiceTopMarketing = new SalesOrderBFC().RetriveSalesOrderMarketingList();
            ViewBag.InvoiceMarketingList = invoiceTopMarketing;

            //Qty Sold
            //List<ProductModel> productSold = new ProductBFC().RetriveSalesOrderItemSoldList(filter.GetSelectFilters(), DateTime.Now.Year);
            //ViewBag.SalesOrderItemSold = productSold;

            ViewBag.Year = DateTime.Now.Year.ToString();
            //SendEmail(); Comment out, causing unnecessary SQL connection time out problems

            ViewBag.FilterFields = filter.FilterFields;

            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
