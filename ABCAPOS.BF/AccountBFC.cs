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
    public class AccountBFC:GenericBFC<Account,v_Account,AccountModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetAccountCode()
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var accountPrefix = "";

            if (prefixSetting != null)
                accountPrefix = prefixSetting.AccountPrefix;

            var code = new ABCAPOSDAC().RetrieveAccountMaxCode(accountPrefix, 4);

            return code;
        }

        protected override GenericDAC<Account, AccountModel> GetDAC()
        {
            return new GenericDAC<Account, AccountModel>("ID", false, "Code");
        }

        protected override GenericDAC<v_Account, AccountModel> GetViewDAC()
        {
            return new GenericDAC<v_Account, AccountModel>("ID", false, "Code");
        }

        public override void Create(AccountModel dr)
        {
            //if (string.IsNullOrEmpty(dr.Code))
                dr.Code = GetAccountCode();
            
            base.Create(dr);
        }

        public AccountModel RetrieveByCode(string accountCode)
        {
            return new ABCAPOSDAC().RetrieveAccountByCode(accountCode);
        }

        public AccountModel RetrieveByUserCode(string accountUserCode)
        {
            return new ABCAPOSDAC().RetrieveAccountByUserCode(accountUserCode);
        }

        public List<AccountModel> Retrieve(bool isActive)
        {
            return new ABCAPOSDAC().RetrieveAccount(isActive);
        }

        public List<AccountModel> RetrieveAutoComplete(string key)
        {
            return new ABCAPOSDAC().RetrieveAccountAutoComplete(key);
        }

        public List<AccountModel> RetrieveInvoicePaymentAutoComplete(string key)
        {
            return new ABCAPOSDAC().RetrieveAccountInvoicePaymentAutoComplete(key);
        }

        public List<AccountModel> RetrieveInvoicePaymentAutoComplete()
        {
            return new ABCAPOSDAC().RetrieveAccountInvoicePaymentAutoComplete();
        }

        public List<AccountModel> RetrieveDebitAutoComplete(string key)
        {
            return new ABCAPOSDAC().RetrieveDebitAccount(key);
        }

        public List<AccountModel> RetrieveCreditAutoComplete(string key)
        {
            return new ABCAPOSDAC().RetrieveCreditAccount(key);
        }
    }
}
