using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;

namespace ABCAPOS.Models
{
    public class ProductDetailModel
    {
        public long ProductID { get; set; }
        public int ItemNo { get; set; }
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public decimal AssetPrice { get; set; }
        public decimal AssetPriceInDollar { get; set; }
        public double Quantity { get; set; }
        public double QtyStart { get; set; }
        public double QtyAvailable { get; set; }
        public long PurchaseOrderID { get; set; }
        public long WarehouseID { get; set; }
        public long PurchaseDeliveryID { get; set; }
        public int PurchaseOrderItemNo { get; set; }
        public int PurchaseDeliveryItemNo { get; set; }
        public decimal Discount { get; set; }
        public string PurchaseOrderCode { get; set; }
        public bool IsSaved { get; set; }
        public long UnitID { get; set; }
        //public int PurchaseOrderStatus { get; set; }

        //public string PurchaseOrderStatusDesc
        //{
        //    get
        //    {
        //        if (PurchaseOrderStatus == (int)MPL.DocumentStatus.New)
        //            return "Pending Authorization";
        //        else if (PurchaseOrderStatus == (int)MPL.DocumentStatus.Approved)
        //            return "Pending Fulfilment";
        //        else if (PurchaseOrderStatus == 4)
        //            return "Billed";
        //        else if (PurchaseOrderStatus == (int)MPL.DocumentStatus.Void)
        //            return "Void";
        //        else
        //            return "";
        //    }
        //}

        public ProductDetailModel()
        {
            Date = DateTime.Now;
            IsSaved = false;
        }
    }
}
