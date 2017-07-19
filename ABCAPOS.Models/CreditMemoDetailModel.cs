using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class CreditMemoDetailModel
    {
        public long CreditMemoID { get; set; }
        public int ItemNo { get; set; }
        public int CustomerReturnItemNo { get; set; }
        public long ProductID { get; set; }
        public double Quantity { get; set; }
        public long ConversionID { get; set; }
        public long PriceLevelID { get; set; }
        public int TaxType { get; set; }
        public decimal AssetPrice { get; set; }
        public decimal Discount { get; set; }
        public string Remarks { get; set; }
        public string Barcode { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public double StockQty { get; set; }
        public string ConversionName { get; set; }
        public string PriceLevelName { get; set; }

        public decimal Price { get; set; }
        public decimal HPP { get; set; }
        public double PriceHidden { get; set; }
        public double SaleUnitRateHidden { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal TotalPPN { get; set; }

        public decimal Total { get; set; }

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
    }
}
