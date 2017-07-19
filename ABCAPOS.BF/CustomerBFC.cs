using MPL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using ABCAPOS.DA;
using ABCAPOS.ReportEDS;

namespace ABCAPOS.BF
{
    public class CustomerBFC : GenericBFC<Customer, v_Customer, CustomerModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetCustomerCode()
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var customerPrefix = "";

            if (prefixSetting != null)
                customerPrefix = prefixSetting.CustomerPrefix;

            var code = new ABCAPOSDAC().RetrieveCustomerMaxCode(customerPrefix, 0);

            return code;
        }

        protected override GenericDAC<Customer, CustomerModel> GetDAC()
        {
            return new GenericDAC<Customer, CustomerModel>("ID", false, "Name");
        }

        protected override GenericDAC<v_Customer, CustomerModel> GetViewDAC()
        {
            return new GenericDAC<v_Customer, CustomerModel>("ID", false, "Name");
        }

        public void Validate(CustomerModel obj)
        {
            var extCustomer = RetrieveByCode(obj.Code);

            if (extCustomer != null && extCustomer.ID != obj.ID)
                throw new Exception("Customer code can not be duplicate");

            if (!string.IsNullOrEmpty(obj.ParentName) && obj.ParentID > 0)
            {
                var parent = RetrieveByID(obj.ParentID);

                if (!obj.Name.StartsWith(parent.Name))
                    obj.Name = parent.Name + " : " + obj.Name;

                if (obj.ParentID != 0 && obj.ParentID == obj.ID)
                    throw new Exception("Customer tidak boleh memilih diri sendiri sebagai parent");

                if (parent.ParentID != 0 && parent.ParentID == obj.ID)
                    throw new Exception("Customer tidak boleh memiliki parent sirkular");
            }
            else
            {
                obj.ParentID = 0;
            }

            //obj.CreditLimit = Convert.ToDecimal(obj.StrCreditLimit);
            if (obj.SalesRepName == null || obj.SalesRepName.Length == 0)
            {
                obj.SalesRep = 0;
            }
        }

        public CustomerModel RetrieveByCode(string customerCode)
        {
            return new ABCAPOSDAC().RetrieveCustomerByCode(customerCode);
        }

        public CustomerModel RetrieveByCodeOrName(string customerName)
        {
            return new ABCAPOSDAC().RetrieveCustomerByCodeOrName(customerName);
        }

        public List<CustomerModel> RetrieveAutoComplete(string key)
        {
            return new ABCAPOSDAC().RetrieveCustomerAutoComplete(key);
        }

        public ABCAPOSReportEDSC.CustomerDTRow RetrievePrintOut(long customerID)
        {
            return new ABCAPOSReportDAC().RetrieveCustomer(customerID);
        }

    }
}
