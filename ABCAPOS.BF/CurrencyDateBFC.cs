using ABCAPOS.DA;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.BF
{
    public class CurrencyDateBFC:GenericBFC<CurrencyDate,v_CurrencyDate,CurrencyDateModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        protected override GenericDAC<CurrencyDate, CurrencyDateModel> GetDAC()
        {
            return new GenericDAC<CurrencyDate, CurrencyDateModel>("ID", false);
        }

        protected override GenericDAC<v_CurrencyDate, CurrencyDateModel> GetViewDAC()
        {
            return new GenericDAC<v_CurrencyDate, CurrencyDateModel>("ID", false);
        }

        public void Create(long currencyID, decimal value)
        {
            var currencyDate = new CurrencyDateModel();
            var now = DateTime.Now.Date;

            currencyDate.CurrencyID = currencyID;
            currencyDate.Value = value;

            var extObj = new ABCAPOSDAC().RetrieveCurrencyDate(currencyID, now);

            if (extObj != null)
            {
                extObj.Value = value;
                Update(extObj);
            }
            else
                Create(currencyDate);
        }

        public void Validate(CurrencyDateModel obj)
        {
            var extObj = new ABCAPOSDAC().RetrieveCurrencyDate(obj.CurrencyID, obj.Date);

            if (extObj != null && extObj.ID!=obj.ID)
            {
                obj.CurrencyName = extObj.CurrencyName;

                throw new Exception("Nilai kurs untuk tanggal terpilih sudah tersedia");
            }
        }

        public CurrencyDateModel Retrieve(long currencyID, DateTime date)
        {
            return new ABCAPOSDAC().RetrieveCurrencyDate(currencyID, date);
        }

        public List<CurrencyDateModel> Retrieve(long currencyID)
        {
            return new ABCAPOSDAC().RetrieveCurrencyDate(currencyID);
        }
    }
}
