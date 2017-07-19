using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class TransferDeliveryDetailModel
    {
        public long TransferDeliveryID { get; set; }
        public int ItemNo { get; set; }
        public int TransferOrderItemNo { get; set; }
        public long ProductID { get; set; }
        public double Quantity { get; set; }
        public long ConversionID { get; set; }
        public decimal Price { get; set; }
        public string ProductDate { get; set; }
        public string Remarks { get; set; }
        public long BinID { get; set; }
        public string StringQty { get; set; } // not sure what this is for

        public string Barcode { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }

        public double StockQty { get; set; }
        public double StockQtyHidden { get; set; }
        public double UnitRate { get; set; }

        public string ConversionName { get; set; }

        public string BinNumber { get; set; }
        public double QtyTO { get; set; }
        public double SelisihQuantity { get; set; }

        public decimal Total
        {
            get
            {
                return Convert.ToDecimal(Quantity) * Price;
            }
        }
    }
}
