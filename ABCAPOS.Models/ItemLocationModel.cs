using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class ItemLocationModel
    {
        public long ID { get; set; }
        public long ProductID { get; set; }
        public string ProductCode { get; set; }
        public int ItemNo { get; set; }
        public double QtyOnHand { get; set; }
        public double QtyAvailable { get; set; }
        public long WarehouseID { get; set; }

        public string ProductName { get; set; }
        public string WarehouseName { get; set; }
        public decimal BasePrice { get; set; }

        public double Quantity { get; set; }

        public double BeginningQty { get; set; }
        public double MovingInQty { get; set; }
        public double MovingOutQty { get; set; }
    }
}
