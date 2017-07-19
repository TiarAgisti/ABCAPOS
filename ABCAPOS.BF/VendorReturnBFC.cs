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
    public class VendorReturnBFC : MasterDetailBFC<VendorReturn, v_VendorReturn, VendorReturnDetail, v_VendorReturnDetail, VendorReturnModel, VendorReturnDetailModel>
    {
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetVendorReturnCode(VendorReturnModel header)
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var VendorReturnPrefix = "";

            if (prefixSetting != null)
                VendorReturnPrefix = prefixSetting.VendorReturnPrefix;

            var warehouse = new WarehouseBFC().RetrieveByID(header.WarehouseID);
            var year = DateTime.Now.Year.ToString().Substring(2, 2);

            var prefix = VendorReturnPrefix + year + "-" + warehouse.Code + "-";

            var code = new ABCAPOSDAC().RetrieveVendorReturnMaxCode(prefix, 7);

            return code;
        }

        protected override GenericDetailDAC<VendorReturnDetail, VendorReturnDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<VendorReturnDetail, VendorReturnDetailModel>("VendorReturnID", "LineSequenceNumber", false);
        }

        protected override GenericDetailDAC<v_VendorReturnDetail, VendorReturnDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_VendorReturnDetail, VendorReturnDetailModel>("VendorReturnID", "LineSequenceNumber", false);
        }

        protected override GenericDAC<VendorReturn, VendorReturnModel> GetMasterDAC()
        {
            return new GenericDAC<VendorReturn, VendorReturnModel>("ID", false, "Date DESC");
        }

        protected override GenericDAC<v_VendorReturn, VendorReturnModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_VendorReturn, VendorReturnModel>("ID", false, "Date DESC");
        }

        public override void Create(VendorReturnModel header, List<VendorReturnDetailModel> details)
        {
            header.Code = GetVendorReturnCode(header);
            header.IsPayable = false;
            header.IsDeliverable = true;
            header.StatusDescription = "Pending Approval";
            //CalculatePODiscount(header, details);
            base.Create(header, details);
            //OnCreatedUpdated(header.ID, "Create");
        }

        public void UpdateDetails(long poID, List<VendorReturnDetailModel> poDetails)
        {
            var dac = new ABCAPOSDAC();
            var VendorReturn = RetrieveByID(poID);

            using (TransactionScope trans = new TransactionScope())
            {
                //base.Update(VendorReturn, poDetails);
                GetDetailDAC().DeleteByParentID(poID);
                dac.CreateVendorReturnDetails(poID, poDetails);
                //GetDetailDAC().Create(poID, poDetails);

                trans.Complete();
            }
        }

        public void UpdateStatus(long VendorReturnID)
        {
            var order = RetrieveByID(VendorReturnID);
            string deliveryStatus;
            string billStatus;
            string paymentStatus;

            
            var pdList = new VendorReturnDeliveryBFC().RetrieveByVendorReturnID(order.ID);
            var qtyReceived = pdList.Sum(p => p.Quantity);
            var poList = new VendorReturnBFC().RetrieveDetails(VendorReturnID);
            var qtyBilled = poList.Sum(p => p.CreatedCreditQuantity);
            // TODO: VendorReturnCreditBFC
            var pBillList = new PurchaseBillBFC().RetrieveByPOID(order.ID);
            var amountBilled = pBillList.Sum(p => p.Amount + p.TaxAmount);
            var amountPaid = pBillList.Sum(p => p.CreatedPaymentAmount);

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

            if (qtyReceived == 0)
            {
                order.IsDeliverable = true;
                deliveryStatus = "Pending Receipt";
            }
            else if (qtyReceived == order.Quantity)
            {
                order.IsDeliverable = false;
                deliveryStatus = "";
            }
            else
            {
                order.IsDeliverable = true;
                deliveryStatus = "Partially Received";
            }

            if (qtyBilled == order.Quantity)
            {
                billStatus = "Fully Billed";
            }
            else if (qtyBilled >= qtyReceived)
            {
                billStatus = "";
            }
            else
            {
                billStatus = "Pending Billing";
            }


            if (amountBilled == amountPaid)
            {
                order.IsPayable = false;
                if (qtyBilled == order.Quantity)
                {
                    paymentStatus = "Paid in Full";
                    billStatus = "";
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
            if (deliveryStatus.Length > 0 && billStatus.Length > 0)
                combinator1 = " / ";
            var combinator2 = "";
            if (billStatus.Length > 0 && paymentStatus.Length > 0)
                combinator2 = " / ";
            else if (billStatus.Length == 0 
                && (deliveryStatus.Length > 0 && paymentStatus.Length > 0))
                combinator2 = " / ";

            order.StatusDescription = deliveryStatus
                + combinator1 + billStatus
                + combinator2 + paymentStatus;

            Update(order);
        }

        public void CopyTransaction(VendorReturnModel header, long vendorReturnID)
        {
            var vendorReturn = RetrieveByID(vendorReturnID);
            var vendorReturnDetails = RetrieveDetails(vendorReturnID);

            ObjectHelper.CopyProperties(vendorReturn, header);

            header.Status = (int)MPL.DocumentStatus.New;

            var details = new List<VendorReturnDetailModel>();

            foreach (var vendorReturnDetail in vendorReturnDetails)
            {
                var detail = new VendorReturnDetailModel();

                ObjectHelper.CopyProperties(vendorReturnDetail, detail);

                details.Add(detail);
            }
            header.Details = details;
        }

        public void Validate(VendorReturnModel obj, List<VendorReturnDetailModel> details)
        {
            decimal POTotal = 0;

            foreach (var detail in details)
            {
                if (detail.ProductID == 0)
                    throw new Exception("Product not chosen");

                if (detail.Quantity == 0)
                    throw new Exception("Qty Product cannot be zero");

                var total = Convert.ToDecimal(detail.Quantity) * detail.AssetPrice;
                POTotal = Convert.ToDecimal(total);

                //if (POTotal == 0)
                //    throw new Exception("Total must be higher than zero");
            }
        }

        public void OnApproved(long VendorReturnID, string userName)
        {
            var po = RetrieveByID(VendorReturnID);
            var poDetails = RetrieveDetails(VendorReturnID);
        }

        public void Approve(long VendorReturnID, string userName)
        {
            var VendorReturn = RetrieveByID(VendorReturnID);

            VendorReturn.Status = (int)MPL.DocumentStatus.Approved;
            VendorReturn.ApprovedBy = userName;
            VendorReturn.ApprovedDate = DateTime.Now;
            using (TransactionScope trans = new TransactionScope())
            {
                Update(VendorReturn);
                OnApproved(VendorReturnID, userName);
                trans.Complete();
            }
        }

        public void Void(long VendorReturnID, string voidRemarks, string userName)
        {
            var VendorReturn = RetrieveByID(VendorReturnID);
            var oldStatus = VendorReturn.Status;
            VendorReturn.Status = (int)MPL.DocumentStatus.Void;
            VendorReturn.VoidRemarks = voidRemarks;
            VendorReturn.ApprovedDate = DateTime.Now;
            VendorReturn.ApprovedBy = userName;
            Update(VendorReturn);
            UpdateStatus(VendorReturnID);
        }


        public int RetrieveUncreatedVendorReturnDeliveryCount(List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveUncreatedVendorReturnDeliveryCount(selectFilters);
        }

        public List<VendorReturnModel> RetrieveUncreatedVendorReturnDelivery(int startIndex, int? amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            if (amount == null)
                amount = SystemConstants.ItemPerPage;

            return new ABCAPOSDAC().RetrieveUncreatedVendorReturnDelivery(startIndex, (int)amount, sortParameter, selectFilters);
        }

        public int RetrieveUncreatedBillCreditCount(List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveUncreatedBillCreditReturnCount(selectFilters);
        }

        public List<VendorReturnModel> RetrieveUncreatedBillCredit(int startIndex, int? amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            if (amount == null)
                amount = SystemConstants.ItemPerPage;

            return new ABCAPOSDAC().RetrieveUncreatedBillCreditReturn(startIndex, (int)amount, sortParameter, selectFilters);
        }

        public ABCAPOSReportEDSC.DeliveryOrderDTRow RetrievePrintOut(long vendorReturnID)
        {
            return new ABCAPOSReportDAC().RetrieveVendorReturnPrintOut(vendorReturnID);
        }

        public ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable RetrieveDetailPrintOut(long vendorReturnID)
        {
            return new ABCAPOSReportDAC().RetrieveVendorReturnDetailPrintOut(vendorReturnID);
        }


        //public int RetrieveUncreatedVendorReturnCreditCount(List<SelectFilter> selectFilters)
        //{
        //    return new ABCAPOSDAC().RetrieveUncreatedPurchaseBillOrderCount(selectFilters);
        //}

        //public List<VendorReturnModel> RetrieveUncreatedPurchaseBill(int startIndex, int? amount, string sortParameter, List<SelectFilter> selectFilters)
        //{
        //    if (amount == null)
        //        amount = SystemConstants.ItemPerPage;

        //    return new ABCAPOSDAC().RetrieveUncreatedPurchaseBillOrder(startIndex, (int)amount, sortParameter, selectFilters);
        //}

        //public List<VendorReturnModel> RetrieveAutoComplete(string key)
        //{
        //    return new ABCAPOSDAC().RetrieveVendorReturnAutoComplete(key);
        //}

        //public VendorReturnModel RetrieveByCode(string purchaseCode)
        //{
        //    return new ABCAPOSDAC().RetrieveVendorReturnByCode(purchaseCode);
        //}

        //#region Notification

        //public int RetrieveUnapprovedVendorReturnCount()
        //{
        //    return new ABCAPOSDAC().RetrieveUnapprovedVendorReturnCount();
        //}

        //public int RetrieveUnpaidVendorReturnCount()
        //{
        //    return new ABCAPOSDAC().RetrieveUnpaidPOCount();
        //}

        //public string RetrieveThisMonthVendorReturnCount()
        //{
        //    return new ABCAPOSDAC().RetrieveThisMonthVendorReturnCount();
        //}

        //public int RetrieveVoidVendorReturnCount()
        //{
        //    return new ABCAPOSDAC().RetrieveVoidVendorReturnCount();
        //}

        //#endregion

        public void PreFillWithPurchaseOrderData(VendorReturnModel header, long p)
        {
            var poBFC = new PurchaseOrderBFC();
            var source = poBFC.RetrieveByID(p);
            if (source == null)
                return;

            header.Code = SystemConstants.autoGenerated; //GetVendorReturnCode(header);
            header.Date = DateTime.Now;
            header.SupplierID = source.SupplierID;
            header.VendorCode = source.VendorCode;
            header.VendorName = source.VendorName;
            header.SubTotal = source.SubTotal;
            header.GrandTotal = source.GrandTotal;
            header.CurrencyID = source.CurrencyID;
            header.CurrencyName = source.CurrencyName;
            header.ExchangeRate = source.ExchangeRate;
            header.PurchaseOrderID = p;
            header.PurchaseOrderCode = source.Code;
            header.TaxValue = source.TaxValue;
            header.TaxNumber = source.TaxNumber;
            header.Title = source.Title;

            header.DepartmentID = source.DepartmentID;
            header.DepartmentName = source.DepartmentName;
            header.WarehouseID = source.WarehouseID;
            header.WarehouseName = source.WarehouseName;
            header.POSupplierNo = source.POSupplierNo;

            var sourceDetails = poBFC.RetrieveDetails(p);
            var returnDetails = new List<VendorReturnDetailModel>();

            foreach (var sourceDetail in sourceDetails)
            {
                var detail = new VendorReturnDetailModel();
                detail.ProductID = sourceDetail.ProductID;
                detail.ProductCode = sourceDetail.ProductCode;
                detail.ProductName = sourceDetail.ProductName;
                detail.Quantity = (double) sourceDetail.Quantity;
                detail.ConversionID = sourceDetail.ConversionID;
                detail.ConversionName = sourceDetail.ConversionName;
                detail.TaxType = sourceDetail.TaxType;
                detail.TaxRate = sourceDetail.TaxRate;

                detail.AssetPrice = (decimal) sourceDetail.AssetPrice;

                returnDetails.Add(detail);
            }

            header.Details = returnDetails;
            OnCreateVendorReturn(header, header.Details);
        }

        private void OnCreateVendorReturn(VendorReturnModel header, List<VendorReturnDetailModel> details)
        {
            decimal amount = details.Sum(p => p.TotalAmount);
            decimal taxAmount = details.Sum(p => p.TotalPPN);

            header.GrandTotal = amount + taxAmount;
            header.SubTotal = amount;
            header.TaxValue = taxAmount;
        }
    }
    
}
