using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class InvoiceByItemSumModel
    {
        public long WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public string ProductCode { get; set; }
        public string ItemProduct { get; set; }
        public string ItemBrand { get; set; }
        public string ProductName { get; set; }
        public double Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public string ConversionName { get; set; }
    }
}
