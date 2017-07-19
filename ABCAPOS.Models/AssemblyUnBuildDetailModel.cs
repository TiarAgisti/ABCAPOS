using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class AssemblyUnBuildDetailModel
    {
        public long AssemblyUnBuildID { get; set; }
        public long ProductDetailID { get; set; }
        public int ItemNo { get; set; }
        public long ConversionID { get; set; }
        public double Qty { get; set; }
        public double StockQtyHidden { get; set; }
        public long ConversionIDTemp { get; set; }
        public long BinID { get; set; }

        //Read Only
        public double QtyOnHand { get; set; }
        public double QtyAvailable { get; set; }
        public string ProductDetailCode { get; set; }
        public string ProductDetailName { get; set; }
        public string Type { get; set; }
        public string ConversionName { get; set; }
        public double UnitRate { get; set; }
        public string BinNumber { get; set; }
    }
}
