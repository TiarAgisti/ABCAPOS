using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class ApplyCreditMemoDetailModel
    {
        public long ApplyCreditMemoID { get; set; }
        public int ItemNo { get; set; }
        public long InvoiceID { get; set; }
        public string CreditMemoCode { get; set; }
        public decimal Amount { get; set; }
        public string Remarks { get; set; }
        public string Code { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal CreditedAmount { get; set; }
        public DateTime DueDate { get; set; }
        public bool isAutoApply { get; set; }

        public decimal OriginalAmount { get; set; }
        public decimal AmountDue { get; set; }

        // readonly values (from string)
        public DateTime ApplyDate { get; set; }
        public string ApplyCode { get; set; }
    }
}
