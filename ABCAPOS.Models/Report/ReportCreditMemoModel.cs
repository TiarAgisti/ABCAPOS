using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class ReportCreditMemoModel
    {
        public long ID { get; set; }
        public long CustomerReturnID { get; set; }
        public string Code { get; set; }
        public long WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public long CustomerID { get; set; }
        public string CustomerName { get; set; }
        public DateTime Date { get; set; }
        public int Status { get; set; }
        public string RefSO { get; set; }
        public long ProductID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public double Quantity { get; set; }
        public int ConversionID { get; set; }
        public string ConversionName { get; set; }
        public string SalesReference { get; set; }
        public decimal Price { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPPN { get; set; }
        public decimal Total { get; set; }

    }
}
