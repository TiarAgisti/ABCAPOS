using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class MakeMultiPaySalesDetailModel
    {
        public long MakeMultiPaySalesID { get; set; }
        public int ItemNo { get; set; }
        public long InvoiceID { get; set; }
        public decimal Amount { get; set; }
        public decimal DiscountTaken { get; set; }
        public string Remarks { get; set; }
        public string Code { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal CreditedAmount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime Date { get; set; }
        public bool isAutoApply { get; set; }

        public decimal OriginalAmount { get; set; }
        public decimal AmountDue { get; set; }

        // readonly values (from view)
        public string PaymentCode { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime PaymentEventDate { get; set; }
    }
}
