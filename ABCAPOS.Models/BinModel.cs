using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class BinModel
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public long WarehouseID { get; set; }
        public string Memo { get; set; }
        public string WarehouseName { get; set; }
        public bool IsActive { get; set; }
    }
}
