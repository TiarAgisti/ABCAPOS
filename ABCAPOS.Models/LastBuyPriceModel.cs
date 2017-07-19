using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class LastBuyPriceModel
    {
        public long ProductID { get; set; }
        public long CustomerID { get; set; }
        public decimal Price { get; set; }
        public decimal ExchangeRate { get; set; }

    }
}
