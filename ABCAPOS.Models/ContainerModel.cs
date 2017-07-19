using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class ContainerModel
    {
        /* field table */
        public long ID { get; set; }
        public long ContainerID { get; set; }
        public long ProductID { get; set; }
        public long WarehouseID { get; set; }
        public double Qty { get; set; }
        public double Price { get; set; }


        /* field table */
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string WarehouseName { get; set; }
        public double Pemakaian { get; set; }
        /* field view */

        /* field view */
      

    }
}
