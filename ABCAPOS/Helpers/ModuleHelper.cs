using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ABCAPOS.Util;

namespace ABCAPOS.Helpers
{
    public class ModuleHelper
    {
        public static Dictionary<string, string> ModuleList()
        {
            Dictionary<string, string> moduleList = new Dictionary<string, string>();
            //moduleList["Nama modul di controller"] = "Nama modul di viewx";

            moduleList["Attendance"] = "Absensi";
            moduleList["Account"] = "Akun";
            moduleList["ApplyCreditMemo"] = "Apply Credit Memo";
            moduleList["ApplyBillCredit"] = "Apply Bill Credit";
            //moduleList["AssemblyBuild"] = "Assembly Build";

            if ((MembershipHelper.GetRoleID() == (int)PermissionStatus.root))
            {
                moduleList["AssemblyBuild"] = "Assembly Build";
                moduleList["AssemblyUnBuild"] = "Assembly UnBuild";
            }

            moduleList["AveragePaymentOverDue"] = "Average Payment OverDue";

            moduleList["PPh21Expense"] = "Beban PPh Pasal 21";
            moduleList["BillCredit"] = "Bill Credit";
            moduleList["MakeMultiPay"] = "Bill Payment";
            moduleList["BinNumber"] = "Bin Number";
            moduleList["BookingOrder"] = "Booking Order";
            moduleList["BookingSales"] = "Booking Sales";
            moduleList["IncomeExpense"] = "Bukti dan Kas";

            moduleList["CashSales"] = "Cash Sales";
            moduleList["CreditMemo"] = "Credit Memo";
            moduleList["CustomerReturn"] = "Customer Return";
            moduleList["ReturnReceipt"] = "Customer Return Receipt";

            moduleList["AccountConfiguration"] = "Daftar Transaksi";
            moduleList["Dashboard"] = "Dashboard";
            moduleList["DashboardNotification"] = "Dashboard Notification";
            moduleList["Department"] = "Department";

            moduleList["Expedition"] = "Expedition";

            moduleList["Allowance"] = "Gaji";
            moduleList["Warehouse"] = "Gudang";

            moduleList["CompanySetting"] = "Info Perusahaan";
            moduleList["Invoice"] = "Invoice";
            moduleList["InventoryAdjustment"] = "Inventory Adjustment";
            moduleList["MakeMultiPaySales"] = "Invoice Payment";
            moduleList["DeliveryOrder"] = "Item Fulfillment";
            moduleList["PurchaseDelivery"] = "Item Receipt";

            moduleList["AccountCategory"] = "Kategori Akun";

            moduleList["Currency"] = "Kurs";

            //moduleList["MonthlyReport"] = "Laporan Bulanan";
            moduleList["AccountingReport"] = "Laporan Pembukuan";
           
            moduleList["PaymentMethod"] = "Metode Pembayaran";

            moduleList["AnnualVAT"] = "Pajak Penghasilan";
            moduleList["Payment"] = "Payment";
            moduleList["Customer"] = "Pelanggan";
            moduleList["Accounting"] = "Pembukuan";
            moduleList["PrefixSetting"] = "Pengaturan Prefix";
            moduleList["Role"] = "Peran User";
            moduleList["Product"] = "Produk";
            moduleList["PurchaseBill"] = "Purchase Bill";
            moduleList["PurchaseOrder"] = "Purchase Order";
            moduleList["PurchaseReturn"] = "Purchase Return";

            moduleList["Resi"] = "Resi";
            moduleList["ResiBill"] = "Resi Bill";
            moduleList["ResiPayment"] = "Resi Payment";

            moduleList["Salesman"] = "Marketing";
            moduleList["SalesOrder"] = "Sales Order";
            moduleList["Staff"] = "Staff";
            moduleList["StockMovement"] = "Stock Movement";

            moduleList["TransferOrder"] = "Transfer Order";
            moduleList["TransferDelivery"] = "Transfer Delivery";
            moduleList["TransferReceipt"] = "Transfer Receipt";

            moduleList["Unit"] = "Unit of Measurement";
            moduleList["User"] = "User";
            moduleList["TermsOfPayment"] = "Terms Of Payment";
         

            moduleList["Vendor"] = "Vendor";
            moduleList["VendorReturn"] = "Vendor Return";
            moduleList["VendorReturnDelivery"] = "Vendor Return Delivery";

            //moduleList["WorkOrder"] = "Work Order";

            if ((MembershipHelper.GetRoleID() == (int)PermissionStatus.root))
                moduleList["WorkOrder"] = "Work Order";

            //for report
            moduleList["SalesOrderReport"] = "Sales Order Report";
            moduleList["PurchaseOrderReport"] = "Purchase Order Report";
            moduleList["PurchaseBillReport"] = "Purchase Bill Report";
            moduleList["ItemFulfillmentReport"] = "Item Fulfillment Report";
            moduleList["InvoiceReport"] = "Invoice Report";
            moduleList["PaymentReport"] = "Payment Report";
            moduleList["ReportOverdue"] = "Report Overdue";
            moduleList["ReportStock"] = "Report Stock";
            moduleList["CreditMemoByItemReport"] = "Credit Memo By Item Report";
            moduleList["ManufactureReport"] = "Manufacture Report";
           

            return moduleList;
        }
    }
}