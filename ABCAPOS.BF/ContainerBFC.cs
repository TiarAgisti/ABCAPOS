using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using ABCAPOS.DA;

namespace ABCAPOS.BF
{
    public class ContainerBFC: GenericBFC<Container,v_Container,ContainerModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        protected override GenericDAC<Container, ContainerModel> GetDAC()
        {
            return new GenericDAC<Container, ContainerModel>("ID", false, "WarehouseName");
        }

        protected override GenericDAC<v_Container, ContainerModel> GetViewDAC()
        {
            return new GenericDAC<v_Container, ContainerModel>("ID", false, "WarehouseName");
        }

        public override void Create(ContainerModel dr)
        {
            base.Create(dr);
        }

        public override void Update(ContainerModel dr)
        {
            base.Update(dr);
        }

        public ContainerModel RetreiveByProductIDWarehouseID(long productID, long WarehouseID)
        {
            return new ABCAPOSDAC().RetreiveContainerByProductIDWarehouseID(productID, WarehouseID);
        }

        public ContainerModel RetreiveByLogIDproductIDdocType(long logID, long productID, int docType)
        {
            return new ABCAPOSDAC().RetreiveByLogIDProductIDDocType(logID, productID, docType);
        }

        public ContainerModel RetreiveByContainerIDProductIDWarehouseID(long containerID, long ProductID, long warehouseID)
        {
            return new ABCAPOSDAC().RetreiveByContainerIDByProductIDWarehouseID(containerID, ProductID, warehouseID);
        }


        internal void UpdateByAssemblyBuild(ContainerModel container, double QtyRemaining, double qty)
        {
            var containerHeader = new ContainerModel();
            containerHeader.ID = container.ID;
            containerHeader.ProductID = container.ProductID;
            containerHeader.WarehouseID = container.WarehouseID;
            QtyRemaining = QtyRemaining - qty;
            containerHeader.Qty = container.Qty - qty;
            containerHeader.Price = container.Price;
            new ContainerBFC().Update(containerHeader);
        }
    }
}
