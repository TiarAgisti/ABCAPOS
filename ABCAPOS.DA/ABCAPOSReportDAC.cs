using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using MPL;
using MPL.Business;
using ABCAPOS.EDM;
using ABCAPOS.ReportEDS;
using ABCAPOS.Models;
using System.Data.SqlClient;

namespace ABCAPOS.DA
{
    public class ABCAPOSReportDAC
    {
        private void ApplyFilter<T>(ref IQueryable<T> query, List<SelectFilter> filters)
        {
            if (filters != null)
            {
                object[] param = null;
                string statements = SelectFilter.Build(filters, out param);

                if (!string.IsNullOrEmpty(statements))
                    query = query.Where(statements, param);
            }
        }

        #region Accounting

        public ABCAPOSReportEDSC.GeneralJournalDTDataTable RetrieveGeneralJournalReport(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Rpt_GeneralJournal
                        select i;

            ApplyFilter<v_Rpt_GeneralJournal>(ref query, selectFilters);

            var dt = new ABCAPOSReportEDSC.GeneralJournalDTDataTable();

            if (query != null)
                ObjectHelper.CopyEnumerableToDataTable(query.AsEnumerable(), dt);

            return dt;
        }

        public ABCAPOSReportEDSC.AccountTransactionDTDataTable RetrieveAccountTransactionReport(DateTime startDate, DateTime endDate, long accountID)
        {
            string spName = "sp_Rpt_AccountTransaction";

            var command = DbEngine.CreateCommand(spName, true) as SqlCommand;
            command.Parameters.AddWithValue("@startDate", startDate);
            command.Parameters.AddWithValue("@endDate", endDate);
            command.Parameters.AddWithValue("@accountID", accountID);

            command.CommandTimeout = 20000;

            var dt = new ABCAPOSReportEDSC.AccountTransactionDTDataTable();

            DbEngine.FillDataTable(dt, command);

            return dt;
        }

        public List<TrialBalanceReportModel> RetrieveTrialBalanceReport(DateTime startDate, DateTime endDate,long accountID)
        {
            string spName = "sp_Rpt_TrialBalance";

            var command = DbEngine.CreateCommand(spName, true) as SqlCommand;
            command.Parameters.AddWithValue("@startDate", startDate);
            command.Parameters.AddWithValue("@endDate", endDate);
            command.Parameters.AddWithValue("@accountID", accountID);

            return DbEngine.FillList<TrialBalanceReportModel>(command);
        }
        #endregion

        #region Allowance
        public ABCAPOSReportEDSC.AllowanceDetailDTDataTable RetrieveAllowanceDetailPrintOut(long allowanceID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_AllowanceDetail
                        where i.AllowanceID == allowanceID
                        select i;

            ABCAPOSReportEDSC.AllowanceDetailDTDataTable dt = new ABCAPOSReportEDSC.AllowanceDetailDTDataTable();

            if (query != null)
                ObjectHelper.CopyEnumerableToDataTable(query.AsEnumerable(), dt);

            return dt;
        }

        #endregion

        #region AssemblyBuild
        public List<v_ReportHpp> RetreiveReportHpp(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query =from i in ent.v_ReportHpp
                select i;

            ApplyFilter<v_ReportHpp>(ref query, selectFilters);
            return query.ToList();
        }
        #endregion

        #region Attendance

        public ABCAPOSReportEDSC.AttendanceDetailDTDataTable RetrieveAttendanceReport(DateTime startDate, DateTime endDate)
        {
            string spName = "sp_Rpt_AttendanceTotal";

            var command = DbEngine.CreateCommand(spName, true) as SqlCommand;
            command.Parameters.AddWithValue("@startDate", startDate);
            command.Parameters.AddWithValue("@endDate", endDate);

            var dt = new ABCAPOSReportEDSC.AttendanceDetailDTDataTable();

            DbEngine.FillDataTable(dt, command);

            return dt;
        }

        public List<v_Rpt_AttendanceDetail> RetrieveAttendanceDetailReport(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Rpt_AttendanceDetail
                        select i;

            ApplyFilter<v_Rpt_AttendanceDetail>(ref query, selectFilters);

            return query.ToList();
        }

        #endregion

        #region Cash Sales

        public ABCAPOSReportEDSC.InvoiceDTRow RetrieveCashSalesPrintOut(long cashSalesID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_CashSales
                        where i.ID == cashSalesID
                        select i;

            ABCAPOSReportEDSC.InvoiceDTRow dr = new ABCAPOSReportEDSC.InvoiceDTDataTable().NewInvoiceDTRow();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), dr);
            else
                return null;

            return dr;
        }

        public ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable RetrieveCashSalesDetailPrintOut(long cashSalesID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_CashSalesDetail
                        where i.CashSalesID == cashSalesID
                        select i;

            ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable dt = new ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable();

            ObjectHelper.CopyEnumerableToDataTable(query.AsEnumerable(), dt);

            return dt;
        }

        #endregion

        #region Company Setting

        public ABCAPOSReportEDSC.CompanySettingDTRow RetrieveCompanySetting()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.CompanySetting
                        select i;
            
            ABCAPOSReportEDSC.CompanySettingDTRow dr = new ABCAPOSReportEDSC.CompanySettingDTDataTable().NewCompanySettingDTRow();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), dr);
            else
                return null;

            return dr;
        }

        #endregion

        #region Credit Memo

        public ABCAPOSReportEDSC.InvoiceDTRow RetrieveCreditMemoPrintOut(long creditMemoID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_CreditMemo
                        where i.ID == creditMemoID
                        select i;

            ABCAPOSReportEDSC.InvoiceDTRow dr = new ABCAPOSReportEDSC.InvoiceDTDataTable().NewInvoiceDTRow();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), dr);
            else
                return null;

            return dr;
        }

        public ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable RetrieveCreditMemoDetailPrintOut(long creditMemoID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_CreditMemoDetail
                        where i.CreditMemoID == creditMemoID
                        select i;

            ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable dt = new ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable();

            ObjectHelper.CopyEnumerableToDataTable(query.AsEnumerable(), dt);

            return dt;
        }

        //sp_Rpt_CreditMemoByItem
        public List<ReportCreditMemoModel> RetrieveCreditMemoByItem(DateTime startDate, DateTime endDate, string customer, string salesRef, string product, string warehouse)
        {
            string spName = "sp_Rpt_CreditMemoByItem";

            var command = DbEngine.CreateCommand(spName, true) as SqlCommand;
            command.Parameters.AddWithValue("@startDate", startDate);
            command.Parameters.AddWithValue("@endDate", endDate);
            command.Parameters.AddWithValue("@customer", customer);
            command.Parameters.AddWithValue("@salesReference", salesRef);
            command.Parameters.AddWithValue("@productCode", product);
            command.Parameters.AddWithValue("@warehouseName", warehouse);

            return DbEngine.FillList<ReportCreditMemoModel>(command);
        }


        #endregion 

        #region Customer

        public ABCAPOSReportEDSC.CustomerDTRow RetrieveCustomer(long customerID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.Customer
                        where i.ID == customerID
                        select i;

            ABCAPOSReportEDSC.CustomerDTRow dr = new ABCAPOSReportEDSC.CustomerDTDataTable().NewCustomerDTRow();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), dr);
            else
                return null;

            return dr;
        }

        #endregion

        #region Delivery Order

        public ABCAPOSReportEDSC.DeliveryOrderDTRow RetrieveDeliveryOrderPrintOut(long deliveryOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_DeliveryOrder
                        where i.ID == deliveryOrderID
                        select i;

            ABCAPOSReportEDSC.DeliveryOrderDTRow dr = new ABCAPOSReportEDSC.DeliveryOrderDTDataTable().NewDeliveryOrderDTRow();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), dr);
            else
                return null;

            return dr;
        }

        public ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable RetrieveDeliveryOrderDetailPrintOut(long deliveryOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_DeliveryOrderDetail
                        where i.DeliveryOrderID == deliveryOrderID && i.Quantity > 0
                        select i;

            ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable dt = new ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable();

            ObjectHelper.CopyEnumerableToDataTable(query.AsEnumerable().OrderBy(p => p.SalesOrderItemNo).ToList(), dt);

            return dt;
        }

        public List<v_Rpt_DeliveryOrderDetail> RetrieveDeliveryOrderDetailReport(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Rpt_DeliveryOrderDetail
                        select i;

            ApplyFilter<v_Rpt_DeliveryOrderDetail>(ref query, selectFilters);

            return query.ToList();
        }

        public List<v_Rpt_DeliveryOrder> RetrieveDeliveryOrderReport(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Rpt_DeliveryOrder
                        select i;

            ApplyFilter<v_Rpt_DeliveryOrder>(ref query, selectFilters);

            return query.ToList();
        }

        public List<v_Rpt_FulfillmentByItemDetail> RetreiveDeliveryOrderByItemDetail(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Rpt_FulfillmentByItemDetail
                        select i;

            ApplyFilter<v_Rpt_FulfillmentByItemDetail>(ref query, selectFilters);

            return query.ToList();
        }

        public List<v_Rpt_FulfillmentByItemSummary> RetreiveDeliveryOrderByItemSummary(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_Rpt_FulfillmentByItemSummary
                        select i;
            ApplyFilter<v_Rpt_FulfillmentByItemSummary>(ref query, selectFilters);
            return query.ToList();
        }

        #endregion

        #region Income Expense

        public ABCAPOSReportEDSC.IncomeExpenseDTRow RetrieveIncomeExpensePrintOut(long incomeExpenseID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.IncomeExpense
                        where i.ID == incomeExpenseID
                        select i;

            ABCAPOSReportEDSC.IncomeExpenseDTRow dr = new ABCAPOSReportEDSC.IncomeExpenseDTDataTable().NewIncomeExpenseDTRow();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), dr);
            else
                return null;

            return dr;
        }

        public ABCAPOSReportEDSC.IncomeExpenseDetailDTDataTable RetrieveIncomeExpenseDetailPrintOut(long incomeExpenseID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.IncomeExpenseDetail 
                        where i.IncomeExpenseID == incomeExpenseID
                        select i;

            ABCAPOSReportEDSC.IncomeExpenseDetailDTDataTable dt = new ABCAPOSReportEDSC.IncomeExpenseDetailDTDataTable();

            ObjectHelper.CopyEnumerableToDataTable(query.AsEnumerable(), dt);

            return dt;
        }

        public ABCAPOSReportEDSC.AccountBalanceDTDataTable RetrieveIncomeExpenseReport(DateTime startDate, DateTime endDate)
        {
            string spName = "sp_Rpt_IncomeExpense";

            var command = DbEngine.CreateCommand(spName, true) as SqlCommand;
            command.Parameters.AddWithValue("@startDate", startDate);
            command.Parameters.AddWithValue("@endDate", endDate);

            ABCAPOSReportEDSC.AccountBalanceDTDataTable dt = new ABCAPOSReportEDSC.AccountBalanceDTDataTable();

            DbEngine.FillDataTable(dt, command);

            return dt;
        }

        #endregion

        #region Invoice

        public ABCAPOSReportEDSC.InvoiceDTRow RetrieveInvoicePrintOut(long invoiceID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Invoice
                        where i.ID == invoiceID
                        select i;

            ABCAPOSReportEDSC.InvoiceDTRow dr = new ABCAPOSReportEDSC.InvoiceDTDataTable().NewInvoiceDTRow();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), dr);
            else
                return null;

            return dr;
        }

        public ABCAPOSReportEDSC.TaxInvoiceDTRow RetrieveTaxInvoicePrintOut(long invoiceID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Invoice
                        where i.ID == invoiceID
                        select i;

            ABCAPOSReportEDSC.TaxInvoiceDTRow dr = new ABCAPOSReportEDSC.TaxInvoiceDTDataTable().NewTaxInvoiceDTRow();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), dr);
            else
                return null;

            return dr;
        }

        public ABCAPOSReportEDSC.TaxInvoiceDTRow RetrieveMultipleInvoicingPrintOut(long multipleInvoicingID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_MultipleInvoicing
                        where i.ID == multipleInvoicingID
                        select i;

            ABCAPOSReportEDSC.TaxInvoiceDTRow dr = new ABCAPOSReportEDSC.TaxInvoiceDTDataTable().NewTaxInvoiceDTRow();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), dr);
            else
                return null;

            return dr;
        }

        public ABCAPOSReportEDSC.InvoiceDTRow RetrieveMultipleInvoicePrintOut(long multipleInvoiceID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_MultipleInvoicing
                        where i.ID == multipleInvoiceID
                        select i;

            ABCAPOSReportEDSC.InvoiceDTRow dr = new ABCAPOSReportEDSC.InvoiceDTDataTable().NewInvoiceDTRow();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), dr);
            else
                return null;

            return dr;
        }

        public List<InvoiceAgingReportModel> RetrieveInvoiceAgingReport(DateTime startDate, DateTime endDate, DateTime dueDate, string customerName)
        {
            string spName = "sp_Rpt_InvoiceAging";

            var command = DbEngine.CreateCommand(spName, true) as SqlCommand;
            command.Parameters.AddWithValue("@startDate", startDate);
            command.Parameters.AddWithValue("@endDate", endDate);
            command.Parameters.AddWithValue("@dueDate", dueDate);
            command.Parameters.AddWithValue("@customerName", customerName);
            return DbEngine.FillList<InvoiceAgingReportModel>(command);
        }

        public List<v_Rpt_Invoice> RetrieveInvoiceReport(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Rpt_Invoice
                        select i;

            ApplyFilter<v_Rpt_Invoice>(ref query, selectFilters);

            return query.ToList();
        }

        public List<v_Rpt_Sales> RetrieveSalesReport(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Rpt_Sales
                        select i;

            ApplyFilter<v_Rpt_Sales>(ref query, selectFilters);

            return query.ToList();
        }

        public ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable RetrieveInvoiceDetailPrintOut(long invoiceID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_InvoiceDetail
                        where i.InvoiceID == invoiceID
                        select i;

            ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable dt = new ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable();

            ObjectHelper.CopyEnumerableToDataTable(query.AsEnumerable(), dt);

            return dt;
        }

        public List<v_Rpt_InvoicePaymentSummary> RetreiveInvoicePaymentSummary(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_Rpt_InvoicePaymentSummary
                        select i;

            ApplyFilter<v_Rpt_InvoicePaymentSummary>(ref query, selectFilters);
            return query.ToList();
        }

        public List<v_Rpt_InvoicePaymentDetail> RetreiveInvoicePaymentDetail(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_Rpt_InvoicePaymentDetail
                        select i;

            ApplyFilter<v_Rpt_InvoicePaymentDetail>(ref query, selectFilters);
            return query.ToList();
        }

        #endregion

        #region Multiple Invoicing

        public ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable RetrieveMultipleInvoicingDetailPrintOut(long multipleInvoicingID)
        {
            var query = new ABCAPOSDAC().RetrieveMultipleInvoiceItemsGroup(multipleInvoicingID);
            //ABCAPOSEntities ent = new ABCAPOSEntities();

            //var query = from i in ent.v_MultipleDeliveryInvoicingDetail
            //            where i.MultipleInvoicingID == multipleInvoicingID
            //            select i;
            //var query = from i in ent.v_MultipleInvoiceItemGroup
            //            where i.MultipleInvoicingID == multipleInvoicingID
            //            select i;

            ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable dt = new ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable();

            ObjectHelper.CopyEnumerableToDataTable(query.AsEnumerable(), dt);

            return dt;
        }


        #endregion 

        #region Payment
        public List<v_Rpt_Payment> RetrievePaymentReport(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Rpt_Payment
                        select i;

            ApplyFilter<v_Rpt_Payment>(ref query, selectFilters);

            return query.ToList();
        }

        public List<v_Rpt_PaymentInvoice> RetrievePaymentInvoiceReport(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Rpt_PaymentInvoice
                        select i;

            ApplyFilter<v_Rpt_PaymentInvoice>(ref query, selectFilters);

            return query.ToList();
        }

        #endregion

        #region  Payment Bill

        public List<v_Rpt_PaymentBill> RetrievePaymentBillReport(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Rpt_PaymentBill
                        select i;

            ApplyFilter<v_Rpt_PaymentBill>(ref query, selectFilters);

            return query.ToList();
        }

        #endregion 

        #region Product

        public List<v_ItemLocation> RetrieveProductReport(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_ItemLocation
                        select i;

            ApplyFilter<v_ItemLocation>(ref query, selectFilters);

            return query.ToList();
        }

        public List<LogDetailModel> RetrieveProductStockReport(DateTime date, string productCode, string productName, long warehouseID)
        {
            string spName = "sp_Rpt_Stock";

            var command = DbEngine.CreateCommand(spName, true) as SqlCommand;
            command.Parameters.AddWithValue("@date", date);
            command.Parameters.AddWithValue("@productCode", productCode);
            command.Parameters.AddWithValue("@productName", productName);
            command.Parameters.AddWithValue("@warehouseID", warehouseID);

            return DbEngine.FillList<LogDetailModel>(command);
        }

        public List<LogDetailModel> RetrieveProductSold(DateTime startDate, DateTime endDate, string productCode, string productName)
        {
            string spName = "sp_Rpt_ProductSold";

            var command = DbEngine.CreateCommand(spName, true) as SqlCommand;
            command.Parameters.AddWithValue("@startDate",startDate);
            command.Parameters.AddWithValue("@endDate",endDate);
            command.Parameters.AddWithValue("@productCode", productCode);
            command.Parameters.AddWithValue("@productName",productName);

            return DbEngine.FillList<LogDetailModel>(command);
        }

        public List<LogDetailModel> RetrieveProductSoldByCustomer(DateTime startDate, DateTime endDate, string productCode, string productName, string customerName, string salesReference)
        {
            string spName = "sp_Rpt_ProductSoldCustomer";

            var command = DbEngine.CreateCommand(spName, true) as SqlCommand;
            command.Parameters.AddWithValue("@startDate", startDate);
            command.Parameters.AddWithValue("@endDate", endDate);
            command.Parameters.AddWithValue("@productCode", productCode);
            command.Parameters.AddWithValue("@productName", productName);
            command.Parameters.AddWithValue("@customerName", customerName);
            command.Parameters.AddWithValue("@salesReference", salesReference);
          

            return DbEngine.FillList<LogDetailModel>(command);

        }

        public List<LogDetailModel> RetrieveProductReturnedByCustomer(DateTime startDate, DateTime endDate, string productCode, string productName, string customerName)
        {
            string spName = "sp_Rpt_ProductCustomerRetur";
            var command = DbEngine.CreateCommand(spName, true) as SqlCommand;
            command.Parameters.AddWithValue("@startDate",startDate);
            command.Parameters.AddWithValue("@endDate",endDate);
            command.Parameters.AddWithValue("@productCode",productCode);
            command.Parameters.AddWithValue("@productName", productName);
            command.Parameters.AddWithValue("@customerName", customerName);

            return DbEngine.FillList<LogDetailModel>(command);
        }

        public List<v_ReportStock> RetrieveStockReport(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_ReportStock
                        select i;
            ApplyFilter<v_ReportStock>(ref query, selectFilters);
            return query.ToList();
        }

        public List<LogDetailModel> RetrieveStockReport(DateTime startDate, DateTime endDate, string productCode, string productName, long warehouseID)
        {
            string spName = "sp_Rpt_ReportStock";

            var command = DbEngine.CreateCommand(spName, true) as SqlCommand;
            command.Parameters.AddWithValue("@startDate", startDate);
            command.Parameters.AddWithValue("@endDate", endDate);
            command.Parameters.AddWithValue("@productCode", productCode);
            command.Parameters.AddWithValue("@productName", productName);
            command.Parameters.AddWithValue("@warehouseID", warehouseID);

            return DbEngine.FillList<LogDetailModel>(command);
        }

        #endregion

        #region Profit and Loss
        public ABCAPOSReportEDSC.AccountBalanceDTDataTable RetrieveProfitAndLossReport(DateTime startDate, DateTime endDate)
        {
            string spName = "sp_Rpt_ProfitAndLoss";

            var command = DbEngine.CreateCommand(spName, true) as SqlCommand;
            command.Parameters.AddWithValue("@startDate", startDate);
            command.Parameters.AddWithValue("@endDate", endDate);

            ABCAPOSReportEDSC.AccountBalanceDTDataTable dt = new ABCAPOSReportEDSC.AccountBalanceDTDataTable();

            DbEngine.FillDataTable(dt, command);

            return dt;
        }
        #endregion

        #region Purchase Order

        public ABCAPOSReportEDSC.PurchaseOrderDTRow RetrievePurchaseOrderPrintOut(long purchaseOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchaseOrder
                        where i.ID == purchaseOrderID
                        select i;

            ABCAPOSReportEDSC.PurchaseOrderDTRow dr = new ABCAPOSReportEDSC.PurchaseOrderDTDataTable().NewPurchaseOrderDTRow();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), dr);
            else
                return null;

            return dr;
        }

        public List<PurchaseBillAgingReport> RetrievePurchaseBillAgingReport(DateTime startDate, DateTime endDate, DateTime dueDate, string vendorName,int agingType)
        {
            string spName = "sp_Rpt_PurchaseBillAging";

            var command = DbEngine.CreateCommand(spName, true) as SqlCommand;
            command.Parameters.AddWithValue("@startDate", startDate);
            command.Parameters.AddWithValue("@endDate", endDate);
            command.Parameters.AddWithValue("@dueDate", dueDate);
            command.Parameters.AddWithValue("@vendorName", vendorName);
            command.Parameters.AddWithValue("@agingType", agingType);
            return DbEngine.FillList<PurchaseBillAgingReport>(command);
        }

        public List<v_PurchaseOrderDetail> RetrievePurchaseOrderDetailPrintOut(long purchaseOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchaseOrderDetail
                        where i.PurchaseOrderID == purchaseOrderID
                        select i;

            //ABCAPOSReportEDSC.PurchaseOrderDetailDTDataTable dt = new ABCAPOSReportEDSC.PurchaseOrderDetailDTDataTable();
            //ObjectHelper.CopyEnumerableToDataTable(query.AsEnumerable(), dt);
            return query.ToList();
        }

        public List<v_PurchaseOrderDetail> RetrievePurchaseOrderReport(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchaseOrderDetail
                        select i;

            ApplyFilter<v_PurchaseOrderDetail>(ref query, selectFilters);

            return query.ToList();
        }

        public List<v_PurchaseBill> RetrievePurchaseBillReport (List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchaseBill
                        select i;

            //query.OrderBy(p => p.Date).ThenBy(p=>p.VendorName);

            ApplyFilter<v_PurchaseBill>(ref query, selectFilters);

            return query.ToList();
        }

        public List<v_Rpt_PurchaseBillOverdue> RetrievePurchaseBillOverdueReport(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Rpt_PurchaseBillOverdue
                        select i;

            ApplyFilter<v_Rpt_PurchaseBillOverdue>(ref query, selectFilters);

            return query.ToList();
        }

        #endregion

        #region Sales Order

        public ABCAPOSReportEDSC.SalesOrderDTRow RetrieveSalesOrderPrintOut(long salesOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_SalesOrder
                        where i.ID == salesOrderID
                        select i;

            ABCAPOSReportEDSC.SalesOrderDTRow dr = new ABCAPOSReportEDSC.SalesOrderDTDataTable().NewSalesOrderDTRow();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), dr);
            else
                return null;

            return dr;
        }

        public ABCAPOSReportEDSC.SalesOrderDetailDTDataTable RetrieveSalesOrderDetailPrintOut(long salesOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_SalesOrderDetail
                        where i.SalesOrderID == salesOrderID
                        orderby i.LineSequenceNumber
                        select i;

            ABCAPOSReportEDSC.SalesOrderDetailDTDataTable dt = new ABCAPOSReportEDSC.SalesOrderDetailDTDataTable();

            ObjectHelper.CopyEnumerableToDataTable(query.AsEnumerable().OrderBy(p=> p.ItemNo), dt);

            return dt;
        }

        public List<v_Rpt_SalesOrder> RetrieveSalesOrderReport(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Rpt_SalesOrder
                        select i;

            ApplyFilter<v_Rpt_SalesOrder>(ref query, selectFilters);

            return query.ToList();
        }

        public List<v_Rpt_SalesOrderDetail> RetrieveSalesOrderDetailReport(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Rpt_SalesOrderDetail
                        select i;

            ApplyFilter<v_Rpt_SalesOrderDetail>(ref query, selectFilters);

            return query.ToList();
        }
        #endregion

        #region Transfer Order

        public ABCAPOSReportEDSC.SalesOrderDTRow RetrieveTransferOrderPrintOut(long transferOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_TransferOrder
                        where i.ID == transferOrderID
                        select i;

            ABCAPOSReportEDSC.SalesOrderDTRow dr = new ABCAPOSReportEDSC.SalesOrderDTDataTable().NewSalesOrderDTRow();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), dr);
            else
                return null;

            return dr;
        }

        public ABCAPOSReportEDSC.SalesOrderDetailDTDataTable RetrieveTransferOrderDetailPrintOut(long transferOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_TransferOrderDetail
                        where i.TransferOrderID == transferOrderID
                        select i;

            ABCAPOSReportEDSC.SalesOrderDetailDTDataTable dt = new ABCAPOSReportEDSC.SalesOrderDetailDTDataTable();

            ObjectHelper.CopyEnumerableToDataTable(query.AsEnumerable(), dt);

            return dt;
        }
        
        public List<InvoiceBySalesRepModel> RetrieveInvoiceBySalesReferenceReport(DateTime startDate, DateTime endDate, string warehouse)
        {
            string spName = "sp_Rpt_InvoiceBySalesRepSum";

            var command = DbEngine.CreateCommand(spName, true) as SqlCommand;
            command.Parameters.AddWithValue("@startDate", startDate);
            command.Parameters.AddWithValue("@endDate", endDate);
            command.Parameters.AddWithValue("@warehouse", warehouse);
            return DbEngine.FillList<InvoiceBySalesRepModel>(command);
        }

        public List<InvoiceBySalesRepDetailModel> RetrieveInvoiceBySalesRepDetailReport(DateTime startDate, DateTime endDate, string customer)
        {
            string spName = "sp_Rpt_InvoiceBySalesRepDetail";

            var command = DbEngine.CreateCommand(spName, true) as SqlCommand;
            command.Parameters.AddWithValue("@startDate", startDate);
            command.Parameters.AddWithValue("@endDate", endDate);
            command.Parameters.AddWithValue("@customer", customer);

            return DbEngine.FillList<InvoiceBySalesRepDetailModel>(command);
        }

        public List<InvoiceByItemSumModel> RetrieveInvoiceByItemSumReport(DateTime startDate, DateTime endDate, string warehouseName)
        {
            string spName = "sp_Rpt_InvoiceByItemSum";

            var command = DbEngine.CreateCommand(spName, true) as SqlCommand;
            command.Parameters.AddWithValue("@startDate", startDate);
            command.Parameters.AddWithValue("@endDate", endDate);
            command.Parameters.AddWithValue("@warehouseName", warehouseName);

            return DbEngine.FillList<InvoiceByItemSumModel>(command);
        }
        
        public List<ReportOverdueModel> RetrieveReportOverdue(DateTime endDate, string customer, string warehouseName, string salesRef)
        {
            string spName = "sp_Rpt_ReportOverdue";

            var command = DbEngine.CreateCommand(spName, true) as SqlCommand;
            command.Parameters.AddWithValue("@endDate", endDate);
            command.Parameters.AddWithValue("@customer", customer);
            command.Parameters.AddWithValue("@warehouseName", warehouseName);
            command.Parameters.AddWithValue("@salesReference", salesRef);

            return DbEngine.FillList<ReportOverdueModel>(command);
        }

        public List<ReportBonusModel> RetrieveBonusByInvoice(DateTime startDate, DateTime endDate, string invoiceCode, string customer, string warehouseName,string salesReference)
        {
            string spName = "sp_Rpt_BonusByInvoice";

            var command = DbEngine.CreateCommand(spName, true) as SqlCommand;
            command.Parameters.AddWithValue("@startDate", startDate);
            command.Parameters.AddWithValue("@endDate", endDate);
            //command.Parameters.AddWithValue("@startInvDate", startInvDate);
            //command.Parameters.AddWithValue("@endInvDate", endInvDate);
            command.Parameters.AddWithValue("@invoiceCode", invoiceCode);
            command.Parameters.AddWithValue("@customer", customer);
            command.Parameters.AddWithValue("@warehouseName", warehouseName);
            command.Parameters.AddWithValue("@salesReference", salesReference);

            return DbEngine.FillList<ReportBonusModel>(command);
        }

        public List<InvoiceMonthlyModel> RetrieveInvoiceMonthly(DateTime startDate, DateTime endDate, DateTime startDueDate, DateTime endDueDate, string warehouseName, string customer, string status)
        {
            string spName = "sp_Rpt_MonthlyInvoice";

            var command = DbEngine.CreateCommand(spName, true) as SqlCommand;
            command.Parameters.AddWithValue("@startDate", startDate);
            command.Parameters.AddWithValue("@endDate", endDate);
            command.Parameters.AddWithValue("@startDueDate", startDueDate);
            command.Parameters.AddWithValue("@endDueDate", endDueDate);
            command.Parameters.AddWithValue("@warehouse", warehouseName);
            command.Parameters.AddWithValue("@customer", customer);
            command.Parameters.AddWithValue("@status", status);

            return DbEngine.FillList<InvoiceMonthlyModel>(command);
        }

        public List<InvoiceMonthlyModel> RetrieveInvoiceMonthlySummary(DateTime startDate, DateTime endDate, int warehouseID, string customer, string status)
        {
            string spName = "sp_Rpt_MonthlyInvoiceSummary";

            var command = DbEngine.CreateCommand(spName, true) as SqlCommand;
            command.Parameters.AddWithValue("@startDate", startDate);
            command.Parameters.AddWithValue("@endDate", endDate);
            command.Parameters.AddWithValue("@warehouseID", warehouseID);
            command.Parameters.AddWithValue("@customer", customer);
            command.Parameters.AddWithValue("@status", status);

            return DbEngine.FillList<InvoiceMonthlyModel>(command);
        }

        public List<InvoiceMonthlyModel> RetrieveInvoiceCreditMemo(DateTime startDate, DateTime endDate, string warehouseName, string customer, string salesRef)
        {
            string spName = "sp_Rpt_InvoiceCreditMemoBySalesRep";

            var command = DbEngine.CreateCommand(spName, true) as SqlCommand;
            command.Parameters.AddWithValue("@startDate", startDate);
            command.Parameters.AddWithValue("@endDate", endDate);
            command.Parameters.AddWithValue("@warehouse", warehouseName);
            command.Parameters.AddWithValue("@customer", customer);
            command.Parameters.AddWithValue("@salesRef", salesRef);

            return DbEngine.FillList<InvoiceMonthlyModel>(command);
        }
        #endregion 
        
        #region Transfer Delivery

        public ABCAPOSReportEDSC.DeliveryOrderDTRow RetrieveTransferDeliveryPrintOut(long transferDeliveryID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_TransferDelivery
                        where i.ID == transferDeliveryID
                        select i;

            ABCAPOSReportEDSC.DeliveryOrderDTRow dr = new ABCAPOSReportEDSC.DeliveryOrderDTDataTable().NewDeliveryOrderDTRow();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), dr);
            else
                return null;

            return dr;
        }

        public ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable RetrieveTransferDeliveryDetailPrintOut(long transferDeliveryID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_TransferDeliveryDetail
                        where i.TransferDeliveryID == transferDeliveryID
                        select i;

            ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable dt = new ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable();

            ObjectHelper.CopyEnumerableToDataTable(query.AsEnumerable(), dt);

            return dt;
        }

        #endregion 

        #region Vendor Return

        public ABCAPOSReportEDSC.DeliveryOrderDTRow RetrieveVendorReturnPrintOut(long vendorReturnID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_VendorReturn
                        where i.ID == vendorReturnID
                        select i;

            ABCAPOSReportEDSC.DeliveryOrderDTRow dr = new ABCAPOSReportEDSC.DeliveryOrderDTDataTable().NewDeliveryOrderDTRow();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), dr);
            else
                return null;

            return dr;
        }

        public ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable RetrieveVendorReturnDetailPrintOut(long vendorReturnID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_VendorReturnDetail
                        where i.VendorReturnID == vendorReturnID
                        select i;

            ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable dt = new ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable();

            ObjectHelper.CopyEnumerableToDataTable(query.AsEnumerable(), dt);

            return dt;
        }

        #endregion

        #region Workorder
        public ABCAPOSReportEDSC.WorkOrderDTRow RetreiveWorkOrderPrintOut(long workOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_WorkOrder
                        where i.ID == workOrderID
                        select i;

            ABCAPOSReportEDSC.WorkOrderDTRow dr = new ABCAPOSReportEDSC.WorkOrderDTDataTable().NewWorkOrderDTRow();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), dr);
            else
                return null;

            return dr;
        }

        public ABCAPOSReportEDSC.WorkOrderDTRow RetreiveWorkOrderPrintOutStruk(long workOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PrintStrukWorkOrder
                        where i.ID == workOrderID
                        select i;

            ABCAPOSReportEDSC.WorkOrderDTRow dr = new ABCAPOSReportEDSC.WorkOrderDTDataTable().NewWorkOrderDTRow();

            if (query.FirstOrDefault() != null)
                ObjectHelper.CopyProperties(query.FirstOrDefault(), dr);
            else
                return null;

            return dr;
        }

        public ABCAPOSReportEDSC.WorkOrderDetailDTDataTable RetreiveWorkOrderDetailPrintOut(long workOrderID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_WorkOrderDetail
                        where i.WorkOrderID == workOrderID && i.Qty > 0
                        select i;

            ABCAPOSReportEDSC.WorkOrderDetailDTDataTable dt = new ABCAPOSReportEDSC.WorkOrderDetailDTDataTable();
            ObjectHelper.CopyEnumerableToDataTable(query.AsEnumerable().OrderBy(p=>p.ItemNo).ToList(),dt);

            return dt;
        }

        public List<v_WorkOrderDetail> RetreiveWorkOrderDetailReport(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_WorkOrderDetail
                        select i;

            ApplyFilter<v_WorkOrderDetail>(ref query, selectFilters);

            return query.ToList();
        }

        public List<v_WorkOrder> RetreiveWorkOrderReport(List<SelectFilter> selectFilters)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_WorkOrder
                        select i;

            ApplyFilter<v_WorkOrder>(ref query, selectFilters);

            return query.ToList();
        }
        #endregion
    }
}
