using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class IncomeExpenseModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        public DateTime Date { get; set; }

        public string Title { get; set; }
        public int CategoryID { get; set; }
        public string VoidRemarks { get; set; }
        public string Remarks { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }

        public decimal GrandTotal { get; set; }

        public List<IncomeExpenseDetailModel> Details { get; set; }


        public string StatusDescription
        {
            get
            {
                if (Status == (int)IncomeExpenseStatus.New)
                    return "New";
                else if (Status == (int)IncomeExpenseStatus.Approved)
                    return "Disetujui";
                else if (Status == (int)IncomeExpenseStatus.Void)
                    return "Void";
                else
                    return "";
            }
        }

        public IncomeExpenseModel()
        {
            Date = DateTime.Now;
            Status = (int)IncomeExpenseStatus.New;
            Details = new List<IncomeExpenseDetailModel>();
        }
    }
}
