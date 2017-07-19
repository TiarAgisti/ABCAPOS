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
    public class TransferDeliveryBFC : MasterDetailBFC<TransferDelivery, v_TransferDelivery, TransferDeliveryDetail, v_TransferDeliveryDetail, TransferDeliveryModel, TransferDeliveryDetailModel>
    {
        private void decreaseInventoryQty(TransferDeliveryModel transferDelivery, bool isUndo)
        {
            if (transferDelivery != null)
            {
                var transferOrder = new TransferOrderBFC().RetrieveByID(transferDelivery.TransferOrderID);
                var details = RetrieveDetails(transferDelivery.ID);

                foreach (var detail in details)
                {
                    //var qtyCustomUnit = detail.Quantity;
                    var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                    var qtyBaseUnit = detail.Quantity * unitRate;
                    if (isUndo)
                    {
                        qtyBaseUnit = -qtyBaseUnit;
                    }
                    var itemLocation = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductID, transferDelivery.FromWarehouseID);
                    if (itemLocation != null)
                    {
                        itemLocation.QtyOnHand -= qtyBaseUnit;
                        new ItemLocationBFC().Update(itemLocation);
                    }
                    else
                    {
                        new ItemLocationBFC().Create(detail.ProductID,
                          transferDelivery.FromWarehouseID,
                          -qtyBaseUnit,
                          -qtyBaseUnit);
                    }

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

        #region PostingTableLog
        public void CreateLog(TransferDeliveryModel header)
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

                        LogHeader.WarehouseID = header.FromWarehouseID;
                        LogHeader.Date = header.Date;
                        LogHeader.DocType = (int)DocType.TransferDelivery;

                        foreach (var detail in details)
                        {
                            var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                            var baseQuantity = detail.Quantity * unitRate;

                            double QtyRemaining = baseQuantity;
                            do
                            {
                                var LogDetail = new LogDetailModel();
                                var container = new ContainerBFC().RetreiveByProductIDWarehouseID(detail.ProductID, header.FromWarehouseID);
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
                                    containerHeader.WarehouseID = header.FromWarehouseID;
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

        private void UndoContainerStock(TransferDeliveryModel header)
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
                            var container = new ContainerBFC().RetreiveByLogIDproductIDdocType(header.LogID, detail.ProductID, (int)DocType.TransferDelivery);
                            if (container != null)
                            {
                                var containerDetail = new ContainerBFC().RetreiveByContainerIDProductIDWarehouseID(container.ContainerID, detail.ProductID, header.FromWarehouseID);
                                if (containerDetail != null)
                                {
                                    var LogDetail = new LogBFC().RetreiveByLogIDContainerIDProductIDWarehouseID(header.LogID, container.ContainerID, detail.ProductID, header.FromWarehouseID);
                                    if (LogDetail != null)
                                    {
                                        var ContainerHeader = new ContainerModel();
                                        ContainerHeader.ID = container.ContainerID;
                                        ContainerHeader.ProductID = detail.ProductID;
                                        ContainerHeader.WarehouseID = header.FromWarehouseID;
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

        private void UpdateLog(TransferDeliveryModel header, List<TransferDeliveryDetailModel> details)
        {
            try
            {
                var LogHeader = new LogModel();
                var Log = new LogBFC().RetrieveByID(header.LogID);
                if (Log != null)
                {
                    //var details = base.RetrieveDetails(header.ID);
                    LogHeader.Date = header.Date;
                    LogHeader.WarehouseID = header.FromWarehouseID;
                    LogHeader.DocType = (int)DocType.TransferDelivery;
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
                            var container = new ContainerBFC().RetreiveByProductIDWarehouseID(detail.ProductID, header.FromWarehouseID);
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
                                containerHeader.WarehouseID = header.FromWarehouseID;
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

        private void VoidLog(TransferDeliveryModel header)
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

                        var container = new ContainerBFC().RetreiveByLogIDproductIDdocType(header.LogID, detail.ProductID, (int)DocType.TransferDelivery);
                        if (container != null)
                        {
                            var containerDetail = new ContainerBFC().RetreiveByContainerIDProductIDWarehouseID(container.ContainerID, detail.ProductID, header.FromWarehouseID);
                            if (containerDetail != null)
                            {
                                var LogDetail = new LogBFC().RetreiveByLogIDContainerIDProductIDWarehouseID(header.LogID, container.ContainerID, detail.ProductID, header.FromWarehouseID);
                                if (LogDetail != null)
                                {
                                    var ContainerHeader = new ContainerModel();
                                    ContainerHeader.ID = container.ContainerID;
                                    ContainerHeader.ProductID = detail.ProductID;
                                    ContainerHeader.WarehouseID = header.FromWarehouseID;
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
                    LogHeader.WarehouseID = header.FromWarehouseID;
                    LogHeader.DocType = (int)DocType.Void;
                    new LogBFC().Update(LogHeader);
                }
            }
            catch (Exception ex)
            {
                throw(ex);
            }
        }
        #endregion

        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetTransferDeliveryCode(TransferDeliveryModel header)
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var transferDeliveryPrefix = "";

            if (prefixSetting != null)
                transferDeliveryPrefix = prefixSetting.TransferDeliveryPrefix;

            var warehouse = new WarehouseBFC().RetrieveByID(header.FromWarehouseID);
            var year = DateTime.Now.Year.ToString().Substring(2, 2);

            var prefix = transferDeliveryPrefix + year + "-" + warehouse.Code + "-";

            var code = new ABCAPOSDAC().RetrieveTransferDeliveryMaxCode(prefix, 7);

            return code;
        }

        protected override GenericDetailDAC<TransferDeliveryDetail, TransferDeliveryDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<TransferDeliveryDetail, TransferDeliveryDetailModel>("TransferDeliveryID", "ItemNo", false);
        }

        protected override GenericDetailDAC<v_TransferDeliveryDetail, TransferDeliveryDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_TransferDeliveryDetail, TransferDeliveryDetailModel>("TransferDeliveryID", "ItemNo", false);
        }

        protected override GenericDAC<TransferDelivery, TransferDeliveryModel> GetMasterDAC()
        {
            return new GenericDAC<TransferDelivery, TransferDeliveryModel>("ID", false, "Date DESC");
        }

        protected override GenericDAC<v_TransferDelivery, TransferDeliveryModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_TransferDelivery, TransferDeliveryModel>("ID", false, "Date DESC");
        }

        public override void Create(TransferDeliveryModel header, List<TransferDeliveryDetailModel> details)
        {
            header.Code = GetTransferDeliveryCode(header);

            using (TransactionScope trans = new TransactionScope())
            {

                base.Create(header, details);
                //UpdateOrderQty(header.ID);
                new TransferOrderBFC().UpdateStatus(header.TransferOrderID, header.CreatedBy);

                trans.Complete();
            }
        }

        public override void Update(TransferDeliveryModel header, List<TransferDeliveryDetailModel> details)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                base.Update(header, details);


                new TransferOrderBFC().UpdateStatus(header.TransferOrderID, header.CreatedBy);

                //this.UndoContainerStock(header);

                trans.Complete();
            }
        }

        // this function consolidates MarkPicked, Packed, and Shipped
        public void ImplementStatus(long deliveryID, string userName, DeliveryOrderStatus previousStatus, DeliveryOrderStatus newStatus)
        {
            var transferDelivery = RetrieveByID(deliveryID);
            transferDelivery.Status = (int)newStatus;

            using (TransactionScope trans = new TransactionScope())
            {
                switch (previousStatus)
                {
                    case DeliveryOrderStatus.Void:
                        if (newStatus == DeliveryOrderStatus.Shipped)
                        {
                            decreaseInventoryQty(transferDelivery, false);
                            this.CreateLog(transferDelivery);
                        }
                        break;
                    case DeliveryOrderStatus.New:
                        if (newStatus == DeliveryOrderStatus.Shipped)
                        {
                            decreaseInventoryQty(transferDelivery, false);
                            this.CreateLog(transferDelivery);
                        }
                        break;
                    case DeliveryOrderStatus.Packed:
                        if (newStatus == DeliveryOrderStatus.Shipped)
                        {
                            decreaseInventoryQty(transferDelivery, false);
                            this.CreateLog(transferDelivery);
                        }
                        break;
                    case DeliveryOrderStatus.Shipped:
                        if (newStatus != DeliveryOrderStatus.Shipped)
                        {
                            decreaseInventoryQty(transferDelivery, true);
                            this.UndoContainerStock(transferDelivery);
                        }
                        else if (newStatus == DeliveryOrderStatus.Void)
                        {
                            this.decreaseInventoryQty(transferDelivery, true);
                            this.VoidLog(transferDelivery);
                        }
                        break;
                    default:
                        break;

                }
                Update(transferDelivery);
                new TransferOrderBFC().UpdateStatus(transferDelivery.TransferOrderID, userName);
                trans.Complete();
            }
        }

        public void Void(long transferDeliveryID, string voidRemarks, string userName)
        {
            var transferDelivery = RetrieveByID(transferDeliveryID);
            var oldStatus = transferDelivery.Status;

            transferDelivery.Status = (int)MPL.DocumentStatus.Void;
            transferDelivery.VoidRemarks = voidRemarks;
            transferDelivery.ApprovedBy = "";
            transferDelivery.ApprovedDate = SystemConstants.UnsetDateTime;

            using (TransactionScope trans = new TransactionScope())
            {
                Update(transferDelivery);
                
                //if (oldStatus == (int)MPL.DocumentStatus.Approved)
                OnVoid(transferDeliveryID, voidRemarks, userName);

                new TransferOrderBFC().UpdateStatus(transferDelivery.TransferOrderID, userName);

                trans.Complete();
            }
        }

        public void UpdateTDItemLocationBin(TransferDeliveryModel header, List<TransferDeliveryDetailModel> details)
        {
            var oldTD = RetrieveByID(header.ID);

            foreach (var detail in details)
            {
                var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                var baseSelisihQuantity = detail.SelisihQuantity * unitRate;
                var itemLocation = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductID, header.FromWarehouseID);
                if (itemLocation != null)
                {
                    if (oldTD.Status == (int)DeliveryOrderStatus.Shipped)
                    {
                        //itemLocation.QtyAvailable -= baseSelisihQuantity;
                        itemLocation.QtyOnHand -= baseSelisihQuantity;
                        new ItemLocationBFC().Update(itemLocation);
                    }
                    

                }
                if (detail.BinID != null || detail.BinID != 0)
                {
                    var binProduct = new BinProductWarehouseBFC().Retrieve(detail.BinID, detail.ProductID);
                    if (binProduct != null)
                    {
                        if (oldTD.Status == (int)DeliveryOrderStatus.Shipped)
                        {
                            binProduct.Quantity -= baseSelisihQuantity;
                            new BinProductWarehouseBFC().Update(binProduct);
                        }
                    }
                }
            }
        }

        public void voidFromOldQty(TransferDeliveryModel header, List<TransferDeliveryDetailModel> details)
        {
            var oldDetails = RetrieveDetails(header.ID);

            foreach (var detail in details)
            {
                detail.SelisihQuantity = -detail.Quantity;
            }

            //UpdateTDItemLocationBin(header, details);


        }

        public void OnVoid(long transferDeliveryID, string voidRemarks, string userName)
        {
            var transferDelivery = RetrieveByID(transferDeliveryID);
            var transferDeliveryDetails = RetrieveDetails(transferDeliveryID);

            voidFromOldQty(transferDelivery, transferDeliveryDetails);
        }

        public void Validate(TransferDeliveryModel transferDelivery, List<TransferDeliveryDetailModel> deliveryDetails)
        {
            var transferDetails = new TransferOrderBFC().RetrieveDetails(transferDelivery.TransferOrderID);

            foreach (var deliveryDetail in deliveryDetails)
            {
                //if (!string.IsNullOrEmpty(deliveryDetail.StringQty))
                //    deliveryDetail.Quantity = Convert.ToDouble(deliveryDetail.StringQty);
                //else
                //    deliveryDetail.Quantity = 0;

                var originalTransferDetail = (from i in transferDetails
                                              where i.ProductID == deliveryDetail.ProductID &&
                                                   i.ItemNo == deliveryDetail.TransferOrderItemNo
                                              select i).FirstOrDefault();
                if (deliveryDetail.Quantity < originalTransferDetail.Quantity)
                    throw new Exception("Transfer orders cannot be partially fulfilled / received");
                else if (deliveryDetail.Quantity > originalTransferDetail.Quantity)
                    throw new Exception("The amount spcified for " + deliveryDetail.ProductCode + " is higher than the amount in the original Transfer Order.");
            }
        }

        public void PreFillWithTransferOrderData(TransferDeliveryModel transferDelivery, long transferOrderID)
        {
            var transferOrder = new TransferOrderBFC().RetrieveByID(transferOrderID);

            if (transferOrder != null)
            {
                transferDelivery.TransferOrderID = transferOrderID;
                transferDelivery.Date = DateTime.Now;
                transferDelivery.PostingPeriod = transferOrder.PostingPeriod;
                transferDelivery.TransferOrderCode = transferOrder.Code;
                transferDelivery.FromWarehouseID = transferOrder.FromWarehouseID;
                
                transferDelivery.Code = GetTransferDeliveryCode(transferDelivery);

                var transferDetails = new TransferOrderBFC().RetrieveDetails(transferOrderID);
                var transferDeliveryDetails = new List<TransferDeliveryDetailModel>();

                foreach (var transferDetail in transferDetails)
                {
                    var qtyDelivered = transferDetail.QtyPacked + transferDetail.QtyPicked + transferDetail.QtyShipped;
                    if (transferDetail.Quantity > qtyDelivered)
                    {
                        var detail = new TransferDeliveryDetailModel();
                        detail.TransferOrderItemNo = transferDetail.ItemNo;
                        detail.ProductID = transferDetail.ProductID;
                        detail.ProductCode = transferDetail.ProductCode;
                        detail.ProductName = transferDetail.ProductName;
                        detail.QtyTO = transferDetail.Quantity;
                        detail.Quantity = transferDetail.Quantity - qtyDelivered;
                        detail.Price = transferDetail.TransferPrice;
                        //detail.StringQty = detail.Price.ToString().Replace(",", "").Replace(".", ",");
                        detail.ConversionName = transferDetail.ConversionName;
                        detail.ConversionID = transferDetail.ConversionID;

                        var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                        var itemLoc = new ItemLocationBFC().RetrieveByProductIDWarehouseID(transferDetail.ProductID, transferOrder.FromWarehouseID);
                        if (itemLoc != null)
                            detail.StockQty = itemLoc.QtyOnHand / unitRate;
                        else
                            detail.StockQty = 0;

                        var product = new ProductBFC().RetrieveByID(detail.ProductID);
                        if (product.UseBin)
                        {
                            detail.BinID = new BinBFC().RetrieveDefaultBinID(product.ID, transferDelivery.FromWarehouseID);
                        }

                        transferDeliveryDetails.Add(detail);
                    }
                }
                transferDelivery.Details = transferDeliveryDetails;
            }
        }

        public List<TransferDeliveryModel> RetrieveByTransferOrderID(long toID)
        {
            return new ABCAPOSDAC().RetrieveTransferDeliveryByTransferOrderID(toID);
        }

        public List<TransferDeliveryModel> RetrieveShippedByTransferOrderID(long toID)
        {
            return new ABCAPOSDAC().RetrieveTransferDeliveryByTransferOrderID(toID, DeliveryOrderStatus.Shipped);
        }

        public List<TransferDeliveryDetailModel> RetrieveDetailsByTransferOrderID(long spkID)
        {
            return new ABCAPOSDAC().RetrieveTransferDeliveryDetailByTransferOrderID(spkID);
        }

        public ABCAPOSReportEDSC.DeliveryOrderDTRow RetrievePrintOut(long deliveryOrderID)
        {
            return new ABCAPOSReportDAC().RetrieveTransferDeliveryPrintOut(deliveryOrderID);
        }

        public ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable RetrieveDetailPrintOut(long deliveryOrderID)
        {
            return new ABCAPOSReportDAC().RetrieveTransferDeliveryDetailPrintOut(deliveryOrderID);
        }

        #region Notification

        // TODO: unapproved Transfer DO Count
        //public int RetrieveUnapprovedDeliveryOrderCount(CustomerGroupModel customerGroup)
        //{
        //    return new ABCAPOSDAC().RetrieveUnapprovedDeliveryOrderCount(customerGroup);
        //}

        //public int RetrieveVoidDeliveryOrderCount(CustomerGroupModel customerGroup)
        //{
        //    return new ABCAPOSDAC().RetrieveVoidDeliveryOrderCount(customerGroup);
        //}

        #endregion

        //private void UpdateOrderQty(long transferDeliveryID)
        //{
        //    var delivery = RetrieveByID(transferDeliveryID);
        //    var order = new TransferOrderBFC().RetrieveByID(delivery.TransferOrderID);
        //    var orderDetails = order.Details;

        //    foreach (var deliveryDetail in delivery.Details)
        //    {
        //        foreach (var orderDetail in order.Details)
        //        {
        //            if (orderDetail.ItemNo == deliveryDetail.TransferOrderItemNo)
        //            {
        //                orderDetail.QtyFulfilled += deliveryDetail.Quantity;
        //            }
        //        }
        //    }
        //    new TransferOrderBFC().Update(order);
        //}
    }
}
