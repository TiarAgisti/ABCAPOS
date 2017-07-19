using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using ABCAPOS.DA;
using ABCAPOS.Util;

namespace ABCAPOS.BF
{
    public class StaffBFC : GenericBFC<Staff, v_Staff, StaffModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        protected override GenericDAC<Staff, StaffModel> GetDAC()
        {
            return new GenericDAC<Staff, StaffModel>("ID", false, "Name");
        }

        protected override GenericDAC<v_Staff, StaffModel> GetViewDAC()
        {
            return new GenericDAC<v_Staff, StaffModel>("ID", false, "Name");
        }

        public override void Update(StaffModel dr)
        {
            if (dr.LoanAmount == 0)
                dr.LastInstallmentNo = 0;

            base.Update(dr);
        }

        public List<StaffModel> Retrieve(bool isActive)
        {
            return new ABCAPOSDAC().RetrieveStaff(isActive);
        }

        public List<StaffModel> Retrieve(string jobTitle, bool isActive)
        {
            return new ABCAPOSDAC().RetrieveStaff(jobTitle, isActive);
        }

        public List<StaffModel> Retrieve(StaffType staffType, bool isActive)
        {
            return new ABCAPOSDAC().RetrieveStaff(staffType, isActive);
        }

        public StaffModel RetrieveByCode(string employeeName)
        {
            return new ABCAPOSDAC().RetrieveEmployeeByCode(employeeName);
        }

        public List<StaffModel> RetrieveAutoComplete(string key)
        {
            return new ABCAPOSDAC().RetrieveEmployeeAutoComplete(key);
        }

        public List<StaffModel> RetrieveAutoComplete(string key, bool isSalesRep)
        {
            return new ABCAPOSDAC().RetrieveEmployeeAutoComplete(key, isSalesRep);
        }

        public StaffModel RetrieveByName(string staffRepName)
        {
            return new ABCAPOSDAC().RetrieveEmployeeByUserName(staffRepName);
        }

        public List<StaffModel> RetrieveAutoCompleteByJobTitle(string key, string jobTitle)
        {
            return new ABCAPOSDAC().RetrieveEmployeeAutoCompleteByJobTitle(key, jobTitle);
        }
        
        public long RetrieveDefaultWarehouseID(string userName)
        {
            var staff = new ABCAPOSDAC().RetrieveEmployeeByUserName(userName);
            if (staff == null)
                return 0;
            return staff.WarehouseID;
        }

        public List<string> RetrieveAssignedUserNames()
        {
            return new ABCAPOSDAC().RetrieveEmployeeUserNames();
        }
    }
}
