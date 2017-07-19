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
    public class BinProductWarehouseBFC : GenericBFC<BinProductWarehouse, v_BinProductWarehouse, BinProductWarehouseModel>
    {

        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        protected override GenericDAC<BinProductWarehouse, BinProductWarehouseModel> GetDAC()
        {
            return new GenericDAC<BinProductWarehouse, BinProductWarehouseModel>("ID", false, "ProductID");
        }

        protected override GenericDAC<v_BinProductWarehouse, BinProductWarehouseModel> GetViewDAC()
        {
            return new GenericDAC<v_BinProductWarehouse, BinProductWarehouseModel>("ID", false, "ProductID");
        }

        public void Create(BinProductWarehouseModel binProduct, string userName)
        {
            var dac = new ABCAPOSDAC();

            using (TransactionScope trans = new TransactionScope())
            {
                dac.CreateBinProductWarehouse(binProduct);
                trans.Complete();
            }
        }

        public void Update(BinProductWarehouseModel binProduct, string userName)
        {
            var dac = new ABCAPOSDAC();

            var extObj = RetrieveByID(binProduct.ID);

            extObj.Quantity = binProduct.Quantity;
            

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

        public BinProductWarehouseModel RetrieveByBinIDProductIDWarehouseID(long binID, long productID, long warehouseID)
        {
            return new ABCAPOSDAC().RetrieveBinProductByBinIDProductID(binID, productID, warehouseID);
        }

        public BinProductWarehouseModel Retrieve(long binID, long productID)
        {
            return new ABCAPOSDAC().RetrieveBin(binID, productID);
        }

        public List<BinProductWarehouseModel> RetrieveByProductIDWarehouseID(long productID, long warehouseID)
        {
            return new ABCAPOSDAC().RetrieveBinProductWarehouse(productID, warehouseID);
        }
    }
}
