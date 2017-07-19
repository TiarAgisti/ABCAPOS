using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ABCAPOS.Controllers
{
    public class ReportViewerController : Controller
    {
        private string GetReportTitle(ReportViewerType type, string queryString)
        {
            if (type == ReportViewerType.PurchaseOrder)
                return "Report Purchase Order (Detail)";
            
            if (type == ReportViewerType.PurchaseBill)
                return "REPORT PURCHASE BILL";

            if (type == ReportViewerType.PurchaseBillOverdue)
                return "REPORT PURCHASE BILL OVERDUE";

            if (type == ReportViewerType.PurchaseBillAging)
                return "REPORT PURCHASE BILL AGING";

            if (type == ReportViewerType.SalesOrder)
                return "Laporan Daftar Sales Order";

            if (type == ReportViewerType.SalesOrderDetail)
                return "Laporan Rincian Sales Order";
            
            if (type == ReportViewerType.SalesReport)
                return "Laporan Penjualan";

            if (type == ReportViewerType.DeliveryOrder)
                return "Daftar Fulfillment";

            if (type == ReportViewerType.FulfillmentByItemDetail)
                return "Fulfillment By Item Detail";

            if (type == ReportViewerType.FulfillmentByItemSummary)
                return "Fulfillment By Item Summary";

            if (type == ReportViewerType.Invoice)
                return "Laporan Invoice";

            if (type == ReportViewerType.InvoiceAging)
                return "Laporan Invoice Aging";

            if (type == ReportViewerType.Payment)
                return "Laporan Pembayaran";

            if (type == ReportViewerType.GeneralJournal)
                return "Laporan Jurnal Umum";

            if (type == ReportViewerType.AccountTransaction)
                return "Laporan Buku Besar";

            if (type == ReportViewerType.TrialBalance)
                return "Laporan Neraca Saldo";

            if (type == ReportViewerType.ProfitAndLoss)
                return "Laporan Laba Rugi";

            if (type == ReportViewerType.IncomeExpense)
                return "Laporan Neraca";

            if (type == ReportViewerType.Attendance)
                return "Daftar Uang Lembur Kantor";

            if (type == ReportViewerType.AttendanceDetail)
                return "Rincian Uang Lembur Kantor";

            if (type == ReportViewerType.IncomeBank)
                return "Bukti Pemasukan";

            if (type == ReportViewerType.ExpenseBank)
                return "Bukti Pemasukan";

            if (type == ReportViewerType.SalesOrderBySalesRepSumNoCustomer)
                return "Sales Order by Sales Rep";

            if (type == ReportViewerType.InvoiceBySalesRepSum)
                return "Sales Order by Sales Rep Sum";

            if (type == ReportViewerType.InvoiceBySalesRepDetail)
                return "Sales Order by Sales Rep Detail";

            if (type == ReportViewerType.InvoiceByItemSum)
                return "Invoices by Item Sum";

            if (type == ReportViewerType.PaymentInvoice)
                return "Payment History By Invoice";

            if (type == ReportViewerType.PaymentBill)
                return "Payment History By Bill";

            if (type == ReportViewerType.ReportOverdue)
                return "Report Overdue";

            if (type == ReportViewerType.ReportBonus)
                return "Bonus By Invoice";

            if (type == ReportViewerType.InvoiceMonthly)
                return "Invoice Monthly";

            if (type == ReportViewerType.InvoiceMonthlySummary)
                return "Invoice Monthly Summary";

            if (type == ReportViewerType.InvoicePaymentSummary)
                return "Invoice Payment Summary";

            if (type == ReportViewerType.InvoicePaymentDetail)
                return "Invoice Payment Detail";

            if (type == ReportViewerType.Product)
                return "Report Stock";

            if (type == ReportViewerType.CreditMemo)
                return "Credit Memo By Item";

            if (type == ReportViewerType.InvoiceCreditMemo)
                return "Report Invoice & Credit Memo by Sales Rep";

            if (type == ReportViewerType.Stock)
                return "Stock Report";

            if (type == ReportViewerType.TransactionStock)
                return "Transaction Stock Report";

            if (type == ReportViewerType.GoodsSoldByProduct)
                return "Goods Sold By Product";

            if (type == ReportViewerType.GoodsSoldByCustomer)
                return "Goods Sold By Customer";

            if (type == ReportViewerType.GoodsReturnedByCustomer)
                return "Goods Returned By Customer";

            return "";
        }

        private string GetReportPath(ReportViewerType type)
        {
            if (type == ReportViewerType.PurchaseOrder)
                return "/Report/PurchaseOrder/PurchaseOrderReport.aspx";

            if (type == ReportViewerType.PurchaseBill)
                return "/Report/PurchaseBill/PurchaseBillReport.aspx";

            if (type == ReportViewerType.PurchaseBillOverdue)
                return "/Report/PurchaseBill/PurchaseBillOverdueReport.aspx";

            if (type == ReportViewerType.PurchaseBillAging)
                return "/Report/PurchaseBill/PurchaseBillAgingReport.aspx";

            if (type == ReportViewerType.SalesOrder)
                return "/Report/SalesOrder/SalesOrderReport.aspx";

            if (type == ReportViewerType.SalesOrderDetail)
                return "/Report/SalesOrder/SalesOrderDetailReport.aspx";

            if (type == ReportViewerType.SalesReport)
                return "/Report/Invoice/SalesReport.aspx";

            if (type == ReportViewerType.DeliveryOrder)
                return "/Report/DeliveryOrder/DeliveryOrderReport.aspx";

            if (type == ReportViewerType.FulfillmentByItemDetail)
                return "/Report/DeliveryOrder/FulfillmentByItemDetail.aspx";

            if (type == ReportViewerType.FulfillmentByItemSummary)
                return "/Report/DeliveryOrder/FulfillmentByItemSummary.aspx";

            if (type == ReportViewerType.Invoice)
                return "/Report/Invoice/InvoiceReport.aspx";

            if (type == ReportViewerType.InvoiceAging)
                return "/Report/Invoice/InvoiceAgingReport.aspx";

            if (type == ReportViewerType.Payment)
                return "/Report/Payment/PaymentReport.aspx";

            if (type == ReportViewerType.GeneralJournal)
                return "/Report/Accounting/GeneralJournal.aspx";

            if (type == ReportViewerType.AccountTransaction)
                return "/Report/Accounting/AccountTransaction.aspx";

            if (type == ReportViewerType.TrialBalance)
                return "/Report/Accounting/TrialBalance.aspx";

            if (type == ReportViewerType.ProfitAndLoss)
                return "/Report/Accounting/ProfitAndLoss.aspx";

            if (type == ReportViewerType.IncomeExpense)
                return "/Report/Accounting/IncomeExpense.aspx";

            if (type == ReportViewerType.Attendance)
                return "/Report/Attendance/AttendanceReport.aspx";

            if (type == ReportViewerType.AttendanceDetail)
                return "/Report/Attendance/AttendanceDetailReport.aspx";

            if (type == ReportViewerType.SalesOrderBySalesRepSumNoCustomer)
                return "/Report/Invoice/SalesOrderBySalesRepSumNoCustomer.aspx";

            if (type == ReportViewerType.InvoiceBySalesRepSum)
                return "/Report/Invoice/InvoiceBySalesRepSum.aspx";

            if (type == ReportViewerType.InvoiceBySalesRepDetail)
                return "/Report/Invoice/InvoiceBySalesRepDetail.aspx";

            if (type == ReportViewerType.InvoiceByItemSum)
                return "/Report/Invoice/InvoiceByItemSum.aspx";

            if (type == ReportViewerType.ReportOverdue)
                return "/Report/Invoice/ReportOverdue.aspx";

            if (type == ReportViewerType.ReportBonus)
                return "/Report/Invoice/ReportBonus.aspx";

            if (type == ReportViewerType.InvoiceMonthly)
                return "/Report/Invoice/InvoiceMonthly.aspx";

            if (type == ReportViewerType.InvoiceMonthlySummary)
                return "/Report/Invoice/InvoiceMonthlySummary.aspx";

            if (type == ReportViewerType.InvoicePaymentSummary)
                return "/Report/Invoice/InvoicePaymentSummary.aspx";

            if (type == ReportViewerType.InvoicePaymentDetail)
                return "/Report/Invoice/InvoicePaymentDetail.aspx";

            if (type == ReportViewerType.PaymentInvoice)
                return "/Report/Payment/PaymentInvoiceReport.aspx";

            if (type == ReportViewerType.PaymentBill)
                return "/Report/Payment/PaymentBillReport.aspx";

            if (type == ReportViewerType.Product)
                return "/Report/Product/ProductReport.aspx";

            if (type == ReportViewerType.CreditMemo)
                return "/Report/CreditMemo/CreditMemoByItemReport.aspx";

            if (type == ReportViewerType.InvoiceCreditMemo)
                return "/Report/Invoice/InvoiceCreditMemo.aspx";

            if (type == ReportViewerType.Stock)
                return "/Report/Stock/StockReport.aspx";

            if (type == ReportViewerType.TransactionStock)
                return "/Report/Stock/TransactionStockReport.aspx";

            if (type == ReportViewerType.GoodsSoldByProduct)
                return "/Report/GoodsSoldByProduct/GoodsSoldByProduct.aspx";

            if (type == ReportViewerType.GoodsSoldByCustomer)
                return "/Report/GoodsSoldByCustomer/GoodsSoldByCustomer.aspx";

            if (type == ReportViewerType.GoodsReturnedByCustomer)
                return "/Report/GoodsReturnedByCustomer/GoodsReturnedByCustomer.aspx";

            if (type == ReportViewerType.AssemblyBuild)
                return "/Report/AssemblyBuild/ReportHpp.aspx";

            return "";
        }

        private string GetPrintOutTitle(PrintOutType type)
        {
            if (type == PrintOutType.SalesOrder)
                return "Sales Order";

            if (type == PrintOutType.BookingSales)
                return "Booking Sales";

            if (type == PrintOutType.PickingTicket)
                return "Picking Ticket";

            if (type == PrintOutType.TransferPickingTicket)
                return "Picking Ticket";

            if (type == PrintOutType.DeliveryOrder)
                return "Item Fulfillment";

            if (type == PrintOutType.TransferDelivery)
                return "Transfer Delivery";

            if (type == PrintOutType.Invoice)
                return "Invoice";

            if (type == PrintOutType.TaxInvoice)
                return "Faktur Pajak";

            if (type == PrintOutType.MultipleInvoicing)
                return "Faktur Pajak";

            if (type == PrintOutType.Allowance)
                return "Slip Gaji";

            if (type == PrintOutType.IncomeBank)
                return "Bukti Pemasukan";

            if (type == PrintOutType.ExpenseBank)
                return "Bukti Pengeluaran";

            if (type == PrintOutType.CashInBank)
                return "Kas Masuk";

            if (type == PrintOutType.PurchaseOrder)
                return "Purchase Order";

            if (type == PrintOutType.VendorReturn)
                return "Vendor Return";

            if (type == PrintOutType.WorkOrder)
                return "Bill Of Material";

            return "";
        }

        private string GetPrintOutPath(PrintOutType type)
        {
            if (type == PrintOutType.SalesOrder)
                return "/PrintOut/SalesOrder/SalesOrderPrintOut.aspx";

            if (type == PrintOutType.BookingSales)
                return "/PrintOut/BookingSales/BookingSalesPrintOut.aspx";

            if (type == PrintOutType.PickingTicket)
                return "/PrintOut/SalesOrder/PickingTicketPrintOut.aspx";

            if (type == PrintOutType.TransferPickingTicket)
                return "/PrintOut/TransferOrder/PickingTicketPrintOut.aspx";

            if (type == PrintOutType.DeliveryOrder)
                return "/PrintOut/DeliveryOrder/DeliveryOrderPrintOut.aspx";

            if (type == PrintOutType.TransferDelivery)
                return "/PrintOut/TransferDelivery/TransferDeliveryPrintOut.aspx";

            if (type == PrintOutType.Invoice)
                return "/PrintOut/Invoice/InvoicePrintout.aspx";

            if (type == PrintOutType.TaxInvoice)
                return "/PrintOut/TaxInvoice/TaxinvoiceExcelPrintOut.aspx";

            if (type == PrintOutType.MultipleInvoicing)
                return "/PrintOut/MultipleInvoicing/MultipleInvoicingPrintOut.aspx";

            if (type == PrintOutType.Allowance)
                return "/PrintOut/Allowance/AllowancePrintout.aspx";

            if (type == PrintOutType.IncomeBank)
                return "/PrintOut/IncomeExpense/IncomeExpensePrintout.aspx";

            if (type == PrintOutType.ExpenseBank)
                return "/PrintOut/IncomeExpense/IncomeExpensePrintout.aspx";

            if (type == PrintOutType.CashInBank)
                return "/PrintOut/IncomeExpense/IncomeExpensePrintout.aspx";

            if (type == PrintOutType.CashOutBank)
                return "/PrintOut/IncomeExpense/IncomeExpensePrintout.aspx";

            if (type == PrintOutType.PurchaseOrder)
                return "/PrintOut/PurchaseOrder/PurchaseOrderPrintOut.aspx";

            if (type == PrintOutType.VendorReturn)
                return "/PrintOut/VendorReturn/VendorReturnPrintOut.aspx";

            if (type == PrintOutType.WorkOrder)
                return "/PrintOut/WorkOrder/WorkOrderPrintOut.aspx";

            if (type == PrintOutType.WorkOrderLetter)
                return "/PrintOut/WorkOrder/WorkOrderPrintOutLetter.aspx";

            if (type == PrintOutType.WorkOrderStruk)
                return "/PrintOut/WorkOrder/WorkOrderPrintOutStruk.aspx";

            return "";
        }

        public ActionResult Index(ReportViewerType type, string queryString)
        {
            ViewBag.Title = GetReportTitle(type, queryString);

            var reportPath = GetReportPath(type);

            if (!string.IsNullOrEmpty(queryString))
            {
                reportPath += "?" + queryString;
            }

            ViewBag.ReportPath = reportPath;

            return View();
        }

        public ActionResult PopUp(PrintOutType type, string queryString)
        {
            ViewBag.Title = GetPrintOutTitle(type);

            var reportPath = GetPrintOutPath(type);

            if (!string.IsNullOrEmpty(queryString))
            {
                reportPath += "?" + queryString;
            }

            ViewBag.ReportPath = reportPath;

            return View();
        }

        public enum PrintOutType
        {
            SalesOrder,
            PickingTicket,
            CashSalesPickingTicket,
            CashSalesInvoice,
            TransferPickingTicket,
            DeliveryOrder,
            TransferDelivery,
            Invoice,
            TaxInvoice,
            MultipleInvoicing,
            Payment,
            Allowance,
            IncomeExpense,
            IncomeBank,
            ExpenseBank,
            CashInBank,
            CashOutBank,
            PurchaseOrder,
            WorkOrder,
            WorkOrderLetter,
            WorkOrderStruk,
            BookingOrder,
            BookingSales,
            VendorReturn,
        }

        public enum ReportViewerType
        {
            PurchaseOrder,
            PurchaseBill,
            PurchaseBillOverdue,
            PurchaseBillAging,
            SalesOrder,
            SalesOrderDetail,
            SalesReport,
            DeliveryOrder,
            FulfillmentByItemDetail,
            FulfillmentByItemSummary,
            Invoice,
            InvoiceAging,
            Payment,
            GeneralJournal,
            AccountTransaction,
            TrialBalance,
            ProfitAndLoss,
            IncomeExpense,
            Attendance,
            AttendanceDetail,
            IncomeBank,
            ExpenseBank,
            InvoiceBySalesRepSum,
            InvoiceBySalesRepDetail,
            InvoicePaymentSummary,
            InvoicePaymentDetail,
            SalesOrderBySalesRepSumNoCustomer,
            InvoiceByItemSum,
            PaymentInvoice,
            PaymentBill,
            ReportOverdue,
            ReportBonus,
            InvoiceMonthly,
            InvoiceMonthlySummary,
            Product,
            CreditMemo,
            InvoiceCreditMemo,
            Stock,
            TransactionStock,
            AssemblyBuild,
            GoodsSoldByProduct,
            GoodsSoldByCustomer,
            GoodsReturnedByCustomer
        }
    }
}
