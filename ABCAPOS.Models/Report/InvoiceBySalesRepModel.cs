using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class InvoiceBySalesRepModel
    {
        public long ID { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string SalesReference { get; set; }
        public long WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
