﻿using System;
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
    public class PurchasePaymentBFC : GenericTransactionBFC<PurchasePayment, v_PurchasePayment, PurchasePaymentModel>
    {

        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetPurchasePaymentCode(PurchasePaymentModel header)
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var purchasePaymentPrefix = "";

            if (prefixSetting != null)
                purchasePaymentPrefix = prefixSetting.PurchasePaymentPrefix;

            var warehouse = new WarehouseBFC().RetrieveByID(header.WarehouseID);
            var year = DateTime.Now.Year.ToString().Substring(2, 2);

            var prefix = purchasePaymentPrefix + year + "-" + warehouse.Code + "-";
            var code = new ABCAPOSDAC().RetrievePurchasePaymentMaxCode(prefix, 7);

            return code;
        }

        protected override GenericDAC<PurchasePayment, PurchasePaymentModel> GetDAC()
        {
            return new GenericDAC<PurchasePayment, PurchasePaymentModel>("ID", false, "Code DESC");
        }

        protected override GenericDAC<v_PurchasePayment, PurchasePaymentModel> GetViewDAC()
        {
            return new GenericDAC<v_PurchasePayment, PurchasePaymentModel>("ID", false, "Code DESC");
        }

        private void OnUpdated(long purchasePaymentID)
        {
            var purchasePayment = RetrieveByID(purchasePaymentID);

            if (purchasePayment != null)
            {
                var purchaseBill = new PurchaseBillBFC().RetrieveByID(purchasePayment.PurchaseBillID);

                if (purchaseBill != null)
                {
                    var purchasePaymentList = RetrieveByBill(purchaseBill.ID);

                    purchaseBill.CreatedPaymentAmount = purchasePaymentList.Sum(p => p.Amount);

                    new PurchaseBillBFC().Update(purchaseBill);
                }
            }
        }

        public void OnApproved(long purchasePaymentID, string userName)
        {
            var purchasePayment = RetrieveByID(purchasePaymentID);

            if (purchasePayment != null)
            {
                var purchaseOrder = new PurchaseOrderBFC().RetrieveByID(purchasePayment.PurchaseOrderID);
                
                if (purchaseOrder != null)
                {
                    var purchaseBill = new PurchaseBillBFC().RetrieveByID(purchasePayment.PurchaseBillID);
                    var purchasePaymentList = RetrieveByBill(purchaseOrder.ID).Where(p => p.Status == (int)MPL.DocumentStatus.New);

                    var paidAmount = purchasePaymentList.Sum(p => p.Amount);
                    purchaseBill.CreatedPaymentAmount = paidAmount;
                    purchaseBill.ApprovedPaymentAmount = paidAmount;

                    if (paidAmount == Convert.ToDecimal(purchaseBill.GrandTotal))
                    {
                        //purchaseOrder.Status = (int)PurchaseOrderStatus.Paid;
                        purchaseBill.Status = (int)PurchaseBillStatus.Paid;
                        new PurchaseBillBFC().Update(purchaseBill);
                    }
                    else
                    {
                        //purchaseOrder.Status = (int)PurchaseOrderStatus.PartialBill;
                    }

                    //new PurchaseOrderBFC().Update(purchaseOrder);

                    //new AccountingResultBFC().Void(purchasePaymentID, AccountingResultDocumentType.PurchasePayment);
                    //PostAccounting(purchasePaymentID, userName);
                }
            }
        }

        public void CreateByBill(PurchasePaymentModel purchasePayment, long purchaseBillID)
        {
            var purchaseBill = new PurchaseBillBFC().RetrieveByID(purchaseBillID);
            
            if (purchaseBill != null)
            {
                var purchaseOrder = new PurchaseOrderBFC().RetrieveByID(purchaseBill.PurchaseOrderID);

                purchasePayment.WarehouseID = purchaseOrder.WarehouseID;
                purchasePayment.Code = SystemConstants.autoGenerated;//GetPurchasePaymentCode();
                purchasePayment.PurchaseOrderID = purchaseBill.PurchaseOrderID;
                purchasePayment.PurchaseBillID = purchaseBillID;
                purchasePayment.DueDate = purchaseBill.DueDate;
                purchasePayment.PurchaseBillCode = purchaseBill.Code;
                purchasePayment.VendorCode = purchaseBill.VendorCode;
                purchasePayment.VendorName = purchaseBill.VendorName;
                purchasePayment.SupplierInvNo = purchaseBill.SupplierInvNo;
                purchasePayment.CurrencyID = purchaseOrder.CurrencyID;
                purchasePayment.CurrencyName = purchaseOrder.CurrencyName;
                purchasePayment.ExchangeRate = purchaseOrder.ExchangeRate;
                purchasePayment.PurchaseBillAmount = Convert.ToDecimal(purchaseBill.GrandTotal);
                //var purchasePaymentList = RetrieveByBill(purchaseBill.ID);
                //purchasePayment.OutstandingAmount = purchasePaymentList.Sum(p => p.Amount);
                purchasePayment.Amount = purchaseBill.OutstandingAmount;
                purchasePayment.SisaAmount = purchaseBill.OutstandingAmount;
                //purchasePayment.PaymentMethodID = purchaseBill.PaymentMethodID;
                //purchasePayment.PaymentMethodName = purchaseBill.PaymentMethodName;
            }
        }

        public override void Create(PurchasePaymentModel dr)
        {
            dr.Code = GetPurchasePaymentCode(dr);

            using (TransactionScope trans = new TransactionScope())
            {
                base.Create(dr);
                OnUpdated(dr.ID);

                trans.Complete();
            }
            OnApproved(dr.ID, dr.CreatedBy);
            new PurchaseOrderBFC().UpdateStatus(dr.PurchaseOrderID);
        }

        public override void Update(PurchasePaymentModel dr)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                base.Update(dr);
                OnUpdated(dr.ID);

                trans.Complete();
            }
            OnApproved(dr.ID, dr.CreatedBy);
        }

        public void Approve(long purchasePaymentID, string userName)
        {
            var purchasePayment = RetrieveByID(purchasePaymentID);

            purchasePayment.Status = (int)MPL.DocumentStatus.Approved;
            purchasePayment.ApprovedBy = userName;
            purchasePayment.ApprovedDate = DateTime.Now;

            using (TransactionScope trans = new TransactionScope())
            {
                GetDAC().Update(purchasePayment);
                OnApproved(purchasePaymentID, userName);

                trans.Complete();
            }
        }

        public void Void(long purchasePaymentID, string voidRemarks, string userName)
        {
            var purchasePayment = RetrieveByID(purchasePaymentID);

            purchasePayment.Status = (int)MPL.DocumentStatus.Void;
            purchasePayment.VoidRemarks = voidRemarks;
            purchasePayment.ApprovedBy = "";
            purchasePayment.ApprovedDate = SystemConstants.UnsetDateTime;

            using (TransactionScope trans = new TransactionScope())
            {
                GetDAC().Update(purchasePayment);
                //OnApproved(purchasePaymentID);
                new AccountingResultBFC().Void(purchasePaymentID, AccountingResultDocumentType.PurchasePayment);

                trans.Complete();
            }
        }

        public void Validate(PurchasePaymentModel purchasePayment)
        {
            var obj = RetrieveByID(purchasePayment.ID);
            var purchaseBill = new PurchaseBillBFC().RetrieveByID(purchasePayment.PurchaseBillID );

            var createdPaymentAmount = purchaseBill.CreatedPaymentAmount;

            if (obj != null)
                purchaseBill.CreatedPaymentAmount -= obj.Amount;

            if (purchaseBill.OutstandingAmount < purchasePayment.Amount)
                throw new Exception("Amount paid is more than outstanding amount.");
        }

        //public void PostAccounting(long purchasePaymentID, string userName)
        //{
        //    var accountingResultList = new AccountingResultBFC().Retrieve(purchasePaymentID, AccountingResultDocumentType.PurchasePayment);

        //    if (accountingResultList == null || accountingResultList.Count == 0)
        //    {
        //        var purchasePayment = RetrieveByID(purchasePaymentID);
        //        var purchase = new PurchaseOrderBFC().RetrieveByID(purchasePayment.PurchaseOrderID);

        //        var result = new AccountingResultModel();

        //        result = new AccountingResultModel();
        //        result.DocumentID = purchasePayment.ID;
        //        result.DocumentType = (int)AccountingResultDocumentType.PurchasePayment;
        //        result.Type = (int)AccountingResultType.Credit;
        //        result.Date = purchasePayment.Date;
        //        result.AccountID = SystemConstants.OutcomeAccount;
        //        result.DocumentNo = purchasePayment.Code;
        //        result.Amount = purchasePayment.Amount * purchasePayment.ExchangeRate - ( purchasePayment.Amount * (purchasePayment.ExchangeRate - purchase.ExchangeRate));
        //        result.DebitAmount = purchasePayment.Amount * purchasePayment.ExchangeRate - ( purchasePayment.Amount * (purchasePayment.ExchangeRate - purchase.ExchangeRate));
        //        result.Remarks = purchasePayment.VendorName + "," + purchasePayment.Code;
        //        accountingResultList.Add(result);

        //        if (purchasePayment.ExchangeRate != purchase.ExchangeRate)
        //        {
        //            result = new AccountingResultModel();
        //            result.DocumentID = purchasePayment.ID;
        //            result.DocumentType = (int)AccountingResultDocumentType.PurchasePayment;
        //            result.Type = (int)AccountingResultType.Credit;
        //            result.Date = purchasePayment.Date;
        //            result.AccountID = SystemConstants.SelisihKursAccount;
        //            result.DocumentNo = purchasePayment.Code;
        //            result.Amount = purchasePayment.Amount * (purchasePayment.ExchangeRate - purchase.ExchangeRate);
        //            result.DebitAmount = purchasePayment.Amount * (purchasePayment.ExchangeRate - purchase.ExchangeRate);
        //            result.Remarks = purchasePayment.VendorName + "," + purchasePayment.Code;
        //            accountingResultList.Add(result);
        //        }

        //        result = new AccountingResultModel();
        //        result.DocumentID = purchasePayment.ID;
        //        result.DocumentType = (int)AccountingResultDocumentType.PurchasePayment;
        //        result.Type = (int)AccountingResultType.Debit;
        //        result.Date = purchasePayment.Date;
        //        //Kas
        //        result.AccountID = 3;
        //        result.DocumentNo = purchasePayment.Code;
        //        result.Amount = purchasePayment.Amount * purchasePayment.ExchangeRate;
        //        result.CreditAmount = purchasePayment.Amount * purchasePayment.ExchangeRate;
        //        result.Remarks = purchasePayment.VendorName + "," + purchasePayment.Code;
        //        accountingResultList.Add(result);

        //        if (purchasePayment.ExchangeRate != purchase.ExchangeRate)
        //        {
        //            result = new AccountingResultModel();
        //            result.DocumentID = purchasePayment.ID;
        //            result.DocumentType = (int)AccountingResultDocumentType.PurchasePayment;
        //            result.Type = (int)AccountingResultType.Credit;
        //            result.Date = purchasePayment.Date;
        //            result.AccountID = SystemConstants.SelisihKursAccount;
        //            result.DocumentNo = purchasePayment.Code;
        //            result.Amount = purchasePayment.Amount * (purchasePayment.ExchangeRate - purchase.ExchangeRate);
        //            result.DebitAmount = purchasePayment.Amount * (purchasePayment.ExchangeRate - purchase.ExchangeRate);
        //            result.Remarks = purchasePayment.VendorName + "," + purchasePayment.Code;
        //            accountingResultList.Add(result);

        //            result = new AccountingResultModel();
        //            result.DocumentID = purchasePayment.ID;
        //            result.DocumentType = (int)AccountingResultDocumentType.PurchasePayment;
        //            result.Type = (int)AccountingResultType.Debit;
        //            result.Date = purchasePayment.Date;
        //            result.AccountID = SystemConstants.OutcomeAccount;
        //            result.DocumentNo = purchasePayment.Code;
        //            result.Amount = purchasePayment.Amount * (purchasePayment.ExchangeRate - purchase.ExchangeRate);
        //            result.CreditAmount = purchasePayment.Amount * (purchasePayment.ExchangeRate - purchase.ExchangeRate);
        //            result.Remarks = purchasePayment.VendorName + "," + purchasePayment.Code;
        //            accountingResultList.Add(result);

        //        }

        //        new AccountingResultBFC().Posting(accountingResultList);
        //    }
        //}

        public List<PurchasePaymentModel> RetrieveByBill(long purchaseBillID)
        {
            return new ABCAPOSDAC().RetrievePurchasePaymentByBill(purchaseBillID);
        }

        #region Notification

        public int RetrieveUnapprovedPurchasePaymentCount()
        {
            return new ABCAPOSDAC().RetrieveUnapprovedPurchasePaymentCount();
        }

        public int RetrieveVoidPurchasePaymentCount()
        {
            return new ABCAPOSDAC().RetrieveVoidPurchasePaymentCount();
        } 

        #endregion

    }
}
