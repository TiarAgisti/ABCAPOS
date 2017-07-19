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
    public class PaymentMethodBFC:GenericBFC<PaymentMethod,v_PaymentMethod,PaymentMethodModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        protected override GenericDAC<PaymentMethod, PaymentMethodModel> GetDAC()
        {
            return new GenericDAC<PaymentMethod, PaymentMethodModel>("ID", false, "Name");
        }

        protected override GenericDAC<v_PaymentMethod, PaymentMethodModel> GetViewDAC()
        {
            return new GenericDAC<v_PaymentMethod, PaymentMethodModel>("ID", false, "Name");
        }

        public List<PaymentMethodModel> Retrieve(bool isActive)
        {
            return new ABCAPOSDAC().RetrievePaymentMethod(isActive);
        }
    }
}
