using ABCAPOS.EDM;
using ABCAPOS.Models;
using ABCAPOS.DA;
using ABCAPOS.Util;
using ABCAPOS.ReportEDS;
using MPL;
using MPL.Business;
using System;
using System.Collections.Generic;
using System.Data.Objects.SqlClient;
using System.Linq;
using System.Text;
using System.Transactions;

namespace ABCAPOS.BF
{
    public class SalesOrderBFC : MasterDetailBFC<SalesOrder, v_SalesOrder, SalesOrderDetail, v_SalesOrderDetail, SalesOrderModel, SalesOrderDetailModel>
    {
        private void IncreaseQtyAvailable(SalesOrderModel header,List<SalesOrderDetailModel>details)
        {
            if (header != null && details !=null )
            {
                foreach (var detail in details)
                {
                    var itemLoc = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductID, header.WarehouseID);
                    var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                    double qtyBaseUnit=0;
                    if (unitRate != null)
                    {
                        qtyBaseUnit = Convert.ToDouble(detail.Quantity) * unitRate;
                    }
                    else
                    {
                        qtyBaseUnit = Convert.ToDouble(detail.Quantity);
                    }
                    if (itemLoc != null)
                    {
                        itemLoc.QtyAvailable += qtyBaseUnit;
                        new ItemLocationBFC().Update(itemLoc);
                    }
                    else
                    {
                        new ItemLocationBFC().Create(detail.ProductID, header.WarehouseID, qtyBaseUnit, qtyBaseUnit);
                    }
                }
            }
        }

        private void DecreaseQtyAvailable(SalesOrderModel header, List<SalesOrderDetailModel> details)
        {
            if (header != null && details != null)
            {
                foreach (var detail in details)
                {
                    var itemLoc = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductID, header.WarehouseID);
                    var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                    double qtyBaseUnit = 0;
                    if (unitRate != null)
                    {
                        qtyBaseUnit = Convert.ToDouble(detail.Quantity) * unitRate;
                    }
                    else
                    {
                        qtyBaseUnit = Convert.ToDouble(detail.Quantity);
                    }
                    if (itemLoc != null)
                    {
                        itemLoc.QtyAvailable -= qtyBaseUnit;
                        new ItemLocationBFC().Update(itemLoc);
                    }
                    else
                    {
                        new ItemLocationBFC().Create(detail.ProductID, header.WarehouseID, -qtyBaseUnit, -qtyBaseUnit);
                    }
                }
            }
        }

        private void CalculateSODiscount(SalesOrderModel header, List<SalesOrderDetailModel> details)
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

            header.SOTotal = SOTotal;
            header.DiscountTotal = discountTotal;
        }

        public void ValidasiCreditLimit(SalesOrderModel header,List<SalesOrderDetailModel>details)
        {
            var cust = new CustomerBFC().RetrieveByID(header.CustomerID);
            if (cust != null)
            {
                if (cust.OnCreditHold == true)
                {
                    decimal total = details.Sum(p => p.Total);
                    if ((cust.CreditLimit - cust.InvoiceBalance) < total)
                        throw new Exception("Transaksi melewati batas credit limit,silahkan hubungin administrator");
                }
               
            }
        }

        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetSalesOrderCode(SalesOrderModel header)
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var salesOrderPrefix = "";

            if (prefixSetting != null)
                salesOrderPrefix = prefixSetting.SalesOrderPrefix;

            var warehouse = new WarehouseBFC().RetrieveByID(header.WarehouseID);
            var year = DateTime.Now.Year.ToString().Substring(2, 2);

            var prefix = salesOrderPrefix + year + "-" + warehouse.Code + "-";
            var code = new ABCAPOSDAC().RetrieveSalesOrderMaxCode(prefix, 7);

            return code;
        }

        protected override GenericDetailDAC<SalesOrderDetail, SalesOrderDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<SalesOrderDetail, SalesOrderDetailModel>("SalesOrderID", "LineSequenceNumber", false);
        }

        protected override GenericDetailDAC<v_SalesOrderDetail, SalesOrderDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_SalesOrderDetail, SalesOrderDetailModel>("SalesOrderID", "LineSequenceNumber", false);
        }

        protected override GenericDAC<SalesOrder, SalesOrderModel> GetMasterDAC()
        {
            return new GenericDAC<SalesOrder, SalesOrderModel>("ID", false, "Date DESC");
        }

        protected override GenericDAC<v_SalesOrder, SalesOrderModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_SalesOrder, SalesOrderModel>("ID", false, "Date DESC");
        }

        public SalesOrderModel RetrieveByCode(string salesCode)
        {
            return new ABCAPOSDAC().RetrieveSalesByCode(salesCode);
        }

        public List<SalesOrderModel> RetrieveByBSID(long bookingSalesID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from j in ent.v_SalesOrder
                        where j.BookingSalesID == bookingSalesID && j.Status != (int)MPL.DocumentStatus.Void
                        select j;

            return ObjectHelper.CopyList<v_SalesOrder, SalesOrderModel>(query.ToList());
        }

        public void ErrorTransaction(SalesOrderModel header, List<SalesOrderDetailModel> details)
        {
            
            foreach (var detail in details)
            {
                var product = new ProductBFC().RetrieveByID(detail.ProductID);

                if (product != null)
                {
                    detail.ProductName = product.ProductName;
                    var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                    detail.StockQty = detail.StockQtyHidden / unitRate;
                    detail.StockAvailable = detail.StockAvailableHidden / unitRate;
                    var beforePPN = Convert.ToDecimal(detail.Quantity) * detail.Price;
                    decimal Total = 0;
                    if (detail.TaxType == (int)TaxType.NonTax)
                        Total = beforePPN;
                    else
                        Total = (beforePPN * Convert.ToDecimal(0.1)) + beforePPN;
                    header.SubTotal += Total;
                }

            }

            header.Details = details;
        }

        public override void Create(SalesOrderModel header, List<SalesOrderDetailModel> details)
        {
            header.Code = GetSalesOrderCode(header);
            CalculateSODiscount(header, details);
            header.IsDeliverable = true;
            header.IsInvoiceable = false;
            header.IsPayable = false;
            header.StatusDescription = "Pending Approval";
            //this.ValidasiCreditLimit(header, details);
            base.Create(header, details);
            UpdateStatus(header.ID);
        }

        public bool UpdateInventory(string key)
        {
            //var salesOrderID = int.Parse(key);

            var salesOrder = RetrieveByID(key);

            if (salesOrder == null)
                return false;

            var details = RetrieveDetails(salesOrder.ID);

            foreach (var detail in details)
            {
                var qtyCustomUnit = detail.Quantity;
                var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                var qtyBaseUnit = detail.Quantity * unitRate;

                var itemLocation = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductID, salesOrder.WarehouseID);
                if (itemLocation != null)
                {
                    itemLocation.QtyAvailable -= qtyBaseUnit;
                    new ItemLocationBFC().Update(itemLocation);
                }
                else
                {
                    new ItemLocationBFC().Create(detail.ProductID,
                      salesOrder.WarehouseID,
                      0,
                      -qtyBaseUnit);
                }
            }
            return true;
        }

        public override void Update(SalesOrderModel header, List<SalesOrderDetailModel> details)
        {
            CalculateSODiscount(header, details);

            using (TransactionScope trans = new TransactionScope())
            {
                //updateFromOldQty(header, details);
                //this.ValidasiCreditLimit(header, details);
                base.Update(header, details);
                UpdateStatus(header.ID);

                trans.Complete();
            }
        }

        public void CopyTransaction(SalesOrderModel header, long salesOrderID)
        {
            var salesOrder = RetrieveByID(salesOrderID);
            var salesOrderDetails = RetrieveDetails(salesOrderID);

            ObjectHelper.CopyProperties(salesOrder, header);

            header.Status = (int)MPL.DocumentStatus.New;

            var details = new List<SalesOrderDetailModel>();

            foreach (var salesOrderDetail in salesOrderDetails)
            {
                var detail = new SalesOrderDetailModel();

                ObjectHelper.CopyProperties(salesOrderDetail, detail);
                detail.HPP = detail.AssetPrice;
                
                details.Add(detail);
            }

            header.Details = details;
        }

        public void CopyTransactionBooking(SalesOrderModel header, long bookingSalesID)
        {
            var bookingSales = new BookingSalesBFC().RetrieveByID(bookingSalesID);
            var bookingSalesDetails = new BookingSalesBFC().RetrieveDetails(bookingSalesID);

            ObjectHelper.CopyProperties(bookingSales, header);

            var customer = new CustomerBFC().RetrieveByID(bookingSales.CustomerID);
            header.Date = DateTime.Now;
            header.BookingSalesID = bookingSalesID;
            header.BookingSalesCode = bookingSales.Code;
            header.TermsOfPaymentID = customer.TermsID;
            header.SalesReference = customer.SalesRepName;
            header.PriceLevelID = customer.PriceLevelID;
            header.BookingSalesID = bookingSalesID;
            header.Status = (int)MPL.DocumentStatus.New;

            var details = new List<SalesOrderDetailModel>();

            foreach (var bookingSalesDetail in bookingSalesDetails)
            {
                var detail = new SalesOrderDetailModel();

                ObjectHelper.CopyProperties(bookingSalesDetail, detail);
                detail.BookingSalesItemNo = bookingSalesDetail.ItemNo;
                detail.Quantity = bookingSalesDetail.Quantity - bookingSalesDetail.CreatedSOQuantity;
                detail.QtyRemaining = detail.Quantity;

                var product = new ProductBFC().RetrieveByID(detail.ProductID);
                detail.SaleUnitRateHidden = new ProductBFC().GetUnitRate(product.SaleUnitID);
                product = new SalesOrderBFC().GetSellingPrice(product, customer);
                var itemLoc = new ItemLocationBFC().RetrieveByProductIDWarehouseID(product.ID, bookingSales.WarehouseID);
                if (itemLoc != null)
                {
                    detail.StockQty = itemLoc.QtyOnHand;
                    detail.StockAvailable = itemLoc.QtyAvailable;
                }
                else
                {
                    detail.StockQty = 0;
                    detail.StockAvailable = 0;
                }
                detail.StockQtyHidden = detail.StockQty;
                detail.StockAvailableHidden = detail.StockAvailable;

                detail.PriceHidden = Convert.ToDouble(product.SellingPrice);

                details.Add(detail);
            }

            header.Details = details;
        }

        public void UpdateStatus(long salesOrderID)
        {
            var order = RetrieveByID(salesOrderID);
            string deliveryStatus;
            string invoiceStatus;
            string paymentStatus;

            var shippedDeliveries = new DeliveryOrderBFC().RetrieveBySalesOrderID(salesOrderID, DeliveryOrderStatus.Shipped);
            var qtyShipped = shippedDeliveries.Sum(p => p.Quantity);

            var soList = new SalesOrderBFC().RetrieveDetails(salesOrderID);
            var qtyInvoiced = soList.Sum(p => p.CreatedInvQuantity);

            var invList = new InvoiceBFC().RetrieveBySalesOrder(salesOrderID);
            var amountInvoiced = invList.Sum(p => p.Amount + p.TaxAmount);
            var amountPaid = invList.Sum(p => p.CreatedPaymentAmount);

            if (order.Status == (int)MPL.DocumentStatus.New)
            {
                order.StatusDescription = "Pending Approval";
                Update(order);
                return;
            }
            else if (order.Status == (int)MPL.DocumentStatus.Void)
            {
                order.StatusDescription = "Void";
                Update(order);
                return;
            }

            if (order.Quantity == order.CreatedDOQuantity)
                order.IsDeliverable = false;
            else
                order.IsDeliverable = true;

            if (qtyShipped == 0)
            {
                deliveryStatus = "Pending Fulfillment";
            }
            else if (qtyShipped == order.Quantity)
            {
                deliveryStatus = "";
            }
            else
            {
                deliveryStatus = "Partially Fulfilled";
            }

            if (qtyInvoiced == order.Quantity)
            {
                order.IsInvoiceable = false;
                invoiceStatus = "Fully Billed";
            }
            else if (qtyInvoiced >= qtyShipped)
            {
                order.IsInvoiceable = true;
                invoiceStatus = "";
            }
            else
            {
                order.IsInvoiceable = true;
                invoiceStatus = "Pending Billing";
            }


            if (amountInvoiced == amountPaid)
            {
                order.IsPayable = false;
                if (qtyInvoiced == order.Quantity)
                {
                    paymentStatus = "Paid in Full";
                    invoiceStatus = "";
                }
                else
                    paymentStatus = "";
            }
            else
            {
                order.IsPayable = true;
                paymentStatus = "Pending Payment";
            }

            var combinator1 = "";
            if (deliveryStatus.Length > 0 && invoiceStatus.Length > 0)
                combinator1 = " / ";
         
            order.StatusDescription = deliveryStatus
                + combinator1 + invoiceStatus;
                //+ combinator2 + paymentStatus;

            Update(order);
        }

        public void UpdateDetails(long salesOrderID, List<SalesOrderDetailModel> soDetails)
        {
            using (TransactionScope trans = new TransactionScope())
            {

                GetDetailDAC().DeleteByParentID(salesOrderID);
                GetDetailDAC().Create(salesOrderID, soDetails);

                trans.Complete();
            }
        }

        public void Approve(long salesOrderID, string userName)
        {
            var salesOrder = RetrieveByID(salesOrderID);
            var details = RetrieveDetails(salesOrderID);

            salesOrder.Status = (int)MPL.DocumentStatus.Approved;
            salesOrder.ApprovedBy = userName;
            salesOrder.ApprovedDate = DateTime.Now;

            Update(salesOrder);

            if (salesOrder.BookingSalesID != 0)
                new BookingSalesBFC().UpdateStatus(salesOrder.BookingSalesID);

            //using (TransactionScope trans = new TransactionScope())
            //{
            //    Update(salesOrder);

            //    if (salesOrder.BookingSalesID != 0)
            //        new BookingOrderBFC().UpdateStatus(salesOrder.BookingSalesID);

            //    //this.DecreaseQtyAvailable(salesOrder, details);

            //    trans.Complete();
            //}
        }

        public void Void(long quotationID, string voidRemarks, string userName)
        {
            var salesOrder = RetrieveByID(quotationID);
            var details = RetrieveDetails(quotationID);

            salesOrder.Status = (int)MPL.DocumentStatus.Void;
            salesOrder.VoidRemarks = voidRemarks;
            salesOrder.ApprovedDate = DateTime.Now;
            salesOrder.ApprovedBy = userName;

            using (TransactionScope trans = new TransactionScope())
            {
                Update(salesOrder);
                //OnVoid(quotationID, voidRemarks, userName);
                //this.IncreaseQtyAvailable(salesOrder, details);

                trans.Complete();
            }
        }

        public void Validate(SalesOrderModel obj, List<SalesOrderDetailModel> details)
        {
            var bsDetails = new BookingSalesBFC().RetrieveDetails(obj.BookingSalesID);

            foreach (var detail in details)
            {
                if (detail.Discount == null)
                    detail.Discount = 0;

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

                var warehouse = new WarehouseBFC().RetrieveByID(obj.WarehouseID);
                var qtyCustomUnit = detail.Quantity;
                var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                var qtyBaseUnit = detail.Quantity * unitRate;
                
                //var itemLocation = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductID, obj.WarehouseID);
                //if (itemLocation == null)
                //{
                //    throw new Exception("Produk "+ product.Code +" tidak ada di gudang " + warehouse.Name);
                //}
                //else
                //{
                    //if (itemLocation.QtyOnHand < qtyBaseUnit)
                    //    throw new Exception("Produk " + product.Code + " tidak ada stok fisik di gudang " + warehouse.Name);
                //}

                if (bsDetails != null)
                {
                    var query = from i in bsDetails
                                where i.ProductID == detail.ProductID && i.ItemNo == detail.BookingSalesItemNo
                                select i;

                    var bsDetail = query.FirstOrDefault();
                    if (bsDetail != null)
                    {
                        var soQty = detail.Quantity;

                        if ((bsDetail.Quantity - bsDetail.CreatedSOQuantity) < soQty)
                            throw new Exception("Jumlah SO tidak boleh lebih dari Booking");
                    }
                }
            }
        }

        public void UpdateValidation(SalesOrderModel obj, List<SalesOrderDetailModel> details)
        {
            var delivery = new DeliveryOrderBFC().RetrieveBySalesOrderID(obj.ID);
            if (delivery != null)
            {
                foreach (var detail in details)
                {
                    if (detail.ConversionIDTemp != 0)
                        detail.ConversionID = detail.ConversionIDTemp;

                    if (detail.Quantity < detail.CreatedDOQuantity)
                        throw new Exception("new QTY" + detail.ProductCode + "cannot less than Qty Fulfilled");

                    if (detail.Quantity < detail.CreatedInvQuantity)
                        throw new Exception("new QTY" + detail.ProductCode + "cannot less than Qty Invoiced");
                    
                }
                //Validate(obj, details);
            }
        }

        public List<SalesOrderModel> RetrieveUncreatedDeliveryOrder(int startIndex, int? amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            if (amount == null)
                amount = SystemConstants.ItemPerPage;

            return new ABCAPOSDAC().RetrieveUncreatedDeliveryOrderSalesOrder(startIndex, (int)amount, sortParameter, selectFilters);
        }

        public int RetrieveUncreatedDeliveryOrderCount(List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveUncreatedDeliveryOrderSalesOrderCount(selectFilters);
        }

        public int RetrieveUnfulfillDeliveryOrderCount()
        {
            return new ABCAPOSDAC().RetrieveUnfulfillDeliveryOrderCount();
        }

        public ABCAPOSReportEDSC.SalesOrderDTRow RetrievePrintOut(long salesOrderID)
        {
            return new ABCAPOSReportDAC().RetrieveSalesOrderPrintOut(salesOrderID);
        }

        public ABCAPOSReportEDSC.SalesOrderDetailDTDataTable RetrieveDetailPrintOut(long salesOrderID)
        {
            return new ABCAPOSReportDAC().RetrieveSalesOrderDetailPrintOut(salesOrderID);
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

            if (product.IsPPNIncluded == true)
                product.SellingPrice = Convert.ToDecimal(Convert.ToDecimal(product.SellingPrice / Convert.ToDecimal(1.1)).ToString("N2"));

            return product;
        }

        public int RetrieveUncreatedInvoiceCount(List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveUncreatedInvoiceOrderCount(selectFilters);
        }

        public List<SalesOrderModel> RetrieveUncreatedInvoice(int startIndex, int? amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            if (amount == null)
                amount = SystemConstants.ItemPerPage;

            return new ABCAPOSDAC().RetrieveUncreatedInvoiceOrder(startIndex, (int)amount, sortParameter, selectFilters);
        }

        public string RetrieveThisMonthSalesOrderCount(long customerID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_SalesOrder
                        where i.Status >= (int)MPL.DocumentStatus.Approved && SqlFunctions.DatePart("month", i.Date) == MPL.Configuration.CurrentDateTime.Month && SqlFunctions.DatePart("year", i.Date) == MPL.Configuration.CurrentDateTime.Year
                        select i;

            if (customerID != 0)
                query = query.Where(p => p.CustomerID == customerID);
            
            return "Rp. " + Convert.ToDecimal(query.Sum(p => p.SubTotal)).ToString("N0");
        }

        public string RetrieveLastMonthSalesOrderCount(long customerID)
        {
            var lastMonth = DateTime.Now.AddMonths(-1);
            
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_SalesOrder
                        where i.Status >= (int)MPL.DocumentStatus.Approved && SqlFunctions.DatePart("month", i.Date) == lastMonth.Month && SqlFunctions.DatePart("year", i.Date) == lastMonth.Year
                        select i;

            if (customerID != 0)
                query = query.Where(p => p.CustomerID == customerID);
            
            return "Rp. " + Convert.ToDecimal(query.Sum(p => p.SubTotal)).ToString("N0");
        }

        public string RetrieveThisYearSalesOrderCount(long customerID)
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_SalesOrder
                        where i.Status >= (int)MPL.DocumentStatus.Approved && SqlFunctions.DatePart("year", i.Date) == MPL.Configuration.CurrentDateTime.Year
                        select i;

            if (customerID != 0)
                query = query.Where(p => p.CustomerID == customerID);
            
            return "Rp. " + Convert.ToDecimal(query.Sum(p => p.SubTotal)).ToString("N0");
        }

        public string RetrieveLastYearSalesOrderCount(long customerID)
        {
            var lastYear = DateTime.Now.AddYears(-1);

            ABCAPOSEntities ent = new ABCAPOSEntities();

            var query = from i in ent.v_SalesOrder
                        where i.Status >= (int)MPL.DocumentStatus.New && SqlFunctions.DatePart("year", i.Date) == lastYear.Year
                        select i;

            if (customerID != 0)
                query = query.Where(p => p.CustomerID == customerID);
            
            return "Rp. " + Convert.ToDecimal(query.Sum(p => p.SubTotal)).ToString("N0");
        }

        public List<SalesOrderModel> RetriveSalesOrderTopList()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_SalesOrderTopTen
                        where i.Status != 0
                        select i;

            return ObjectHelper.CopyList<v_SalesOrderTopTen, SalesOrderModel>(query.ToList());
        }

        public List<SalesOrderModel> RetriveSalesOrderMarketingList()
        {
            ABCAPOSEntities ent = new ABCAPOSEntities();
            var query = from i in ent.v_SalesOrderMarketingTopTen
                        select i;

            return ObjectHelper.CopyList<v_SalesOrderMarketingTopTen, SalesOrderModel>(query.ToList());
        }

        public List<SalesOrderModel> RetreiveByCustomerID(long customerID)
        {
            return new ABCAPOSDAC().RetreiveByCustomerID(customerID);
        }

        #region Notification

        public int RetrieveUncreatedDeliveryOrderCount(CustomerGroupModel customerGroup)
        {
            return new ABCAPOSDAC().RetrieveUncreatedDeliveryOrderSalesOrderCount(customerGroup);
        }

        public int RetrieveUnapprovedSalesOrderCount(CustomerGroupModel customerGroup)
        {
            return new ABCAPOSDAC().RetrieveUnapprovedSalesOrderCount(customerGroup);
        }

        public int RetrieveVoidSalesOrderCount(CustomerGroupModel customerGroup)
        {
            return new ABCAPOSDAC().RetrieveVoidSalesOrderCount(customerGroup);
        }

        #endregion

        #region Sourcode Old
        //public void UpdateSOItemLocationBin(SalesOrderModel header, List<SalesOrderDetailModel> details)
        //{
        //    foreach (var detail in details)
        //    {
        //        var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
        //        var baseSelisihQuantity = detail.SelisihQuantity * unitRate;
        //        var itemLocation = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductID, header.WarehouseID);
        //        if (itemLocation != null)
        //        {
        //            //itemLocation.QtyAvailable -= baseSelisihQuantity;
        //            //itemLocation.QtyOnHand += baseSelisihQuantity;

        //            //new ItemLocationBFC().Update(itemLocation);
        //        }
        //    }
        //}

        //public void updateFromOldQty(SalesOrderModel header, List<SalesOrderDetailModel> details)
        //{
        //    var oldDetails = RetrieveDetails(header.ID);

        //    foreach (var detail in details)
        //    {
        //        var oldDetail = from i in oldDetails
        //                        where i.ProductID == detail.ProductID && i.ItemNo == detail.ItemNo
        //                        select i;

        //        var oldPdDetail = oldDetail.FirstOrDefault();

        //        if (oldPdDetail != null)
        //            detail.SelisihQuantity = detail.Quantity - oldPdDetail.Quantity;
        //        else
        //            detail.SelisihQuantity = detail.Quantity;

        //    }

        //    //Update CreatedDOQuantity
        //    //new SalesOrderBFC().UpdateCreatedPDQuantity(header.PurchaseOrderID, details);
        //    //Update ItemLocation
        //    //Update Bin
        //    //UpdateSOItemLocationBin(header, details);


        //}

        //public void OnVoid(long salesOrderID, string voidRemarks, string userName)
        //{
        //    var salesOrder = RetrieveByID(salesOrderID);
        //    var salesOrderDetails = RetrieveDetails(salesOrderID);

        //    if(salesOrder.Status == (int)MPL.DocumentStatus.Approved)
        //        voidFromOldQty(salesOrder, salesOrderDetails);
        //}

        //public void voidFromOldQty(SalesOrderModel header, List<SalesOrderDetailModel> details)
        //{
        //    var oldDetails = RetrieveDetails(header.ID);

        //    foreach (var detail in details)
        //    {
        //        detail.SelisihQuantity = -detail.Quantity;
        //    }

        //    //Update CreatedDOQuantity
        //    //new SalesOrderBFC().UpdateCreatedPDQuantity(header.PurchaseOrderID, details);
        //    //Update ItemLocation
        //    //Update Bin
        //    //UpdateSOItemLocationBin(header, details);


        //}
        #endregion

       
    }
}
