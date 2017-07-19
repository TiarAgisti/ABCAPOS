using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class AllowanceModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public bool isTHRPaid { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }

        public List<AllowanceDetailModel> Details { get; set; }

        public string StatusDescription
        {
            get
            {
                if (Status == (int)MPL.DocumentStatus.New)
                    return MPL.DocumentStatus.New.ToString();
                else if (Status == (int)MPL.DocumentStatus.Approved)
                    return MPL.DocumentStatus.Approved.ToString();
                else if (Status == (int)MPL.DocumentStatus.Void)
                    return MPL.DocumentStatus.Void.ToString();
                else
                    return "";

            }
        }

        public string DateDescription
        {
            get
            {
                return Date.ToString("MMMM yyyy");
            }
        }

        public AllowanceModel()
        {
            Date = DateTime.Now;
            Status = (int)MPL.DocumentStatus.New;

            Details = new List<AllowanceDetailModel>();
        }
    }
}
