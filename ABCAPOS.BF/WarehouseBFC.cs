using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using ABCAPOS.DA;

namespace ABCAPOS.BF
{
    public class WarehouseBFC : GenericBFC<Warehouse, Warehouse, WarehouseModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetWarehouseCode()
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var warehousePrefix = "";

            if (prefixSetting != null)
                warehousePrefix = prefixSetting.WarehousePrefix;

            var code = new ABCAPOSDAC().RetrieveWarehouseMaxCode(warehousePrefix, 4);

            return code;
        }

        protected override GenericDAC<Warehouse, WarehouseModel> GetDAC()
        {
            return new GenericDAC<Warehouse, WarehouseModel>("ID", false, "Code");
        }

        protected override GenericDAC<Warehouse, WarehouseModel> GetViewDAC()
        {
            return new GenericDAC<Warehouse, WarehouseModel>("ID", false, "Code");
        }

        public List<WarehouseModel> Retrieve(bool isActive)
        {
            return new ABCAPOSDAC().RetrieveWarehouse(isActive);
        }

        public override void Create(WarehouseModel dr)
        {
            dr.Code = GetWarehouseCode();
            
            base.Create(dr);
        }


        public dynamic RetrieveActive()
        {
            return base.RetrieveAll().Where(e => e.IsActive).ToList();
        }
    }
}
