using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPL.Business;
using ABCAPOS.DA;
using ABCAPOS.EDM;
using ABCAPOS.Util;
using ABCAPOS.BF;
using ABCAPOS.BF.GenericBFC;
using ABCAPOS.Models;


namespace ABCAPOS.BF
{
    public class ExpeditionBFC:PSIMasterDetailBFC<Expedition,v_Expedition,ExpeditionDetail,v_ExpeditionDetail,ExpeditionModel,ExpeditionDetailModel>
    {
        public override void Create(ExpeditionModel header, List<ExpeditionDetailModel> details)
        {
            header.Code = base.GenerateCode("EXP", 6);
            header.Code = header.Code;
            base.Create(header, details);
        }

        public List<ExpeditionModel> RetrieveActive()
        {
            return base.RetrieveAll().Where(e => e.IsActive).ToList();
        }

        public List<ExpeditionModel> RetrieveAutoComplete(string key)
        {
            return new ABCAPOSDAC().RetrieveExpeditionAutoComplete(key);
        }

        public ExpeditionModel RetrieveByCodeOrName(string expeditionName)
        {
            return new ABCAPOSDAC().RetrieveExpeditionByCodeOrName(expeditionName);
        }
    }
}
