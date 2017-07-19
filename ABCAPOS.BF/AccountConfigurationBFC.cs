using ABCAPOS.DA;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using ABCAPOS.Util;
using MPL.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

namespace ABCAPOS.BF
{
    public class AccountConfigurationBFC:MasterDetailBFC<AccountConfiguration,AccountConfiguration,AccountConfigurationDetail,v_AccountConfigurationDetail,AccountConfigurationModel,AccountConfigurationDetailModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetCode()
        {
            var code = new ABCAPOSDAC().RetrieveAccountConfigurationMaxCode("ACN", 5);

            return code;
        }

        protected override GenericDetailDAC<AccountConfigurationDetail, AccountConfigurationDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<AccountConfigurationDetail, AccountConfigurationDetailModel>("ConfigurationID", "ItemNo", false);
        }

        protected override GenericDetailDAC<v_AccountConfigurationDetail, AccountConfigurationDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_AccountConfigurationDetail, AccountConfigurationDetailModel>("ConfigurationID", "ItemNo", false);
        }

        protected override GenericDAC<AccountConfiguration, AccountConfigurationModel> GetMasterDAC()
        {
            return new GenericDAC<AccountConfiguration, AccountConfigurationModel>("ID", false, "Code");
        }

        protected override GenericDAC<AccountConfiguration, AccountConfigurationModel> GetMasterViewDAC()
        {
            return new GenericDAC<AccountConfiguration, AccountConfigurationModel>("ID", false, "Code");
        }

        public override void Create(AccountConfigurationModel header, List<AccountConfigurationDetailModel> details)
        {
            header.Code = GetCode();
            CombineToDetails(details, header.CreditDetails);

            using (TransactionScope trans = new TransactionScope())
            {
                base.Create(header, details);

                trans.Complete();
            }
        }

        public override void Update(AccountConfigurationModel header, List<AccountConfigurationDetailModel> details)
        {
            CombineToDetails(details, header.CreditDetails);

            base.Update(header, details);
        }

        private List<AccountConfigurationDetailModel> CombineToDetails(List<AccountConfigurationDetailModel> details, List<AccountConfigurationDetailModel> creditDetails)
        {
            foreach (var detail in details)
            {
                detail.Type = (int)AccountType.Debit;
            }

            foreach (var creditDetail in creditDetails)
            {
                creditDetail.Type = (int)AccountType.Credit;

                details.Add(creditDetail);
            }

            return details;
        }
    }
}
