using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class MultipleInvoicingDetailModel
    {
        public long MultipleInvoicingID { get; set; }
        public int ItemNo { get; set; }
        public long InvoiceID { get; set; }
        public string InvoiceCode { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public string DeliveryOrderCodeList { get; set; }
        public string SalesOrderCode { get; set; }

        public int Status { get; set; }

        public decimal GrandTotal
        {
            get
            {
                return Amount + TaxAmount;
            }
        }

    }
}
