using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using ABCAPOS.DA;
using System.Data.Objects.SqlClient;
using System.Transactions;
using ABCAPOS.Util;
using ABCAPOS.ReportEDS;
using MPL;

namespace ABCAPOS.BF
{
    public class InvoiceBFC : MasterDetailBFC<Invoice, v_Invoice, InvoiceDetail, v_InvoiceDetail, InvoiceModel, InvoiceDetailModel>
    {
        private void UpdateStatusResi(List<InvoiceResiDetailModel>resiDetails,int status)
        {
            foreach (var resiDetail in resiDetails)
            {
                var resi = new ResiBFC().RetrieveByID(resiDetail.ResiID);
                if (resi != null && status != (int)MPL.DocumentStatus.Void)
                {
                    resi.IsHasInvoice = true;
                    resi.Status = (int)ResiStatus.FullBilling;
                }
                else if (resi != null && status == (int)MPL.DocumentStatus.Void)
                {
                    resi.IsHasInvoice = false;
                    resi.Status = (int)ResiStatus.PendingBilling;
                }
                new ResiBFC().Update(resi);
            }
        }

        private void CreateInvoiceResiDetails(long invoiceID, List<InvoiceResiDetailModel> resiDetails)
        {
            var dac = new ABCAPOSDAC();
            var itemNo = 1;
            foreach (var resiDetail in resiDetails)
            {
                resiDetail.InvoiceID = invoiceID;
                resiDetail.ItemNo = itemNo++;
                dac.CreateInvoiceResiDetail(resiDetail);
            }
        }

        private void UpdateInvoiceResiDetails(long invoiceID, List<InvoiceResiDetailModel> resiDetails)
        {
            var dac = new ABCAPOSDAC();
            dac.DeleteInvoiceResiDetail(invoiceID);
            this.CreateInvoiceResiDetails(invoiceID, resiDetails);
        }

        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetInvoiceCode(InvoiceModel header)
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var invoicePrefix = "";

            if (prefixSetting != null)
                invoicePrefix = prefixSetting.InvoicePrefix;

            var warehouse = new WarehouseBFC().RetrieveByID(header.WarehouseID);
            var year = DateTime.Now.Year.ToString().Substring(2, 2);

            var prefix = invoicePrefix + year + "-" + warehouse.Code + "-";
            var code = new ABCAPOSDAC().RetrieveInvoiceMaxCode(prefix, 7);

            return code;
        }

        protected override GenericDetailDAC<InvoiceDetail, InvoiceDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<InvoiceDetail, InvoiceDetailModel>("InvoiceID", "LineSequenceNumber", false);
        }

        protected override GenericDetailDAC<v_InvoiceDetail, InvoiceDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_InvoiceDetail, InvoiceDetailModel>("InvoiceID", "LineSequenceNumber", false);
        }

        protected override GenericDAC<Invoice, InvoiceModel> GetMasterDAC()
        {
            return new GenericDAC<Invoice, InvoiceModel>("ID", false, "Date DESC");
        }

        protected override GenericDAC<v_Invoice, InvoiceModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_Invoice, InvoiceModel>("ID", false, "Date DESC");
        }

        public override void Create(InvoiceModel header, List<InvoiceDetailModel> details)
        {
            header.Code = GetInvoiceCode(header);
            OnCreateInvoice(header, details);
            base.Create(header, details);

            if (header.ResiDetails != null)
            {
                this.CreateInvoiceResiDetails(header.ID, header.ResiDetails);
                this.UpdateStatusResi(header.ResiDetails, (int)MPL.DocumentStatus.New);
            }

            OnUpdated(header.ID, header.CreatedBy);
            PostAccounting(header.ID, header.Status);
            new SalesOrderBFC().UpdateStatus(header.SalesOrderID);

        }

        public override void Update(InvoiceModel header, List<InvoiceDetailModel> details)
        {
            //Please dont change the location after base.Update
            updateFromOldQty(header, details);
            OnCreateInvoice(header, details);
            base.Update(header, details);
            if (header.ResiDetails.Count > 0)
                this.UpdateInvoiceResiDetails(header.ID, header.ResiDetails);

            //OnUpdated(header.ID, header.CreatedBy);

            PostAccounting(header.ID, header.Status);
            OnUpdated(header.ID, header.CreatedBy);
            new SalesOrderBFC().UpdateStatus(header.SalesOrderID);
        }

        public void CalculateGrossAmountInvDetail(InvoiceDetailModel detail)
        {
            if (detail.GrossAmount == 0)
            {
                detail.TotalAmount = Convert.ToDecimal(Convert.ToDecimal(Convert.ToDecimal(detail.Price) * Convert.ToDecimal(detail.Quantity)).ToString("N2"));
                //detail.TotalAmount = Convert.ToDecimal(detail.TotalAmount.ToString("N2"));
   
                if (detail.TaxType == 2)
                    detail.TotalPPN = Convert.ToDecimal(Convert.ToDecimal(Convert.ToDouble(detail.TotalAmount) * 0.1).ToString("N2"));
                   // detail.TotalPPN = Convert.ToDecimal(detail.TotalPPN.ToString("N2"));
                else
                    detail.TotalPPN = 0;

                //detail.TotalPPN = Convert.ToDecimal(detail.TotalPPN.ToString("N2"));
                detail.GrossAmount = detail.TotalAmount + detail.TotalPPN;
                //detail.GrossAmount = Convert.ToDecimal(detail.GrossAmount.ToString("N2"));
            }
            else
            {
                if (detail.TaxType == 2)
                {
                    detail.TotalAmount = Convert.ToDecimal(Convert.ToDecimal(Convert.ToDouble(detail.GrossAmount) / 1.1).ToString("N2"));
                    detail.TotalPPN = detail.GrossAmount - detail.TotalAmount;
                    //detail.TotalPPN=Convert.ToDecimal(detail.TotalPPN.ToString("N2"));
                }
                else
                {
                    detail.TotalAmount = detail.GrossAmount;
                    //detail.TotalAmount = Convert.ToDecimal(detail.TotalAmount.ToString("N2"));
                    detail.TotalPPN = 0;
                }
            }
        }

        public void Validate(InvoiceModel inv, List<InvoiceDetailModel> invDetails)
        {
            var obj = RetrieveByID(inv.ID);
            var soDetails = new SalesOrderBFC().RetrieveDetails(inv.SalesOrderID);

            var extDetails = new List<InvoiceDetailModel>();

            if (obj != null)
                extDetails = RetrieveDetails(obj.ID);

            foreach (var invDetail in invDetails)
            {
                if (!string.IsNullOrEmpty(invDetail.StrQuantity))
                    invDetail.Quantity = Convert.ToDouble(invDetail.StrQuantity);
                else
                    invDetail.Quantity = 0;

                if (invDetail.Quantity == 0)
                {
                    var product = new ProductBFC().RetrieveByID(invDetail.ProductID);

                    throw new Exception("Product Qty " + product.Code + " cannot be zero");
                }

                var query = from i in soDetails
                            where i.ProductID == invDetail.ProductID && i.ItemNo == invDetail.SalesOrderItemNo
                            select i;

                var soDetail = query.FirstOrDefault();
                var invQty = invDetail.Quantity;

                //if (obj != null)
                //{
                //    var extDetail = (from i in extDetails
                //                     where i.ProductID == invDetail.ProductID && i.ItemNo == invDetail.SalesOrderItemNo
                //                     select i).FirstOrDefault();

                //    soDetail.CreatedDOQuantity -= extDetail.Quantity;
                //}

                if (soDetail.Quantity < invQty)
                    throw new Exception("Invoiced quantity of " + invDetail.ProductCode + " will be greater than ordered");
            }
        }

        public void OnCreateInvoice(InvoiceModel header, List<InvoiceDetailModel> details)
        {
            header.Amount = 0;
            header.TaxAmount = 0;
            foreach (var detail in details)
            {
                //decimal amount = details.Sum(p => p.TotalAmount);
                //decimal taxAmount = details.Sum(p => p.TotalPPN);
                if (detail.GrossAmount == 0)
                    detail.GrossAmount = detail.TotalAmount + detail.TotalPPN;
                //new function
                CalculateGrossAmountInvDetail(detail);
                header.Amount += detail.TotalAmount;
                header.TaxAmount += detail.TotalPPN;
            }
            // force 0 decimal places to all Invoice grand totals
            header.Amount = Math.Round(header.Amount, 0);
            header.TaxAmount = Math.Round(header.TaxAmount, 0);
            //header.Amount = header.Amount;
            //header.TaxAmount = header.TaxAmount;
            //header.Amount = amount + taxAmount;
        }

        public void updateFromOldQty(InvoiceModel header, List<InvoiceDetailModel> details)
        {
            
            var oldDetails = RetrieveDetails(header.ID);

            foreach (var detail in details)
            {
                var oldDetail = from i in oldDetails
                                where i.ProductID == detail.ProductID && i.ItemNo == detail.ItemNo
                                select i;

                var oldInvDetail = oldDetail.FirstOrDefault();

                detail.SelisihQuantity = detail.Quantity - oldInvDetail.Quantity;
            }
            //Update CreatedInvQuantity. No more need. CreatedInvQuantity is now a view
            //new SalesOrderBFC().UpdateCreatedInvQuantity(header.SalesOrderID, details);

        }

        public void UpdateInvoiceDetailInvoiceItem(MultipleInvoiceItemModel multiInvItem, List<InvoiceDetailModel> invDetails, bool setBoolValue)
        {
            foreach (var invDetail in invDetails)
            {
                var selectedInvDetails = RetrieveDetails(multiInvItem.InvoiceID);

                var selectedInvDetail = from i in selectedInvDetails
                                        where i.InvoiceID == multiInvItem.InvoiceID && i.ItemNo == multiInvItem.InvoiceDetailItemNo && invDetail.InvoiceID == multiInvItem.InvoiceID && invDetail.ItemNo == multiInvItem.InvoiceDetailItemNo
                                        select i;

                var newInvDetail = selectedInvDetail.FirstOrDefault();
                if (newInvDetail != null)
                    invDetail.HasInvoiceItem = setBoolValue;
            }
            UpdateDetails(multiInvItem.InvoiceID, invDetails);

        }

        public void UpdateDetails(long invoiceID, List<InvoiceDetailModel> invDetails)
        {
            var inv = new InvoiceBFC().RetrieveByID(invoiceID);
            new InvoiceBFC().Update(inv, invDetails);
            //using (TransactionScope trans = new TransactionScope())
            //{
            //    //GetDetailDAC().DeleteByParentID(salesOrderID);
            //    //GetDetailDAC().Create(salesOrderID, soDetails);
            //    var inv = new InvoiceBFC().RetrieveByID(invoiceID);
            //    new InvoiceBFC().Update(inv, invDetails);

            //    trans.Complete();
            //}
        }

        public void OnUpdated(long invID, string userName)
        {
            var inv = RetrieveByID(invID);

            if (inv != null)
            {
                var salesOrder = new SalesOrderBFC().RetrieveByID(inv.SalesOrderID);

                if (salesOrder != null)
                {
                    var pbDetails = RetrieveDetailsBySOID(salesOrder.ID);
                    var poDetails = new SalesOrderBFC().RetrieveDetails(salesOrder.ID);

                    var fulfilledCount = 0;

                    foreach (var poDetail in poDetails)
                    {
                        if (poDetail.Quantity == poDetail.CreatedInvQuantity)
                            fulfilledCount += 1;
                    }

                    if (pbDetails.Count == 0)
                        salesOrder.HasInv = false;
                    else
                        salesOrder.HasInv = true;

                    if (fulfilledCount == poDetails.Count)
                    {
                        salesOrder.IsInvoiceFulfilled = true;
                        //salesOrder.Status = (int)SalesOrderStatus.Inv;
                    }
                    else
                    {
                        salesOrder.IsInvoiceFulfilled = false;
                        //salesOrder.Status = (int)SalesOrderStatus.PartialInv;
                    }

                    new SalesOrderBFC().Update(salesOrder, poDetails);
                    new SalesOrderBFC().UpdateDetails(salesOrder.ID, poDetails);
                }
            }
        }

        public void CreateInvoiceBySalesOrder(InvoiceModel inv, long salesOrderID)
        {
            var salesOrder = new SalesOrderBFC().RetrieveByID(salesOrderID);

            if (salesOrder != null)
            {
                inv.Code = SystemConstants.autoGenerated;//GetInvoiceCode();

                inv.SalesOrderID = salesOrderID;
                inv.SalesOrderCode = salesOrder.Code;

                var deliveries = new DeliveryOrderBFC().RetrieveBySalesOrderID(salesOrderID);
                for (int i = 0; i < deliveries.Count; i++)
                {
                    inv.DeliveryOrderCodeList += deliveries[i].Code;
                    if (i < deliveries.Count - 1)
                    {
                        inv.DeliveryOrderCodeList += ", ";
                    }
                }

                //initial shipping if not cover by abca
                var invresiDetails = new List<InvoiceResiDetailModel>();
                var CS = new CustomerBFC().RetrieveByID(salesOrder.CustomerID);
                if (!CS.IsCoverExpeditionByABCA)
                {
                    var resies = new ResiBFC().RetrieveResiByCustomerID(salesOrder.CustomerID);
                    if (resies.Count > 0)
                    {
                        //add ref resi code
                        for (int t = 0; t < resies.Count; t++)
                        {
                            inv.ResiCodeList += resies[t].Code;
                            if (t < resies.Count - 1)
                            {
                                inv.ResiCodeList += ", ";
                            }
                        }
                        // finish add

                        //add Detail Shipping
                        //var resiDetails = new ResiBFC().RetrieveResiByCustomerID(salesOrder.CustomerID);
                        foreach (var resiDetail in resies)
                        {
                            var Detail = new InvoiceResiDetailModel();
                            Detail.ResiID = resiDetail.ID;
                            Detail.ResiDate = resiDetail.Date;
                            Detail.ResiCode = resiDetail.Code;
                            Detail.ExpeditionName = resiDetail.ExpeditionName;
                            Detail.Amount = resiDetail.Amount;

                            invresiDetails.Add(Detail);
                        }
                        inv.ShippingAmount = resies.Sum(p => p.Amount);
                        //finish add detail Shipping
                    }
                }
               
                inv.CustomerCode = salesOrder.CustomerCode;
                inv.CustomerName = salesOrder.CustomerName;
                inv.POCustomerNo = salesOrder.POCustomerNo;
                inv.DueDate = inv.Date.AddDays(salesOrder.Terms);

                var warehouse = new WarehouseBFC().RetrieveByID(salesOrder.WarehouseID);
                inv.WarehouseID = warehouse.ID;
                inv.WarehouseName = warehouse.Name;

                var department = new DepartmentBFC().RetrieveByID(salesOrder.DepartmentID);

                if (department != null)
                    inv.DepartmentName = department.Name;

                var soDetails = new SalesOrderBFC().RetrieveDetails(salesOrderID);
                var invDetails = new List<InvoiceDetailModel>();

                foreach (var soDetail in soDetails)
                {
                    if (soDetail.Quantity - soDetail.CreatedInvQuantity > 0)
                    {
                        var detail = new InvoiceDetailModel();

                        detail.SalesOrderItemNo = soDetail.ItemNo;
                        detail.ProductID = soDetail.ProductID;
                        detail.Barcode = soDetail.Barcode;
                        detail.ProductCode = soDetail.ProductCode;
                        detail.ProductName = soDetail.ProductName;
                        detail.ConversionName = soDetail.ConversionName;
                        detail.TaxType = soDetail.TaxType;
                        detail.Price = soDetail.Price;
                        detail.PriceLevelID = soDetail.PriceLevelID;
                        detail.PriceLevelName = soDetail.PriceLevelName;
                        detail.QtySO = Convert.ToDouble(soDetail.Quantity);
                        detail.CreatedDOQuantity = Convert.ToDouble(soDetail.CreatedDOQuantity);
                        detail.CreatedInvQuantity = soDetail.CreatedInvQuantity;
                        detail.QtyRemain = detail.Quantity = soDetail.OutstandingInvQuantity;
                        detail.StrQuantity = soDetail.OutstandingInvQuantity.ToString().Replace(",", "").Replace(".", ",");
                        if (soDetail.OutstandingInvQuantity < 0)
                        {
                            detail.Quantity = 0;
                            detail.StrQuantity = "0";
                        }
                        detail.Remarks = soDetail.Remarks;

                        var itemLoc = new ItemLocationBFC().RetrieveByProductIDWarehouseID(soDetail.ProductID, salesOrder.WarehouseID);
                        if (itemLoc != null)
                            detail.StockQty = itemLoc.QtyOnHand;
                        else
                            detail.StockQty = 0;

                        //harga
                        //Convert.ToDecimal(Convert.ToDecimal(Convert.ToDouble(detail.GrossAmount) / 1.1).ToString("N2"));
                        detail.TotalAmount = Convert.ToDecimal(Convert.ToDecimal(Convert.ToDecimal(detail.Price) * Convert.ToDecimal(detail.Quantity)).ToString("N2"));
                        if (detail.TaxType == 2)
                            detail.TotalPPN = Convert.ToDecimal(Convert.ToDecimal(Convert.ToDouble(detail.TotalAmount) * 0.1).ToString("N2"));
                        else
                            detail.TotalPPN = 0;

                        detail.GrossAmount = detail.TotalAmount + detail.TotalPPN;
                        //inv.Amount += detail.TotalAmount;
                        //inv.TaxAmount += detail.TotalPPN;
                        
                        invDetails.Add(detail);
                    }
                }

                inv.ResiDetails = invresiDetails;
                

                inv.Details = invDetails;

                OnCreateInvoice(inv, invDetails);
            }
        }

        public void Void(long invoiceID, string voidRemarks, string userName)
        {
            var invoice = RetrieveByID(invoiceID);
            var oldStatus = invoice.Status;

            var invoiceResiDetail = RetrieveInvoiceResiDetails(invoiceID);

            invoice.Status = (int)MPL.DocumentStatus.Void;
            invoice.VoidRemarks = voidRemarks;
            invoice.ApprovedBy = "";
            invoice.ApprovedDate = SystemConstants.UnsetDateTime;

            PostAccounting(invoiceID, invoice.Status);

            this.UpdateStatusResi(invoiceResiDetail, invoice.Status);

            Update(invoice);

            OnUpdated(invoiceID, userName);
        }

        public InvoiceModel RetrieveByDeliveryOrder(long deliveryOrderID)
        {
            return new ABCAPOSDAC().RetrieveInvoiceByDeliveryOrder(deliveryOrderID);
        }

        public List<InvoiceModel> RetrieveBySalesOrder(long salesOrderID)
        {
            return new ABCAPOSDAC().RetrieveInvoiceBySalesOrder(salesOrderID);
        }

        public List<InvoiceModel> RetrieveByCustomerIDStartEndDate(long customerID, DateTime dateFrom, DateTime dateTo)
        {
            return new ABCAPOSDAC().RetrieveInvoiceByCustomerIDStartEndDate(customerID, dateFrom, dateTo);
        }

        public List<InvoiceModel> RetrieveByBSID(long bookingSalesID)
        {
            var soList = new SalesOrderBFC().RetrieveByBSID(bookingSalesID);
            var doList = new List<InvoiceModel>();

            foreach (var so in soList)
            {
                var doList2 = new ABCAPOSDAC().RetrieveInvBySOID(so.ID);
                doList.AddRange(doList2);
            }

            return doList;
        }

        public List<InvoiceModel> RetrieveUncreatedPayment(int startIndex, int? amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            if (amount == null)
                amount = SystemConstants.ItemPerPage;

            return new ABCAPOSDAC().RetrieveUncreatedPaymentInvoice(startIndex, (int)amount, sortParameter, selectFilters);
        }

        public int RetrieveUncreatedPaymentCount(List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveUncreatedPaymentInvoiceCount(selectFilters);
        }

        public int RetreiveInvoiceNotification(List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetreiveInvoiceNotification(selectFilters);
        }

        public decimal RetrieveUnpaidGrandTotal(DateTime startDate, DateTime endDate)
        {
            return new ABCAPOSDAC().RetrieveUnpaidInvoiceGrandTotal(startDate, endDate);
        }

        public ABCAPOSReportEDSC.InvoiceDTRow RetrievePrintOut(long invoiceID)
        {
            return new ABCAPOSReportDAC().RetrieveInvoicePrintOut(invoiceID);
        }

        public ABCAPOSReportEDSC.TaxInvoiceDTRow RetrieveTaxInvoicePrintOut(long invoiceID)
        {
            return new ABCAPOSReportDAC().RetrieveTaxInvoicePrintOut(invoiceID);
        }

        public ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable RetrieveDetailPrintOut(long invoiceID)
        {
            return new ABCAPOSReportDAC().RetrieveInvoiceDetailPrintOut(invoiceID);
        }

        public int RetrieveUncreatedMultipleInvoicingCount(List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveUncreatedMultipleInvoicingCount(selectFilters);
        }

        public List<InvoiceModel> RetrieveUncreatedMultipleInvoicing(int startIndex, int? amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            if (amount == null)
                amount = SystemConstants.ItemPerPage;

            return new ABCAPOSDAC().RetrieveUncreatedMultipleInvoicing(startIndex, (int)amount, sortParameter, selectFilters);
        }

        public List<InvoiceModel> RetrievePayableInvoices(long customerID)
        {
            return new ABCAPOSDAC().RetrieveInvoicesByCustomerID(customerID, PaymentStatus.Unpaid);
        }

        public List<InvoiceResiDetailModel> RetrieveInvoiceResiDetails(long invoiceID)
        {
            return new ABCAPOSDAC().RetrieveInvoiceResiByInvoiceID(invoiceID);
        }

        public List<InvoiceResiDetailModel> RetrieveInvoiceResiByResiID(long resiID)
        {
            return new ABCAPOSDAC().RetrieveInvoiceResiByResiID(resiID);
        }

        public int RetrieveInvoiceCount(List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveInvoiceCount(selectFilters);
        }

        public List<InvoiceModel> RetrieveInvoice(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveInvoice(startIndex, amount, sortParameter, selectFilters);
        }

        #region Post to Accounting Result

        public void PostAccounting(long invoiceID, int Status)
        {
            var invoice = RetrieveByID(invoiceID);

            new ABCAPOSDAC().DeleteAccountingResults(invoiceID, AccountingResultDocumentType.Invoice);

            if (Status != (int)MPL.DocumentStatus.Void)
                CreateAccountingResult(invoiceID);
        }

        private void CreateAccountingResult(long invoiceID)
        {
            var invoice = RetrieveByID(invoiceID);
            var invoiceDetails = RetrieveDetails(invoiceID);

            var salesOrder = new SalesOrderBFC().RetrieveByID(invoice.SalesOrderID);
            var salesOrderDetails = new SalesOrderBFC().RetrieveDetails(invoice.SalesOrderID);

            decimal salesAmount = 0;
            decimal taxAmount = 0;
            decimal basePriceTotal = 0;

            var accountingResultList = new List<AccountingResultModel>();

            foreach (var invoiceDetail in invoiceDetails)
            {
                var soDetail = (from i in salesOrderDetails
                                where i.ProductID == invoiceDetail.ProductID
                                select i).FirstOrDefault();

                var product = new ProductBFC().RetrieveByID(invoiceDetail.ProductID);

                if (soDetail != null)
                {
                    salesAmount += Convert.ToDecimal(invoiceDetail.Quantity) * soDetail.Price;
                    taxAmount += Convert.ToDecimal(invoiceDetail.Quantity) * (soDetail.TotalPPN / Convert.ToDecimal(soDetail.Quantity));
                }

                if (product != null)
                {
                    basePriceTotal += Convert.ToDecimal(invoiceDetail.Quantity) * product.BasePrice;
                }
            }

            salesAmount = invoice.Amount * salesOrder.ExchangeRate;
            taxAmount = invoice.TaxAmount * salesOrder.ExchangeRate;

            var totalSales = salesAmount + taxAmount;

            accountingResultList = AddToAccountingResultList(accountingResultList, invoice, (long)PostingAccount.PiutangDagang, AccountingResultType.Debit, totalSales, "Piutang dagang invoice " + invoice.Code);

            accountingResultList = AddToAccountingResultList(accountingResultList, invoice, (long)PostingAccount.PenjualanKredit, AccountingResultType.Credit, invoice.Amount, "Penjualan kredit invoice " + invoice.Code);

            accountingResultList = AddToAccountingResultList(accountingResultList, invoice, (long)PostingAccount.HutangDagangPPN, AccountingResultType.Credit, invoice.TaxAmount, "Hutang dagang PPN invoice " + invoice.Code);

            accountingResultList = AddToAccountingResultList(accountingResultList, invoice, (long)PostingAccount.HargaPokokPenjualan, AccountingResultType.Debit, basePriceTotal, "HPP invoice " + invoice.Code);

            accountingResultList = AddToAccountingResultList(accountingResultList, invoice, (long)PostingAccount.PersediaanDalamPerjalanan, AccountingResultType.Credit, basePriceTotal, "Persediaan dalam perjalanan invoice " + invoice.Code);

            new AccountingResultBFC().Posting(accountingResultList);
        }

        private List<AccountingResultModel> AddToAccountingResultList(List<AccountingResultModel> resultList, InvoiceModel obj, long accountID, AccountingResultType resultType, decimal amount, string remarks)
        {
            if (amount > 0)
            {
                var account = new AccountBFC().RetrieveByID(accountID);
                var result = new AccountingResultModel();

                result.DocumentID = obj.ID;
                result.DocumentType = (int)AccountingResultDocumentType.Invoice;
                result.Type = (int)resultType;
                result.Date = obj.Date;
                result.AccountID = account.ID;
                result.DocumentNo = obj.Code;
                result.Amount = amount;

                if (resultType == AccountingResultType.Debit)
                    result.DebitAmount = amount;
                else
                    result.CreditAmount = amount;

                result.Remarks = remarks;

                resultList.Add(result);
            }

            return resultList;
        }
        #endregion

        #region Notification

        public int RetrieveUncreatedPaymentCount(CustomerGroupModel customerGroup)
        {
            return new ABCAPOSDAC().RetrieveUncreatedPaymentInvoiceCount(customerGroup);
        }

        public int RetrieveUnapprovedInvoiceCount(CustomerGroupModel customerGroup)
        {
            return new ABCAPOSDAC().RetrieveUnapprovedInvoiceCount(customerGroup);
        }

        public int RetrieveUnpaidCount(CustomerGroupModel customerGroup)
        {
            return new ABCAPOSDAC().RetrieveUnpaidInvoiceCount(customerGroup);
        }

        public int RetrieveOverdueInvoiceCount(CustomerGroupModel customerGroup)
        {
            return new ABCAPOSDAC().RetrieveOverdueInvoiceCount(customerGroup);
        }

        public int RetrieveNotOverdueCount()
        {
            return new ABCAPOSDAC().RetrieveNotOverdueInvoiceCount();
        }

        public string RetrieveThisMonthInvoiceCount()
        {
            return new ABCAPOSDAC().RetrieveThisMonthInvoiceCount();
        }

        public string RetrieveThisMonthOutstandingInvoiceCount()
        {
            return new ABCAPOSDAC().RetrieveThisMonthOutstandingInvoiceCount();
        }

        public List<InvoiceDetailModel> RetrieveDetailsBySOID(long soID)
        {
            return new ABCAPOSDAC().RetrieveInvoiceDetailBySOID(soID);
        }

        public string RetrieveAveragePaymentDays(long customerID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Invoice
                        where (i.StatusDesc == "New" || i.StatusDesc == "Unpaid") && i.OutstandingAmount > 0 && i.DueDate < MPL.Configuration.CurrentDateTime && i.CreatedBy != "MIGRATION"
                        select i;

            if (customerID != 0)
                query = query.Where(p => p.CustomerID == customerID);
            
            return Convert.ToDecimal(query.Average(p => SqlFunctions.DateDiff("day",p.DueDate,MPL.Configuration.CurrentDateTime))).ToString("N0") + " days"; //"Rp. " + Convert.ToDecimal(query.Sum(p => p.SubTotal)).ToString("N0")
        }

        public List<InvoiceModel> RetreiveInvByCustomerID(long customerID)
        {
            return new ABCAPOSDAC().RetreiveInvByCustomerID(customerID);
        }

        public List<InvoiceModel> RetrieveAveragePaymentOverDue(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveAveragePaymentOverDue(startIndex, amount, sortParameter, selectFilters);
        }

        public int RetrieveAveragePaymentOverDueCount(List<SelectFilter> selectFilter)
        {
            return new ABCAPOSDAC().RetrieveAveragePaymentOverDueCount(selectFilter);
        }

        #endregion

        //public void voidFromOldQty(InvoiceModel header, List<InvoiceDetailModel> details)
        //{
        //    var oldDetails = RetrieveDetails(header.ID);

        //    foreach (var detail in details)
        //    {
        //        detail.SelisihQuantity = -detail.Quantity;
        //    }

        //    //UpdateInvItemLocationBin(header, details);


        //}

        //public void OnVoid(long invoiceID, string voidRemarks, string userName)
        //{
        //    var invoice = RetrieveByID(invoiceID);
        //    var invoiceDetails = RetrieveDetails(invoiceID);

        //    voidFromOldQty(invoice, invoiceDetails);
        //}

    }
}
