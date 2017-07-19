using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class PrefixSettingModel
    {
        public int ID { get; set; }
        public string ProductPrefix { get; set; }
        public string CustomerPrefix { get; set; }
        public string SupplierPrefix { get; set; }
        public string VendorPrefix { get; set; }
        public string SalesmanPrefix { get; set; }
        public string DepartmentPrefix { get; set; }
        public string WarehousePrefix { get; set; }
        public string AdjustmentPrefix { get; set; }
        public string QuotationPrefix { get; set; }
        public string DeliveryOrderPrefix { get; set; }
        public string SalesOrderPrefix { get; set; }
        public string CashSalesPrefix { get; set; }
        public string PurchasePrefix { get; set; }
        public string InvoicePrefix { get; set; }
        public string VendorInvoicePrefix { get; set; }
        public string IncomePrefix { get; set; }
        public string ExpensePrefix { get; set; }
        public string AccountPrefix { get; set; }
        public string PaymentPrefix { get; set; }
        public string AccountingPrefix { get; set; }
        public string AllowancePrefix { get; set; }
        public string AttendancePrefix { get; set; }
        public string IncomeBankPrefix { get; set; }
        public string ExpenseBankPrefix { get; set; }
        public string CashInPrefix { get; set; }
        public string CashOutPrefix { get; set; }
        public string PurchaseDeliveryPrefix { get; set; }
        public string PurchaseBillPrefix { get; set; }
        public string PurchasePaymentPrefix { get; set; }
        public string PurchaseReturnPrefix { get; set; }
        public string TransferPrefix { get; set; }
        public string TransferDeliveryPrefix { get; set; }
        public string TransferReceiptPrefix { get; set; }
        public string MultipleInvoicingPrefix { get; set; }
        public string MakeMultiPayPrefix { get; set; }
        public string MakeMultiPaySalesPrefix { get; set; }
        public string VendorReturnPrefix { get; set; }
        public string VendorReturnDeliveryPrefix { get; set; }
        public string VendorReturnCreditPrefix { get; set; }
        public string CustomerReturnPrefix { get; set; }
        public string CustomerReturnReceiptPrefix { get; set; }
        public string CustomerReturnCreditPrefix { get; set; }
        public string BookingPrefix { get; set; }
        public string BookingSalesPrefix { get; set; }
        public string WorkOrderPrefix { get; set; }
        public string AssemblyBuildPrefix { get; set; }
        public string AssemblyUnBuildPrefix { get; set; }

    }
}
