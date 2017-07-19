using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using ABCAPOS.DA;

namespace ABCAPOS.BF
{
    public class AttendanceSettingBFC : GenericBFC<AttendanceSetting, AttendanceSetting, AttendanceSettingModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        protected override GenericDAC<AttendanceSetting, AttendanceSettingModel> GetDAC()
        {
            return new GenericDAC<AttendanceSetting, AttendanceSettingModel>("ID", false);
        }

        protected override GenericDAC<AttendanceSetting, AttendanceSettingModel> GetViewDAC()
        {
            return new GenericDAC<AttendanceSetting, AttendanceSettingModel>("ID", false);
        }

        public AttendanceSettingModel Retrieve()
        {
            return new ABCAPOSDAC().RetrieveAttendanceSetting();
        }
    }
}
