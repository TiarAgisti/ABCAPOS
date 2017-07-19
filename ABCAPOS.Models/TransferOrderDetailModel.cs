using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class TransferOrderDetailModel
    {
        public long TransferOrderID { get; set; }
        public int ItemNo { get; set; }
        public int LineSequenceNumber { get; set; }
        [Required]
        public long ProductID { get; set; }
        [Required]
        public string ProductCode { get; set; }
        [Required]
        public string ProductName { get; set; }
        public double Quantity { get; set; }
        public long ConversionID { get; set; }
        public string ConversionName { get; set; }
        public decimal TransferPrice { get; set; }

        public double QtyPicked { get; set; }
        public double QtyPacked { get; set; }
        public double QtyShipped { get; set; }
        public double QtyReceived { get; set; }

        public decimal Amount
        {
            get
            {
                return Convert.ToDecimal(TransferPrice) * Convert.ToDecimal(Quantity);
            }
        }

        public double QtyAvailable { get; set; }
        public double StockQty { get; set; }
        public double QtyAvailableHidden { get; set; }
        public double StockQtyHidden { get; set; }
        
    }
}
