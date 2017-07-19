using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class BillCreditDetailModel
    {
        public long BillCreditID { get; set; }
        public int VendorReturnItemNo { get; set; }
        public int ItemNo { get; set; }
        public long ProductID { get; set; }
        public double Quantity { get; set; }
        public long ConversionID { get; set; }
        public decimal AssetPrice { get; set; }
        public int TaxType { get; set; }
        public string Remarks { get; set; }

        public string Barcode { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ConversionName { get; set; }

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

    }
}
