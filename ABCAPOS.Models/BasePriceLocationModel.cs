using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class BasePriceLocationModel
    {
        public long ID { get; set; }
        public long ProductID { get; set; }
        public int ItemNo { get; set; }
        public long WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public string ProductName { get; set; }
        public decimal BasePrice { get; set; }
    }
}
