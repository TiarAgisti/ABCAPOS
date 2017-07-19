using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using ABCAPOS.Models;
using ABCAPOS.BF;
using ABCAPOS.Util;
using MPL;

namespace ABCAPOS
{
    /// <summary>
    /// Summary description for WebService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class WebService : System.Web.Services.WebService
    {
        #region Account
        [WebMethod]
        public List<AccountModel> RetrieveAccountByKey(string q, int limit)
        {
            return new AccountBFC().RetrieveAutoComplete(q);
        }

        [WebMethod]
        public List<AccountModel> RetrieveAccountInvoicePaymentByKey(string q, int limit)
        {
            return new AccountBFC().RetrieveInvoicePaymentAutoComplete(q);
        }

        [WebMethod]
        public List<AccountModel> RetrieveDebitAccount(string q, int limit)
        {
            return new AccountBFC().RetrieveDebitAutoComplete(q);
        }

        [WebMethod]
        public List<AccountModel> RetrieveCreditAccount(string q, int limit)
        {
            return new AccountBFC().RetrieveCreditAutoComplete(q);
        }

        [WebMethod]
        public AccountModel RetrieveAccount(string accountName)
        {
            return new AccountBFC().RetrieveByCode(accountName);
        }
        #endregion

        #region Bin
        [WebMethod]
        public List<BinModel> RetrieveBinByKey(string q, int limit, long warehouseID)
        {
            return new ProductBFC().RetrieveBinAutoComplete(q, Convert.ToInt64(warehouseID));
        }

        [WebMethod]
        public BinModel RetrieveBin(string binName, long warehouseID)
        {
            return new ProductBFC().RetrieveBinByCode(binName, Convert.ToInt64(warehouseID));
        }

        [WebMethod]
        public List<BinModel> RetrieveBinByProductIDWarehouseID(long productID, long warehouseID)
        {
            return new BinBFC().Retrieve(productID, warehouseID);
        }

        [WebMethod]
        public List<BinModel> RetrieveBinByProductIDWarehouseIDInverse(long productID, long warehouseID)
        {
            return new BinBFC().RetrieveInverse(productID, warehouseID);
        }

        [WebMethod]
        public List<BinProductWarehouseModel> RetrieveBinProductWarehouse(long productID, long warehouseID)
        {
            return new BinProductWarehouseBFC().RetrieveByProductIDWarehouseID(productID, warehouseID);
        }

        #endregion

        #region Customer
        [WebMethod]
        public List<CustomerModel> RetrieveCustomerByKey(string q, int limit)
        {
            return new CustomerBFC().RetrieveAutoComplete(q);
        }

        [WebMethod]
        public CustomerModel RetrieveCustomer(string customerName)
        {
            return new CustomerBFC().RetrieveByCodeOrName(customerName);
        }
        #endregion

        #region Currency
        [WebMethod]
        public CurrencyDateModel RetrieveCurrency(string currencyID)
        {
            var now = DateTime.Now.Date;

            var currencyDate = new CurrencyDateBFC().Retrieve(Convert.ToInt64(currencyID), now);

            if (currencyDate == null)
                currencyDate = new CurrencyDateModel();

            return currencyDate;
        }
        #endregion

        #region DeliveryOrder

        [WebMethod]
        public int UpdateDeliveryOrderSJReturnByDOID(long doID, bool isSJReturn)
        {
            var deliveryOrder = new DeliveryOrderBFC().RetrieveByID(doID);
            deliveryOrder.SJReturn = isSJReturn ? 1 : 0;

            new DeliveryOrderBFC().Update(deliveryOrder);

            return 1;
        }


        #endregion 

        #region Employee

        [WebMethod]
        public List<StaffModel> RetrieveEmployeeByKey(string q, int limit)
        {
            return new StaffBFC().RetrieveAutoComplete(q);
        }

        [WebMethod]
        public List<StaffModel> RetrieveEmployeeByKeyAndJobTitle(string q, int limit, string jobTitle)
        {
            return new StaffBFC().RetrieveAutoCompleteByJobTitle(q, jobTitle);
        }

        [WebMethod]
        public StaffModel RetrieveEmployee(string employeeName)
        {
            return new StaffBFC().RetrieveByCode(employeeName);
        }

        #endregion

        #region Expedition
        [WebMethod]
        public List<ExpeditionModel> RetrieveExpeditionByKey(string q, int limit)
        {
            return new ExpeditionBFC().RetrieveAutoComplete(q);
        }

        [WebMethod]
        public ExpeditionModel RetrieveExpedition(string expeditionName)
        {
            return new ExpeditionBFC().RetrieveByCodeOrName(expeditionName);
        }
       
        #endregion

        #region Home

        [WebMethod]
        public string RetrieveThisMonthSalesOrderCount(long customerID)
        {
            return new SalesOrderBFC().RetrieveThisMonthSalesOrderCount(customerID);
        }

        [WebMethod]
        public string RetrieveLastMonthSalesOrderCount(long customerID)
        {
            return new SalesOrderBFC().RetrieveLastMonthSalesOrderCount(customerID);
        }
            
        [WebMethod]
        public string RetrieveThisYearSalesOrderCount(long customerID)
        {
            return new SalesOrderBFC().RetrieveThisYearSalesOrderCount(customerID);
        }

        [WebMethod]
        public string RetrieveLastYearSalesOrderCount(long customerID)
        {
            return new SalesOrderBFC().RetrieveLastYearSalesOrderCount(customerID);
        }
        
        [WebMethod]
        public string RetrieveAveragePaymentDays(long customerID)
        {
            return new InvoiceBFC().RetrieveAveragePaymentDays(customerID);
        }
        
        
        #endregion

        #region Invoice

        [WebMethod]
        public List<MultipleInvoiceItemModel> GetMultipleItemByInvoiceID(long multiInvoiceID, long invoiceID)
        {
            return new MultipleInvoicingBFC().RetrieveMultipleInvoiceItem(multiInvoiceID, invoiceID);
        }
        
        [WebMethod]
        public List<MultipleInvoiceItemModel> GetAllMultipleItemByInvoiceID(long multiInvoiceID, long invoiceID)
        {
            return new MultipleInvoicingBFC().RetrieveAllMultipleInvoiceItem(multiInvoiceID, invoiceID);
        }
        #endregion 

        #region Purchase Order
        [WebMethod]
        public List<PurchaseOrderModel> RetrievePurchaseOrderByKey(string q, int limit)
        {
            var salesOrderList = new PurchaseOrderBFC().RetrieveAutoComplete(q);
            return salesOrderList;
        }

        [WebMethod]
        public PurchaseOrderModel RetrievePurchaseOrder(string poCode)
        {
            return new PurchaseOrderBFC().RetrieveByCode(poCode);
        }
        #endregion

        #region Purchase Bill
        [WebMethod]
        public bool CheckPurchaseBillCode(string billCode, long ID)
        {
            var list = new PurchaseBillBFC().CodeExists(billCode);
            foreach (var item in list)
            {
                if (item.ID != ID)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Product

        [WebMethod]
        public bool CheckProductCode(string productCode, long productID)
        {
            var list = new ProductBFC().RetrieveByCode(productCode, true);
            foreach (var item in list)
            {
                if (item.ID != productID)
                {
                    return true;
                }
            }
            return false;
        }

        [WebMethod]
        public List<ProductModel> RetrieveProductByKey(string q, int limit)
        {
            return new ProductBFC().RetrieveAutoComplete(q);
        }

        [WebMethod]
        public List<ProductModel> RetrieveProductByKeyOnSalesOrder(string q, int limit)
        {
            return new ProductBFC().RetrieveAutoCompleteOnSales(q);
        }

        [WebMethod]
        public List<ProductModel> RetrieveAllProductsByKey(string q, int limit)
        {
            return new ProductBFC().RetrieveAutoCompleteAll(q);
        }

        [WebMethod]
        public List<ProductModel> RetreiveProductWorkOrder(string q)
        {
            if (MembershipHelper.GetRoleID() == (int)PermissionStatus.root || MembershipHelper.GetRoleID() == (int)PermissionStatus.production)
            {
                return new ProductBFC().RetreiveAutoCompleteWorkOrder(q); ;
            }
            else
            {
                return new ProductBFC().RetreiveAutoCompleteWorkOrderFG(q);
            }
          
        }

        [WebMethod]
        public ProductModel RetrieveProduct(string productCode, long customerID, long vendorID)
        {
            var product = new ProductBFC().RetrieveByCode(productCode);
            var productDetails = new ProductBFC().RetrieveDetails(product.ID);
            var itemLoc = new ItemLocationBFC().RetrieveByProductIDWarehouseID(product.ID, customerID);
            if (itemLoc != null)
            {
                product.StockQty = itemLoc.QtyOnHand;
                product.StockAvailable = itemLoc.QtyAvailable;
            }
            else
            {
                product.StockQty = 0;
                product.StockAvailable = 0;
            }

            var lastBuyPrice = new ProductBFC().RetrieveLastBuyPrice(product.ID, vendorID);

            if (lastBuyPrice != null && lastBuyPrice.Price != 0)
                product.AssetPrice = lastBuyPrice.Price;


            return product;
        }

        [WebMethod]
        public ProductModel RetrieveProductOnSalesOrder(string productCode, long customerID, long warehouseID)
        {
            var product = new ProductBFC().RetrieveByCode(productCode);
            var customer = new CustomerBFC().RetrieveByID(customerID);

            product = new SalesOrderBFC().GetSellingPrice(product, customer);
            var itemLoc = new ItemLocationBFC().RetrieveByProductIDWarehouseID(product.ID, warehouseID);
            if (itemLoc != null)
            {
                product.StockQty = itemLoc.QtyOnHand;
                product.StockAvailable = itemLoc.QtyAvailable;
                //product.BasePrice = itemLoc.BasePrice;
            }
            else
            {
                product.StockQty = 0;
                product.StockAvailable = 0;
            }

            //product = new SalesOrderBFC().GetSellingPrice(product, customer);
            return product;

        }

        //[WebMethod]
        //public ProductModel GetProductByIDPriceLevelIDTaxTypeID(string productID, string priceLevelID, string taxType)
        //{
        //    var product = new ProductBFC().RetrieveByIDPriceLevelIDTaxType(Convert.ToInt64(productID), Convert.ToInt64(priceLevelID), Convert.ToInt32(taxType));

        //    return product;
        //}
        [WebMethod]
        public ProductModel GetProductByIDPriceLevelIDTaxTypeID(string productID, string priceLevelID, string taxType,string warehouseID)
        {
            var product = new ProductBFC().RetrieveByIDPriceLevelIDTaxTypeWarehouseID(Convert.ToInt64(productID), Convert.ToInt64(priceLevelID), Convert.ToInt32(taxType), Convert.ToInt64(warehouseID));

            return product;
        }

        [WebMethod]
        public ProductModel RetrieveProductOnTransferOrder(string productCode, long fromWarehouseID)
        {
            var product = new ProductBFC().RetrieveByCode(productCode);

            var itemLoc = new ItemLocationBFC().RetrieveByProductIDWarehouseID(product.ID, fromWarehouseID);
            if (itemLoc != null)
            {
                product.StockQty = itemLoc.QtyOnHand;
                product.StockAvailable = itemLoc.QtyAvailable;
            }
            else
            {
                product.StockQty = 0;
                product.StockAvailable = 0;
            }

            return product;

        }

        [WebMethod]
        public ProductModel RetrieveProductOnInventoryAdjustment(string productCode, long warehouseID)
        {
            var product = new ProductBFC().RetrieveByCode(productCode);

            var itemLoc = new ItemLocationBFC().RetrieveByProductIDWarehouseID(product.ID, warehouseID);
            if (itemLoc != null)
            {
                product.StockQty = itemLoc.QtyOnHand;
                product.StockAvailable = itemLoc.QtyAvailable;
            }
            else
            {
                product.StockQty = 0;
                product.StockAvailable = 0;
            }

            return product;

        }

        [WebMethod]
        public ProductModel RetrieveProductOnLogDetail(string productCode, long warehouseID,DateTime date)
        {
            var product = new ProductBFC().RetrieveByCode(productCode);

            var LogDet = new LogBFC().RetreiveProductQtyOnLogDetail(product.ID, warehouseID, date);
            if (LogDet != null)
            {
                product.StockQty = LogDet.Sum(p => p.MovingInQty - p.MovingOutQty);
            }
            else
            {
                product.StockQty = 0;
            }
            return product;
        }

        /*penambahan by tiar */
        [WebMethod]
        public ProductModel RetrieveProductByCode(string productCode)
        {
            var product = new ProductBFC().RetrieveByCode(productCode);
            var itemLoc = new ItemLocationBFC().RetrieveByProductIDWarehouseID(product.ID, product.WarehouseID);
            if (itemLoc != null)
            {
                product.StockQty = itemLoc.QtyOnHand;
                product.StockAvailable = itemLoc.QtyAvailable;
                product.StockQty = Math.Round(product.StockQty, 0);
                product.StockAvailable = Math.Round(product.StockAvailable, 0);
                //product.BasePrice = itemLoc.BasePrice;
            }
            else
            {
                product.StockQty = 0;
                product.StockAvailable = 0;
            }

            return product;
        }

        [WebMethod]
        public ProductModel RetrieveProductByProductCode(string productCode)
        {
            return new ProductBFC().RetrieveByCode(productCode);
        }

        [WebMethod]
        public ProductModel RetreiveProductFormulasi(string productCode)
        {
            var product = new ProductBFC().RetrieveByCode(productCode);

            return product;
        }
        /*end*/
        #endregion

        #region Vendor
        [WebMethod]
        public List<VendorModel> RetrieveVendorByKey(string q, int limit)
        {
            return new VendorBFC().RetrieveAutoComplete(q);
        }

        [WebMethod]
        public VendorModel RetrieveVendor(string vendorName)
        {
            return new VendorBFC().RetrieveByCode(vendorName);
        }
        #endregion

        #region Warehouse

        [WebMethod]
        public WarehouseModel RetrieveWarehouse(string warehouseID)
        {
            return new WarehouseBFC().RetrieveByID(warehouseID);
        }
        #endregion 

        #region Rate
        [WebMethod]
        public CurrencyDateModel RetrieveRate(string currencyID)
        {
            //var currency = new CurrencyBFC().RetrieveAll();
            //var rate = new RateModel();

            var currencyDate = new CurrencyDateBFC().Retrieve(Convert.ToInt64(currencyID), DateTime.Now);

            return currencyDate;
        }

        [WebMethod]
        public LastBuyPriceModel RetrieveLastExchangeRate(long vendorID)
        {
            return new ProductBFC().RetrieveLastExchangeRate(vendorID);
        }

        #endregion

        #region Resi
        [WebMethod]
        public bool CheckResiCode(string resiCode)
        {
            var list = new ResiBFC().RetrieveResiCode(resiCode);
            if (list != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Resi Payment
        [WebMethod]
        public bool CheckResiPaymentCode(string resipaymentCode)
        {
            var list = new ResiPaymentBFC().RetrieveResiPaymentCode(resipaymentCode);
            if (list != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Staff
        [WebMethod]
        public List<StaffModel> RetrieveStaffByKey(string q, int limit)
        {
            return new StaffBFC().RetrieveAutoComplete(q, true);
        }

        [WebMethod]
        public StaffModel RetrieveStaff(long staffID)
        {
            return new StaffBFC().RetrieveByID(staffID);
        }
        #endregion

        #region Units
        [WebMethod]
        public List<UnitDetailModel> GetUnitsByProductID(long productID)
        {
            return new ProductBFC().RetrieveAllUnits(productID);
        }

        [WebMethod]
        public List<UnitDetailModel> GetUnitsByProductIDInversed(long productID)
        {
            return new ProductBFC().RetrieveAllUnitsInversed(productID);
        }

        [WebMethod]
        public double GetUnitRateByID(long unitDetailID)
        {
            return new ProductBFC().GetUnitRate(unitDetailID);
        }

        [WebMethod]
        public List<UnitDetailModel> GetUnitDetailByUnitID(long unitID)
        {
            return new UnitBFC().RetreiveUnitDetailByUnitID(unitID);
        }

        #endregion

        #region Terms Of Payment
        [WebMethod]
        public TermsOfPaymentModel RetreiveTermsOfPayment(long topID)
        {
            return new TermsOfPaymentBFC().RetrieveByID(topID);
        }
        #endregion

        [WebMethod]
        public string GetAllNotification(string isPurchaseOrderAllowed, string isSalesOrderAllowed, string isDeliveryOrderAllowed, string isInvoiceAllowed, string isPaymentAllowed)
        {
            string textFormat = "{0} ({1}):<br>Anda mempunyai {2} {0} {3}";

            var customerGroupList = new CustomerGroupBFC().RetrieveAll();

            var count = 0;
            var filter = new List<MPL.Business.SelectFilter>();

            var notyList = new List<NotyModel>();

            foreach (var customerGroup in customerGroupList)
            {

                if (Convert.ToBoolean(isPurchaseOrderAllowed) == true)
                {
                    string moduleName = "Purchase Order";

                    var noty = new NotyModel();
                    count = new PurchaseOrderBFC().RetrieveUnapprovedPurchaseOrderCount();
                    noty.ID = "po1";
                    noty.Count = count;
                    noty.Url = "/PurchaseOrder/Index?filter.FilterFields%5B0%5D.Value=21%2F04%2F2014&filter.FilterFields%5B0%5D.Value1=21%2F05%2F2014&filter.FilterFields%5B0%5D.FieldType=DateRange&filter.FilterFields%5B0%5D.PropertyName=TransactionDate&filter.FilterFields%5B0%5D.TargetDataType=System.DateTime&filter.FilterFields%5B0%5D.FilterOperator=Like&filter.FilterFields%5B1%5D.Value=&filter.FilterFields%5B1%5D.FieldType=TextBox&filter.FilterFields%5B1%5D.PropertyName=Code&filter.FilterFields%5B1%5D.TargetDataType=System.String&filter.FilterFields%5B1%5D.FilterOperator=Like&filter.FilterFields%5B2%5D.Value=&filter.FilterFields%5B2%5D.FieldType=TextBox&filter.FilterFields%5B2%5D.PropertyName=CustomerName&filter.FilterFields%5B2%5D.TargetDataType=System.String&filter.FilterFields%5B2%5D.FilterOperator=Like&filter.FilterFields%5B3%5D.Value=1&filter.FilterFields%5B3%5D.FieldType=DropDownList&filter.FilterFields%5B3%5D.PropertyName=CustomerGroupID&filter.FilterFields%5B3%5D.TargetDataType=System.Int64&filter.FilterFields%5B3%5D.FilterOperator=Equal&filter.FilterFields%5B4%5D.Selected=true&filter.FilterFields%5B4%5D.Value=1&filter.FilterFields%5B4%5D.FieldType=DropDownList&filter.FilterFields%5B4%5D.PropertyName=Status&filter.FilterFields%5B4%5D.TargetDataType=System.Int32&filter.FilterFields%5B4%5D.FilterOperator=Equal";
                    noty.Text = String.Format(textFormat, moduleName, customerGroup.Name, count, " yang belum disetujui");
                    notyList.Add(noty);

                    //noty = new NotyModel();
                    //count = new PurchaseOrderBFC().RetrieveUnpaidPurchaseOrderCount();
                    //noty.ID = "po2";
                    //noty.Count = count;
                    //noty.Url = "/UncreatedPurchasePayment";
                    //noty.Text = String.Format(textFormat, moduleName, customerGroup.Name, count, " yang belum lunas");
                    //notyList.Add(noty);

                    noty = new NotyModel();
                    count = new PurchaseOrderBFC().RetrieveVoidPurchaseOrderCount();
                    noty.ID = "po3";
                    noty.Count = count;
                    noty.Url = "/PurchaseOrder/Index?filter.FilterFields%5B0%5D.Value=21%2F04%2F2014&filter.FilterFields%5B0%5D.Value1=21%2F05%2F2014&filter.FilterFields%5B0%5D.FieldType=DateRange&filter.FilterFields%5B0%5D.PropertyName=TransactionDate&filter.FilterFields%5B0%5D.TargetDataType=System.DateTime&filter.FilterFields%5B0%5D.FilterOperator=Like&filter.FilterFields%5B1%5D.Value=&filter.FilterFields%5B1%5D.FieldType=TextBox&filter.FilterFields%5B1%5D.PropertyName=Code&filter.FilterFields%5B1%5D.TargetDataType=System.String&filter.FilterFields%5B1%5D.FilterOperator=Like&filter.FilterFields%5B2%5D.Value=&filter.FilterFields%5B2%5D.FieldType=TextBox&filter.FilterFields%5B2%5D.PropertyName=CustomerName&filter.FilterFields%5B2%5D.TargetDataType=System.String&filter.FilterFields%5B2%5D.FilterOperator=Like&filter.FilterFields%5B3%5D.Value=1&filter.FilterFields%5B3%5D.FieldType=DropDownList&filter.FilterFields%5B3%5D.PropertyName=CustomerGroupID&filter.FilterFields%5B3%5D.TargetDataType=System.Int64&filter.FilterFields%5B3%5D.FilterOperator=Equal&filter.FilterFields%5B4%5D.Selected=true&filter.FilterFields%5B4%5D.Value=0&filter.FilterFields%5B4%5D.FieldType=DropDownList&filter.FilterFields%5B4%5D.PropertyName=Status&filter.FilterFields%5B4%5D.TargetDataType=System.Int32&filter.FilterFields%5B4%5D.FilterOperator=Equal";
                    noty.Text = String.Format(textFormat, moduleName, customerGroup.Name, count, " yang di-void bulan ini");
                    notyList.Add(noty);
                }

                if (Convert.ToBoolean(isSalesOrderAllowed) == true)
                {
                    string moduleName = "Sales Order";

                    var noty = new NotyModel();
                    count = new SalesOrderBFC().RetrieveUnapprovedSalesOrderCount(customerGroup);
                    noty.ID = "so1";
                    noty.Count = count;
                    noty.Url = "/SalesOrder/Index?filter.FilterFields%5B0%5D.Value=21%2F04%2F2014&filter.FilterFields%5B0%5D.Value1=21%2F05%2F2014&filter.FilterFields%5B0%5D.FieldType=DateRange&filter.FilterFields%5B0%5D.PropertyName=TransactionDate&filter.FilterFields%5B0%5D.TargetDataType=System.DateTime&filter.FilterFields%5B0%5D.FilterOperator=Like&filter.FilterFields%5B1%5D.Value=&filter.FilterFields%5B1%5D.FieldType=TextBox&filter.FilterFields%5B1%5D.PropertyName=Code&filter.FilterFields%5B1%5D.TargetDataType=System.String&filter.FilterFields%5B1%5D.FilterOperator=Like&filter.FilterFields%5B2%5D.Value=&filter.FilterFields%5B2%5D.FieldType=TextBox&filter.FilterFields%5B2%5D.PropertyName=CustomerName&filter.FilterFields%5B2%5D.TargetDataType=System.String&filter.FilterFields%5B2%5D.FilterOperator=Like&filter.FilterFields%5B3%5D.Value=1&filter.FilterFields%5B3%5D.FieldType=DropDownList&filter.FilterFields%5B3%5D.PropertyName=CustomerGroupID&filter.FilterFields%5B3%5D.TargetDataType=System.Int64&filter.FilterFields%5B3%5D.FilterOperator=Equal&filter.FilterFields%5B4%5D.Selected=true&filter.FilterFields%5B4%5D.Value=1&filter.FilterFields%5B4%5D.FieldType=DropDownList&filter.FilterFields%5B4%5D.PropertyName=Status&filter.FilterFields%5B4%5D.TargetDataType=System.Int32&filter.FilterFields%5B4%5D.FilterOperator=Equal";
                    noty.Text = String.Format(textFormat, moduleName, customerGroup.Name, count, " yang belum disetujui");
                    notyList.Add(noty);

                    noty = new NotyModel();
                    count = new SalesOrderBFC().RetrieveUncreatedDeliveryOrderCount(customerGroup);
                    noty.ID = "so2";
                    noty.Count = count;
                    noty.Url = "/UncreatedDO";
                    noty.Text = String.Format(textFormat, moduleName, customerGroup.Name, count, " yang belum dibuatkan Delivery Order");
                    notyList.Add(noty);

                    noty = new NotyModel();
                    count = new SalesOrderBFC().RetrieveVoidSalesOrderCount(customerGroup);
                    noty.ID = "so3";
                    noty.Count = count;
                    noty.Url = "/SalesOrder/Index?filter.FilterFields%5B0%5D.Value=21%2F04%2F2014&filter.FilterFields%5B0%5D.Value1=21%2F05%2F2014&filter.FilterFields%5B0%5D.FieldType=DateRange&filter.FilterFields%5B0%5D.PropertyName=TransactionDate&filter.FilterFields%5B0%5D.TargetDataType=System.DateTime&filter.FilterFields%5B0%5D.FilterOperator=Like&filter.FilterFields%5B1%5D.Value=&filter.FilterFields%5B1%5D.FieldType=TextBox&filter.FilterFields%5B1%5D.PropertyName=Code&filter.FilterFields%5B1%5D.TargetDataType=System.String&filter.FilterFields%5B1%5D.FilterOperator=Like&filter.FilterFields%5B2%5D.Value=&filter.FilterFields%5B2%5D.FieldType=TextBox&filter.FilterFields%5B2%5D.PropertyName=CustomerName&filter.FilterFields%5B2%5D.TargetDataType=System.String&filter.FilterFields%5B2%5D.FilterOperator=Like&filter.FilterFields%5B3%5D.Value=1&filter.FilterFields%5B3%5D.FieldType=DropDownList&filter.FilterFields%5B3%5D.PropertyName=CustomerGroupID&filter.FilterFields%5B3%5D.TargetDataType=System.Int64&filter.FilterFields%5B3%5D.FilterOperator=Equal&filter.FilterFields%5B4%5D.Selected=true&filter.FilterFields%5B4%5D.Value=0&filter.FilterFields%5B4%5D.FieldType=DropDownList&filter.FilterFields%5B4%5D.PropertyName=Status&filter.FilterFields%5B4%5D.TargetDataType=System.Int32&filter.FilterFields%5B4%5D.FilterOperator=Equal";
                    noty.Text = String.Format(textFormat, moduleName, customerGroup.Name, count, " yang di-void");
                    notyList.Add(noty);
                }

                if (Convert.ToBoolean(isDeliveryOrderAllowed) == true)
                {
                    string moduleName = "Item Fulfillment";

                    var noty = new NotyModel();
                    count = new DeliveryOrderBFC().RetrieveUnapprovedDeliveryOrderCount(customerGroup);
                    noty.ID = "do1";
                    noty.Count = count;
                    noty.Url = "/DeliveryOrder/Index?filter.FilterFields%5B0%5D.Value=21%2F04%2F2014&filter.FilterFields%5B0%5D.Value1=21%2F05%2F2014&filter.FilterFields%5B0%5D.FieldType=DateRange&filter.FilterFields%5B0%5D.PropertyName=TransactionDate&filter.FilterFields%5B0%5D.TargetDataType=System.DateTime&filter.FilterFields%5B0%5D.FilterOperator=Like&filter.FilterFields%5B1%5D.Value=&filter.FilterFields%5B1%5D.FieldType=TextBox&filter.FilterFields%5B1%5D.PropertyName=Code&filter.FilterFields%5B1%5D.TargetDataType=System.String&filter.FilterFields%5B1%5D.FilterOperator=Like&filter.FilterFields%5B2%5D.Value=&filter.FilterFields%5B2%5D.FieldType=TextBox&filter.FilterFields%5B2%5D.PropertyName=SalesOrderCode&filter.FilterFields%5B2%5D.TargetDataType=System.String&filter.FilterFields%5B2%5D.FilterOperator=Like&filter.FilterFields%5B3%5D.Value=&filter.FilterFields%5B3%5D.FieldType=TextBox&filter.FilterFields%5B3%5D.PropertyName=CustomerName&filter.FilterFields%5B3%5D.TargetDataType=System.String&filter.FilterFields%5B3%5D.FilterOperator=Like&filter.FilterFields%5B4%5D.Selected=true&filter.FilterFields%5B4%5D.Value=1&filter.FilterFields%5B4%5D.FieldType=DropDownList&filter.FilterFields%5B4%5D.PropertyName=Status&filter.FilterFields%5B4%5D.TargetDataType=System.Int32&filter.FilterFields%5B4%5D.FilterOperator=Equal";
                    noty.Text = String.Format(textFormat, moduleName, customerGroup.Name, count, " yang belum disetujui");
                    notyList.Add(noty);

                    noty = new NotyModel();
                    count = new DeliveryOrderBFC().RetrieveVoidDeliveryOrderCount(customerGroup);
                    noty.ID = "do2";
                    noty.Count = count;
                    noty.Url = "/DeliveryOrder/Index?filter.FilterFields%5B0%5D.Value=21%2F04%2F2014&filter.FilterFields%5B0%5D.Value1=21%2F05%2F2014&filter.FilterFields%5B0%5D.FieldType=DateRange&filter.FilterFields%5B0%5D.PropertyName=TransactionDate&filter.FilterFields%5B0%5D.TargetDataType=System.DateTime&filter.FilterFields%5B0%5D.FilterOperator=Like&filter.FilterFields%5B1%5D.Value=&filter.FilterFields%5B1%5D.FieldType=TextBox&filter.FilterFields%5B1%5D.PropertyName=Code&filter.FilterFields%5B1%5D.TargetDataType=System.String&filter.FilterFields%5B1%5D.FilterOperator=Like&filter.FilterFields%5B2%5D.Value=&filter.FilterFields%5B2%5D.FieldType=TextBox&filter.FilterFields%5B2%5D.PropertyName=SalesOrderCode&filter.FilterFields%5B2%5D.TargetDataType=System.String&filter.FilterFields%5B2%5D.FilterOperator=Like&filter.FilterFields%5B3%5D.Value=&filter.FilterFields%5B3%5D.FieldType=TextBox&filter.FilterFields%5B3%5D.PropertyName=CustomerName&filter.FilterFields%5B3%5D.TargetDataType=System.String&filter.FilterFields%5B3%5D.FilterOperator=Like&filter.FilterFields%5B4%5D.Selected=true&filter.FilterFields%5B4%5D.Value=0&filter.FilterFields%5B4%5D.FieldType=DropDownList&filter.FilterFields%5B4%5D.PropertyName=Status&filter.FilterFields%5B4%5D.TargetDataType=System.Int32&filter.FilterFields%5B4%5D.FilterOperator=Equal";
                    noty.Text = String.Format(textFormat, moduleName, customerGroup.Name, count, " yang di-void");
                    notyList.Add(noty);
                }

                if (Convert.ToBoolean(isInvoiceAllowed) == true)
                {
                    string moduleName = "Invoice";

                    var noty = new NotyModel();
                    count = new InvoiceBFC().RetrieveUnapprovedInvoiceCount(customerGroup);
                    noty.ID = "invoice1";
                    noty.Count = count;
                    noty.Url = "/Invoice/Index?filter.FilterFields%5B0%5D.Value=21%2F04%2F2014&filter.FilterFields%5B0%5D.Value1=21%2F05%2F2014&filter.FilterFields%5B0%5D.FieldType=DateRange&filter.FilterFields%5B0%5D.PropertyName=Date&filter.FilterFields%5B0%5D.TargetDataType=System.DateTime&filter.FilterFields%5B0%5D.FilterOperator=Like&filter.FilterFields%5B1%5D.Value=&filter.FilterFields%5B1%5D.FieldType=TextBox&filter.FilterFields%5B1%5D.PropertyName=Code&filter.FilterFields%5B1%5D.TargetDataType=System.String&filter.FilterFields%5B1%5D.FilterOperator=Like&filter.FilterFields%5B2%5D.Value=&filter.FilterFields%5B2%5D.FieldType=TextBox&filter.FilterFields%5B2%5D.PropertyName=DeliveryOrderCode&filter.FilterFields%5B2%5D.TargetDataType=System.String&filter.FilterFields%5B2%5D.FilterOperator=Like&filter.FilterFields%5B3%5D.Value=&filter.FilterFields%5B3%5D.FieldType=TextBox&filter.FilterFields%5B3%5D.PropertyName=CustomerName&filter.FilterFields%5B3%5D.TargetDataType=System.String&filter.FilterFields%5B3%5D.FilterOperator=Like&filter.FilterFields%5B4%5D.Value=1&filter.FilterFields%5B4%5D.FieldType=DropDownList&filter.FilterFields%5B4%5D.PropertyName=CustomerGroupID&filter.FilterFields%5B4%5D.TargetDataType=System.Int64&filter.FilterFields%5B4%5D.FilterOperator=Equal&filter.FilterFields%5B5%5D.Selected=true&filter.FilterFields%5B5%5D.Value=1&filter.FilterFields%5B5%5D.FieldType=DropDownList&filter.FilterFields%5B5%5D.PropertyName=Status&filter.FilterFields%5B5%5D.TargetDataType=System.Int32&filter.FilterFields%5B5%5D.FilterOperator=Equal";
                    noty.Text = String.Format(textFormat, moduleName, customerGroup.Name, count, " yang belum disetujui");
                    notyList.Add(noty);

                    noty = new NotyModel();
                    count = new InvoiceBFC().RetrieveUncreatedPaymentCount(customerGroup);
                    noty.ID = "invoice2";
                    noty.Count = count;
                    noty.Url = "/UncreatedPayment";
                    noty.Text = String.Format(textFormat, moduleName, customerGroup.Name, count, " yang belum lunas");
                    notyList.Add(noty);

                    noty = new NotyModel();
                    count = new InvoiceBFC().RetrieveOverdueInvoiceCount(customerGroup);
                    noty.ID = "invoice3";
                    noty.Count = count;
                    noty.Url = "/UncreatedPayment";
                    noty.Text = String.Format(textFormat, moduleName, customerGroup.Name, count, " yang jatuh tempo");
                    notyList.Add(noty);
                }

                if (Convert.ToBoolean(isPaymentAllowed) == true)
                {
                    string moduleName = "Payment";

                    var noty = new NotyModel();
                    count = new PaymentBFC().RetrieveUnapprovedPaymentCount(customerGroup);
                    noty.ID = "payment";
                    noty.Count = count;
                    noty.Url = "/Payment/Index?filter.FilterFields%5B0%5D.Value=22%2F04%2F2014&filter.FilterFields%5B0%5D.Value1=22%2F05%2F2014&filter.FilterFields%5B0%5D.FieldType=DateRange&filter.FilterFields%5B0%5D.PropertyName=Date&filter.FilterFields%5B0%5D.TargetDataType=System.DateTime&filter.FilterFields%5B0%5D.FilterOperator=Like&filter.FilterFields%5B1%5D.Value=&filter.FilterFields%5B1%5D.FieldType=TextBox&filter.FilterFields%5B1%5D.PropertyName=Code&filter.FilterFields%5B1%5D.TargetDataType=System.String&filter.FilterFields%5B1%5D.FilterOperator=Like&filter.FilterFields%5B2%5D.Value=&filter.FilterFields%5B2%5D.FieldType=TextBox&filter.FilterFields%5B2%5D.PropertyName=InvoiceCode&filter.FilterFields%5B2%5D.TargetDataType=System.String&filter.FilterFields%5B2%5D.FilterOperator=Like&filter.FilterFields%5B3%5D.Value=&filter.FilterFields%5B3%5D.FieldType=TextBox&filter.FilterFields%5B3%5D.PropertyName=CustomerName&filter.FilterFields%5B3%5D.TargetDataType=System.String&filter.FilterFields%5B3%5D.FilterOperator=Like&filter.FilterFields%5B4%5D.Value=1&filter.FilterFields%5B4%5D.FieldType=DropDownList&filter.FilterFields%5B4%5D.PropertyName=CustomerGroupID&filter.FilterFields%5B4%5D.TargetDataType=System.Int64&filter.FilterFields%5B4%5D.FilterOperator=Equal&filter.FilterFields%5B5%5D.Selected=true&filter.FilterFields%5B5%5D.Value=1&filter.FilterFields%5B5%5D.FieldType=DropDownList&filter.FilterFields%5B5%5D.PropertyName=Status&filter.FilterFields%5B5%5D.TargetDataType=System.Int32&filter.FilterFields%5B5%5D.FilterOperator=Equal";
                    noty.Text = String.Format(textFormat, moduleName, customerGroup.Name, count, " yang belum disetujui");
                    notyList.Add(noty);

                    noty = new NotyModel();
                    count = new PaymentBFC().RetrieveVoidPaymentCount(customerGroup);
                    noty.ID = "payment2";
                    noty.Count = count;
                    noty.Url = "/Payment/Index?filter.FilterFields%5B0%5D.Value=22%2F04%2F2014&filter.FilterFields%5B0%5D.Value1=22%2F05%2F2014&filter.FilterFields%5B0%5D.FieldType=DateRange&filter.FilterFields%5B0%5D.PropertyName=Date&filter.FilterFields%5B0%5D.TargetDataType=System.DateTime&filter.FilterFields%5B0%5D.FilterOperator=Like&filter.FilterFields%5B1%5D.Value=&filter.FilterFields%5B1%5D.FieldType=TextBox&filter.FilterFields%5B1%5D.PropertyName=Code&filter.FilterFields%5B1%5D.TargetDataType=System.String&filter.FilterFields%5B1%5D.FilterOperator=Like&filter.FilterFields%5B2%5D.Value=&filter.FilterFields%5B2%5D.FieldType=TextBox&filter.FilterFields%5B2%5D.PropertyName=InvoiceCode&filter.FilterFields%5B2%5D.TargetDataType=System.String&filter.FilterFields%5B2%5D.FilterOperator=Like&filter.FilterFields%5B3%5D.Value=&filter.FilterFields%5B3%5D.FieldType=TextBox&filter.FilterFields%5B3%5D.PropertyName=CustomerName&filter.FilterFields%5B3%5D.TargetDataType=System.String&filter.FilterFields%5B3%5D.FilterOperator=Like&filter.FilterFields%5B4%5D.Value=1&filter.FilterFields%5B4%5D.FieldType=DropDownList&filter.FilterFields%5B4%5D.PropertyName=CustomerGroupID&filter.FilterFields%5B4%5D.TargetDataType=System.Int64&filter.FilterFields%5B4%5D.FilterOperator=Equal&filter.FilterFields%5B5%5D.Selected=true&filter.FilterFields%5B5%5D.Value=0&filter.FilterFields%5B5%5D.FieldType=DropDownList&filter.FilterFields%5B5%5D.PropertyName=Status&filter.FilterFields%5B5%5D.TargetDataType=System.Int32&filter.FilterFields%5B5%5D.FilterOperator=Equal";
                    noty.Text = String.Format(textFormat, moduleName, customerGroup.Name, count, " yang di-void");
                    notyList.Add(noty);
                }
            }

            var json = JSONHelper.ToJSON(notyList);

            return json;
        }

       

    }
}
