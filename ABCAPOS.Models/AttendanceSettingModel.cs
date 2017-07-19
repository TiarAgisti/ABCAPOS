using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class AttendanceSettingModel
    {
        public long ID { get; set; }
        public decimal MealAllowanceCourier { get; set; }
        public decimal MealAllowanceOperator { get; set; }
        public decimal OvertimeCourier { get; set; }
        public decimal OvertimeOperator { get; set; }
        public decimal LatePenaltyCourier { get; set; }
        public decimal LatePenaltyOperator { get; set; }
        public decimal AlphaPenaltyCourier { get; set; }
        public decimal AlphaPenaltyCourierOnSaturday { get; set; }
        public decimal AlphaPenaltyOperator { get; set; }
        public string OffDutyCourier { get; set; }
        public string OffDutyCourierOnSaturday { get; set; }
        public string OffDutyOperator { get; set; }
        public string OffDutyOperatorOnSaturday { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
