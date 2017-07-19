using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class BinProductWarehouseModel
    {
        public long ID { get; set; }
        public long ProductID { get; set; }
        public long WarehouseID { get; set; }
        public long BinID { get; set; }
        public double Quantity { get; set; }
        public bool IsDefaultBin { get; set; }

        public string ProductName { get; set; }
        public string BinName { get; set; }
        public string WarehouseName { get; set; }

    }
}
