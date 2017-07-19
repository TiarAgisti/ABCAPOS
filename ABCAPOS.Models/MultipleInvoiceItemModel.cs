using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class MultipleInvoiceItemModel
    {
        public long MultipleInvoicingID { get; set; }
        public int ItemNo { get; set; }
        public long InvoiceID { get; set; }
        public int InvoiceDetailItemNo { get; set; }
        public long ProductID { get; set; }
        public string ProductName { get; set; }
        public int TaxType { get; set; }
        public double Quantity { get; set; }
        public string ConversionName { get; set; }
        public decimal Price { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPPN { get; set; }
        public decimal GrossAmount { get; set; }
    }
}
