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
    public class BinBFC : GenericBFC<Bin, v_Bin, BinModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        protected override GenericDAC<Bin, BinModel> GetDAC()
        {
            return new GenericDAC<Bin, BinModel>("ID", false, "Name");
        }

        protected override GenericDAC<v_Bin, BinModel> GetViewDAC()
        {
            return new GenericDAC<v_Bin, BinModel>("ID", false, "Name");
        }

        /// <summary>
        /// Returns active bins for specified productID and warehouseID
        /// </summary>
        /// <param name="productID"></param>
        /// <param name="warehouseID"></param>
        /// <returns></returns>
        public List<BinModel> Retrieve(long productID, long warehouseID)
        {
            var returnList = new List<BinModel>();
            var objs = new ABCAPOSDAC().RetrieveBinProductWarehouse(productID, warehouseID);
            if (objs != null)
            {
                foreach (var obj in objs)
                {
                    var bin = RetrieveByID(obj.BinID);
                    if (bin.IsActive)
                        returnList.Add(bin);
                }
            }
            return returnList;
        }

        /// <summary>
        /// Returns bins that should be hidden when productID and warehouseID are selected
        /// </summary>
        /// <param name="productID"></param>
        /// <param name="warehouseID"></param>
        /// <returns></returns>
        public List<BinModel> RetrieveInverse(long productID, long warehouseID)
        {
            var dac = new ABCAPOSDAC();
            var binIDs = dac.RetrieveBinIDs();
            var returnList = new List<BinModel>();
            var objs = new ABCAPOSDAC().RetrieveBinProductWarehouse(productID, warehouseID);
            foreach (var obj in objs)
            {
                binIDs.Remove(obj.BinID);
            }

            foreach (var binID in binIDs)
            {
                var bin = RetrieveByID(binID);
                returnList.Add(bin);
            }
            return returnList;
        }

        public BinModel RetrieveDefaultBin(long productID, long warehouseID)
        {
            var obj = new ABCAPOSDAC().RetrieveBinProductWarehouseDefault(productID, warehouseID);
            if (obj == null)
                return null;

            return RetrieveByID(obj.BinID);
        }

        public long RetrieveDefaultBinID(long productID, long warehouseID)
        {
            var obj = new ABCAPOSDAC().RetrieveBinProductWarehouseDefault(productID, warehouseID);
            if (obj == null)
                return 0;

            return obj.BinID;
        }
    }
}
