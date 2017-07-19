using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using System.Transactions;
using ABCAPOS.Util;
using ABCAPOS.DA;

namespace ABCAPOS.BF
{
    public class AttendanceBFC : MasterDetailBFC<Attendance, Attendance, AttendanceDetail, v_AttendanceDetail, AttendanceModel, AttendanceDetailModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetAttendanceCode()
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var attendancePrefix = "";

            if (prefixSetting != null)
                attendancePrefix = prefixSetting.AttendancePrefix;

            var code = new ABCAPOSDAC().RetrieveAttendanceMaxCode(attendancePrefix, 5);

            return code;
        }

        protected override GenericDetailDAC<AttendanceDetail, AttendanceDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<AttendanceDetail, AttendanceDetailModel>("AttendanceID", "ItemNo", false);
        }

        protected override GenericDetailDAC<v_AttendanceDetail, AttendanceDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_AttendanceDetail, AttendanceDetailModel>("AttendanceID", "ItemNo", false);
        }

        protected override GenericDAC<Attendance, AttendanceModel> GetMasterDAC()
        {
            return new GenericDAC<Attendance, AttendanceModel>("ID", false, "Code DESC");
        }

        protected override GenericDAC<Attendance, AttendanceModel> GetMasterViewDAC()
        {
            return new GenericDAC<Attendance, AttendanceModel>("ID", false, "Code DESC");
        }

        public override void Create(AttendanceModel header, List<AttendanceDetailModel> details)
        {
            header.Code = GetAttendanceCode();

            using (TransactionScope trans = new TransactionScope())
            {
                base.Create(header, details);

                PostAccounting(header.ID);

                trans.Complete();
            }
        }

        public override void Update(AttendanceModel header, List<AttendanceDetailModel> details)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                base.Update(header, details);

                new AccountingResultBFC().Void(header.ID, AccountingResultDocumentType.Attendance);
                PostAccounting(header.ID);

                trans.Complete();
            }
        }

        public void CreateAttendance(AttendanceModel header)
        {
            var courierList = new StaffBFC().Retrieve(StaffType.Courier, true);
            var operatorList = new StaffBFC().Retrieve(StaffType.Operator, true);

            var details = new List<AttendanceDetailModel>();
            var operatorAttendance = new List<AttendanceDetailModel>();

            courierList.AddRange(operatorList);

            var attendanceSetting = new AttendanceSettingBFC().Retrieve();

            foreach (var courier in courierList)
            {
                var detail = new AttendanceDetailModel();

                detail.StaffID = courier.ID;
                detail.StaffName = courier.Name;
                detail.StaffRoleID = courier.RoleID;
                detail.OnDutyTime = "09:00";
                detail.OffDutyTime = "17:00";
                detail.ClockInTime = "09:00";
                detail.ClockOutTime = "17:00";
                detail.LateTotal = "00:00";
                detail.OvertimeTotal = "00:00";

                if (detail.StaffRoleID == (int)StaffType.Courier)
                {
                    detail.Amount = attendanceSetting.MealAllowanceCourier;
                    details.Add(detail);
                }
                else
                {
                    detail.Amount = attendanceSetting.MealAllowanceOperator;
                    operatorAttendance.Add(detail);
                }
            }

            header.Details = details;
            header.OperatorAttendance = operatorAttendance;
        }

        private void AssignDetailValues(AttendanceModel header, List<AttendanceDetailModel> details)
        {
            foreach (var detail in details)
            {
                var onDutyHour = Convert.ToInt32(detail.OnDutyTime.Substring(0, 2));
                var onDutyMinute = Convert.ToInt32(detail.OnDutyTime.Substring(3));

                var offDutyHour = Convert.ToInt32(detail.OffDutyTime.Substring(0, 2));
                var offDutyMinute = Convert.ToInt32(detail.OffDutyTime.Substring(3));

                var clockInHour = Convert.ToInt32(detail.ClockInTime.Substring(0, 2));
                var clockInMinute = Convert.ToInt32(detail.ClockInTime.Substring(3));

                var clockOutHour = Convert.ToInt32(detail.ClockOutTime.Substring(0, 2));
                var clockOutMinute = Convert.ToInt32(detail.ClockOutTime.Substring(3));

                detail.OnDuty = header.Date.Date.AddHours(onDutyHour).AddMinutes(onDutyMinute);
                detail.OffDuty = header.Date.Date.AddHours(offDutyHour).AddMinutes(offDutyMinute);
                detail.ClockIn = header.Date.Date.AddHours(clockInHour).AddMinutes(clockInMinute);
                detail.ClockOut = header.Date.Date.AddHours(clockOutHour).AddMinutes(clockOutMinute);
            }
        }

        public void PostAccounting(long attendanceID)
        {
            var account = new AccountBFC().RetrieveByUserCode(SystemConstants.OfficeOvertimeAccountUserCode);

            var results = new List<AccountingResultModel>();

            var attendance = RetrieveByID(attendanceID);
            var attendanceDetails = RetrieveDetails(attendanceID);

            var amount = attendanceDetails.Sum(p => p.Amount);
            var penaltyamount = attendanceDetails.Sum(p => p.PenaltyAmount);
            amount = amount - penaltyamount;
            var result = new AccountingResultModel();

            result.DocumentID = attendance.ID;
            result.DocumentType = (int)AccountingResultDocumentType.Attendance;
            result.Type = (int)AccountingResultType.Debit;
            result.Date = attendance.Date;
            result.AccountID = account.ID;
            result.DocumentNo = attendance.Code;
            result.Amount = amount;
            result.DebitAmount = amount;
            result.Remarks = "Lembur Kantor (absensi :" + attendance.Code + ")";
            results.Add(result);

            result = new AccountingResultModel();
            result.DocumentID = attendance.ID;
            result.DocumentType = (int)AccountingResultDocumentType.Attendance;
            result.Type = (int)AccountingResultType.Credit;
            result.Date = attendance.Date;
            //result.AccountID = account.ReferenceID;
            result.DocumentNo = attendance.Code;
            result.Amount = amount;
            result.CreditAmount = amount;
            result.Remarks = "Lembur Kantor (absensi :" + attendance.Code + ")";

            results.Add(result);

            new AccountingResultBFC().Posting(results);
        }

        public void Validate(AttendanceModel header, List<AttendanceDetailModel> details)
        {
            details.AddRange(header.OperatorAttendance);

            AssignDetailValues(header, details);

            var attendanceSetting = new AttendanceSettingBFC().Retrieve();

            foreach (var detail in details)
            {
                if (detail.Leave)
                {
                    detail.LateTotal = "00:00";
                    detail.OvertimeTotal = "00:00";

                    continue;
                }

                var staff = new StaffBFC().RetrieveByID(detail.StaffID);

                decimal alpha = 0;
                decimal mealAllowance = 0;
                decimal overtimeAllowance = 0;
                decimal latePenalty = 0;
                double overtimeHour = 0;
                double lateHour = 0;

                if (staff.RoleID == (int)StaffType.Courier)
                {
                    if (detail.Alpha)
                    {
                        alpha = attendanceSetting.AlphaPenaltyCourier;
                    }
                    else
                    {
                        mealAllowance = attendanceSetting.MealAllowanceCourier;
                        overtimeAllowance = attendanceSetting.OvertimeCourier;
                        latePenalty = attendanceSetting.LatePenaltyCourier;
                    }
                }
                else
                {
                    if (detail.Alpha)
                    {
                        alpha = attendanceSetting.AlphaPenaltyOperator;
                    }
                    else
                    {
                        mealAllowance = attendanceSetting.MealAllowanceOperator;
                        overtimeAllowance = attendanceSetting.OvertimeOperator;
                        latePenalty = attendanceSetting.LatePenaltyOperator;
                    }
                }

                if (!detail.Alpha)
                {
                    if (detail.ClockIn > detail.OnDuty)
                    {
                        var timeSpan = detail.ClockIn - detail.OnDuty;

                        lateHour = timeSpan.Hours;
                        var lateMinute = timeSpan.Minutes;

                        if (lateMinute > 0)
                            lateHour += 1;

                        detail.LateTotal = timeSpan.Hours.ToString("00") + ":" + lateMinute.ToString("00");
                    }
                    else
                        detail.LateTotal = "00:00";

                    if (detail.ClockOut.Hour >= detail.OffDuty.Hour || detail.ClockOut.Hour < 9)
                    {
                        if (detail.ClockOut.Hour < 9)
                            detail.ClockOut = detail.ClockOut.AddHours(24);

                        var timeSpan = detail.ClockOut - detail.OffDuty;

                        overtimeHour = timeSpan.Hours;
                        var overtimeMinute = timeSpan.Minutes;

                        if (overtimeMinute > 15)
                        {
                            if (overtimeMinute < 46)
                                overtimeHour += 0.5;
                            else
                                overtimeHour += 1;
                        }

                        detail.OvertimeTotal = timeSpan.Hours.ToString("00") + ":" + overtimeMinute.ToString("00");
                    }
                    else
                        detail.OvertimeTotal = "00:00";
                }
                else
                {
                    detail.LateTotal = "00:00";
                    detail.OvertimeTotal = "00:00";
                }

                detail.Amount = mealAllowance + (overtimeAllowance * Convert.ToDecimal(overtimeHour));
                detail.PenaltyAmount = (latePenalty * Convert.ToDecimal(lateHour)) + alpha;
            }
        }
    }
}
