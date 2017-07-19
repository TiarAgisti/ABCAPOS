using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class ReportOverdueModel
    {
        public long ID { get; set; }
        public string InvoiceCode { get; set; }
        public DateTime Date { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
        public long CustomerID { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public long WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public decimal OutstandingAmount { get; set; }
        public string SalesReference { get; set; }

    }
}
