using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;

namespace ABCAPOS.BF
{
    public class CustomerGroupBFC : GenericBFC<CustomerGroup, CustomerGroup, CustomerGroupModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        protected override GenericDAC<CustomerGroup, CustomerGroupModel> GetDAC()
        {
            return new GenericDAC<CustomerGroup, CustomerGroupModel>("ID", false, "Name");
        }

        protected override GenericDAC<CustomerGroup, CustomerGroupModel> GetViewDAC()
        {
            return new GenericDAC<CustomerGroup, CustomerGroupModel>("ID", false, "Name");
        }
    }
}