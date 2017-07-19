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
    public class BasePriceLocationBFC : GenericBFC<BasePriceLocation, v_BasePriceLocation, BasePriceLocationModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        protected override GenericDAC<BasePriceLocation, BasePriceLocationModel> GetDAC()
        {
            return new GenericDAC<BasePriceLocation, BasePriceLocationModel>("ID", false, "WarehouseName");
        }

        protected override GenericDAC<v_BasePriceLocation, BasePriceLocationModel> GetViewDAC()
        {
            return new GenericDAC<v_BasePriceLocation, BasePriceLocationModel>("ID", false, "WarehouseName");
        }

        public void Create(BasePriceLocationModel baseLoc, string userName)
        {
            var dac = new ABCAPOSDAC();

            using (TransactionScope trans = new TransactionScope())
            {
                dac.CreateBasePriceLocation(baseLoc);
                trans.Complete();
            }
        }
        /// <summary>
        /// Creates new BasePriceLocation Entity for ProductID WarehouseID combination
        /// </summary>
        /// <param name="productID">ProductID</param>
        /// <param name="warehouseID">WarehouseID</param>
        /// <param name="qtyOnHand">Initial QTY On Hand</param>
        /// <param name="qtyAvailable">Initial QTY Available</param>
        public void Create(long productID, long warehouseID, decimal basePrice)
        {
            var itemLoc = new BasePriceLocationModel();
            itemLoc.ProductID = productID;
            itemLoc.WarehouseID = warehouseID;
            itemLoc.BasePrice = basePrice;

            Create(itemLoc);
        }

        public void Update(BasePriceLocationModel itemLoc, string userName)
        {
            var dac = new ABCAPOSDAC();

            var extObj = RetrieveByID(itemLoc.ID);

            extObj.WarehouseID = itemLoc.WarehouseID;
            extObj.BasePrice = itemLoc.BasePrice;

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

        public List<BasePriceLocationModel> Retrieve(bool isActive)
        {
            return new ABCAPOSDAC().RetrieveBasePriceLocation();
        }

        public BasePriceLocationModel RetrieveByProductIDWarehouseID(long productID, long warehouseID)
        {
            return new ABCAPOSDAC().RetrieveBasePriceLocationByProductID(productID, warehouseID);
        }

        public List<BasePriceLocationModel> RetrieveByProductID(long productID)
        {
            return new ABCAPOSDAC().RetrieveBasePriceLocationsByProductID(productID);
        }
    }
}
