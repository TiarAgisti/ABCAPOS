using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class ReturnReceiptDetailModel
    {
        public long ReturnReceiptID { get; set; }
        public int ItemNo { get; set; }
        public int CustomerReturnItemNo { get; set; }
        public long ProductID { get; set; }
        public double Quantity { get; set; }
        public long ConversionID { get; set; }
        public string Remarks { get; set; }
        public long BinID { get; set; }
        public bool Restock { get; set; }
        public string Barcode { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ConversionName { get; set; }
        public decimal? AssetPrice { get; set; }
        public int TaxType { get; set; }
        public double QtyPO { get; set; }
        public double QtyReceive { get; set; }
        public string BinNumber { get; set; }
        
        public string StrQuantity { get; set; }
        public double QtyRemain { get; set; }
        public double StockQty { get; set; }
        public double StockQtyAvailable { get; set; }
        public double ReceivedQuantity { get; set; }
        public double SelisihQuantity { get; set; }

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
        //public double OutstandingPBQuantity
        //{
        //    get
        //    {
        //        return Convert.ToDouble(Quantity - QtyReceive);
        //    }
        //}

    }
}
