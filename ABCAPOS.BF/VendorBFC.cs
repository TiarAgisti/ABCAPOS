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
    public class VendorBFC:GenericBFC<Vendor,v_Vendor,VendorModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetVendorCode()
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var vendorPrefix = "";

            if (prefixSetting != null)
                vendorPrefix = prefixSetting.VendorPrefix;

            var code = new ABCAPOSDAC().RetrieveVendorMaxCode(vendorPrefix, 4);

            return code;
        }
        
        protected override GenericDAC<Vendor, VendorModel> GetDAC()
        {
            return new GenericDAC<Vendor, VendorModel>("ID", false, "Name");
        }

        protected override GenericDAC<v_Vendor, VendorModel> GetViewDAC()
        {
            return new GenericDAC<v_Vendor, VendorModel>("ID", false, "Name");
        }

        public override void Create(VendorModel dr)
        {
            //if (string.IsNullOrEmpty(dr.Code))
                dr.Code = GetVendorCode();

            base.Create(dr);
        }

        public VendorModel RetrieveByCode(string customerCode)
        {
            return new ABCAPOSDAC().RetrieveVendorByCode(customerCode);
        }

        public List<VendorModel> RetrieveAutoComplete(string key)
        {
            return new ABCAPOSDAC().RetrieveVendorAutoComplete(key);
        }
    }
}
