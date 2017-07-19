using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.BF
{
    public class AccountCategoryBFC:GenericBFC<AccountCategory,AccountCategory,AccountCategoryModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        protected override GenericDAC<AccountCategory, AccountCategoryModel> GetDAC()
        {
            return new GenericDAC<AccountCategory, AccountCategoryModel>("ID", false, "Description");
        }

        protected override GenericDAC<AccountCategory, AccountCategoryModel> GetViewDAC()
        {
            return new GenericDAC<AccountCategory, AccountCategoryModel>("ID", false, "Description");
        }
    }
}
