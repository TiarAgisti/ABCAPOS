using MPL.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.BF.GenericBFC
{
    public abstract class PSIGenericTransactionBFC<Type, vType, ModelType> : GenericTransactionBFC<Type, vType, ModelType>
    {
        private string headerID = "ID";
        private string sortBy = "Code";
        private Boolean sortDesc = true;
        public string HeaderID { get { return this.headerID; } set { this.headerID = value; } }
        public string SortBy { get { return this.sortBy; } set { this.sortBy = value; } }
        public Boolean SortDesc { get { return this.sortDesc; } set { this.sortDesc = value; } }
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }
        protected override GenericDAC<Type, ModelType> GetDAC()
        {
            try
            {
                string defaultSort = this.SortBy + (this.SortDesc ? " DESC" : "");
                return new GenericDAC<Type, ModelType>(this.HeaderID, false, defaultSort);
            }
            catch (Exception e)
            {
                string defaultSort = this.HeaderID + (this.SortDesc ? " DESC" : "");
                return new GenericDAC<Type, ModelType>(this.HeaderID, false, defaultSort);
            }
        }
        protected override GenericDAC<vType, ModelType> GetViewDAC()
        {
            try
            {
                string defaultSort = this.SortBy + (this.SortDesc ? " DESC" : "");
                return new GenericDAC<vType, ModelType>(this.HeaderID, false, defaultSort);
            }
            catch (Exception e)
            {
                string defaultSort = this.HeaderID + (this.SortDesc ? " DESC" : "");
                return new GenericDAC<vType, ModelType>(this.HeaderID, false, defaultSort);
            }
        }
        public virtual string GenerateCode(string prefix, int maxLength = 5)
        {
            string strNumber = this.RetrieveAll().Count > 0 ? this.RetrieveAll().Max(e => e.GetType().GetProperty("Code").GetValue(e, null)).ToString().Split('-')[1] : "0";
            string code = (int.Parse(strNumber) + 1).ToString();
            while (code.Length < maxLength)
            {
                code = "0" + code;
            }
            return prefix + "-" + code;
        }

        public void Approve(string key, string username, int roleID)
        {
            ModelType model = base.RetrieveByID(Convert.ToInt64(key));
            model.GetType().GetProperty("Status").SetValue(model, (int)MPL.DocumentStatus.Approved, null);
            model.GetType().GetProperty("ApprovedBy").SetValue(model, username, null);
            model.GetType().GetProperty("ApprovedDate").SetValue(model, DateTime.Now, null);
            base.Update(model);
        }

        public void Void(string key, string username, int roleID)
        {
            ModelType model = base.RetrieveByID(Convert.ToInt64(key));
            model.GetType().GetProperty("Status").SetValue(model, (int)MPL.DocumentStatus.Void, null);
            model.GetType().GetProperty("VoidedBy").SetValue(model, username, null);
            model.GetType().GetProperty("VoidedDate").SetValue(model, DateTime.Now, null);
            base.Update(model);
        }
    }
}
