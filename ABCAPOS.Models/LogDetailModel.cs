using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class LogDetailModel
    {
        /* Field Table */
        public long ID { get; set; }
        public long LogID { get; set; }
        public int ItemNo { get; set; }
        public long ContainerID { get; set; }
        public long ProductID { get; set; }
        //public double BeginningQty { get; set; }
        //public double BeginningValue { get; set; }
        public double MovingInQty { get; set; }
        public double MovingInValue { get; set; }
        public double MovingOutQty { get; set; }
        public double MovingOutValue { get; set; }
        //public double EndingQty { get; set; }
        //public double EndingValue { get; set; }
        public long purchaseorderID { get; set; }
        public long PurchaseDeliveryID { get; set; }
        public long WarehouseID { get; set; }

        //Transaction Stock
        public string WarehouseName { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public double StockAwal { get; set; }
        //public double MovingInQty { get; set; }
        //public double MovingOutQty { get; set; }
        public double StockAkhir { get; set; }
        public double QtyOnHand { get; set; }
        public double Price { get; set; }
        public DateTime Date { get; set; }
        public double BeginningQty { get; set; }
        public string ItemTypeDesc { get; set; }
        //Transaction Stock

        public int ItemTypeID { get; set; }

        //Goods Sold By Product
        public double QtySold { get; set; }
        //Goods Sold By Product

        //Goods Sold By Customer
        public string SalesReference { get; set; }
        public string CustomerName { get; set; }
        //Goods Sold By Customer

        //Goods Returned By Customer
        public double QtyReturned { get; set; }


      
    }
}
