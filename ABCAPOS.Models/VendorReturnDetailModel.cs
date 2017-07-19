using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class VendorReturnDetailModel
    {
        public long VendorReturnID { get; set; }
        public int ItemNo { get; set; }
        public int LineSequenceNumber { get; set; }
        public long ProductID { get; set; }
        public decimal AssetPrice { get; set; }
        public double Quantity { get; set; }
        public decimal Discount { get; set; }
        public double CreatedDeliveryQuantity { get; set; }
        public double CreatedCreditQuantity { get; set; }

        public string Remarks { get; set; }
        public string Barcode { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string VendorName { get; set; }

        public int TaxType { get; set; }
        public string TaxRate { get; set; }
        public long ConversionID { get; set; }
        public string ConversionName { get; set; }
        public double StockQty { get; set; }
        public double StockQtyHidden { get; set; }
        public double StockAvailable { get; set; }
        public double StockAvailableHidden { get; set; }

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

        public decimal TotalAmount
        {
            get
            {
                return Convert.ToDecimal(AssetPrice) * Convert.ToDecimal(Quantity);
            }
        }

        public decimal TotalPPN
        {
            get
            {
                if (TaxType == 2)
                    return TotalAmount * Convert.ToDecimal(0.1);
                else
                    return 0;
            }
        }

        public decimal Total
        {
            get
            {
                return TotalAmount + TotalPPN;
            }
        }

        public double OutstandingQuantity
        {
            get
            {
                return Convert.ToDouble(Quantity) - CreatedDeliveryQuantity;
            }
        }

        public double OutstandingPBQuantity
        {
            get
            {
                return Convert.ToDouble(CreatedDeliveryQuantity - CreatedCreditQuantity);
            }
        }
    }
}
