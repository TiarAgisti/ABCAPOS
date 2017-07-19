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
    public class ItemLocationBFC : GenericBFC<ItemLocation, v_ItemLocation, ItemLocationModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        protected override GenericDAC<ItemLocation, ItemLocationModel> GetDAC()
        {
            return new GenericDAC<ItemLocation, ItemLocationModel>("ID", false, "WarehouseName");
        }

        protected override GenericDAC<v_ItemLocation, ItemLocationModel> GetViewDAC()
        {
            return new GenericDAC<v_ItemLocation, ItemLocationModel>("ID", false, "WarehouseName");
        }

        public void Create(ItemLocationModel itemLoc, string userName)
        {
            var dac = new ABCAPOSDAC();

            using (TransactionScope trans = new TransactionScope())
            {
                dac.CreateItemLocation(itemLoc);
                trans.Complete();
            }
        }
        /// <summary>
        /// Creates new ItemLocation Entity for ProductID WarehouseID combination
        /// </summary>
        /// <param name="productID">ProductID</param>
        /// <param name="warehouseID">WarehouseID</param>
        /// <param name="qtyOnHand">Initial QTY On Hand</param>
        /// <param name="qtyAvailable">Initial QTY Available</param>
        public void Create(long productID, long warehouseID, double qtyOnHand, double qtyAvailable)
        {
            var itemLoc = new ItemLocationModel();
            itemLoc.ProductID = productID;
            itemLoc.WarehouseID = warehouseID;
            itemLoc.QtyOnHand = qtyOnHand;
            itemLoc.QtyAvailable = qtyAvailable;
            Create(itemLoc);
        }

        public void Update(ItemLocationModel itemLoc, string userName)
        {
            var dac = new ABCAPOSDAC();

            var extObj = RetrieveByID(itemLoc.ID);

            extObj.WarehouseID = itemLoc.WarehouseID;
            extObj.QtyOnHand = itemLoc.QtyOnHand;
            extObj.QtyAvailable = itemLoc.QtyAvailable;
            
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

        public List<ItemLocationModel> Retrieve(bool isActive)
        {
            return new ABCAPOSDAC().RetrieveItemLocation();
        }

        public ItemLocationModel RetrieveByProductIDWarehouseID(long productID, long warehouseID)
        {
            return new ABCAPOSDAC().RetrieveItemLocationByProductID(productID, warehouseID);
        }

        public List<ItemLocationModel> RetrieveByProductID(long productID)
        {
            return new ABCAPOSDAC().RetrieveItemLocationsByProductID(productID);
        }
    }
}
