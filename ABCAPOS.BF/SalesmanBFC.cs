using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Models;
using ABCAPOS.DA;
using ABCAPOS.Util;
using ABCAPOS.EDM;
using MPL.Business;
using MPL;

namespace ABCAPOS.BF
{
    public class SalesmanBFC:GenericBFC<Salesman,Salesman,SalesmanModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetSalesmanCode()
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var salesmanPrefix = "";

            if (prefixSetting != null)
                salesmanPrefix = prefixSetting.SalesmanPrefix;

            var code = new ABCAPOSDAC().RetrieveSalesmanMaxCode(salesmanPrefix, 4);

            return code;
        }

        protected override GenericDAC<Salesman, SalesmanModel> GetDAC()
        {
            return new GenericDAC<Salesman, SalesmanModel>("ID", false, "Name");
        }

        protected override GenericDAC<Salesman, SalesmanModel> GetViewDAC()
        {
            return new GenericDAC<Salesman, SalesmanModel>("ID", false, "Name");
        }

        public override void Create(SalesmanModel dr)
        {
            dr.Code = GetSalesmanCode();

            base.Create(dr);
        }

        public List<SalesmanModel> Retrieve(bool isActive)
        {
            return new ABCAPOSDAC().RetrieveSalesman(isActive);
        }

    }
}
