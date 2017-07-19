﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using ABCAPOS.DA;
using ABCAPOS.Util;
using System.Transactions;
using ABCAPOS.ReportEDS;
using MPL;

namespace ABCAPOS.BF
{
    public class MakeMultiPaySalesBFC : MasterDetailBFC<MakeMultiPaySales, v_MakeMultiPaySales, MakeMultiPaySalesDetail, v_MakeMultiPaySalesDetail, MakeMultiPaySalesModel, MakeMultiPaySalesDetailModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetMakeMultiPaySalesCode(MakeMultiPaySalesModel header)
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var makeMultiPaySalesPrefix = "";

            if (prefixSetting != null)
                makeMultiPaySalesPrefix = prefixSetting.MakeMultiPaySalesPrefix;

            var warehouse = new WarehouseBFC().RetrieveByID(header.WarehouseID);
            var year = DateTime.Now.Year.ToString().Substring(2, 2);

            var prefix = makeMultiPaySalesPrefix + year + "-" + warehouse.Code + "-";
            var code = new ABCAPOSDAC().RetrieveMakeMultiPaymentSalesMaxCode(prefix, 7);

            return code;
        }

        protected override GenericDetailDAC<MakeMultiPaySalesDetail, MakeMultiPaySalesDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<MakeMultiPaySalesDetail, MakeMultiPaySalesDetailModel>("MakeMultiPaySalesID", "ItemNo", false);
        }

        protected override GenericDetailDAC<v_MakeMultiPaySalesDetail, MakeMultiPaySalesDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_MakeMultiPaySalesDetail, MakeMultiPaySalesDetailModel>("MakeMultiPaySalesID", "ItemNo", false);
        }

        protected override GenericDAC<MakeMultiPaySales, MakeMultiPaySalesModel> GetMasterDAC()
        {
            return new GenericDAC<MakeMultiPaySales, MakeMultiPaySalesModel>("ID", false, "Date DESC");
        }

        protected override GenericDAC<v_MakeMultiPaySales, MakeMultiPaySalesModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_MakeMultiPaySales, MakeMultiPaySalesModel>("ID", false, "Date DESC");
        }

        public override void Create(MakeMultiPaySalesModel header, List<MakeMultiPaySalesDetailModel> details)
        {
            header.Code = GetMakeMultiPaySalesCode(header);
            using (TransactionScope trans = new TransactionScope())
            {
                if (header.ExchangeRate == 0)
                {
                    header.ExchangeRate = 1;
                }

                base.Create(header, details);

                PostAccounting(header.ID,header.Status);

                trans.Complete();
            }
        }

        public override void Update(MakeMultiPaySalesModel header, List<MakeMultiPaySalesDetailModel> details)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                base.Update(header, details);

                PostAccounting(header.ID,header.Status);

                trans.Complete();
            }
        }

        public void Void(long MakeMultiPaySalesID, string voidRemarks, string userName)
        {
            var MakeMultiPaySales = RetrieveByID(MakeMultiPaySalesID);
            var oldStatus = MakeMultiPaySales.Status;
            MakeMultiPaySales.Status = (int)MPL.DocumentStatus.Void;
            MakeMultiPaySales.VoidRemarks = voidRemarks;
            MakeMultiPaySales.ApprovedDate = DateTime.Now;
            MakeMultiPaySales.ApprovedBy = userName;

            using (TransactionScope trans = new TransactionScope())
            {
                PostAccounting(MakeMultiPaySalesID,MakeMultiPaySales.Status);

                Update(MakeMultiPaySales);

                trans.Complete();
            }

        }

        public void PreFillWithInvoiceData(MakeMultiPaySalesModel header, long p)
        {
            var invoiceBFC = new InvoiceBFC();
            var source = invoiceBFC.RetrieveByID(p);
            if (source == null)
                return;

            header.Code = SystemConstants.autoGenerated;//GetMakeMultiPaySalesCode();
            header.Date = DateTime.Now;
            header.CustomerID = source.CustomerID;
            header.CustomerName = source.CustomerName;
            header.CurrencyID = source.CurrencyID;
            header.CurrencyName = source.CurrencyName;
            header.ExchangeRate = source.ExchangeRate;
            header.Title = source.Remarks;
            // prepopulate "Sum" field with bill's due amount
            header.AmountHelp = source.Amount + source.TaxAmount - source.PaymentAmount - source.CreditedAmount;
            header.TotalPayment = header.AmountHelp;
            header.AmountChecked = header.TotalPayment;

            var multiDetails = new List<MakeMultiPaySalesDetailModel>();

            //first detail
            var newDetail = new MakeMultiPaySalesDetailModel();
            newDetail.InvoiceID = source.ID;
            newDetail.Code = source.Code;
            newDetail.Subtotal = source.Amount;
            newDetail.TaxTotal = source.TaxAmount;
            newDetail.OriginalAmount = newDetail.Subtotal + newDetail.TaxTotal + Convert.ToDecimal(source.ShippingAmount);
            newDetail.PaymentAmount = source.PaymentAmount;
            newDetail.CreditedAmount = source.CreditedAmount;
            newDetail.AmountDue = newDetail.OriginalAmount - newDetail.PaymentAmount - newDetail.CreditedAmount;
            newDetail.DueDate = source.DueDate;
            newDetail.Date = source.Date;
            newDetail.Amount = newDetail.AmountDue;
            
            newDetail.isAutoApply = true;

            multiDetails.Add(newDetail);

            var payableInvoices = new InvoiceBFC().RetrievePayableInvoices(header.CustomerID);
            
            foreach (var invoice in payableInvoices)
            {
                if (invoice.ID != p)
                {
                    var detail = new MakeMultiPaySalesDetailModel();
                    detail.InvoiceID = invoice.ID;
                    detail.Code = invoice.Code;
                    detail.Subtotal = invoice.Amount;
                    detail.TaxTotal = invoice.TaxAmount;
                    detail.OriginalAmount = detail.Subtotal + detail.TaxTotal + Convert.ToDecimal(invoice.ShippingAmount);
                    detail.PaymentAmount = invoice.PaymentAmount;
                    detail.CreditedAmount = invoice.CreditedAmount;
                    detail.AmountDue = detail.OriginalAmount - detail.PaymentAmount - detail.CreditedAmount;
                    detail.DueDate = invoice.DueDate;
                    detail.Date = invoice.Date;
                    
                    if (Math.Round(detail.AmountDue,2) > 0)
                        multiDetails.Add(detail);

                    //multiDetails.Add(detail);
                }
            }

            header.Details = multiDetails;
        }

        public void AddOtherPayableInvoices(MakeMultiPaySalesModel header, List<MakeMultiPaySalesDetailModel> details)
        {
            var payableInvoices = new InvoiceBFC().RetrievePayableInvoices(header.CustomerID);

            var invoiceIDs = details.Select(p => p.InvoiceID);

            foreach (var invoice in payableInvoices)
            {
                if (!invoiceIDs.Contains(invoice.ID))
                {
                    var detail = new MakeMultiPaySalesDetailModel();
                    detail.InvoiceID = invoice.ID;
                    detail.Code = invoice.Code;
                    detail.Subtotal = invoice.Amount;
                    detail.TaxTotal = invoice.TaxAmount;
                    detail.OriginalAmount = detail.Subtotal + detail.TaxTotal;
                    detail.PaymentAmount = invoice.PaymentAmount;
                    detail.CreditedAmount = invoice.CreditedAmount;
                    detail.AmountDue = detail.OriginalAmount - detail.PaymentAmount - detail.CreditedAmount;
                    detail.DueDate = invoice.DueDate;
                    detail.Date = invoice.Date;

                    if (Math.Round(detail.AmountDue, 2) > 0)
                        details.Add(detail);

                    //details.Add(detail);
                }
            }
        }

        public List<MakeMultiPaySalesDetailModel> RetrieveByInvoiceID(long invoiceID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var payDetailsQuery = from detail in ent.v_MakeMultiPaySalesDetail
                                  where detail.InvoiceID == invoiceID 
                                    && detail.Status != (int)MPL.DocumentStatus.Void
                                  select detail;
            return ObjectHelper.CopyList<v_MakeMultiPaySalesDetail, MakeMultiPaySalesDetailModel>(payDetailsQuery.ToList());
        }

        public List<MakeMultiPaySalesModel> RetrievePaymentSoldList()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PaymentSold
                        select i;

            return ObjectHelper.CopyList<v_PaymentSold, MakeMultiPaySalesModel>(query.ToList());
        }

        public List<MakeMultiPaySalesModel> RetreiveDataPostingUlangInvoicePayment()
        {
            return new ABCAPOSDAC().RetreiveDataPostingUlangInvoicePayment();
        }

        public List<MakeMultiPaySalesModel> RetreiveInvPaymentByCustomerID(long customerID)
        {
            return new ABCAPOSDAC().RetreiveInvPaymentByCustomerID(customerID);
        }

        #region Post to Accounting Result

        public void PostAccounting(long paymentID, int Status)
        {
            var payment = RetrieveByID(paymentID);

            new ABCAPOSDAC().DeleteAccountingResults(paymentID, AccountingResultDocumentType.InvoicePayment);

            if (Status != (int)MPL.DocumentStatus.Void)
                CreateAccountingResult(paymentID);
        }

        private void CreateAccountingResult(long paymentID)
        {
            var payment = RetrieveByID(paymentID);

            decimal totalAmount = 0;

            var accountingResultList = new List<AccountingResultModel>();

            if (payment.ExchangeRate == 0)
            {
                payment.ExchangeRate = 1;
            }

            totalAmount = payment.TotalPayment * payment.ExchangeRate;

            accountingResultList = AddToAccountingResultList(accountingResultList, payment, (long)payment.AccountID, AccountingResultType.Debit, totalAmount, "Kas/bank invoice payment" + payment.Code);

            accountingResultList = AddToAccountingResultList(accountingResultList, payment, (long)PostingAccount.PiutangDagang, AccountingResultType.Credit, totalAmount, "Piutang dagang invoice payment " + payment.Code);

            new AccountingResultBFC().Posting(accountingResultList);
        }

        private List<AccountingResultModel> AddToAccountingResultList(List<AccountingResultModel> resultList, MakeMultiPaySalesModel obj, long accountID, AccountingResultType resultType, decimal amount, string remarks)
        {
            if (amount > 0)
            {
                var account = new AccountBFC().RetrieveByID(accountID);
                var result = new AccountingResultModel();

                result.DocumentID = obj.ID;
                result.DocumentType = (int)AccountingResultDocumentType.InvoicePayment;
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

        //public void Validate(MakeMultiPaySalesModel obj, List<MakeMultiPaySalesDetailModel> details)
        //{
        //foreach (var detail in details)
        //{
        //    var bill = new InvoiceBFC().RetrieveByID(detail.InvoiceID);
        //    if (detail.Amount + detail.DiscountTaken > bill.OutstandingAmount)
        //    {
        //        throw new Exception("Payment amount for " + detail.Code + " is greater than due");
        //    }
        //}
        //}

        //private void ValidationPayment(MakeMultiPaySalesModel header, List<MakeMultiPaySalesDetailModel> details)
        //{
        //    var totalApply = details.Sum(p => p.Amount);

        //    if (header.TotalPayment < totalApply)
        //    {
        //        //details.Clear();
        //        throw new Exception("Total apply payment tidak boleh lebih besar dari payment yang di terima");
        //    }
        //}
    }
}