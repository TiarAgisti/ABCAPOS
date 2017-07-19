using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class PPh21ExpenseModel
    {
        public long ID { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }

        public string StrYear { get; set; }

        public int Year
        {
            get
            {
                return Date.Year;
            }
        }

        public PPh21ExpenseModel()
        {
            Date = DateTime.Now;
        }
    }
}
