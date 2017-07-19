using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using ABCAPOS.DA;
using MPL.Business;

namespace ABCAPOS.BF
{
    public class PrefixSettingBFC : GenericBFC<PrefixSetting, PrefixSetting, PrefixSettingModel>
    {

        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        protected override GenericDAC<PrefixSetting, PrefixSettingModel> GetDAC()
        {
            return new GenericDAC<PrefixSetting, PrefixSettingModel>("ID", false);
        }

        protected override GenericDAC<PrefixSetting, PrefixSettingModel> GetViewDAC()
        {
            return new GenericDAC<PrefixSetting, PrefixSettingModel>("ID", false);
        }

        public PrefixSettingModel Retrieve()
        {
            return new ABCAPOSDAC().RetrievePrefixSetting();
        }
    }
}
