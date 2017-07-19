using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using System.Transactions;
using ABCAPOS.Util;
using ABCAPOS.DA;

namespace ABCAPOS.BF
{
    public class PaymentBFC : GenericTransactionBFC<Payment, v_Payment, PaymentModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetPaymentCode(PaymentModel header)
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var paymentPrefix = "";

            if (prefixSetting != null)
                paymentPrefix = prefixSetting.PaymentPrefix;

            var warehouse = new WarehouseBFC().RetrieveByID(header.WarehouseID);
            var year = DateTime.Now.Year.ToString().Substring(2, 2);

            var prefix = paymentPrefix + year + "-" + warehouse.Code + "-";
            var code = new ABCAPOSDAC().RetrievePaymentMaxCode(paymentPrefix, 7);

            return code;
        }

        protected override GenericDAC<Payment, PaymentModel> GetDAC()
        {
            return new GenericDAC<Payment, PaymentModel>("ID", false, "Code DESC");
        }

        protected override GenericDAC<v_Payment, PaymentModel> GetViewDAC()
        {
            return new GenericDAC<v_Payment, PaymentModel>("ID", false, "Code DESC");
        }

        public override void Create(PaymentModel dr)
        {
            dr.Code = GetPaymentCode(dr);

            using (TransactionScope trans = new TransactionScope())
            {
                base.Create(dr);
                OnUpdated(dr.ID);

                trans.Complete();
            }
            OnApproved(dr.ID, dr.CreatedBy);
        }

        public override void Update(PaymentModel dr)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                base.Update(dr);
                OnUpdated(dr.ID);

                trans.Complete();
            }
            OnApproved(dr.ID, dr.CreatedBy);
        }

        public void Approve(long paymentID, string userName)
        {
            var payment = RetrieveByID(paymentID);

            payment.Status = (int)MPL.DocumentStatus.Approved;
            payment.ApprovedBy = userName;
            payment.ApprovedDate = DateTime.Now;

            using (TransactionScope trans = new TransactionScope())
            {
                GetDAC().Update(payment);
                OnApproved(paymentID, userName);

                trans.Complete();
            }
        }

        public void Void(long paymentID, string voidRemarks, string userName)
        {
            var payment = RetrieveByID(paymentID);

            payment.Status = (int)MPL.DocumentStatus.Void;
            payment.VoidRemarks = voidRemarks;
            payment.ApprovedBy = "";
            payment.ApprovedDate = SystemConstants.UnsetDateTime;

            using (TransactionScope trans = new TransactionScope())
            {
                GetDAC().Update(payment);
                //OnApproved(paymentID);
                new AccountingResultBFC().Void(paymentID, AccountingResultDocumentType.Payment);
                trans.Complete();
            }
        }

        public void OnUpdated(long paymentID)
        {
            var payment = RetrieveByID(paymentID);

            if (payment != null)
            {
                var invoice = new InvoiceBFC().RetrieveByID(payment.InvoiceID);

                if (invoice != null)
                {
                    var paymentList = RetrieveByInvoice(invoice.ID);

                    invoice.CreatedPaymentAmount = paymentList.Sum(p => p.Amount);

                    new InvoiceBFC().Update(invoice);
                }
            }
        }

        public void PostAccounting(long paymentID, string userName)
        {
            var accountingResultList = new AccountingResultBFC().Retrieve(paymentID, AccountingResultDocumentType.Payment);

            if (accountingResultList == null || accountingResultList.Count == 0)
            {
                var payment = RetrieveByID(paymentID);

                var result = new AccountingResultModel();

                result = new AccountingResultModel();
                result.DocumentID = payment.ID;
                result.DocumentType = (int)AccountingResultDocumentType.Payment;
                result.Type = (int)AccountingResultType.Credit;
                result.Date = payment.Date;
                //Kas
                result.AccountID = 3;
                result.DocumentNo = payment.Code;
                result.Amount = payment.Amount;
                result.DebitAmount = payment.Amount;
                result.Remarks = payment.CustomerName + "," + payment.Code;
                accountingResultList.Add(result);

                result = new AccountingResultModel();
                result.DocumentID = payment.ID;
                result.DocumentType = (int)AccountingResultDocumentType.Payment;
                result.Type = (int)AccountingResultType.Debit;
                result.Date = payment.Date;
                result.AccountID = SystemConstants.IncomeAccount;
                result.DocumentNo = payment.Code;
                result.Amount = payment.Amount;
                result.CreditAmount = payment.Amount;
                result.Remarks = payment.CustomerName + "," + payment.Code;
                accountingResultList.Add(result);

                new AccountingResultBFC().Posting(accountingResultList);
            }
        }

        public void OnApproved(long paymentID, string userName)
        {
            var payment = RetrieveByID(paymentID);

            if (payment != null)
            {
                var invoice = new InvoiceBFC().RetrieveByID(payment.InvoiceID);

                if (invoice != null)
                {
                    var salesOrder = new SalesOrderBFC().RetrieveByID(payment.SalesOrderID);

                    var paymentList = RetrieveByInvoice(invoice.ID).Where(p => p.Status == (int)MPL.DocumentStatus.New);

                    var paidAmount = paymentList.Sum(p => p.Amount);
                    invoice.CreatedPaymentAmount = paidAmount;
                    invoice.ApprovedPaymentAmount = paidAmount;

                    if (paidAmount == invoice.GrandTotal)
                    {
                        invoice.Status = (int)InvoiceStatus.Paid;
                        //quotation.Status = (int)QuotationStatus.Paid;
                    }
                    else
                    {
                        invoice.Status = (int)InvoiceStatus.Approved;
                        //quotation.Status = (int)QuotationStatus.Approved;
                    }

                    new InvoiceBFC().Update(invoice);
                    //new SalesOrderBFC().Update(salesOrder);

                    new AccountingResultBFC().Void(paymentID, AccountingResultDocumentType.Payment);
                    PostAccounting(paymentID, userName);
                }
            }
        }

        public void Validate(PaymentModel payment)
        {
            var obj = RetrieveByID(payment.ID);
            var invoice = new InvoiceBFC().RetrieveByID(payment.InvoiceID);

            var createdPaymentAmount = invoice.CreatedPaymentAmount;

            if (obj != null)
                invoice.CreatedPaymentAmount -= obj.Amount;

            if (invoice.OutstandingAmount < payment.Amount)
                throw new Exception("Jumlah yang sudah dibayar melebihi sisa tagihan");
        }

        public void CreateByInvoice(PaymentModel payment, long invoiceID)
        {
            var invoice = new InvoiceBFC().RetrieveByID(invoiceID);
            var salesOrder = new SalesOrderBFC().RetrieveByID(invoice.SalesOrderID);

            if (invoice != null)
            {
                payment.Code = SystemConstants.autoGenerated;//GetPaymentCode(payment);
                payment.WarehouseID = salesOrder.WarehouseID;

                payment.InvoiceID = invoiceID;
                payment.InvoiceCode = invoice.Code;
                payment.DeliveryOrderCode = invoice.DeliveryOrderCodeList;
                payment.CustomerCode = invoice.CustomerCode;
                payment.CustomerName = invoice.CustomerName;
                payment.InvoiceAmount = invoice.GrandTotal;
                payment.Amount = invoice.OutstandingAmount;
                payment.SisaAmount = invoice.OutstandingAmount;
                payment.PaymentMethodID = invoice.PaymentMethodID;
                payment.PaymentMethodName = invoice.PaymentMethodName;
            }
        }

        public List<PaymentModel> RetrieveByInvoice(long invoiceID)
        {
            return new ABCAPOSDAC().RetrievePaymentByInvoice(invoiceID);
        }

        public string RetrieveThisMonthPaymentCount()
        {
            return new ABCAPOSDAC().RetrieveThisPaymentCount();
        }

        #region Notification

        public int RetrieveUnapprovedPaymentCount(CustomerGroupModel customerGroup)
        {
            return new ABCAPOSDAC().RetrieveUnapprovedPaymentCount(customerGroup);
        }

        public int RetrieveVoidPaymentCount(CustomerGroupModel customerGroup)
        {
            return new ABCAPOSDAC().RetrieveVoidPaymentCount(customerGroup);
        } 

        #endregion
    }
}
