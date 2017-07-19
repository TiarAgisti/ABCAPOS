using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public  class FormulasiModel
    {
        public long ProductID { get; set; }
        public int ItemNo { get; set; }
        public long ProductDetailID { get; set; }
        public string ProductName { get; set; }
        public double Qty { get; set; }
        public long ConversionID { get; set; }
        public string ConversionName { get; set; }
        public string ProductCode { get; set; }
        public long ConversionIDTemp { get; set; }
    }
}
