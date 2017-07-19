using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Models;
using ABCAPOS.DA;
using ABCAPOS.Util;
using ABCAPOS.EDM;
using MPL.Business;

namespace ABCAPOS.BF
{
    public class PriceLevelBFC : GenericBFC<PriceLevel, PriceLevel, PriceLevelModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        protected override GenericDAC<PriceLevel, PriceLevelModel> GetDAC()
        {
            return new GenericDAC<PriceLevel, PriceLevelModel>("ID", false, "Description");
        }

        protected override GenericDAC<PriceLevel, PriceLevelModel> GetViewDAC()
        {
            return new GenericDAC<PriceLevel, PriceLevelModel>("ID", false, "Description");
        }

        public List<PriceLevelModel> Retrieve(bool isActive)
        {
            return new ABCAPOSDAC().RetrievePriceLevels(isActive);
        }

        public PriceLevelModel RetrieveByName(string name)
        {
            return new ABCAPOSDAC().RetrievePriceLevelByName(name);
        }


    }
}
