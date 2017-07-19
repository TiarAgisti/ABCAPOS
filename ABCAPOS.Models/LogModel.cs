using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;

namespace ABCAPOS.Models
{
    public class LogModel
    {
        /* Field table */
        public long ID { get; set; }
        public long WarehouseID { get; set; }
        public DateTime Date { get; set; }
        public int DocType { get; set; }
        public List<LogDetailModel> Details { get; set; }

        public LogModel()
        {
            Date = DateTime.Now;
            Details = new List<LogDetailModel>();
        }
        /* Field table */

        /* Field View */
        public string DocTypeDescription { get; set; }
        public string WarehouseName { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        /* Field View */
    }
}
