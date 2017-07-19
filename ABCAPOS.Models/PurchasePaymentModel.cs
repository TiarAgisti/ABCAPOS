using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;
namespace ABCAPOS.Models
{
    public class PurchasePaymentModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        public long PurchaseOrderID { get; set; }
        public long PurchaseBillID { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public DateTime DueDate { get; set; }
        public long PaymentMethodID { get; set; }
        public decimal Amount { get; set; }
        public string VoidRemarks { get; set; }
        public string Remarks { get; set; }
        public int Status { get; set; }
        public decimal ExchangeRate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
        public string PurchaseOrderCode { get; set; }
        public string PurchaseBillCode { get; set; }
        public string VendorCode { get; set; }
        public string VendorName { get; set; }
        public string POSupplierNo { get; set; }
        public string SupplierInvNo { get; set; }
        public string PaymentMethodName { get; set; }
        public decimal SisaAmount { get; set; }
        public decimal PurchaseBillAmount { get; set; }
        public long CurrencyID { get; set; }
        public string CurrencyName { get; set; }
        public long WarehouseID { get; set; }

        public double OutstandingAmount
        {
            get
            {
                return Convert.ToDouble(PurchaseBillAmount - Amount);
            }
        }
        public string StatusDescription
        {
            get
            {
                if (Status == (int)MPL.DocumentStatus.New)
                    return MPL.DocumentStatus.New.ToString();
                else if (Status == (int)MPL.DocumentStatus.Approved)
                    return MPL.DocumentStatus.Approved.ToString();
                else if (Status == (int)MPL.DocumentStatus.Void)
                    return MPL.DocumentStatus.Void.ToString();
                else
                    return "";
            }
        }

        public PurchasePaymentModel()
        {
            Date = DateTime.Now;
            Status = (int)MPL.DocumentStatus.New;
        }
    }
}
