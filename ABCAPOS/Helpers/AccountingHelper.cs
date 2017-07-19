using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ABCAPOS.BF;
using ABCAPOS.ReportEDS;
using ABCAPOS.Models;
using ABCAPOS.Util;
using ABCAPOS.DA;

namespace ABCAPOS.Helpers
{
    public class AccountingHelper
    {
        #region Profit And Loss
        private ABCAPOSReportEDSC.ProfitLossDTRow GetRow(List<TrialBalanceReportModel> balanceList, string groupID, string groupDescription, string itemDescription, string accountUserCode)
        {
            var row = new ABCAPOSReportEDSC.ProfitLossDTDataTable().NewProfitLossDTRow();
            row.GroupID = groupID;
            row.GroupDescription = groupDescription;
            row.ItemDescription = itemDescription;

            if (!string.IsNullOrEmpty(accountUserCode))
            {
                var balance = (from i in balanceList
                               where i.AccountUserCode == accountUserCode
                               select i).FirstOrDefault();

                if (balance != null)
                {
                    if (balance.DebitBalance > 0)
                        row.Amount = balance.DebitBalance;
                    else
                        row.Amount = balance.CreditBalance;
                }
                else
                    row.Amount = 0;
            }
            else
                row.Amount = 0;

            return row;
        }

        public ABCAPOSReportEDSC.ProfitLossDTDataTable GetSalesDataTable(List<TrialBalanceReportModel> balanceList)
        {
            var salesDT = new ABCAPOSReportEDSC.ProfitLossDTDataTable();

            var salesRetailDR = GetRow(balanceList, "Sales", "Penjualan", "Penjualan Retail", SystemConstants.SalesRetailAccountUserCode);
            salesDT.LoadDataRow(salesRetailDR.ItemArray, false);

            var salesNonRetailDR = GetRow(balanceList, "Sales", "Penjualan", "Penjualan Non Retail", SystemConstants.SalesNonRetailAccountUserCode);
            salesDT.LoadDataRow(salesNonRetailDR.ItemArray, false);

            var salesReturnDR = GetRow(balanceList, "Sales", "Penjualan", "Retur Penjualan", "");
            salesDT.LoadDataRow(salesReturnDR.ItemArray, false);

            var salesDiscountDR = GetRow(balanceList, "Sales", "Penjualan", "Diskon Penjualan", "");
            salesDT.LoadDataRow(salesDiscountDR.ItemArray, false);

            return salesDT;
        }

        public ABCAPOSReportEDSC.ProfitLossDTDataTable GetModalDataTable(List<TrialBalanceReportModel> balanceList)
        {
            var modalDT = new ABCAPOSReportEDSC.ProfitLossDTDataTable();

            var purchaseDR = GetRow(balanceList, "Modal", "Harga Pokok Penjualan", "Pembelian", SystemConstants.PurchaseAccountUserCode);
            modalDT.LoadDataRow(purchaseDR.ItemArray, false);

            var oldPurchaseDR = GetRow(balanceList, "Modal", "Harga Pokok Penjualan", "Pembelian Lama", "");
            modalDT.LoadDataRow(oldPurchaseDR.ItemArray, false);

            var purchaseReturnDR = GetRow(balanceList, "Modal", "Harga Pokok Penjualan", "Retur Pembelian", "");
            modalDT.LoadDataRow(purchaseReturnDR.ItemArray, false);

            var purchaseDiscountDR = GetRow(balanceList, "Modal", "Harga Pokok Penjualan", "Diskon Pembelian", "");
            modalDT.LoadDataRow(purchaseDiscountDR.ItemArray, false);

            return modalDT;
        }

        public ABCAPOSReportEDSC.ProfitLossDTDataTable GetBusinessDataTable(List<TrialBalanceReportModel> balanceList)
        {
            var businessDT = new ABCAPOSReportEDSC.ProfitLossDTDataTable();

            var salaryDR = GetRow(balanceList, "Business", "Biaya Usaha", "Beban Gaji", SystemConstants.SalaryAccountUserCode);
            businessDT.LoadDataRow(salaryDR.ItemArray, false);

            var officeOvertimeDR = GetRow(balanceList, "Business", "Biaya Usaha", "Uang Lembur Kantor", SystemConstants.OfficeOvertimeAccountUserCode);
            businessDT.LoadDataRow(officeOvertimeDR.ItemArray, false);

            var generalOvertimeDR = GetRow(balanceList, "Business", "Biaya Usaha", "Uang Lembur Borongan", SystemConstants.GeneralOvertimeAccountUserCode);
            businessDT.LoadDataRow(generalOvertimeDR.ItemArray, false);

            var transportationDR = GetRow(balanceList, "Business", "Biaya Usaha", "Beban Transportasi", SystemConstants.TransportationAccountUserCode);
            businessDT.LoadDataRow(transportationDR.ItemArray, false);

            var communicationDR = GetRow(balanceList, "Business", "Biaya Usaha", "Beban Komunikasi", SystemConstants.CommunicationAccountUserCode);
            businessDT.LoadDataRow(communicationDR.ItemArray, false);

            var phoneDR = GetRow(balanceList, "Business", "Biaya Usaha", "Beban Telepon Kantor", SystemConstants.PhoneAccountUserCode);
            businessDT.LoadDataRow(phoneDR.ItemArray, false);

            var internetDR = GetRow(balanceList, "Business", "Biaya Usaha", "Beban Internet", SystemConstants.InternetAccountUserCode);
            businessDT.LoadDataRow(internetDR.ItemArray, false);

            var installationDR = GetRow(balanceList, "Business", "Biaya Usaha", "Beban Pemasangan", SystemConstants.InstallationAccountUserCode);
            businessDT.LoadDataRow(installationDR.ItemArray, false);

            var officeSuppliesDR = GetRow(balanceList, "Business", "Biaya Usaha", "Beban Keperluan Kantor", SystemConstants.OfficeSuppliesExpenseAccountUserCode);
            businessDT.LoadDataRow(officeSuppliesDR.ItemArray, false);

            var electricDR = GetRow(balanceList, "Business", "Biaya Usaha", "Beban Air & Listrik", SystemConstants.ElectricAccountUserCode);
            businessDT.LoadDataRow(electricDR.ItemArray, false);

            var mealAllowanceDR = GetRow(balanceList, "Business", "Biaya Usaha", "Uang Makan", SystemConstants.MealAllowanceAccountUserCode);
            businessDT.LoadDataRow(mealAllowanceDR.ItemArray, false);

            var operationalDR = GetRow(balanceList, "Business", "Biaya Usaha", "Beban Operasional", SystemConstants.OperationalAccountUserCode);
            businessDT.LoadDataRow(operationalDR.ItemArray, false);

            var deliveryDR = GetRow(balanceList, "Business", "Biaya Usaha", "Beban Pengiriman Barang", SystemConstants.DeliveryAccountUserCode);
            businessDT.LoadDataRow(deliveryDR.ItemArray, false);

            var medicalDR = GetRow(balanceList, "Business", "Biaya Usaha", "Beban Pengobatan Pegawai", SystemConstants.MedicalAccountUserCode);
            businessDT.LoadDataRow(medicalDR.ItemArray, false);

            var vehicleMaintenanceDR = GetRow(balanceList, "Business", "Biaya Usaha", "Beban Perawatan Kendaraan", SystemConstants.VehicleMaintenanceAccountUserCode);
            businessDT.LoadDataRow(vehicleMaintenanceDR.ItemArray, false);

            var officeMaintenanceDR = GetRow(balanceList, "Business", "Biaya Usaha", "Beban Perawatan Kantor", SystemConstants.OfficeMaintenanceAccountUserCode);
            businessDT.LoadDataRow(officeMaintenanceDR.ItemArray, false);

            var salesCommissionDR = GetRow(balanceList, "Business", "Biaya Usaha", "Komisi Penjualan", SystemConstants.SalesCommissionAccountUserCode);
            businessDT.LoadDataRow(salesCommissionDR.ItemArray, false);

            var officeSuppliesMaintenanceDR = GetRow(balanceList, "Business", "Biaya Usaha", "Beban Perawatan Peralatan Kantor", SystemConstants.OfficeSuppliesMaintenanceAccountUserCode);
            businessDT.LoadDataRow(officeSuppliesMaintenanceDR.ItemArray, false);

            var securityDR = GetRow(balanceList, "Business", "Biaya Usaha", "Beban Retribusi dan Keamanan", SystemConstants.SecurityAccountUserCode);
            businessDT.LoadDataRow(securityDR.ItemArray, false);

            var officeSuppliesDepreciationDR = GetRow(balanceList, "Business", "Biaya Usaha", "Beban Depresiasi - Peralatan Kantor", SystemConstants.OfficeSuppliesDepreciationAccountUserCode);
            businessDT.LoadDataRow(officeSuppliesDepreciationDR.ItemArray, false);

            var machineDepreciationDR = GetRow(balanceList, "Business", "Biaya Usaha", "Beban Depresiasi - Mesin", SystemConstants.MachineDepreciationAccountUserCode);
            businessDT.LoadDataRow(machineDepreciationDR.ItemArray, false);

            var vehicleDepreciationDR = GetRow(balanceList, "Business", "Biaya Usaha", "Beban Depresiasi - Kendaraan", SystemConstants.VehicleDepreciationAccountUserCode);
            businessDT.LoadDataRow(vehicleDepreciationDR.ItemArray, false);

            var buildingDepreciationDR = GetRow(balanceList, "Business", "Biaya Usaha", "Beban Depresiasi - Bangunan", SystemConstants.BuildingDepreciationAccountUserCode);
            businessDT.LoadDataRow(buildingDepreciationDR.ItemArray, false);

            return businessDT;
        }

        public ABCAPOSReportEDSC.ProfitLossDTDataTable GetOtherExpenseDataTable(List<TrialBalanceReportModel> balanceList)
        {
            var othersDT = new ABCAPOSReportEDSC.ProfitLossDTDataTable();

            var bankAdministrationDR = GetRow(balanceList, "Others", "Pendapatan / Biaya Lainnya", "Beban Administrasi Bank", SystemConstants.BankAdministrationAccountUserCode);
            othersDT.LoadDataRow(bankAdministrationDR.ItemArray, false);

            var pph21DR = GetRow(balanceList, "Others", "Pendapatan / Biaya Lainnya", "Beban PPh Pasal 21", SystemConstants.PPh21AccountUserCode);
            othersDT.LoadDataRow(pph21DR.ItemArray, false);

            var pph25DR = GetRow(balanceList, "Others", "Pendapatan / Biaya Lainnya", "Beban PPh Pasal 25", SystemConstants.PPh25AccountUserCode);
            othersDT.LoadDataRow(pph25DR.ItemArray, false);

            return othersDT;
        }
        #endregion

        #region Income Expense

        private ABCAPOSReportEDSC.ProfitLossDTRow GetIncomeExpenseRow(List<TrialBalanceReportModel> balanceList, string groupID, string groupDescription, string itemDescription, string accountUserCode)
        {
            var row = new ABCAPOSReportEDSC.ProfitLossDTDataTable().NewProfitLossDTRow();
            row.GroupID = groupID;
            row.GroupDescription = groupDescription;
            row.ItemDescription = itemDescription;

            if (!string.IsNullOrEmpty(accountUserCode))
            {
                var balance = (from i in balanceList
                               where i.AccountUserCode == accountUserCode
                               select i).FirstOrDefault();

                if (balance != null)
                {
                    if (balance.DebitBalance > 0)
                        row.Amount = balance.DebitBalance;
                    else
                    {
                        if (accountUserCode == SystemConstants.CashAccountUserCode ||
                            accountUserCode == SystemConstants.OfficeSuppliesAccumulationAccountUserCode ||
                            accountUserCode == SystemConstants.MachineAccumulationAccountUserCode ||
                            accountUserCode == SystemConstants.VehicleAccumulationAccountUserCode ||
                            accountUserCode == SystemConstants.BuildingAccumulationAccountUserCode)
                            row.Amount = balance.CreditBalance * -1;
                        else
                            row.Amount = balance.CreditBalance;
                    }
                }
                else
                    row.Amount = 0;
            }

            return row;
        }

        public ABCAPOSReportEDSC.ProfitLossDTRow GetContinuousProfitDR(List<TrialBalanceReportModel> balanceList, DateTime startDate)
        {
            var salesDT = GetSalesDataTable(balanceList);
            var modalDT = GetModalDataTable(balanceList);
            var businessDT = GetBusinessDataTable(balanceList);
            var otherExpenseDT = GetOtherExpenseDataTable(balanceList);

            var nettoVAT = salesDT.Sum(p => p.Amount) - modalDT.Sum(p => p.Amount) - businessDT.Sum(p => p.Amount) - otherExpenseDT.Sum(p => p.Amount);

            var row = new ABCAPOSReportEDSC.ProfitLossDTDataTable().NewProfitLossDTRow();
            row.GroupID = "Asset";
            row.GroupDescription = "Modal";
            row.ItemDescription = "Laba (Rugi) Bulan Berjalan";
            row.Amount = nettoVAT;

            return row;
        }

        public ABCAPOSReportEDSC.ProfitLossDTDataTable GetContinuousIncomeDataTable(List<TrialBalanceReportModel> balanceList)
        {
            var continuousIncomeDT = new ABCAPOSReportEDSC.ProfitLossDTDataTable();

            var bankDR = GetIncomeExpenseRow(balanceList, "ContIncome", "Aktiva Lancar", "Bank", SystemConstants.BankAccountUserCode);
            continuousIncomeDT.LoadDataRow(bankDR.ItemArray, false);

            var cashDR = GetIncomeExpenseRow(balanceList, "ContIncome", "Aktiva Lancar", "Kas", SystemConstants.CashAccountUserCode);
            continuousIncomeDT.LoadDataRow(cashDR.ItemArray, false);

            var travellingDR = GetIncomeExpenseRow(balanceList, "ContIncome", "Aktiva Lancar", "Uang Muka Dinas Perjalanan", SystemConstants.TravellingDPAccountUserCode);
            continuousIncomeDT.LoadDataRow(travellingDR.ItemArray, false);

            var incomeDR = GetIncomeExpenseRow(balanceList, "ContIncome", "Aktiva Lancar", "Piutang Usaha", SystemConstants.IncomeAccountUserCode);
            continuousIncomeDT.LoadDataRow(incomeDR.ItemArray, false);

            var otherIncomeDR = GetIncomeExpenseRow(balanceList, "ContIncome", "Aktiva Lancar", "Piutang Lain-lain", SystemConstants.OtherIncomeAccountUserCode);
            continuousIncomeDT.LoadDataRow(otherIncomeDR.ItemArray, false);

            var taxIncomeDR = GetIncomeExpenseRow(balanceList, "ContIncome", "Aktiva Lancar", "PPN Masukan", SystemConstants.TaxIncomeAccountUserCode);
            continuousIncomeDT.LoadDataRow(taxIncomeDR.ItemArray, false);

            return continuousIncomeDT;
        }

        public ABCAPOSReportEDSC.ProfitLossDTDataTable GetStaticIncomeDataTable(List<TrialBalanceReportModel> balanceList)
        {
            var incomeDT = new ABCAPOSReportEDSC.ProfitLossDTDataTable();

            var officeSuppliesDR = GetIncomeExpenseRow(balanceList, "Income", "Aktiva Tetap", "Peralatan Kantor", SystemConstants.OfficeSuppliesAccountUserCode);
            incomeDT.LoadDataRow(officeSuppliesDR.ItemArray, false);

            var officeSuppliesAccumulationDR = GetIncomeExpenseRow(balanceList, "Income", "Aktiva Tetap", "Akumulasi Depresiasi - Peralatan Kantor", SystemConstants.OfficeSuppliesAccumulationAccountUserCode);
            incomeDT.LoadDataRow(officeSuppliesAccumulationDR.ItemArray, false);

            var machineDR = GetIncomeExpenseRow(balanceList, "Income", "Aktiva Tetap", "Mesin", SystemConstants.MachineAccountUserCode);
            incomeDT.LoadDataRow(machineDR.ItemArray, false);

            var machineAccumulationDR = GetIncomeExpenseRow(balanceList, "Income", "Aktiva Tetap", "Akumulasi Depresiasi - Mesin", SystemConstants.MachineAccumulationAccountUserCode);
            incomeDT.LoadDataRow(machineAccumulationDR.ItemArray, false);

            var vehicleDR = GetIncomeExpenseRow(balanceList, "Income", "Aktiva Tetap", "Kendaraan", SystemConstants.VehicleAccountUserCode);
            incomeDT.LoadDataRow(vehicleDR.ItemArray, false);

            var vehicleAccumulationDR = GetIncomeExpenseRow(balanceList, "Income", "Aktiva Tetap", "Akumulasi Depresiasi - Kendaraan", SystemConstants.VehicleAccumulationAccountUserCode);
            incomeDT.LoadDataRow(vehicleAccumulationDR.ItemArray, false);

            var buildingDR = GetIncomeExpenseRow(balanceList, "Income", "Aktiva Tetap", "Bangunan", SystemConstants.BuildingAccountUserCode);
            incomeDT.LoadDataRow(buildingDR.ItemArray, false);

            var buildingAccumulationDR = GetIncomeExpenseRow(balanceList, "Income", "Aktiva Tetap", "Akumulasi Depresiasi - Bangunan", SystemConstants.BuildingAccumulationAccountUserCode);
            incomeDT.LoadDataRow(buildingAccumulationDR.ItemArray, false);

            return incomeDT;
        }

        public ABCAPOSReportEDSC.ProfitLossDTDataTable GetContinuousExpenseDataTable(List<TrialBalanceReportModel> balanceList)
        {
            var continuousExpenseDT = new ABCAPOSReportEDSC.ProfitLossDTDataTable();

            var cashDR = GetIncomeExpenseRow(balanceList, "ContExpense", "Hutang Lancar", "Hutang Dagang", SystemConstants.ExpenseAccountUserCode);
            continuousExpenseDT.LoadDataRow(cashDR.ItemArray, false);

            var bankDR = GetIncomeExpenseRow(balanceList, "ContExpense", "Hutang Lancar", "Hutang PPh Pasal 21", SystemConstants.ExpensePPh21AccountUserCode);
            continuousExpenseDT.LoadDataRow(bankDR.ItemArray, false);

            var travellingDR = GetIncomeExpenseRow(balanceList, "ContExpense", "Hutang Lancar", "PPN Keluaran", SystemConstants.TaxExpenseAccountUserCode);
            continuousExpenseDT.LoadDataRow(travellingDR.ItemArray, false);

            return continuousExpenseDT;
        }

        public ABCAPOSReportEDSC.ProfitLossDTDataTable GetAssetDataTable(List<TrialBalanceReportModel> balanceList, DateTime startDate, DateTime endDate)
        {
            var assetDT = new ABCAPOSReportEDSC.ProfitLossDTDataTable();

            decimal nettoVAT = 0;

            //nettoVAT += CalculateNettoVAT(startDate, endDate);

            //var assetDR = GetIncomeExpenseRow(balanceList, "Asset", "Modal", "Modal Ditempatkan dan Disetor Penuh", SystemConstants.AssetAccountUserCode);
            //assetDT.LoadDataRow(assetDR.ItemArray, false);

            var row = new ABCAPOSReportEDSC.ProfitLossDTDataTable().NewProfitLossDTRow();
            row.GroupID = "Asset";
            row.GroupDescription = "Modal";
            row.ItemDescription = "Laba (Rugi) Ditahan";
            row.Amount = nettoVAT;
            //var holdProfitDR = GetIncomeExpenseRow(balanceList, "Asset", "Modal", "Laba (Rugi) Ditahan", SystemConstants.HoldProfitAccountUserCode);
            assetDT.LoadDataRow(row.ItemArray, false);

            //var continuousProfitDR = GetContinuousProfitDR(balanceList, startDate);
            //assetDT.LoadDataRow(continuousProfitDR.ItemArray, false);

            //var otherEquityDR = GetIncomeExpenseRow(balanceList, "Asset", "Modal", "Ekuitas Lainnya", SystemConstants.OtherEquityAccountUserCode);
            //assetDT.LoadDataRow(otherEquityDR.ItemArray, false);

            return assetDT;
        }

        #endregion

        #region CalculateGrossAmount
        public void CalculateGrossAmountMultiInvDetail(MultipleInvoiceItemModel detail)
        {
            if (detail.GrossAmount == 0)
            {
                detail.TotalAmount = Convert.ToDecimal(Convert.ToDecimal(Convert.ToDecimal(detail.Price) * Convert.ToDecimal(detail.Quantity)).ToString("N2"));
                if (detail.TaxType == 2)
                    detail.TotalPPN = Convert.ToDecimal(Convert.ToDecimal(Convert.ToDouble(detail.TotalAmount) * 0.1).ToString("N2"));
                else
                    detail.TotalPPN = 0;
                detail.GrossAmount = detail.TotalAmount + detail.TotalPPN;
            }
            else
            {
                if (detail.TaxType == 2)
                {
                    detail.TotalAmount = Convert.ToDecimal(Convert.ToDecimal(Convert.ToDouble(detail.GrossAmount) / 1.1).ToString("N2"));
                    detail.TotalPPN = detail.GrossAmount - detail.TotalAmount;
                }
                else
                {
                    detail.TotalAmount = detail.GrossAmount;
                    detail.TotalPPN = 0;
                }
            }
        }

        public void CalculateGrossAmount(ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable invoiceDetailDT)
        {
            foreach (var detail in invoiceDetailDT)
            {
                if (detail.GrossAmount == 0)
                {
                    detail.TotalAmount = Convert.ToDecimal(Convert.ToDecimal(Convert.ToDecimal(detail.Price) * Convert.ToDecimal(detail.Quantity)).ToString("N2"));
                    if (detail.TaxType == 2)
                        detail.TotalPPN = Convert.ToDecimal(Convert.ToDecimal(Convert.ToDouble(detail.TotalAmount) * 0.1).ToString("N2"));
                    else
                        detail.TotalPPN = 0;
                    detail.GrossAmount = detail.TotalAmount + detail.TotalPPN;
                }
                else
                {
                    if (detail.TaxType == 2)
                    {
                        detail.TotalAmount = Convert.ToDecimal(Convert.ToDecimal(Convert.ToDouble(detail.GrossAmount) / 1.1).ToString("N2"));
                        detail.TotalPPN = detail.GrossAmount - detail.TotalAmount;
                    }
                    else
                    {
                        detail.TotalAmount = detail.GrossAmount;
                        detail.TotalPPN = 0;
                    }
                }
            }
        }

        #endregion


    }
}