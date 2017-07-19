using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class InvoiceMonthlyModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        public long SalesOrderID { get; set; }
        public string DeliveryOrderCodeList { get; set; }

        public DateTime Date { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public string CustomerName { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentCode { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal DiscountTaken { get; set; }
        public decimal ApplyPayment { get; set; }
        public string AccountName { get; set; }
        public string PaymentMethodName { get; set; }
        public string StatusDesc { get; set; }
        public string TypeName { get; set; }
        public string SalesReference { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }
    }
}
