using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class CurrencyDateModel
    {
        public long ID { get; set; }
        public long CurrencyID { get; set; }
        public string CurrencyName { get; set; }
        public DateTime Date { get; set; }
        public decimal Value { get; set; }

        public CurrencyDateModel()
        {
            Date = DateTime.Now;
        }
    }
}
