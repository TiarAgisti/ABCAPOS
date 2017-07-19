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
    public class ExpedisiBFC : GenericBFC<Expedisi, Expedisi, ExpedisiModel>
    {

        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        protected override GenericDAC<Expedisi, ExpedisiModel> GetDAC()
        {
            return new GenericDAC<Expedisi, ExpedisiModel>("ID", false, "Name");
        }

        protected override GenericDAC<Expedisi, ExpedisiModel> GetViewDAC()
        {
            return new GenericDAC<Expedisi, ExpedisiModel>("ID", false, "Name");
        }

        
    }
}
