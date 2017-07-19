using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.BF
{
    public class CurrencyBFC:GenericBFC<Currency,Currency,CurrencyModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        protected override GenericDAC<Currency, CurrencyModel> GetDAC()
        {
            return new GenericDAC<Currency, CurrencyModel>("ID", false);
        }

        protected override GenericDAC<Currency, CurrencyModel> GetViewDAC()
        {
            return new GenericDAC<Currency, CurrencyModel>("ID", false);
        }
        public List<CurrencyModel> RetrieveActive()
        {
            return base.RetrieveAll().ToList();
        }

    }
}
