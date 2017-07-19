using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class BookingSalesDetailModel
    {
        public long BookingSalesID { get; set; }
        public int ItemNo { get; set; }
        public long ProductID { get; set; }
        public int LineSequenceNumber { get; set; }

        public decimal Price { get; set; }
        public decimal AssetPrice { get; set; }
        public double Quantity { get; set; }
        public double CreatedSOQuantity { get; set; }
        public double CreatedDOQuantity { get; set; }
        public double CreatedInvQuantity { get; set; }
        public decimal Discount { get; set; }

        public string Remarks { get; set; }
        public string Barcode { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string VendorName { get; set; }
        public double PriceHidden { get; set; }
        public double SaleUnitRateHidden { get; set; }

        public long PriceLevelID { get; set; }
        public string PriceLevelName { get; set; }
        public int TaxType { get; set; }
        public string TaxRate { get; set; }
        public long ConversionID { get; set; }
        public string ConversionName { get; set; }
        public long ConversionIDTemp { get; set; }
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
                return (decimal)Price * (decimal)Quantity;
            }
        }

        public decimal TotalPPN
        {
            get
            {
                if (TaxType == 2)
                    return (decimal)TotalAmount * (decimal)0.1;
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


    }
}
