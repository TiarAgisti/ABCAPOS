using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class PurchaseReturnDetailModel
    {
        public long PurchaseReturnID { get; set; }
        public int ItemNo { get; set; }
        public int PurchaseOrderItemNo { get; set; }
        public string Barcode { get; set; }
        public long ProductID { get; set; }
        public double POQuantity { get; set; }
        public double Quantity { get; set; }
        public long ConversionID { get; set; }
        public string ProductDate { get; set; }
        public string Remarks { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ConversionName { get; set; }
        public decimal Price { get; set; }
    }
}
