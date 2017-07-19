using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;

namespace ABCAPOS.Models
{
    public class AttendanceDetailModel
    {
        public long AttendanceID { get; set; }
        public int ItemNo { get; set; }
        public long StaffID { get; set; }
        public bool Alpha { get; set; }
        public bool Leave { get; set; }
        public DateTime OnDuty { get; set; }
        public DateTime OffDuty { get; set; }
        public DateTime ClockIn { get; set; }
        public DateTime ClockOut { get; set; }
        public string LateTotal { get; set; }
        public string OvertimeTotal { get; set; }
        public decimal Amount { get; set; }
        public decimal PenaltyAmount { get; set; }
        public string StaffName { get; set; }
        public int StaffRoleID { get; set; }

        public string OnDutyTime { get; set; }
        public string OffDutyTime { get; set; }
        public string ClockInTime { get; set; }
        public string ClockOutTime { get; set; }

        public string RoleDescription
        {
            get
            {
                if (StaffRoleID == (int)StaffType.Courier)
                    return "Kurir";
                else if (StaffRoleID == (int)StaffType.Operator)
                    return "Operator";
                else
                    return "";
            }
        }

        public string AlphaDescription
        {
            get
            {
                if (Alpha)
                    return "Ya";
                else
                    return "Tidak";
            }
        }

        public string LeaveDescription
        {
            get
            {
                if (Leave)
                    return "Ya";
                else
                    return "Tidak";
            }
        }
    }
}
