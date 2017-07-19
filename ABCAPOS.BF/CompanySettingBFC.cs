using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.DA;
using ABCAPOS.Models;
using ABCAPOS.EDM;
using MPL.Business;
using ABCAPOS.ReportEDS;

namespace ABCAPOS.BF
{
    public class CompanySettingBFC:GenericBFC<CompanySetting,CompanySetting,CompanySettingModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        protected override GenericDAC<CompanySetting, CompanySettingModel> GetDAC()
        {
            return new GenericDAC<CompanySetting, CompanySettingModel>("ID", false);
        }

        protected override GenericDAC<CompanySetting, CompanySettingModel> GetViewDAC()
        {
            return new GenericDAC<CompanySetting, CompanySettingModel>("ID", false);
        }

        public CompanySettingModel Retrieve()
        {
            return new ABCAPOSDAC().RetrieveCompanySetting();
        }

        public ABCAPOSReportEDSC.CompanySettingDTRow RetrieveForPrintOut()
        {
            return new ABCAPOSReportDAC().RetrieveCompanySetting();
        }

    }
}
