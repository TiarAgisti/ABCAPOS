using ABCAPOS.DA;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

namespace ABCAPOS.BF
{
    public class LimitStockBFC:GenericBFC<LimitStock,v_LimitStock,LimitStockModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        protected override GenericDAC<LimitStock, LimitStockModel> GetDAC()
        {
            return new GenericDAC<LimitStock, LimitStockModel>("ID", false, "WarehouseName");
        }

        protected override GenericDAC<v_LimitStock, LimitStockModel> GetViewDAC()
        {
            return new GenericDAC<v_LimitStock, LimitStockModel>("ID", false, "WarehouseName");
        }

        public void Create(LimitStockModel Ls, string userName)
        {
            var dac = new ABCAPOSDAC();

            using (TransactionScope trans = new TransactionScope())
            {
                dac.CreateLimitStock(Ls);
                trans.Complete();
            }
        }

        public void Create(long productID, long warehouseID, double QtyMinimum,long UnitID)
        {
            var Ls = new LimitStockModel();
            Ls.ProductID = productID;
            Ls.WarehouseID = warehouseID;
            Ls.Qty_Minimum = QtyMinimum;
            Ls.UnitID = UnitID;

            Create(Ls);
        }

        public void Update(LimitStockModel Ls, string userName)
        {
            var dac = new ABCAPOSDAC();

            var extObj = RetrieveByID(Ls.ID);

            extObj.WarehouseID = Ls.WarehouseID;
            extObj.Qty_Minimum = Ls.Qty_Minimum;
            extObj.UnitID = Ls.UnitID;

            using (TransactionScope trans = new TransactionScope())
            {
                GetDAC().Update(extObj);
                trans.Complete();
            }
        }

        public override void DeleteByID(object id)
        {
            var dac = new ABCAPOSDAC();
            using (TransactionScope trans = new TransactionScope())
            {
                base.DeleteByID(id);
                trans.Complete();
            }
        }

        public List<LimitStockModel> Retrieve(bool isActive)
        {
            return new ABCAPOSDAC().RetrieveLimitStock();
        }

        public LimitStockModel RetrieveByProductIDWarehouseID(long productID, long warehouseID)
        {
            return new ABCAPOSDAC().RetrieveLimitStockByProductID(productID, warehouseID);
        }

        public List<LimitStockModel> RetrieveByProductID(long productID)
        {
            return new ABCAPOSDAC().RetrieveLimitStocksByProductID(productID);
        }
    }


}
