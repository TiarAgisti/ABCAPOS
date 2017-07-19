//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using ABCAPOS.Models;
//using ABCAPOS.DA;
//using ABCAPOS.Util;
//using ABCAPOS.EDM;
//using MPL.Business;

//namespace ABCAPOS.BF
//{
//    public class RateBFC : GenericBFC<Rate, Rate, RateModel>
//    {
//        public override string GenerateID()
//        {
//            throw new NotImplementedException();
//        }

//        protected override GenericDAC<Rate, RateModel> GetDAC()
//        {
//            return new GenericDAC<Rate, RateModel>("ID", false, "Name");
//        }

//        protected override GenericDAC<Rate, RateModel> GetViewDAC()
//        {
//            return new GenericDAC<Rate, RateModel>("ID", false, "Name");
//        }

//        public List<RateModel> Retrieve(bool isActive)
//        {
//            return new ABCAPOSDAC().RetrieveRate(isActive);
//        }

//        public List<VendorModel> RetrieveAutoComplete(string key)
//        {
//            return new ABCAPOSDAC().RetrieveVendorAutoComplete(key);
//        }
//    }
//}
