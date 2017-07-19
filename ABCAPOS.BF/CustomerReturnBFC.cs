using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using ABCAPOS.DA;
using ABCAPOS.Util;
using System.Transactions;
using ABCAPOS.ReportEDS;
using MPL;

namespace ABCAPOS.BF
{
    public class CustomerReturnBFC : MasterDetailBFC<CustomerReturn, v_CustomerReturn, CustomerReturnDetail, v_CustomerReturnDetail, CustomerReturnModel, CustomerReturnDetailModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetCustomerReturnCode()
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var customerReturnPrefix = "";

            if (prefixSetting != null)
                customerReturnPrefix = prefixSetting.CustomerReturnPrefix;

            var code = new ABCAPOSDAC().RetrieveCustomerReturnMaxCode(customerReturnPrefix, 4);

            return code;
        }

        protected override GenericDetailDAC<CustomerReturnDetail, CustomerReturnDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<CustomerReturnDetail, CustomerReturnDetailModel>("CustomerReturnID", "LineSequenceNumber", false);
        }

        protected override GenericDetailDAC<v_CustomerReturnDetail, CustomerReturnDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_CustomerReturnDetail, CustomerReturnDetailModel>("CustomerReturnID", "LineSequenceNumber", false);
        }

        protected override GenericDAC<CustomerReturn, CustomerReturnModel> GetMasterDAC()
        {
            return new GenericDAC<CustomerReturn, CustomerReturnModel>("ID", false, "Date DESC");
        }

        protected override GenericDAC<v_CustomerReturn, CustomerReturnModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_CustomerReturn, CustomerReturnModel>("ID", false, "Date DESC");
        }

        private void CalculateSODiscount(CustomerReturnModel header, List<CustomerReturnDetailModel> details)
        {
            decimal SOTotal = 0;
            decimal discountTotal = 0;

            foreach (var detail in details)
            {
                if (detail.Discount == 0)
                    detail.Discount = 0;

                var total = Convert.ToDecimal(detail.Quantity) * detail.Price;
                SOTotal += Convert.ToDecimal(total);

                var discount = Convert.ToDecimal(detail.Quantity) * detail.Discount;
                discountTotal += Convert.ToDecimal(discount);
            }

            //header.SOTotal = SOTotal;
            header.DiscountTotal = discountTotal;
        }

        public override void Create(CustomerReturnModel header, List<CustomerReturnDetailModel> details)
        {
            header.Code = GetCustomerReturnCode();
            base.Create(header, details);
        }

        //public bool UpdateInventory(string key)
        //{
        //    var customerReturn = RetrieveByID(key);

        //    if (customerReturn == null)
        //        return false;

        //    var details = RetrieveDetails(customerReturn.ID);

        //    foreach (var detail in details)
        //    {
        //        var qtyCustomUnit = detail.Quantity;
        //        var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
        //        var qtyBaseUnit = detail.Quantity * unitRate;

        //        var itemLocation = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductID, customerReturn.WarehouseID);
        //        if (itemLocation != null)
        //        {
        //            itemLocation.QtyAvailable -= qtyBaseUnit;
        //            new ItemLocationBFC().Update(itemLocation);
        //        }
        //        else
        //        {
        //            new ItemLocationBFC().Create(detail.ProductID,
        //              customerReturn.WarehouseID,
        //              0,
        //              -qtyBaseUnit);
        //        }
        //    }
        //    return true;
        //}


        public override void Update(CustomerReturnModel header, List<CustomerReturnDetailModel> details)
        {
            CalculateSODiscount(header, details);
            base.Update(header, details);
        }

        public void CopyTransaction(CustomerReturnModel header, long customerReturnID)
        {
            var customerReturn = RetrieveByID(customerReturnID);
            var customerReturnDetails = RetrieveDetails(customerReturnID);

            ObjectHelper.CopyProperties(customerReturn, header);

            header.Status = (int)MPL.DocumentStatus.New;

            var details = new List<CustomerReturnDetailModel>();

            foreach (var customerReturnDetail in customerReturnDetails)
            {
                var detail = new CustomerReturnDetailModel();

                ObjectHelper.CopyProperties(customerReturnDetail, detail);
                detail.HPP = detail.AssetPrice;

                details.Add(detail);
            }

            header.Details = details;
        }


        public void UpdateDetails(long customerReturnID, List<CustomerReturnDetailModel> spkDetails)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                GetDetailDAC().DeleteByParentID(customerReturnID);
                GetDetailDAC().Create(customerReturnID, spkDetails);

                trans.Complete();
            }
        }

        public void Void(long customerReturnID, string voidRemarks, string userName)
        {
            var customerReturn = RetrieveByID(customerReturnID);

            customerReturn.Status = (int)MPL.DocumentStatus.Void;
            customerReturn.VoidRemarks = voidRemarks;
            customerReturn.ApprovedDate = DateTime.Now;
            customerReturn.ApprovedBy = userName;

            using (TransactionScope trans = new TransactionScope())
            {
                Update(customerReturn);

                trans.Complete();
            }
        }

        public void Validate(CustomerReturnModel obj, List<CustomerReturnDetailModel> details)
        {
            foreach (var detail in details)
            {
                if (detail.ProductID == 0)
                    throw new Exception("Produk belum dipilih");

                if (detail.Quantity == 0)
                    throw new Exception("Qty Produk tidak boleh nol");

                var product = new ProductBFC().RetrieveByID(detail.ProductID);
                var customer = new CustomerBFC().RetrieveByID(obj.CustomerID);

                var total = Convert.ToDecimal(detail.Quantity) * detail.Price;
                var totalPrice = total - (total * detail.Discount);

                //if (totalPrice == 0)
                //    throw new Exception("Total harus lebih besar dari nol");
            }
        }

        public List<CustomerReturnModel> RetrieveUncreatedReturnReceipt(int startIndex, int? amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            if (amount == null)
                amount = SystemConstants.ItemPerPage;

            return new ABCAPOSDAC().RetrieveUncreatedReceiptsCustomerReturn(startIndex, (int)amount, sortParameter, selectFilters);
        }

        public int RetrieveUncreatedReturnReceiptCount(List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveUncreatedReceiptsCustomerReturnCount(selectFilters);
        }

        public ProductModel GetSellingPrice(ProductModel product, CustomerModel customer)
        {
            if (customer != null)
            {
                if (customer.PriceLevelID == (int)SOPriceLevel.D1)
                    product.SellingPrice = product.Discount1;
                else if (customer.PriceLevelID == (int)SOPriceLevel.D2)
                    product.SellingPrice = product.Discount2;
                else if (customer.PriceLevelID == (int)SOPriceLevel.D3)
                    product.SellingPrice = product.Discount3;
                else if (customer.PriceLevelID == (int)SOPriceLevel.D4)
                    product.SellingPrice = product.Discount4;
                else if (customer.PriceLevelID == (int)SOPriceLevel.D5)
                    product.SellingPrice = product.Discount5;
                else if (customer.PriceLevelID == (int)SOPriceLevel.D6)
                    product.SellingPrice = product.Discount6;
                else
                    product.SellingPrice = product.BasePrice;
            }
            else
                product.SellingPrice = product.BasePrice;

            return product;
        }

        //public int RetrieveUncreatedInvoiceCount(List<SelectFilter> selectFilters)
        //{
        //    return new ABCAPOSDAC().RetrieveUncreatedInvoiceOrderCount(selectFilters);
        //}

        //public List<CustomerReturnModel> RetrieveUncreatedInvoice(int startIndex, int? amount, string sortParameter, List<SelectFilter> selectFilters)
        //{
        //    if (amount == null)
        //        amount = SystemConstants.ItemPerPage;

        //    return new ABCAPOSDAC().RetrieveUncreatedInvoiceOrder(startIndex, (int)amount, sortParameter, selectFilters);
        //}

        #region Notification

        //public int RetrieveUncreatedDeliveryOrderCount(CustomerGroupModel customerGroup)
        //{
        //    return new ABCAPOSDAC().RetrieveUncreatedDeliveryOrderCustomerReturnCount(customerGroup);
        //}

        //public int RetrieveUnapprovedCustomerReturnCount(CustomerGroupModel customerGroup)
        //{
        //    return new ABCAPOSDAC().RetrieveUnapprovedCustomerReturnCount(customerGroup);
        //}

        //public int RetrieveVoidCustomerReturnCount(CustomerGroupModel customerGroup)
        //{
        //    return new ABCAPOSDAC().RetrieveVoidCustomerReturnCount(customerGroup);
        //}

        #endregion

        public void PreFillWithSalesOrderData(CustomerReturnModel header, long p)
        {
            var soBFC = new SalesOrderBFC();
            var source = soBFC.RetrieveByID(p);
            if (source == null)
                return;

            header.Code = GetCustomerReturnCode();
            header.Date = DateTime.Now;
            header.CustomerID = source.CustomerID;
            header.CustomerCode = source.CustomerCode;
            header.CustomerName = source.CustomerName;
            header.PriceLevelID = new CustomerBFC().RetrieveByID(source.CustomerID).PriceLevelID;
            header.RefPO = source.POCustomerNo;
            header.Title = source.Title;
            header.RefSO = source.Code;

            // find all DO's for this SalesOrder
            var deliveries = new DeliveryOrderBFC().RetrieveBySalesOrderID(source.ID);
            for (int i = 0; i < deliveries.Count; i++)
            {
                header.RefDO += deliveries[i].Code;
                if (i < deliveries.Count - 1)
                {
                    header.RefDO += ", ";
                }
            }

            header.SalesEffectiveDate = DateTime.Now;
            //header.LeadSourceID = ?
            //header.PartnerID = ?
            //header.ExcludeCommisions = ?
            header.SalesOrderID = source.ID;
            header.SalesOrderCode = source.Code;
            header.DepartmentID = source.DepartmentID;
            header.DepartmentName = source.DepartmentName;
            header.WarehouseID = source.WarehouseID;
            header.WarehouseName = source.WarehouseName;
            header.SalesmanID = source.SalesmanID;
            header.SalesName = source.SalesName;
            header.ShipTo = source.ShipTo;
            header.EmployeeID = source.EmployeeID;
            header.EmployeeName = source.EmployeeName;
            header.Currency = source.Currency;
            header.ExchangeRate = source.ExchangeRate;
            //header.AltSalesTotal = ?
            header.TaxType = source.TaxType;
            header.DiscountTotal = source.DiscountTotal;

            header.SalesReference = source.SalesReference;

            header.ExchangeRate = source.ExchangeRate;

            var sourceDetails = soBFC.RetrieveDetails(p);
            var returnDetails = new List<CustomerReturnDetailModel>();
            var prodBFC = new ProductBFC();

            foreach (var sourceDetail in sourceDetails)
            {
                var detail = new CustomerReturnDetailModel();
                detail.ProductID = sourceDetail.ProductID;
                detail.ProductCode = sourceDetail.ProductCode;
                detail.ProductName = sourceDetail.ProductName;
                detail.Quantity = sourceDetail.Quantity;
                detail.ConversionID = sourceDetail.ConversionID;
                detail.ConversionName = sourceDetail.ConversionName;
                detail.PriceLevelID = sourceDetail.PriceLevelID;
                detail.Price = sourceDetail.Price;
                detail.AssetPrice = sourceDetail.AssetPrice;
                if (sourceDetail.Discount != null)
                    detail.Discount = (decimal) sourceDetail.Discount;
                detail.Remarks = sourceDetail.Remarks;
                detail.Barcode = sourceDetail.Barcode;
                detail.PriceLevelName = sourceDetail.PriceLevelName;
                detail.HPP = sourceDetail.HPP;
                detail.TaxType = sourceDetail.TaxType;
                detail.PriceHidden = sourceDetail.PriceHidden;
                detail.SaleUnitRateHidden = prodBFC.GetUnitRate(detail.ConversionID);
                returnDetails.Add(detail);
            }

            header.Details = returnDetails;
            OnCreateCustomerReturn(header, header.Details);
        }

        private void OnCreateCustomerReturn(CustomerReturnModel header, List<CustomerReturnDetailModel> details)
        {
            decimal amount = details.Sum(p => p.TotalAmount);
            decimal taxAmount = details.Sum(p => p.TotalPPN);
            decimal discountAmount = Convert.ToDecimal(details.Sum(p => p.Discount));
            double quantity = details.Sum(p => p.Quantity);

            header.DiscountTotal = discountAmount;

        }

        public List<CustomerReturnModel> RetrieveBySalesOrder(long salesOrderID)
        {
            return new ABCAPOSDAC().RetrieveCustomerReturnBySalesOrder(salesOrderID);
        }
    }
}
