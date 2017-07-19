using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class IncomeExpenseDetailModel
    {
        public long IncomeExpenseID { get; set; }
        public int ItemNo { get; set; }
        public decimal? Price { get; set; }
        public string Remarks { get; set; }
    }
}
