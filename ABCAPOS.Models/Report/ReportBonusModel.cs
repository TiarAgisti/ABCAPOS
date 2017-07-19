using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class ReportBonusModel
    {
        public string MakeMultiPaySalesCode { get; set; }
        public DateTime Date { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime InvoiceDueDate { get; set; }
        public string WarehouseName { get; set; }
        public string InvoiceCode { get; set; }
        public string DeliveryOrderCodeList { get; set; }
        public int DaysOverdue { get; set; }
        public long CustomerID { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal PaymentAmount { get; set; }
        public long WarehouseID { get; set; }
        public decimal OutstandingAmount { get; set; }
        public string SalesReference { get; set; }

    }
}
