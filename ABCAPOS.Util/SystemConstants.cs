using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Util
{
    public class SystemConstants
    {
        public static int AutoCompleteItemCount = 50;
        public static int ItemPerPage = 20;

        public static string ReportXMLFolder;
        public static string TempReportFolder;

        #region Account
        public static long SalesNonRetailAccount = 63;
        public static long TaxExpenseAccount = 65;
        public static long TaxIncomeAccount = 32;
        public static long IncomeAccount = 28;
        public static long SalesDiscountAccount = 79;
        public static long PurchaseDiscountAccount = 80;
        public static long PurchaseAccount = 27;
        public static long OutcomeAccount = 24;
        public static long SelisihKursAccount = 81;
        public static long CostDeliveryAccount = 16;
        public static long CashAccount = 3;

        public static string SalesNonRetailAccountUserCode = "41200";
        public static string SalesRetailAccountUserCode = "41100";

        public static string PurchaseAccountUserCode = "51100";

        public static string SalaryAccountUserCode = "61100";
        public static string OfficeOvertimeAccountUserCode = "61410";
        public static string GeneralOvertimeAccountUserCode = "61420";
        public static string TransportationAccountUserCode = "61600";
        public static string CommunicationAccountUserCode = "61800";
        public static string PhoneAccountUserCode = "62040";
        public static string InternetAccountUserCode = "62050";
        public static string InstallationAccountUserCode = "62060";
        public static string OfficeSuppliesExpenseAccountUserCode = "62070";
        public static string ElectricAccountUserCode = "62080";
        public static string MealAllowanceAccountUserCode = "61500";
        public static string OperationalAccountUserCode = "62130";
        public static string DeliveryAccountUserCode = "62180";
        public static string MedicalAccountUserCode = "62090";
        public static string VehicleMaintenanceAccountUserCode = "62110";
        public static string OfficeMaintenanceAccountUserCode = "62140";
        public static string SalesCommissionAccountUserCode = "62150";
        public static string OfficeSuppliesMaintenanceAccountUserCode = "62170";
        public static string SecurityAccountUserCode = "62190";
        public static string OfficeSuppliesDepreciationAccountUserCode = "63100";
        public static string MachineDepreciationAccountUserCode = "63200";
        public static string VehicleDepreciationAccountUserCode = "63300";
        public static string BuildingDepreciationAccountUserCode = "63400";

        public static string BankAdministrationAccountUserCode = "81000";
        public static string OtherExAccountUserCode = "61101";
        public static string GasolineAccountUserCode = "61601";
        public static string TollRoadAccountUserCode = "61602";
        public static string interestAccountUserCode = "81001";
        public static string PPh21AccountUserCode = "83100";
        public static string PPh25AccountUserCode = "83200";
        public static string ModalUsahaAccountUserCode = "11100";

        public static string CashAccountUserCode = "11200";
        public static string BankAccountUserCode = "11300";
        public static string TravellingDPAccountUserCode = "11600";
        public static string IncomeAccountUserCode = "11700";
        public static string OtherIncomeAccountUserCode = "11900";
        public static string TaxIncomeAccountUserCode = "11920";

        public static string OfficeSuppliesAccountUserCode = "12100";
        public static string OfficeSuppliesAccumulationAccountUserCode = "12110";
        public static string MachineAccountUserCode = "12200";
        public static string MachineAccumulationAccountUserCode = "12210";
        public static string VehicleAccountUserCode = "12300";
        public static string VehicleAccumulationAccountUserCode = "12310";
        public static string BuildingAccountUserCode = "12400";
        public static string BuildingAccumulationAccountUserCode = "12410";

        public static string ExpenseAccountUserCode = "21100";
        public static string ExpensePPh21AccountUserCode = "21500";
        public static string TaxExpenseAccountUserCode = "21600";
        public static string DirectorAccountUserCode = "21101";

        public static string AssetAccountUserCode = "31100";
        public static string HoldProfitAccountUserCode = "31300";
        public static string ContinuousProfitAccountUserCode = "";
        public static string OtherEquityAccountUserCode = "31500";

        public static string SalesDiscountUserCode = "41101";
        public static string PurchaseDiscountUserCode = "51101";
        #endregion

        #region Attendance
        public static decimal MealAllowanceCourier = 12000;
        public static decimal MealAllowanceOperator = 12000;
        public static decimal OvertimeCourier = 7000;
        public static decimal OvertimeOperator = 7000;
        public static decimal LatePenaltyCourier = 20000;
        public static decimal LatePenaltyOperator = 7000;
        public static decimal AlphaPenaltyCourier = 50000;
        public static decimal AlphaPenaltyCourierOnSaturday = 100000;
        public static decimal AlphaPenaltyOperator = 50000;
        public static string OffDutyCourier = "17:00";
        public static string OffDutyOperator = "17:00";
        public static string OffDutyCourierOnSaturday = "15:00";
        public static string OffDutyOperatorOnSaturday = "15:00";
        #endregion

        public static DateTime UnsetDateTime = new DateTime(1900, 1, 1);

        public const string str_AllowanceID = "AllowanceID";
        public const string str_DeliveryOrderID = "DeliveryOrderID";
        public const string str_InvoiceID = "InvoiceID";
        public const string str_MultipleInvoicingID = "MultipleInvoicingID";
        public const string str_MultipleInvoicingTaxID = "MultipleInvoicingTaxID";
        public const string str_SalesOrderID = "SalesOrderID";
        public const string str_CashSalesID = "CashSalesID";
        public const string str_CreditMemoID = "CreditMemoID";
        public const string str_TransferOrderID = "TransferOrderID";
        public const string str_TransferDeliveryID = "TransferDeliveryID";
        public const string str_IncomeExpenseID = "IncomeExpenseID";
        public const string str_PurchaseOrderID = "PurchaseOrderID";
        public const string str_BookingOrderID = "BookingOrderID";
        public const string str_BookingSalesID = "BookingSalesID";
        public const string str_VendorReturnID = "VendorReturnID";
        public const string str_WorkOrderID = "WorkOrderID";

        public const string autoGenerated = "Auto-Generated";

        public const string str_dateFormat = "MM/dd/yyyy";

        public const string str_permission_Create = "Create";
        public const string str_permission_Edit = "Edit";
        public const string str_permission_View = "View";
        public const string str_permission_Void = "Void";
        public const string str_notif_success = "Document Saved";
    }

    public enum PostingAccount
    {
        PersediaanBahanBaku = 318,
        PersediaanBahanPembantu = 319,
        PersediaanDalamPerjalanan = 322,
        PersediaanBarangJadi = 321,
        CadanganBiayaLainnya = 368,
        KasBesar = 298,
        UangMukaPPN = 333,
        HutangDagangPPN = 381,
        HutangDagang = 372,
        HutangDagangYangBelumDifakturkan = 387,
        PiutangDagang = 314,
        PenjualanTunai = 398,
        PenjualanKredit = 399,
        HargaPokokPenjualan = 402,
        //HPPBarangJadi = 503,
        UndepositedFund = 117,
        ReturPenjualan = 400
    }

    public enum GenericStatus { None = 0, Partial = 1, Full = 2 }
    public enum ItemTypeProduct { RawMaterial = 1, Supporting = 2, BarangSetengahJadi = 3, FinishGood = 4, NonInventory = 5 }
    public enum AccountType { Debit = 1, Credit = 2 }
    public enum AccountGroup { IncomeExpense = 1, ProfitAndLoss = 2 }
    public enum AccountingResultDocumentType { Accounting = 1, PurchaseDelivery = 2, PurchaseBill = 3, MakeMultiPay = 4, VendorReturnDelivery = 5, BillCredit = 6, DeliveryOrder = 7, Invoice = 8, InvoicePayment = 9, CashSales = 10, ReturnReceipt = 11, CreditMemo = 12, Assembly = 13, Allowance = 3, Attendance = 4, PPh21 = 5, PurchaseOrder = 6, PurchasePayment = 7, Payment = 8 }
    public enum AccountingResultType { Debit = 1, Credit = 2 }

    public enum CustomerCategory { Individual = 1, Agent = 2, Industry = 3 }

    public enum PaymentStatus { All = 0, Unpaid = 1, Paid = 2 }

    public enum Currency { IDR = 1, USD = 2 }
    public enum CustomerType { NonRetail = 1, Retail = 2 }

    public enum DeliveryOrderStatus { New = 1, Packed = 2, Shipped = 3, Void = 0 }
    public enum DocType { PurchaseDelivery = 1, DeliveryOrder = 2, InventoryAdjustment = 3, TransferDelivery = 4, TransferReceipts = 5, AssemblyBuild = 6, VendorReturnDelivery = 7, ReturnReceipt = 8, Void = 0, CashSales = 10 , AssemblyUnBuild = 11}

    public enum EditMode { Create = 1, Update = 2 }

    public enum InvoiceStatus { New = 1, Approved = 3, Paid = 4, Void = 0 }

    public enum PaymentMethodType { Cash = 1, Credit = 2 }
    public enum PermissionStatus { production = 23, root = 25, AdminProduksi = 24 }
    public enum ProductCategory { Product = 1, Service = 2, NonStock = 3 }

    public enum PurchaseDeliveryStatus { New = 1, Fully = 2, Void = 0 }
    public enum PurchaseBillStatus { New = 1, Paid = 4, Void = 0 }
    public enum ResiBillStatus { New = 1, PendingPayment = 3, Void = 0, PartialyPayment = 4 ,FullyPayment = 5 }
    public enum ResiStatus { New = 1, PendingBilling = 3, PartialyBilling = 4, FullBilling = 5,Void = 0 }

    //public enum SalesOrderStatus { New = 1, Approved = 3, PartialDO = 4, DO = 5, PartialInv = 6, Inv = 7,  PartialPaid = 8, Paid = 9, Void = 0 }
    public enum SOPriceLevel { D1 = 9, D2 = 10, D3 = 11, D4 = 12, D5 = 13, D6 = 14 }

    public enum PurchasePaymentStatus { New = 1, Approved = 3, Void = 0 }
    public enum PurchaseReturnStatus { New = 1, Approved = 3, Void = 0 }
    public enum PurchaseReturnPosition { Returned = 1, Recycled = 2 }

    public enum SPKStatus { New = 1, Closed = 2, Void = 0 }
    public enum StaffType { Courier = 1, Operator = 2, Others = 3, Owner = 4 }

    public enum TaxType { NonTax = 1, PPN = 2 }
    public enum TransferOrderStatus { New = 1, PendingFulfillment = 3, PendingReceipt = 4, Received = 5, Void = 0 }

    public enum IncomeExpenseStatus { New = 1, Approved = 3, Void = 0 }
    public enum IncomeExpenseCategory { Expense = 1, Income = 2, CashIn = 3, CashOut = 4 }

    public enum WorkOrderStatus { New = 1, PartialyBuild = 4, FullyBuild = 5, Void = 0, Approved = 3 };

    public enum AssemblyBuildStatus { New = 1, Fully = 3, Void = 0 }

    public enum TransactionType { PurchaseDelivery = 1, DeliveryOrder = 2, InventoryAdjustment = 4, TransferDelivery = 7, TransferReceipts = 8, AssemblyBuild = 3, VendorReturnDelivery = 9, ReturnReceipt = 6, CashSales = 5, AssemblyUnBuild = 10, ALL = 0 }
}
