 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using ABCAPOS.Util;
using MPL;
using MPL.Business;
using ABCAPOS.DA;
using ABCAPOS;
using System.Transactions;

namespace ABCAPOS.BF
{
    public class ProductBFC:MasterDetailBFC<Product,v_Product, ProductDetail, v_ProductDetail, ProductModel,ProductDetailModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetProductCode()
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var productPrefix = "";

            if (prefixSetting != null)
                productPrefix = prefixSetting.ProductPrefix;

            var code = new ABCAPOSDAC().RetrieveProductMaxCode(productPrefix, 4);

            return code;
        }

        protected override GenericDetailDAC<ProductDetail, ProductDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<ProductDetail, ProductDetailModel>("ProductID", "ItemNo", false);
        }

        protected override GenericDetailDAC<v_ProductDetail, ProductDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_ProductDetail, ProductDetailModel>("ProductID", "ItemNo", false);
        }

        protected override GenericDAC<Product, ProductModel> GetMasterDAC()
        {
            return new GenericDAC<Product, ProductModel>("ID", false, "ProductName");
        }

        protected override GenericDAC<v_Product, ProductModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_Product, ProductModel>("ID", false, "ProductName");
        }

        public override void Create(ProductModel header, List<ProductDetailModel> details)
        {
            if(header.Code.Length <= 0)
                header.Code = GetProductCode();
          

            using (TransactionScope trans = new TransactionScope())
            {
                base.Create(header, details);

                trans.Complete();
            }
        }

        public LastBuyPriceModel RetrieveLastExchangeRate(long vendorID)
        {
            return new ABCAPOSDAC().RetrieveLastExchangeRate(vendorID);
        }

        public LastBuyPriceModel RetrieveLastBuyPrice(long productID, long customerID)
        {
            return new ABCAPOSDAC().RetrieveLastBuyPrice(productID, customerID);
        }

        public int RetrieveLowQtyCount()
        {
            return new ABCAPOSDAC().RetrieveLowQtyProductCount();
        }

        public List<UnitDetailModel> RetrieveAllUnits(long productID)
        {
            return new ABCAPOSDAC().RetrieveUnitsByProductID(productID);
        }

        public List<UnitDetailModel> RetreiveDetailUnit(long UnitID)
        {
            return new ABCAPOSDAC().RetreiveUnitDetailID(UnitID);
        }

        public List<UnitDetailModel> RetrieveAllUnitsInversed(long productID)
        {
            return new ABCAPOSDAC().RetrieveUnitsByProductIDInversed(productID);
        }

        public List<UnitDetailModel> RetrieveAllUnits()
        {
            return new ABCAPOSDAC().RetrieveUnitsAll();
        }

        public double GetUnitRate(long unitID)
        {
            return new ABCAPOSDAC().RetrieveUnitRateByUnitID(unitID);
        }

        public List<Product> CodeExists(string productCode)
        {
            throw new NotImplementedException();
        }

        public List<ProductModel> RetriveSalesOrderItemSoldList(List<SelectFilter> selectFilters, int Year)
        {
            return new ABCAPOSDAC().RetrieveSalesOrderItemSoldList(selectFilters, Year);
        }

        public List<ProductModel> RetriveProductItemSoldList(List<SelectFilter> selectFilters, int Year)
        {
            return new ABCAPOSDAC().RetrieveProductItemSoldList(selectFilters, Year);
        }
       
        public ProductModel RetrieveByCode(string productCode)
        {
            return new ABCAPOSDAC().RetrieveProductByCode(productCode);
        }

        public List<ProductModel> RetrieveByCode(string productCode, bool isActive)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Product
                        where i.Code == productCode
                            && i.IsActive == isActive
                        select i;

            var obj = new ProductModel();

            return ObjectHelper.CopyList<v_Product, ProductModel>(query.ToList());
        }

        public List<ProductModel> RetreiveByProductID(long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_Product
                        where i.ID == productID
                        select i;

            var obj = new ProductModel();

            return ObjectHelper.CopyList<v_Product, ProductModel>(query.ToList());
        }

        public ProductModel RetrieveByIDPriceLevelIDTaxType(long productID, long priceLevelID, int taxType)
        {
            var product = RetrieveByID(productID);

            if (priceLevelID == (int)SOPriceLevel.D1)
                product.SellingPrice = product.Discount1;
            else if (priceLevelID == (int)SOPriceLevel.D2)
                product.SellingPrice = product.Discount2;
            else if (priceLevelID == (int)SOPriceLevel.D3)
                product.SellingPrice = product.Discount3;
            else if (priceLevelID == (int)SOPriceLevel.D4)
                product.SellingPrice = product.Discount4;
            else if (priceLevelID == (int)SOPriceLevel.D5)
                product.SellingPrice = product.Discount5;
            else if (priceLevelID == (int)SOPriceLevel.D6)
                product.SellingPrice = product.Discount6;
            else
                product.SellingPrice = product.BasePrice;

            if (product.IsPPNIncluded == true)
            {
                if (taxType == (int)TaxType.PPN)
                    product.SellingPrice = Convert.ToDecimal(Convert.ToDecimal(product.SellingPrice / Convert.ToDecimal(1.1)).ToString("N2"));
            }
            return product;
        }

        public ProductModel RetrieveByIDPriceLevelIDTaxTypeWarehouseID(long productID, long priceLevelID, int taxType, long warehouseID)
        {
            var product = RetrieveByID(productID);
            var basePrice = new ABCAPOSDAC().RetrieveBasePriceLocationByProductID(productID, warehouseID);
            if (basePrice != null)
            {
                decimal price = 0;
                decimal price2 = 0;
                if (priceLevelID == (int)SOPriceLevel.D1)
                {
                    price = basePrice.BasePrice - (basePrice.BasePrice * Convert.ToDecimal(0.1));
                    price2 = price - (price * Convert.ToDecimal(0.05));
                    product.SellingPrice = price2 - (price2 * Convert.ToDecimal(0.05));
                    //product.SellingPrice = product.Discount1;
                }
                else if (priceLevelID == (int)SOPriceLevel.D2)
                {
                    price = basePrice.BasePrice - (basePrice.BasePrice * Convert.ToDecimal(0.1));
                    product.SellingPrice = price - (price * Convert.ToDecimal(0.05));
                    //product.SellingPrice = product.Discount2;
                }
                else if (priceLevelID == (int)SOPriceLevel.D3)
                {
                    product.SellingPrice = basePrice.BasePrice - (basePrice.BasePrice * Convert.ToDecimal(0.1));
                    //product.SellingPrice = product.Discount3;
                }
                else if (priceLevelID == (int)SOPriceLevel.D4)
                {
                    price = basePrice.BasePrice - (basePrice.BasePrice * Convert.ToDecimal(0.1));
                    product.SellingPrice = price - (price * Convert.ToDecimal(0.03));
                    //product.SellingPrice = product.Discount4;
                }
                else if (priceLevelID == (int)SOPriceLevel.D5)
                {
                    price = basePrice.BasePrice - (basePrice.BasePrice * Convert.ToDecimal(0.1));
                    product.SellingPrice = price - (price * Convert.ToDecimal(0.08));
                    //product.SellingPrice = product.Discount5;
                }
                else if (priceLevelID == (int)SOPriceLevel.D6)
                {
                    product.SellingPrice = basePrice.BasePrice - (basePrice.BasePrice * Convert.ToDecimal(0.2));
                    //product.SellingPrice = product.Discount6;
                }
                else
                {
                    product.SellingPrice = basePrice.BasePrice;
                }
            }

            if (product.IsPPNIncluded == true)
            {
                if (taxType == (int)TaxType.PPN)
                    product.SellingPrice = Convert.ToDecimal(Convert.ToDecimal(product.SellingPrice / Convert.ToDecimal(1.1)).ToString("N2"));
            }
            return product;
        }

        public List<ProductModel> RetrieveAutoComplete(string key)
        {
            return new ABCAPOSDAC().RetrieveProductAutoComplete(key);
        }

        public List<ProductModel> RetrieveAutoCompleteOnSales(string key)
        {
            return new ABCAPOSDAC().RetrieveProductAutoCompleteOnSales(key);
        }

        public List<ProductModel> RetrieveAutoCompleteAll(string key)
        {
            return new ABCAPOSDAC().RetrieveProductAutoCompleteAll(key);
        }

        public List<ProductModel> RetreiveAutoCompleteWorkOrder(string key)
        {
            return new ABCAPOSDAC().RetreiveProductAutoCompleteWorkOrder(key);
        }

        public List<ProductModel> RetreiveAutoCompleteWorkOrderFG(string key)
        {
            return new ABCAPOSDAC().RetreiveProductAutoCompleteWorkOrderFG(key);
        }

        #region Purchase / Receipt / Bill

        public List<PurchaseOrderDetailModel> RetrievePurchaseOrderDetailByProductID(long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchaseOrderDetail
                        where i.ProductID == productID
                        select i;

            return ObjectHelper.CopyList<v_PurchaseOrderDetail, PurchaseOrderDetailModel>(query.OrderByDescending(p => p.Date).ToList());
            //return ObjectHelper.CopyList<v_PurchaseOrderDetail, PurchaseOrderDetailModel>(query.OrderByDescending(p=>p.Date).Take(SystemConstants.ItemPerPage).ToList());
        }

        public List<PurchaseDeliveryDetailModel> RetrievePurchaseDeliveryDetailByProductID(long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchaseDeliveryDetail
                        where i.ProductID == productID
                        select i;

            return ObjectHelper.CopyList<v_PurchaseDeliveryDetail, PurchaseDeliveryDetailModel>(query.OrderByDescending(p => p.Date).ToList());
            //return ObjectHelper.CopyList<v_PurchaseDeliveryDetail, PurchaseDeliveryDetailModel>(query.OrderByDescending(p => p.Date).Take(SystemConstants.ItemPerPage).ToList());
        }

        public List<PurchaseBillDetailModel> RetrievePurchaseBillDetailByProductID(long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_PurchaseBillDetail
                        where i.ProductID == productID
                        select i;

            return ObjectHelper.CopyList<v_PurchaseBillDetail, PurchaseBillDetailModel>(query.OrderByDescending(p => p.Date).ToList());
            //return ObjectHelper.CopyList<v_PurchaseBillDetail, PurchaseBillDetailModel>(query.OrderByDescending(p => p.Date).Take(SystemConstants.ItemPerPage).ToList());
        }

        #endregion 

        #region Sales / DO / Invoice

        public List<SalesOrderDetailModel> RetrieveSalesOrderDetailByProductID(long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_LogSalesOrderDetail
                        where i.ProductID == productID
                        select i;

            return ObjectHelper.CopyList<v_LogSalesOrderDetail, SalesOrderDetailModel>(query.OrderByDescending(p => p.Date).Take(SystemConstants.ItemPerPage).ToList());
            //return ObjectHelper.CopyList<v_LogSalesOrderDetail, SalesOrderDetailModel>(query.OrderByDescending(p => p.Date).Take(SystemConstants.ItemPerPage).ToList());
        }

        public List<DeliveryOrderDetailModel> RetrieveDeliveryOrderDetailByProductID(long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_LogDeliveryOrderDetail
                        where i.ProductID == productID
                        select i;

            return ObjectHelper.CopyList<v_LogDeliveryOrderDetail, DeliveryOrderDetailModel>(query.OrderByDescending(P => P.Date).Take(SystemConstants.ItemPerPage).ToList());
            //return ObjectHelper.CopyList<v_LogDeliveryOrderDetail, DeliveryOrderDetailModel>(query.OrderByDescending(P=>P.Date).Take(SystemConstants.ItemPerPage).ToList());
        }

        public List<InvoiceDetailModel> RetrieveInvoiceDetailByProductID(long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_LogInvoiceDetail
                        where i.ProductID == productID
                        select i;

            return ObjectHelper.CopyList<v_LogInvoiceDetail, InvoiceDetailModel>(query.OrderByDescending(p => p.Date).Take(SystemConstants.ItemPerPage).ToList());
            //return ObjectHelper.CopyList<v_LogInvoiceDetail, InvoiceDetailModel>(query.OrderByDescending(p => p.Date).Take(SystemConstants.ItemPerPage).ToList());
        }

        public List<InventoryAdjustmentDetailModel> RetrieveInventoryAdjustmentDetailByProductID(long productID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_InventoryAdjustmentDetail
                        where i.ProductID == productID
                        select i;

            return ObjectHelper.CopyList<v_InventoryAdjustmentDetail, InventoryAdjustmentDetailModel>(query.OrderByDescending(p => p.Date).ToList());
            //return ObjectHelper.CopyList<v_InventoryAdjustmentDetail, InventoryAdjustmentDetailModel>(query.OrderByDescending(p => p.Date).Take(SystemConstants.ItemPerPage).ToList());
        }

        #endregion 

        #region Bin

        public BinModel RetrieveBinByCode(string binName, long warehouseID )
        {
            return new ABCAPOSDAC().RetrieveBinByCode(binName, warehouseID);
        }

        public List<BinModel> RetrieveBinAutoComplete(string key, long warehouseID)
        {
            return new ABCAPOSDAC().RetrieveBinAutoComplete(key, warehouseID);
        }

        #endregion

        #region Update Details
        public void UpdateBasePrice(long productID, List<BasePriceLocationModel> basePriceLocs)
        {
            new ABCAPOSDAC().DeleteBasePriceLocation(productID);

            var baseLocs = basePriceLocs;
            if (baseLocs != null)
            {
                foreach (var detail in baseLocs)
                {

                    detail.ProductID = productID;
                    new ABCAPOSDAC().CreateBasePriceLocation(detail);
                }
            }
        }

        /* penambahan by tiar */
        public void UpdateLimitStock(long productID, List<LimitStockModel> Ls, ProductModel Prod)
        {
            new ABCAPOSDAC().DeleteLimitStock(productID);

            var LsQty = Ls;
            if (LsQty != null)
            {
                foreach (var detail in LsQty)
                {
                    detail.ProductID = productID;
                    Validate(Prod);
                    new ABCAPOSDAC().CreateLimitStock(detail);
                }
            }
        }
        /*end*/

        /* penambahan by tiar */
        public void UpdateFormulasi(long productID, List<FormulasiModel> Fo, ProductModel Prod)
        {
            new ABCAPOSDAC().DeleteFormulasi(productID);

            var itemNo = 1;

            var FoBFC = Fo;
            if (FoBFC != null)
            {
                foreach (var detail in FoBFC)
                {
                    if (detail.ProductDetailID == 0)
                        throw new Exception("Product not chosen");

                    detail.ProductID = productID;
                    detail.ItemNo = itemNo++;
                    ValidateProductID(Prod);
                    new ABCAPOSDAC().CreateFormulasi(detail);
                }
            }
        }
        /*end*/

        #endregion

        #region ListPendingWO
        /* penambahan by tiar */
        public int RetreiveListPendingWorkOrderCount(List<SelectFilter> SelectFilters,dynamic ViewBag)
        {
            if ((bool)ViewBag.AllowViewFG)
            {
                return new ABCAPOSDAC().RetreiveListPendingWorkOrderCountFinishGood(SelectFilters);
            }
            else
            {
                return new ABCAPOSDAC().RetreiveListPendingWorkOrderCount(SelectFilters);
            }
          
        }

        public int RetreiveListPendingWorkOrderCountFinishGood(List<SelectFilter> SelectFilters)
        {
            return new ABCAPOSDAC().RetreiveListPendingWorkOrderCountFinishGood(SelectFilters);
        }

        public List<ProductModel> RetrieveListPendingWorkOrder(int startIndex, int? amount, string sortParameter, List<SelectFilter> selectFilters,dynamic ViewBag)
        {
            if (amount == null)
                amount = SystemConstants.ItemPerPage;

            if ((bool)ViewBag.AllowViewFG)
            {
                return new ABCAPOSDAC().RetreiveListPendingWorkOrderFinishGood(startIndex, (int)amount, sortParameter, selectFilters);
            }
            else
            {
                return new ABCAPOSDAC().RetrieveListPendingWorkOrder(startIndex, (int)amount, sortParameter, selectFilters);
            }
        }

        public List<ProductModel> RetreiveListPendingWorkOrderFinishGood(int startIndex, int? amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            if (amount == null)
                amount = SystemConstants.ItemPerPage;

            return new ABCAPOSDAC().RetreiveListPendingWorkOrderFinishGood(startIndex, (int)amount, sortParameter, selectFilters);

        }
        /* end */
        #endregion

        #region validasi
        private void Validate(ProductModel product)
        {
            if (product == null)
                throw new Exception("Failed");

            var warehouseIdList = new List<long>();

            foreach (var limitStock in product.LimitStockDetails)
            {
                if (warehouseIdList.Contains(limitStock.WarehouseID))
                {
                    throw new Exception("Warehouse of limit stock can't be duplicate");
                }

                warehouseIdList.Add(limitStock.WarehouseID);
            }
        }

        private void ValidateProductID(ProductModel product)
        {
            if (product == null)
                throw new Exception("Failed");

            var productdetailIDList = new List<long>();
            foreach (var Formulasi in product.FormulasiDetails)
            {
                if (productdetailIDList.Contains(Formulasi.ProductDetailID))
                {
                    throw new Exception("Product of formulasi can't be duplicate");
                }

                productdetailIDList.Add(Formulasi.ProductDetailID);
            }
        }
        #endregion
      
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
