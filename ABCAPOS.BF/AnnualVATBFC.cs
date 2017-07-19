using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;

namespace ABCAPOS.BF
{
    public class AnnualVATBFC : GenericBFC<AnnualVAT, AnnualVAT, AnnualVATModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        protected override GenericDAC<AnnualVAT, AnnualVATModel> GetDAC()
        {
            return new GenericDAC<AnnualVAT, AnnualVATModel>("ID", false, "Date DESC");
        }

        protected override GenericDAC<AnnualVAT, AnnualVATModel> GetViewDAC()
        {
            return new GenericDAC<AnnualVAT, AnnualVATModel>("ID", false, "Date DESC");
        }
    }
}
