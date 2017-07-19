using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABCAPOS.Models;
using MPL.MVC;
using ABCAPOS.BF;
using System.Globalization;
using ABCAPOS.Util;

namespace ABCAPOS.Controllers
{
    public class AttendanceController : MasterDetailController<AttendanceModel, AttendanceDetailModel>
    {
        private string ModuleID
        {
            get
            {
                return "Attendance";
            }
        }

        private void SetViewBagPermission()
        {
            var roleDetails = new RoleBFC().RetrieveActions(MembershipHelper.GetRoleID(), ModuleID);

            ViewBag.AllowEdit = roleDetails.Contains("Edit");
            ViewBag.AllowCreate = roleDetails.Contains("Create");
        }

        public override MPL.Business.IMasterDetailBFC<AttendanceModel, AttendanceDetailModel> GetBFC()
        {
            return new AttendanceBFC();
        }

        protected override void PreCreateDisplay(AttendanceModel header, List<AttendanceDetailModel> details)
        {
            header.Code = new AttendanceBFC().GetAttendanceCode();

            new AttendanceBFC().CreateAttendance(header);

            SetPreEditViewBag(header);

            base.PreCreateDisplay(header, header.Details);
        }

        protected override void PreDetailDisplay(AttendanceModel header, List<AttendanceDetailModel> details)
        {
            SetViewBagNotification();

            SetViewBagPermission();

            SetDetails(header, details);

            ViewBag.AttendanceSetting = new AttendanceSettingBFC().Retrieve();

            base.PreDetailDisplay(header, details);
        }

        protected override void PreUpdateDisplay(AttendanceModel header, List<AttendanceDetailModel> details)
        {
            SetPreEditViewBag(header);

            SetDetails(header, details);

            base.PreUpdateDisplay(header, details);
        }

        public override void CreateData(AttendanceModel obj, List<AttendanceDetailModel> details)
        {
            new AttendanceBFC().Validate(obj, details);

            base.CreateData(obj, details);
        }

        public override void UpdateData(AttendanceModel obj, List<AttendanceDetailModel> details, FormCollection formCollection)
        {
            new AttendanceBFC().Validate(obj, details);

            base.UpdateData(obj, details, formCollection);
        }

        private void SetPreEditViewBag(AttendanceModel header)
        {
            List<object> hourList = new List<object>();

            for (int hour = 1; hour <= 24; hour++)
            {
                for (int minute = 0; minute < 60; minute++)
                    hourList.Add(new { Value = hour.ToString("00") + ":" + minute.ToString("00"), Text = hour.ToString("00") + ":" + minute.ToString("00") });
            }

            ViewBag.HourList = hourList;

            ViewBag.AttendanceSetting = new AttendanceSettingBFC().Retrieve();
        }

        private void SetViewBagNotification()
        {
            if (TempData["SuccessNotification"] != null)
                ViewBag.SuccessNotification = Convert.ToString(TempData["SuccessNotification"]);

            if (!string.IsNullOrEmpty(Request.QueryString["errorMessage"]))
                ViewBag.ErrorNotification = Convert.ToString(Request.QueryString["errorMessage"]);
        }

        private void SetDetails(AttendanceModel obj, List<AttendanceDetailModel> details)
        {
            foreach (var detail in details)
            {
                detail.OnDutyTime = detail.OnDuty.Hour.ToString("00") + ":" + detail.OnDuty.Minute.ToString("00");
                detail.OffDutyTime = detail.OffDuty.Hour.ToString("00") + ":" + detail.OffDuty.Minute.ToString("00");
                detail.ClockInTime = detail.ClockIn.Hour.ToString("00") + ":" + detail.ClockIn.Minute.ToString("00");
                detail.ClockOutTime = detail.ClockOut.Hour.ToString("00") + ":" + detail.ClockOut.Minute.ToString("00");
            }

            var courierAttendance = (from i in details
                                     where i.StaffRoleID == (int)StaffType.Courier
                                     select i).ToList();

            var operatorAttendance = (from i in details
                                      where i.StaffRoleID == (int)StaffType.Operator
                                      select i).ToList();

            obj.Details = details = courierAttendance;
            obj.OperatorAttendance = operatorAttendance;
        }

        public override ActionResult Index(int? startIndex, int? amount, string sortParameter, MPL.MVC.GenericFilter filter)
        {
            SetViewBagPermission();

            return base.Index(startIndex, amount, sortParameter, filter);
        }
    }
}
