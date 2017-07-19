using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using MPL.Business;
using ABCAPOS.DA;
using System.Transactions;
using ABCAPOS.Util;
using ABCAPOS.ReportEDS;

namespace ABCAPOS.BF
{
    public class VendorReturnDeliveryBFC : MasterDetailBFC<VendorReturnDelivery, v_VendorReturnDelivery, VendorReturnDeliveryDetail, v_VendorReturnDeliveryDetail, VendorReturnDeliveryModel, VendorReturnDeliveryDetailModel>
    {
        private void decreaseInventoryQty(VendorReturnDeliveryModel deliveryOrder, bool isUndo)
        {
            if (deliveryOrder != null)
            {
                var salesOrder = new VendorReturnBFC().RetrieveByID(deliveryOrder.VendorReturnID);
                var details = RetrieveDetails(deliveryOrder.ID);

                foreach (var detail in details)
                {
                    var qtyCustomUnit = detail.Quantity;
                    var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                    var qtyBaseUnit = detail.Quantity * unitRate;
                    if (isUndo)
                    {
                        qtyBaseUnit = -qtyBaseUnit;
                    }

                    var itemLocation = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductID, deliveryOrder.WarehouseID);
                    if (itemLocation != null)
                    {
                        itemLocation.QtyOnHand -= qtyBaseUnit;
                        new ItemLocationBFC().Update(itemLocation);
                    }
                    else
                    {
                        new ItemLocationBFC().Create(detail.ProductID,
                          deliveryOrder.WarehouseID,
                          -qtyBaseUnit,
                          -qtyBaseUnit);
                    }

                   

                    //finish insert table log stock

                    if (detail.BinID != 0)
                    {
                        var binProduct = new BinProductWarehouseBFC().Retrieve(detail.BinID, detail.ProductID);
                        if (binProduct != null)
                        {
                            binProduct.Quantity -= qtyBaseUnit;
                            new BinProductWarehouseBFC().Update(binProduct);
                        }
                    }

                   
                }
            }
        }

        private void decreaseInventoryAvailableQty(VendorReturnDeliveryModel header, bool isUndo)
        {
            if (header != null)
            {
                var vendorRetur = new VendorReturnBFC().RetrieveByID(header.VendorReturnID);
                var details = RetrieveDetails(header.ID);
                foreach (var detail in details)
                {
                    var qtyCustomUnit = detail.Quantity;
                    var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                    var qtyBaseUnit = detail.Quantity * unitRate;
                    if (isUndo)
                    {
                        qtyBaseUnit = -qtyBaseUnit;
                    }
                    var itemLocation = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductID, header.WarehouseID);
                    if (itemLocation != null)
                    {
                        itemLocation.QtyAvailable -= qtyBaseUnit;
                        new ItemLocationBFC().Update(itemLocation);
                    }
                    else
                    {
                        new ItemLocationBFC().Create(detail.ProductID,
                          header.WarehouseID,
                          0,
                          -qtyBaseUnit);
                    }
                }
            }
        }

        #region PostingTableLog
        public void CreateLog(VendorReturnDeliveryModel header)
        {
            try
            {
                if (header != null)
                {
                    var details = base.RetrieveDetails(header.ID);
                    if (details != null)
                    {
                        var LogHeader = new LogModel();
                        var LogDetails = new List<LogDetailModel>();

                        LogHeader.WarehouseID = header.WarehouseID;
                        LogHeader.Date = header.Date;
                        LogHeader.DocType = (int)DocType.VendorReturnDelivery;

                        foreach (var detail in details)
                        {
                            var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                            var baseQuantity = detail.Quantity * unitRate;

                            double QtyRemaining = baseQuantity;
                            do
                            {
                                var LogDetail = new LogDetailModel();
                                var container = new ContainerBFC().RetreiveByProductIDWarehouseID(detail.ProductID, header.WarehouseID);
                                if (container != null)
                                {
                                    var containerHeader = new ContainerModel();
                                    containerHeader.ID = container.ID;
                                    containerHeader.ProductID = container.ProductID;
                                    containerHeader.WarehouseID = container.WarehouseID;
                                    double qty = (QtyRemaining > container.Qty) ? container.Qty : QtyRemaining;
                                    QtyRemaining = QtyRemaining - qty;
                                    containerHeader.Qty = container.Qty - qty;
                                    containerHeader.Price = container.Price;
                                    new ContainerBFC().Update(containerHeader);

                                    LogDetail.ContainerID = container.ID;
                                    LogDetail.ProductID = detail.ProductID;
                                    LogDetail.MovingOutQty = qty;
                                    //LogDetail.MovingOutValue = container.Price;
                                    LogDetails.Add(LogDetail);
                                }
                                else
                                {
                                    var containerHeader = new ContainerModel();
                                    containerHeader.ProductID = detail.ProductID;
                                    containerHeader.WarehouseID = header.WarehouseID;
                                    containerHeader.Qty -= QtyRemaining;
                                    containerHeader.Price = 0;
                                    new ContainerBFC().Create(containerHeader);

                                    LogDetail.ContainerID = containerHeader.ID;
                                    LogDetail.ProductID = detail.ProductID;
                                    LogDetail.MovingOutQty = QtyRemaining;
                                    //LogDetail.MovingOutValue = 0;
                                    QtyRemaining = 0;
                                    LogDetails.Add(LogDetail);
                                }
                            } while (QtyRemaining > 0);
                        }
                        new LogBFC().Create(LogHeader, LogDetails);
                        header.LogID = LogHeader.ID;
                        base.Update(header);
                    }

                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        private void UndoContainerStock(VendorReturnDeliveryModel header)
        {
            try
            {
                var Log = new LogBFC().RetrieveByID(header.LogID);
                if (Log != null)
                {
                    var details = base.RetrieveDetails(header.ID);
                    if (details != null)
                    {
                        foreach (var detail in details)
                        {
                            var container = new ContainerBFC().RetreiveByLogIDproductIDdocType(header.LogID, detail.ProductID, (int)DocType.VendorReturnDelivery);
                            if (container != null)
                            {
                                var containerDetail = new ContainerBFC().RetreiveByContainerIDProductIDWarehouseID(container.ContainerID, detail.ProductID, header.WarehouseID);
                                if (containerDetail != null)
                                {
                                    var LogDetail = new LogBFC().RetreiveByLogIDContainerIDProductIDWarehouseID(header.LogID, container.ContainerID, detail.ProductID, header.WarehouseID);
                                    if (LogDetail != null)
                                    {
                                        var ContainerHeader = new ContainerModel();
                                        ContainerHeader.ID = container.ContainerID;
                                        ContainerHeader.ProductID = detail.ProductID;
                                        ContainerHeader.WarehouseID = header.WarehouseID;
                                        ContainerHeader.Qty = containerDetail.Qty + LogDetail.MovingOutQty;
                                        ContainerHeader.Price = containerDetail.Price;
                                        new ContainerBFC().Update(ContainerHeader);

                                        new ABCAPOSDAC().DeleteLogByLogIDContainerIDProductID(header.LogID, ContainerHeader.ID, detail.ProductID);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void UpdateLog(VendorReturnDeliveryModel header, List<VendorReturnDeliveryDetailModel> details)
        {
            try
            {
                var LogHeader = new LogModel();
                var Log = new LogBFC().RetrieveByID(header.LogID);
                if (Log != null)
                {
                    //var details = base.RetrieveDetails(header.ID);
                    LogHeader.Date = header.Date;
                    LogHeader.WarehouseID = header.WarehouseID;
                    LogHeader.DocType = (int)DocType.VendorReturnDelivery;

                    new LogBFC().Update(LogHeader);
                    var itemNo = 1;
                    foreach (var detail in details)
                    {
                        var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                        var baseQuantity = detail.Quantity * unitRate;

                        double QtyRemaining = baseQuantity;
                        do
                        {
                            var LogDetail = new LogDetailModel();
                            var container = new ContainerBFC().RetreiveByProductIDWarehouseID(detail.ProductID, header.WarehouseID);
                            if (container != null)
                            {
                                var containerHeader = new ContainerModel();
                                containerHeader.ID = container.ID;
                                containerHeader.ProductID = container.ProductID;
                                containerHeader.WarehouseID = container.WarehouseID;
                                double qty = (QtyRemaining > container.Qty) ? container.Qty : QtyRemaining;
                                containerHeader.Qty = container.Qty - qty;
                                containerHeader.Price = container.Price;
                                new ContainerBFC().Update(containerHeader);


                                LogDetail.ContainerID = container.ID;
                                LogDetail.LogID = header.LogID;
                                LogDetail.ItemNo = itemNo++;
                                LogDetail.ProductID = detail.ProductID;
                                LogDetail.MovingOutQty = qty;
                                //LogDetail.MovingOutValue = container.Price;
                                QtyRemaining = QtyRemaining - qty;

                                new ABCAPOSDAC().CreateLog(LogDetail);
                            }
                            else
                            {
                                var containerHeader = new ContainerModel();
                                containerHeader.ProductID = detail.ProductID;
                                containerHeader.WarehouseID = header.WarehouseID;
                                containerHeader.Qty -= QtyRemaining;
                                containerHeader.Price = 0;
                                new ContainerBFC().Create(containerHeader);

                                LogDetail.ContainerID = containerHeader.ID;
                                LogDetail.LogID = header.LogID;
                                LogDetail.ItemNo = itemNo++;
                                LogDetail.ProductID = detail.ProductID;
                                LogDetail.MovingOutQty = QtyRemaining;
                                //LogDetail.MovingOutValue = 0;
                                QtyRemaining = 0;

                                new ABCAPOSDAC().CreateLog(LogDetail);
                            }
                        } while (QtyRemaining > 0);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void VoidLog(VendorReturnDeliveryModel header)
        {
            try
            {
                var LogHeader = new LogModel();
                var Log = new LogBFC().RetrieveByID(header.LogID);
                if (Log != null)
                {
                    var details = base.RetrieveDetails(header.ID);
                    var itemNo = 1;
                    foreach (var detail in details)
                    {
                        var LogDetails = new LogDetailModel();

                        var container = new ContainerBFC().RetreiveByLogIDproductIDdocType(header.LogID, detail.ProductID, (int)DocType.VendorReturnDelivery);
                        if (container != null)
                        {
                            var containerDetail = new ContainerBFC().RetreiveByContainerIDProductIDWarehouseID(container.ContainerID, detail.ProductID, header.WarehouseID);
                            if (containerDetail != null)
                            {
                                var LogDetail = new LogBFC().RetreiveByLogIDContainerIDProductIDWarehouseID(header.LogID, container.ContainerID, detail.ProductID, header.WarehouseID);
                                if (LogDetail != null)
                                {
                                    var ContainerHeader = new ContainerModel();
                                    ContainerHeader.ID = container.ContainerID;
                                    ContainerHeader.ProductID = detail.ProductID;
                                    ContainerHeader.WarehouseID = header.WarehouseID;
                                    ContainerHeader.Qty = containerDetail.Qty + LogDetail.MovingOutQty;
                                    ContainerHeader.Price = containerDetail.Price;
                                    new ContainerBFC().Update(ContainerHeader);

                                    new ABCAPOSDAC().DeleteLogByLogIDContainerIDProductID(header.LogID, ContainerHeader.ID, detail.ProductID);
                                    LogDetails.ContainerID = ContainerHeader.ID;
                                    LogDetails.LogID = header.LogID;
                                    LogDetails.ItemNo = itemNo++;
                                    LogDetails.ProductID = detail.ProductID;
                                    LogDetails.MovingOutQty = 0;
                                    //LogDetails.MovingOutValue = 0;

                                    new ABCAPOSDAC().CreateLog(LogDetails);
                                }
                            }
                        }
                    }

                    LogHeader.ID = Log.ID;
                    LogHeader.Date = header.Date;
                    LogHeader.WarehouseID = header.WarehouseID;
                    LogHeader.DocType = (int)DocType.Void;
                    new LogBFC().Update(LogHeader);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #endregion

        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetVendorReturnDeliveryCode(VendorReturnDeliveryModel header)
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var VRDPrefix = "";

            if (prefixSetting != null)
                VRDPrefix = prefixSetting.VendorReturnDeliveryPrefix;

            var warehouse = new WarehouseBFC().RetrieveByID(header.WarehouseID);
            var year = DateTime.Now.Year.ToString().Substring(2, 2);

            var prefix = VRDPrefix + year + "-" + warehouse.Code + "-";
            var code = new ABCAPOSDAC().RetrieveVendorReturnDeliveryMaxCode(prefix, 7);

            return code;
        }

        protected override GenericDetailDAC<VendorReturnDeliveryDetail, VendorReturnDeliveryDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<VendorReturnDeliveryDetail, VendorReturnDeliveryDetailModel>("VendorReturnDeliveryID", "ItemNo", false);
        }

        protected override GenericDetailDAC<v_VendorReturnDeliveryDetail, VendorReturnDeliveryDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_VendorReturnDeliveryDetail, VendorReturnDeliveryDetailModel>("VendorReturnDeliveryID", "ItemNo", false);
        }

        protected override GenericDAC<VendorReturnDelivery, VendorReturnDeliveryModel> GetMasterDAC()
        {
            return new GenericDAC<VendorReturnDelivery, VendorReturnDeliveryModel>("ID", false, "Date DESC");
        }

        protected override GenericDAC<v_VendorReturnDelivery, VendorReturnDeliveryModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_VendorReturnDelivery, VendorReturnDeliveryModel>("ID", false, "Date DESC");
        }

        public override void Create(VendorReturnDeliveryModel header, List<VendorReturnDeliveryDetailModel> details)
        {
            header.Code = GetVendorReturnDeliveryCode(header);

            base.Create(header, details);

            OnUpdated(header.ID, header.CreatedBy);

            new VendorReturnBFC().UpdateStatus(header.VendorReturnID);
        }

        public override void Update(VendorReturnDeliveryModel header, List<VendorReturnDeliveryDetailModel> details)
        {
            base.Update(header, details);

            OnUpdated(header.ID, header.CreatedBy);

            new VendorReturnBFC().UpdateStatus(header.VendorReturnID);
        }

        // Approve really means "Mark Packed"
        public void Approve(long deliveryOrderID, string userName)
        {
            var deliveryOrder = RetrieveByID(deliveryOrderID);

            deliveryOrder.Status = (int)DeliveryOrderStatus.Packed;
            deliveryOrder.ApprovedBy = userName;
            deliveryOrder.ApprovedDate = DateTime.Now;

            using (TransactionScope trans = new TransactionScope())
            {
                Update(deliveryOrder);

                new VendorReturnBFC().UpdateStatus(deliveryOrder.VendorReturnID);
                trans.Complete();
            }
        }

        public void Void(long deliveryOrderID, string voidRemarks, string userName)
        {
            var deliveryOrder = RetrieveByID(deliveryOrderID);
            var oldStatus = deliveryOrder.Status;

            deliveryOrder.Status = (int)MPL.DocumentStatus.Void;
            deliveryOrder.VoidRemarks = voidRemarks;
            deliveryOrder.ApprovedBy = "";
            deliveryOrder.ApprovedDate = SystemConstants.UnsetDateTime;

            using (TransactionScope trans = new TransactionScope())
            {
                //this.VoidLog(deliveryOrder);
                Update(deliveryOrder);
                OnUpdated(deliveryOrderID, userName);

                //if (oldStatus == (int)MPL.DocumentStatus.Approved)
                //    OnVoid(deliveryOrderID, voidRemarks, userName);

                trans.Complete();
            }
        }

        public void OnUpdated(long deliveryOrderID, string userName)
        {
            var deliveryOrder = RetrieveByID(deliveryOrderID);

            if (deliveryOrder == null)
            {
                return;
            }

            var so = new VendorReturnBFC().RetrieveByID(deliveryOrder.VendorReturnID);

            if (so == null)
            {
                return;
            }

            var deliveryOrderDetails = RetrieveDetailsByVendorReturnID(so.ID);
            var soDetails = new VendorReturnBFC().RetrieveDetails(so.ID);

            var fulfilledCount = 0;

            foreach (var soDetail in soDetails)
            {
                //var query = from i in deliveryOrderDetails
                //            where i.ProductID == soDetail.ProductID && i.VendorReturnItemNo == soDetail.ItemNo
                //            select i.Quantity;

                //soDetail.CreatedDeliveryQuantity = query.Sum();

                if (soDetail.Quantity == soDetail.CreatedDeliveryQuantity)
                    fulfilledCount += 1;
            }

            if (deliveryOrderDetails.Count == 0)
                so.HasDelivery = false;
            else
                so.HasDelivery = true;

            if (fulfilledCount == soDetails.Count)
                so.IsDeliveryFulfilled = true;
            else
                so.IsDeliveryFulfilled = false;

            new VendorReturnBFC().Update(so);
            new VendorReturnBFC().UpdateDetails(so.ID, soDetails);

        }

        public void ImplementStatus(long deliveryID, string userName, DeliveryOrderStatus previousStatus, DeliveryOrderStatus newStatus)
        {
            var deliveryOrder = RetrieveByID(deliveryID);
            deliveryOrder.Status = (int)newStatus;

            //using (TransactionScope trans = new TransactionScope())
            //{
                switch (previousStatus)
                {
                    case DeliveryOrderStatus.Void:
                        if (newStatus == DeliveryOrderStatus.New)
                        {
                            decreaseInventoryAvailableQty(deliveryOrder, false);
                        }
                        else if (newStatus == DeliveryOrderStatus.Shipped)
                        {
                            decreaseInventoryAvailableQty(deliveryOrder, false);
                            decreaseInventoryQty(deliveryOrder, false);
                            this.CreateLog(deliveryOrder);
                        }
                        break;
                    case DeliveryOrderStatus.New:
                        if (newStatus == DeliveryOrderStatus.Shipped)
                        {
                            decreaseInventoryQty(deliveryOrder, false);
                            this.CreateLog(deliveryOrder);
                        }
                        else if (newStatus == DeliveryOrderStatus.Void)
                        {
                            decreaseInventoryAvailableQty(deliveryOrder, true);
                        }
                        break;
                    //case DeliveryOrderStatus.Packed:
                    //    if (newStatus == DeliveryOrderStatus.Shipped)
                    //    {
                    //        decreaseInventoryQty(deliveryOrder, false);
                    //    }
                    //    break;
                    case DeliveryOrderStatus.Shipped:
                        if (newStatus == DeliveryOrderStatus.New)
                        {
                            this.decreaseInventoryQty(deliveryOrder, true);
                            this.UndoContainerStock(deliveryOrder);
                        }
                        else if (newStatus == DeliveryOrderStatus.Void)
                        {
                            this.decreaseInventoryQty(deliveryOrder, true);
                            decreaseInventoryAvailableQty(deliveryOrder, true);
                            this.VoidLog(deliveryOrder);
                        }
                        break;
                    default:
                        break;

                }
                Update(deliveryOrder);
                new VendorReturnBFC().UpdateStatus(deliveryOrder.VendorReturnID);
                //trans.Complete();
            //}
        }

        //public void OnVoid(long deliveryOrderID, string voidRemarks, string userName)
        //{

        //}

        public void Validate(VendorReturnDeliveryModel deliveryOrder, List<VendorReturnDeliveryDetailModel> deliveryOrderDetails)
        {
            var obj = RetrieveByID(deliveryOrder.ID);
            var soDetails = new VendorReturnBFC().RetrieveDetails(deliveryOrder.VendorReturnID);

            var extDetails = new List<VendorReturnDeliveryDetailModel>();

            if (obj != null)
                extDetails = RetrieveDetails(deliveryOrder.ID);

            foreach (var deliveryOrderDetail in deliveryOrderDetails)
            {
                if (!string.IsNullOrEmpty(deliveryOrderDetail.StrQuantity))
                    deliveryOrderDetail.Quantity = Convert.ToDouble(deliveryOrderDetail.StrQuantity);
                else
                    deliveryOrderDetail.Quantity = 0;

                var query = from i in soDetails
                            where i.ProductID == deliveryOrderDetail.ProductID && i.ItemNo == deliveryOrderDetail.VendorReturnItemNo
                            select i;

                var spkDetail = query.FirstOrDefault();
                var doQty = deliveryOrderDetail.Quantity;

                if (spkDetail.OutstandingQuantity < doQty)
                    throw new Exception("Jumlah " + spkDetail.ProductCode + " yang dimasukkan melebihi jumlah yg tersisa");
            }
        }

        public void CreateByVendorReturn(VendorReturnDeliveryModel deliveryOrder, long salesOrderID)
        {
            var salesOrder = new VendorReturnBFC().RetrieveByID(salesOrderID);

            if (salesOrder != null)
            {
                deliveryOrder.Code = SystemConstants.autoGenerated;//GetVendorReturnDeliveryCode();

                deliveryOrder.VendorReturnID = salesOrderID;
                deliveryOrder.VendorReturnCode = salesOrder.Code;
                deliveryOrder.POSupplierNo = salesOrder.POSupplierNo;
                deliveryOrder.POSupplierDate = salesOrder.POSupplierDate;
                deliveryOrder.VendorCode = salesOrder.VendorCode;
                deliveryOrder.VendorName = salesOrder.VendorName;
                deliveryOrder.WarehouseID = salesOrder.WarehouseID;

                var warehouse = new WarehouseBFC().RetrieveByID(salesOrder.WarehouseID);
                deliveryOrder.WarehouseName = warehouse.Name;

                var soDetails = new VendorReturnBFC().RetrieveDetails(salesOrderID);
                var deliveryOrderDetails = new List<VendorReturnDeliveryDetailModel>();

                foreach (var soDetail in soDetails)
                {
                    if (soDetail.Quantity > soDetail.CreatedDeliveryQuantity)
                    {
                        var detail = new VendorReturnDeliveryDetailModel();

                        detail.VendorReturnItemNo = soDetail.ItemNo;
                        detail.Barcode = soDetail.Barcode;
                        detail.ProductID = soDetail.ProductID;
                        detail.ProductCode = soDetail.ProductCode;
                        detail.ProductName = soDetail.ProductName;
                        detail.ConversionName = soDetail.ConversionName;
                        detail.ConversionID = soDetail.ConversionID;
                        detail.QtySO = Convert.ToDouble(soDetail.Quantity);
                        detail.QtyDO = Convert.ToDouble(soDetail.CreatedDeliveryQuantity);
                        detail.Quantity = soDetail.OutstandingQuantity;
                        detail.StrQuantity = soDetail.OutstandingQuantity.ToString().Replace(",", "").Replace(".", ",");
                        detail.Remarks = soDetail.Remarks;

                        detail.UnitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                        if (detail.UnitRate == null)
                        {
                            detail.UnitRate = 1;
                        }

                        var itemLoc = new ItemLocationBFC().RetrieveByProductIDWarehouseID(soDetail.ProductID, salesOrder.WarehouseID);
                        if (itemLoc != null)
                        {
                            detail.StockQty = itemLoc.QtyOnHand / detail.UnitRate;
                            detail.StockAvailable = itemLoc.QtyAvailable / detail.UnitRate;
                        }
                        else
                        {
                            detail.StockQty = 0;
                            detail.StockAvailable = 0;
                        }
                           

                        var product = new ProductBFC().RetrieveByID(detail.ProductID);
                        if (product.UseBin)
                        {
                            detail.BinID = new BinBFC().RetrieveDefaultBinID(product.ID, salesOrder.WarehouseID);
                        }

                        deliveryOrderDetails.Add(detail);
                    }
                }

                deliveryOrder.Details = deliveryOrderDetails;
            }

            //return deliveryOrder;
        }

        public List<VendorReturnDeliveryModel> RetrieveByVendorReturnID(long vrID)
        {
            return new ABCAPOSDAC().RetrieveVendorReturnDeliveryByVendorReturnID(vrID);
        }

        public List<VendorReturnDeliveryModel> RetrieveByVendorReturnID(long vrID, DeliveryOrderStatus deliveryStatus)
        {
            return new ABCAPOSDAC().RetrieveVendorReturnDeliveryByVendorReturnID(vrID, deliveryStatus);
        }

        public List<VendorReturnDeliveryDetailModel> RetrieveDetailsByVendorReturnID(long vrID)
        {
            return new ABCAPOSDAC().RetrieveVendorReturnDeliveryDetailByVendorReturnID(vrID);
        }

        #region Notification

        //public int RetrieveUnapprovedVendorReturnDeliveryCount(CustomerGroupModel customerGroup)
        //{
        //    return new ABCAPOSDAC().RetrieveUnapprovedVendorReturnDeliveryCount(customerGroup);
        //}

        //public int RetrieveVoidVendorReturnDeliveryCount(CustomerGroupModel customerGroup)
        //{
        //    return new ABCAPOSDAC().RetrieveVoidVendorReturnDeliveryCount(customerGroup);
        //}

        #endregion

        //#region Post to Accounting Result

        //public void PostAccounting(long returnDeliveryID)
        //{
        //    var returnDelivery = RetrieveByID(returnDeliveryID);

        //    new ABCAPOSDAC().DeleteAccountingResults(returnDeliveryID, AccountingResultDocumentType.MakeMultiPay);

        //    if (returnDelivery.Status != (int)MPL.DocumentStatus.Void)
        //        CreateAccountingResult(returnDeliveryID);
        //}

        //private void CreateAccountingResult(long returnDeliveryID)
        //{
        //    var returnDelivery = RetrieveByID(returnDeliveryID);
        //    var returnDeliveryDetails = RetrieveDetails(returnDeliveryID);

        //    var accountingResultList = new List<AccountingResultModel>();

        //    decimal rawMaterialTotalAmount = 0;
        //    decimal extraMaterialTotalAmount = 0;
        //    decimal otherMaterialTotalAmount = 0;
        //    decimal taxAmount = 0;

        //    var vendorReturn = new VendorReturnBFC().RetrieveByID(returnDelivery.VendorReturnID);
        //    var returnDetails = new VendorReturnBFC().RetrieveDetails(returnDelivery.VendorReturnID);

        //    foreach (var returnDeliveryDetail in returnDeliveryDetails)
        //    {
        //        var returnDetail = (from i in returnDetails
        //                            where i.ProductID == returnDeliveryDetail.ProductID
        //                            select i).FirstOrDefault();

        //        if (returnDetail != null)
        //        {
        //            var product = new ProductBFC().RetrieveByID(returnDeliveryDetail.ProductID);

        //            if (product.ItemTypeID == (int)ItemTypeProduct.RawMaterial)
        //                rawMaterialTotalAmount += Convert.ToDecimal(returnDeliveryDetail.Quantity) * (returnDetail.TotalAmount / Convert.ToDecimal(returnDetail.Quantity));
        //            else if (product.ItemTypeID == (int)ItemTypeProduct.Supporting)
        //                extraMaterialTotalAmount += Convert.ToDecimal(returnDeliveryDetail.Quantity) * (returnDetail.TotalAmount / Convert.ToDecimal(returnDetail.Quantity));
        //            else if (product.ItemTypeID == (int)ItemTypeProduct.NonInventory)
        //                otherMaterialTotalAmount += Convert.ToDecimal(returnDeliveryDetail.Quantity) * (returnDetail.TotalAmount / Convert.ToDecimal(returnDetail.Quantity));

        //            taxAmount += returnDetail.TotalPPN;
        //        }
        //    }

        //    decimal totalAmount = 0;

        //    rawMaterialTotalAmount = rawMaterialTotalAmount * vendorReturn.ExchangeRate;
        //    extraMaterialTotalAmount = extraMaterialTotalAmount * vendorReturn.ExchangeRate;
        //    otherMaterialTotalAmount = otherMaterialTotalAmount * vendorReturn.ExchangeRate;
        //    taxAmount = taxAmount * vendorReturn.ExchangeRate;

        //    totalAmount = rawMaterialTotalAmount + extraMaterialTotalAmount + otherMaterialTotalAmount + taxAmount;

        //    accountingResultList = AddToAccountingResultList(accountingResultList, returnDelivery, (long)PostingAccount.HutangDagang, AccountingResultType.Debit, totalAmount, "Hutang dagang vendor return delivery " + returnDelivery.Code);

        //    accountingResultList = AddToAccountingResultList(accountingResultList, returnDelivery, (long)PostingAccount.PersediaanBahanBaku, AccountingResultType.Credit, rawMaterialTotalAmount, "Persediaan bahan baku vendor return delivery " + returnDelivery.Code);

        //    accountingResultList = AddToAccountingResultList(accountingResultList, returnDelivery, (long)PostingAccount.PersediaanBahanPembantu, AccountingResultType.Credit, extraMaterialTotalAmount, "Persediaan bahan pembantu vendor return delivery " + returnDelivery.Code);

        //    accountingResultList = AddToAccountingResultList(accountingResultList, returnDelivery, (long)PostingAccount.CadanganBiayaLainnya, AccountingResultType.Credit, otherMaterialTotalAmount, "Cadangan biaya lainnya vendor return delivery " + returnDelivery.Code);

        //    accountingResultList = AddToAccountingResultList(accountingResultList, returnDelivery, (long)PostingAccount.UangMukaPPN, AccountingResultType.Credit, taxAmount, "Pajak vendor return delivery " + returnDelivery.Code);

        //    new AccountingResultBFC().Posting(accountingResultList);
        //}

        //private List<AccountingResultModel> AddToAccountingResultList(List<AccountingResultModel> resultList, VendorReturnDeliveryModel obj, long accountID, AccountingResultType resultType, decimal amount, string remarks)
        //{
        //    if (amount > 0)
        //    {
        //        var account = new AccountBFC().RetrieveByID(accountID);
        //        var result = new AccountingResultModel();

        //        result.DocumentID = obj.ID;
        //        result.DocumentType = (int)AccountingResultDocumentType.VendorReturnDelivery;
        //        result.Type = (int)resultType;
        //        result.Date = obj.Date;
        //        result.AccountID = account.ID;
        //        result.DocumentNo = obj.Code;
        //        result.Amount = amount;

        //        if (resultType == AccountingResultType.Debit)
        //            result.DebitAmount = amount;
        //        else
        //            result.CreditAmount = amount;

        //        result.Remarks = remarks;

        //        resultList.Add(result);
        //    }

        //    return resultList;
        //}

        //#endregion
        //public ABCAPOSReportEDSC.VendorReturnDeliveryDTRow RetrievePrintOut(long deliveryOrderID)
        //{
        //    return new ABCAPOSReportDAC().RetrieveVendorReturnDeliveryPrintOut(deliveryOrderID);
        //}

        //public ABCAPOSReportEDSC.VendorReturnDeliveryDetailDTDataTable RetrieveDetailPrintOut(long deliveryOrderID)
        //{
        //    return new ABCAPOSReportDAC().RetrieveVendorReturnDeliveryDetailPrintOut(deliveryOrderID);
        //}
    }
}
