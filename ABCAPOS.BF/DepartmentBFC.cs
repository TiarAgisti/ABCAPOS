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
    public class DepartmentBFC : GenericBFC<Department, v_Department, DepartmentModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetDepartmentCode()
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var departmentPrefix = "";

            if (prefixSetting != null)
                departmentPrefix = prefixSetting.DepartmentPrefix;

            var code = new ABCAPOSDAC().RetrieveDepartmentMaxCode(departmentPrefix, 4);

            return code;
        }

        protected override GenericDAC<Department, DepartmentModel> GetDAC()
        {
            return new GenericDAC<Department, DepartmentModel>("ID", false, "Code");
        }

        protected override GenericDAC<v_Department, DepartmentModel> GetViewDAC()
        {
            return new GenericDAC<v_Department, DepartmentModel>("ID", false, "Code");
        }

        public List<DepartmentModel> Retrieve(bool isActive)
        {
            return new ABCAPOSDAC().RetrieveDepartment(isActive);
        }

        public override void Create(DepartmentModel dr)
        {
            dr.Code = GetDepartmentCode();

            base.Create(dr);
        }
    }
}
