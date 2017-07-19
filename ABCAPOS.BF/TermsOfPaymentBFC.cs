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
    public class TermsOfPaymentBFC : GenericBFC<TermsOfPayment, v_TermsOfPayment, TermsOfPaymentModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        protected override GenericDAC<TermsOfPayment, TermsOfPaymentModel> GetDAC()
        {
            return new GenericDAC<TermsOfPayment, TermsOfPaymentModel>("ID", false, "Name");
        }

        protected override GenericDAC<v_TermsOfPayment, TermsOfPaymentModel> GetViewDAC()
        {
            return new GenericDAC<v_TermsOfPayment, TermsOfPaymentModel>("ID", false, "Name");
        }

    }
}
