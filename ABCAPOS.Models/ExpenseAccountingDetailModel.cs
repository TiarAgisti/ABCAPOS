using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class ExpenseAccountingDetailModel
    {
        public long AccountingID { get; set; }
        public int ItemNo { get; set; }
        public DateTime Date { get; set; }
        public long AccountID { get; set; }
        public string AccountUserCode { get; set; }
        public string AccountDescription { get; set; }
        public string DocumentNo { get; set; }
        public decimal Amount { get; set; }
        public string Remarks { get; set; }

        public ExpenseAccountingDetailModel()
        {
            Date = DateTime.Now;
        }
    }
}
