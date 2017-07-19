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
    public class FormulasiBFC:GenericBFC<Formulasi,v_Formulasi,FormulasiModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        protected override GenericDAC<Formulasi, FormulasiModel> GetDAC()
        {
            return new GenericDAC<Formulasi, FormulasiModel>("ProductID", false, "ProductDetailID");
        }

        protected override GenericDAC<v_Formulasi, FormulasiModel> GetViewDAC()
        {
            return new GenericDAC<v_Formulasi, FormulasiModel>("ProductID", false, "ProductDetailID");
        }

        public void Create(FormulasiModel Fo, string userName)
        {
            var dac = new ABCAPOSDAC();
            if (Fo.ProductDetailID == 0)
                throw new Exception("Product not chosen");

            using (TransactionScope trans = new TransactionScope())
            {
                dac.CreateFormulasi(Fo);
                trans.Complete();
            }
        }

        public void Create(long productID, long productdetailID, Double Qty)
        {
            var Fo = new FormulasiModel();
            if (productdetailID == 0)
                throw new Exception("Product not chosen");

            Fo.ProductID = productID;
            Fo.ProductDetailID = productdetailID;
            Fo.Qty = Qty;
            Create(Fo);
        }

        public void Update(FormulasiModel Fo, string userName)
        {
            var dac = new ABCAPOSDAC();
            var extObj = RetrieveByID(Fo.ProductID);

            extObj.ProductDetailID = Fo.ProductDetailID;
            extObj.Qty = Fo.Qty;

            using (TransactionScope trans = new TransactionScope())
            {
                GetDAC().Update(extObj);
                trans.Complete();
            }
        }

        public override void DeleteByID(object id)
        {
            var dac = new ABCAPOSDAC();
            using (TransactionScope trans= new TransactionScope())
            {
                base.DeleteByID(id);
                trans.Complete();
            }
        }

        public List<FormulasiModel> Retreive(bool isActive)
        {
            return new ABCAPOSDAC().RetreiveFormulasi();
        }

        public FormulasiModel RetreiveByProductIDProductDetailID(long productID, long productdetailID)
        {
            return new ABCAPOSDAC().RetreiveFormulasiByProductID(productID, productdetailID);
        }

        public List<FormulasiModel> RetreiveByProductID(long productID)
        {
            return new ABCAPOSDAC().RetreiveVwFormulasiByProductID(productID);
        }
    }
}
