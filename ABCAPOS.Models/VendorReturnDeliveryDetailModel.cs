using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class VendorReturnDeliveryDetailModel
    {
        public long VendorReturnDeliveryID { get; set; }
        public int ItemNo { get; set; }
        public int VendorReturnItemNo { get; set; }
        public long ProductID { get; set; }
        public long BinID { get; set; }
        public double Quantity { get; set; }
        public decimal Price { get; set; }
        public string Barcode { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductDate { get; set; }
        public string Remarks { get; set; }
        public long ConversionID { get; set; }
        public string ConversionName { get; set; }
        public string BinNumber { get; set; }
        public string StrQuantity { get; set; }

        public double QtySO { get; set; }
        public double QtyDO { get; set; }
        public double QtyHidden { get; set; }
        public double StockQty { get; set; }
        public double StockAvailable { get; set; }
        public double UnitRate { get; set; }
        public double CreatedDOQuantity { get; set; }
        public int TaxType { get; set; }

        public string TaxTypeName
        {
            get
            {
                if (TaxType == 1)
                    return "Non-PPN";
                else
                    return "PPN";
            }
        }

        public decimal Total
        {
            get
            {
                return Convert.ToDecimal(Quantity) * Price;
            }
        }
    }
}
