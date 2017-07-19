using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class TransferReceiptDetailModel
    {
        public long TransferReceiptID { get; set; }
        public int ItemNo { get; set; }
        public int TransferOrderItemNo { get; set; }
        public long ProductID { get; set; }
        public double Quantity { get; set; }
        public long BinID { get; set; }

        public string Barcode { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }

        public double StockQty { get; set; }
        public double StockQtyHidden { get; set; }

        public long ConversionID { get; set; }
        public string ConversionName { get; set; }
        public double UnitRate { get; set; }

        public string BinNumber { get; set; }
        public double QtyShipped { get; set; }

        public string ToWarehouseName { get; set; }
    }
}
