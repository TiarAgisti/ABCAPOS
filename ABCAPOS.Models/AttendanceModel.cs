using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class AttendanceModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }

        public List<AttendanceDetailModel> Details { get; set; }
        public List<AttendanceDetailModel> OperatorAttendance { get; set; }

        public AttendanceModel()
        {
            Date = DateTime.Now;
            Details = new List<AttendanceDetailModel>();
        }
    }
}
