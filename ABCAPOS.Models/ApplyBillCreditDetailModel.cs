using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class ApplyBillCreditDetailModel
    {
        public long ApplyBillCreditID { get; set; }
        public int ItemNo { get; set; }
        public long PurchaseBillID { get; set; }
        public decimal Amount { get; set; }
        public string Remarks { get; set; }
        public string Code { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal CreditedAmount { get; set; }
        public DateTime DueDate { get; set; }

        public decimal OriginalAmount { get; set; }
        public decimal AmountDue { get; set; }

        // readonly values (from string)
        public DateTime ApplyDate { get; set; }
        public DateTime ApplyCode { get; set; }
    }
}
