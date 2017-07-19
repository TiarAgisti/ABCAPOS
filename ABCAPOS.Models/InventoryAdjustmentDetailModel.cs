using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class InventoryAdjustmentDetailModel
    {
        public long InventoryAdjustmentID { get; set; }
        public int ItemNo { get; set; }
        public DateTime Date { get; set; }
        public string InventoryAdjustmentCode { get; set; }
        public long WarehouseIDDetail { get; set; }
        public long ProductID { get; set; }
        public double QtyOnHandOld { get; set; }
        public double Quantity { get; set; }
        public double QtyOnHandNew { get; set; }
        public long ConversionID { get; set; }
        public long BinID { get; set; }
        public string Memo { get; set; }

        public string WarehouseNameDetail { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ConversionName { get; set; }
        public string BinNumber { get; set; }
        public double QtyOnHand { get; set; }
        public double QtyOnHandHidden { get; set; }

        public double NewQty { get; set; }

        /* Penamabahan Qty Available di Inventory Adjustment */
        public double QtyAvailable { get; set; }
        public double QtyAvailableOld { get; set; }
        public double QtyAvailableNew { get; set; }
        public double NewQtyAvailable { get; set; }
        public double QtyAvailableHidden { get; set; }
        public double QuantityAvailable { get; set; }
        /* Penamabahan Qty Available di Inventory Adjustment */
    }
}
