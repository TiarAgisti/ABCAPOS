using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using ABCAPOS.DA;
using System.Transactions;

namespace ABCAPOS.BF
{
    public class UnitBFC : MasterDetailBFC<Unit, v_Unit, UnitDetail, v_UnitDetail, UnitModel, UnitDetailModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        protected override GenericDetailDAC<UnitDetail, UnitDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<UnitDetail, UnitDetailModel>("UnitID", "ID", false);
        }

        protected override GenericDetailDAC<v_UnitDetail, UnitDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_UnitDetail, UnitDetailModel>("UnitID", "ID", false);
        }

        protected override GenericDAC<Unit, UnitModel> GetMasterDAC()
        {
            return new GenericDAC<Unit, UnitModel>("ID", false, "ID");
        }

        protected override GenericDAC<v_Unit, UnitModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_Unit, UnitModel>("ID", false, "ID");
        }

        public List<UnitModel> Retrieve(bool isActive)
        {
            return new ABCAPOSDAC().RetrieveUnits(isActive);
        }

        public override void Create(UnitModel header, List<UnitDetailModel> details)
        {
            base.Create(header);

            var itemNo = 1;
            var Details = new List<UnitDetailModel>();
            foreach (var detail in details)
            {
                detail.ItemNo = itemNo++;
                detail.BaseID = header.ID;
                Details.Add(detail);
            }
            header.Details = Details;
            base.Update(header, header.Details);
        }

        public override void Update(UnitModel header, List<UnitDetailModel> details)
        {
            var itemNo = 1;
            foreach (var detail in details)
            {
                detail.ItemNo = itemNo++;
                detail.BaseID = header.ID;
            }
            base.Update(header, details);
        }
      
        public void Validate(UnitModel obj, List<UnitDetailModel> details)
        {
            var multiBase = false;
            var baseSelected = false;
            
            if (details.Count <= 0)
                throw new Exception("Must have at least 1 unit within a type");

            foreach (var detail in details)
            {
                if (detail.IsBase && !baseSelected)
                    baseSelected = true;
                else if (detail.IsBase && baseSelected)
                    multiBase = true;
            }
            if (!baseSelected)
                throw new Exception("Must select 1 Unit as Base Unit");
            if (multiBase)
                throw new Exception("Only 1 Base Unit per type is allowed");
        }

        public UnitDetail GetUnitDetailByID(long unitDetailID)
        {
            return new ABCAPOSDAC().RetrieveUnitDetailByID(unitDetailID);
        }

        public Unit GetUnitByID(long unitID)
        {
            return new ABCAPOSDAC().RetrieveUnitByID(unitID);
        }

        public List<UnitDetailModel> RetreiveUnitDetailByUnitID(long unitID)
        {
            return new ABCAPOSDAC().RetreiveUnitDetailID(unitID);
        }

        //Retrieve by UnitID & PluralAbbreviation
        public UnitDetail GetUnitDetailByUnitIDAndPluralAbbreviation(long unitID, string PluralAbbreviation)
        {
            return new ABCAPOSDAC().RetrieveUnitDetailByUnitIDAndPluralAbbreviation(unitID, PluralAbbreviation);
        }

        //public void UpdateDetails(long prID, List<UnitDetailModel> prodDetails)
        //{
        //    var dac = new ABCAPOSDAC();

        //    using (TransactionScope trans = new TransactionScope())
        //    {
        //        GetDetailDAC().DeleteByParentID(prID);
        //        GetDetailDAC().Create(prID, prodDetails);

        //        trans.Complete();
        //    }
        //}

        //public BinModel RetrieveByBinCode(string binCode)
        //{
        //    return new ABCAPOSDAC().RetrieveBinByCode(customerCode);
        //}

        //public List<VendorModel> RetrieveBinAutoComplete(string key)
        //{
        //    return new ABCAPOSDAC().RetrieveVendorAutoComplete(key);
        //}
    }
}
