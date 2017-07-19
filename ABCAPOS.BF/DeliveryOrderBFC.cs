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
using MPL;

namespace ABCAPOS.BF
{
    public class DeliveryOrderBFC : MasterDetailBFC<DeliveryOrder, v_DeliveryOrder, DeliveryOrderDetail, v_DeliveryOrderDetail, DeliveryOrderModel, DeliveryOrderDetailModel>
    {
        private void decreaseInventoryAvailableQty(DeliveryOrderModel deliveryOrder, bool isUndo)
        {
            if (deliveryOrder != null)
            {
                var salesOrder = new SalesOrderBFC().RetrieveByID(deliveryOrder.SalesOrderID);
                var details = RetrieveDetails(deliveryOrder.ID);
                if (salesOrder.Date > Convert.ToDateTime("2015-08-29"))
                {
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
                            itemLocation.QtyAvailable -= qtyBaseUnit;
                            new ItemLocationBFC().Update(itemLocation);
                        }
                        else
                        {
                            new ItemLocationBFC().Create(detail.ProductID,
                              deliveryOrder.WarehouseID,
                              0,
                              -qtyBaseUnit);
                        }
                    }
                }
            }
        }

        private void decreaseInventoryQty(DeliveryOrderModel deliveryOrder, bool isUndo)
        {
            if (deliveryOrder != null)
            {
                var salesOrder = new SalesOrderBFC().RetrieveByID(deliveryOrder.SalesOrderID);
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
                          0);
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

        private void CalculateDeliveryOrderDetailPrice(long deliveryOrderID, List<DeliveryOrderDetailModel> deliveryOrderDetails, List<SalesOrderDetailModel> quotationDetails)
        {
            foreach (var deliveryOrderDetail in deliveryOrderDetails)
            {
                var soDetail = (from i in quotationDetails
                                where i.ProductID == deliveryOrderDetail.ProductID && i.ItemNo == deliveryOrderDetail.SalesOrderItemNo
                                select i).FirstOrDefault();

                deliveryOrderDetail.Price = soDetail.PriceAfterDiscount;
            }

            GetDetailDAC().DeleteByParentID(deliveryOrderID);
            GetDetailDAC().Create(deliveryOrderID, deliveryOrderDetails);
        }

        #region PostingTableLog
        public void CreateLog(DeliveryOrderModel header)
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
                        LogHeader.DocType = (int)DocType.DeliveryOrder;

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

        private void UndoContainerStock(DeliveryOrderModel header)
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
                            var container = new ContainerBFC().RetreiveByLogIDproductIDdocType(header.LogID, detail.ProductID, (int)DocType.DeliveryOrder);
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
                throw(ex);
            }
        }

        private void UpdateLog(DeliveryOrderModel header,List<DeliveryOrderDetailModel>details)
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
                    LogHeader.DocType = (int)DocType.DeliveryOrder;

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
                                QtyRemaining = QtyRemaining - qty;

                                new ABCAPOSDAC().CreateLog(LogDetail);
                            }
                            else
                            {
                                var containerHeader = new ContainerModel();
                                //var containerdetail = new ContainerBFC().RetreiveByLogIDproductIDdocType(header.LogID, detail.ProductID, (int)DocType.DeliveryOrder);
                                //containerHeader.ID = containerdetail.ID;
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
                throw(ex);
            }
        }

        private void DecreaseContainer(DeliveryOrderModel header)
        {
            try
            {
                var LogHeader = new LogModel();
                var Log = new LogBFC().RetrieveByID(header.LogID);
                if (Log != null)
                {
                    var details = new LogBFC().RetrieveDetails(header.LogID);
                    foreach (var detail in details)
                    {
                        var LogDetails = new LogDetailModel();

                        var container = new ContainerBFC().RetreiveByLogIDproductIDdocType(header.LogID, detail.ProductID, (int)DocType.DeliveryOrder);
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
                                    ContainerHeader.ContainerID = container.ContainerID;
                                    ContainerHeader.ProductID = detail.ProductID;
                                    ContainerHeader.WarehouseID = header.WarehouseID;
                                    ContainerHeader.Qty = containerDetail.Qty + LogDetail.MovingOutQty;
                                    ContainerHeader.Price = containerDetail.Price;
                                    new ContainerBFC().Update(ContainerHeader);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw(ex);
            }
        }

        private void DecreaseLog(long logID)
        {
            var LogHeader = new LogModel();
            var Log = new LogBFC().RetrieveByID(logID);
            if (Log != null)
            {
                var details = new LogBFC().RetrieveDetails(logID);
                var itemNo = 1;

                new ABCAPOSDAC().DeleteLogByLogID(logID);
                foreach (var detail in details)
                {
                    var LogDetails = new LogDetailModel();
                    LogDetails.ContainerID = detail.ContainerID;
                    LogDetails.LogID = detail.LogID;
                    LogDetails.ItemNo = itemNo++;
                    LogDetails.ProductID = detail.ProductID;
                    LogDetails.MovingOutQty = 0;
                    LogDetails.MovingInQty = 0;

                    new ABCAPOSDAC().CreateLog(LogDetails);
                }
                LogHeader.ID = Log.ID;
                LogHeader.WarehouseID = Log.WarehouseID;
                LogHeader.Date = Log.Date;
                LogHeader.DocType = (int)MPL.DocumentStatus.Void;
                new LogBFC().Update(LogHeader);
            }
        }

        private void VoidLog(DeliveryOrderModel header)
        {
            this.DecreaseContainer(header);
            this.DecreaseLog(header.LogID);
        }
        #endregion

        public string GetDeliveryOrderCode(DeliveryOrderModel header)
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var deliveryOrderPrefix = "";

            if (prefixSetting != null)
                deliveryOrderPrefix = prefixSetting.DeliveryOrderPrefix;

            var warehouse = new WarehouseBFC().RetrieveByID(header.WarehouseID);
            var year = DateTime.Now.Year.ToString().Substring(2, 2);

            var prefix = deliveryOrderPrefix + year + "-" + warehouse.Code + "-";
            var code = new ABCAPOSDAC().RetrieveDeliveryOrderMaxCode(prefix, 7);

            return code;
        }
        
        protected override GenericDetailDAC<DeliveryOrderDetail, DeliveryOrderDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<DeliveryOrderDetail, DeliveryOrderDetailModel>("DeliveryOrderID", "ItemNo", false);
        }

        protected override GenericDetailDAC<v_DeliveryOrderDetail, DeliveryOrderDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_DeliveryOrderDetail, DeliveryOrderDetailModel>("DeliveryOrderID", "ItemNo", false);
        }

        protected override GenericDAC<DeliveryOrder, DeliveryOrderModel> GetMasterDAC()
        {
            return new GenericDAC<DeliveryOrder, DeliveryOrderModel>("ID", false, "Date DESC");
        }

        protected override GenericDAC<v_DeliveryOrder, DeliveryOrderModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_DeliveryOrder, DeliveryOrderModel>("ID", false, "Date DESC");
        }

        public void updateFromOldQty(DeliveryOrderModel header, List<DeliveryOrderDetailModel> details)
        {
            var oldDetails = RetrieveDetails(header.ID);

            foreach (var detail in details)
            {
                var oldDetail = from i in oldDetails
                                where i.ProductID == detail.ProductID && i.ItemNo == detail.ItemNo
                                select i;

                var oldDoDetail = oldDetail.FirstOrDefault();

                detail.SelisihQuantity = detail.Quantity - oldDoDetail.Quantity;
            }

            ////no more need. CreatedDOQuantity is now a view 
            //new SalesOrderBFC().UpdateCreatedDOQuantity(header.SalesOrderID, details);
            ////Update ItemLocation
            ////Update Bin
            UpdateDOItemLocationBin(header, details);


        }

        public void UpdateDOItemLocationBin(DeliveryOrderModel header, List<DeliveryOrderDetailModel> details)
        {
            var oldDO = RetrieveByID(header.ID);

            foreach (var detail in details)
            {
                var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                var baseSelisihQuantity = detail.SelisihQuantity * unitRate;
                var itemLocation = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductID, header.WarehouseID);
                if (itemLocation != null)
                {
                    if (oldDO.Status == (int)DeliveryOrderStatus.Shipped)
                    {
                        itemLocation.QtyAvailable -= baseSelisihQuantity;
                        itemLocation.QtyOnHand -= baseSelisihQuantity;
                        new ItemLocationBFC().Update(itemLocation);
                    }
                    else
                    {
                        itemLocation.QtyAvailable -= baseSelisihQuantity;
                        //itemLocation.QtyOnHand -= baseSelisihQuantity;
                        new ItemLocationBFC().Update(itemLocation);
                    }
                    
                }
                if (detail.BinID != null || detail.BinID != 0)
                {
                    var binProduct = new BinProductWarehouseBFC().Retrieve(detail.BinID, detail.ProductID);
                    if (binProduct != null)
                    {
                        if (oldDO.Status == (int)DeliveryOrderStatus.Shipped)
                        {
                            binProduct.Quantity -= baseSelisihQuantity;
                            new BinProductWarehouseBFC().Update(binProduct);
                        }
                    }
                }
            }
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
                OnApproved(deliveryOrderID, userName);

                new SalesOrderBFC().UpdateStatus(deliveryOrder.SalesOrderID);
                trans.Complete();
            }
        }

        public void Void(long deliveryOrderID, string voidRemarks, string userName)
        {
            var deliveryOrder = RetrieveByID(deliveryOrderID);
            if (deliveryOrder == null)
            {
                return;
            }
            var oldStatus = deliveryOrder.Status;

            using (TransactionScope trans = new TransactionScope())
            {
                
                
                //if (oldStatus == (int)DeliveryOrderStatus.Shipped)
                OnVoid(deliveryOrderID, voidRemarks, userName);

                deliveryOrder.Status = (int)MPL.DocumentStatus.Void;
                deliveryOrder.VoidRemarks = voidRemarks;
                //deliveryOrder.ApprovedBy = "";
                //deliveryOrder.ApprovedDate = SystemConstants.UnsetDateTime;
                //Update(deliveryOrder);
                new SalesOrderBFC().UpdateStatus(deliveryOrder.SalesOrderID);

                PostAccounting(deliveryOrderID, deliveryOrder.Status);

                OnUpdated(deliveryOrder.SalesOrderID, userName);

                base.Update(deliveryOrder);

                trans.Complete();
            }
        }

        public void OnUpdated(long salesOrderID, string userName)
        {
            //var deliveryOrder = RetrieveByID(deliveryOrderID);

            //if (deliveryOrder == null)
            //{
            //    return;
            //}

            var so = new SalesOrderBFC().RetrieveByID(salesOrderID);

            if (so == null)
            {
                return;
            }

            var deliveryOrderDetails = RetrieveDetailsBySalesOrderID(so.ID);
            var soDetails = new SalesOrderBFC().RetrieveDetails(so.ID);

            var fulfilledCount = 0;

            foreach (var soDetail in soDetails)
            {
                if (soDetail.Quantity == soDetail.CreatedDOQuantity)
                    fulfilledCount += 1;
            }

            if (deliveryOrderDetails.Count == 0)
                so.HasDO = false;
            else
                so.HasDO = true;

            if (fulfilledCount == soDetails.Count)
                so.IsDeliveryOrderFulfilled = true;
            else
                so.IsDeliveryOrderFulfilled = false;

            new SalesOrderBFC().Update(so);
            new SalesOrderBFC().UpdateDetails(so.ID, soDetails);


        }

        public void ImplementStatus(long deliveryID, string userName, DeliveryOrderStatus previousStatus, DeliveryOrderStatus newStatus)
        {
            var deliveryOrder = RetrieveByID(deliveryID);
            deliveryOrder.Status = (int)newStatus;

            //var details = RetrieveDetails(deliveryID);

            using (TransactionScope trans = new TransactionScope())
            {
                switch (previousStatus)
                {
                    case DeliveryOrderStatus.Void:
                        if (newStatus == DeliveryOrderStatus.Shipped)
                        {
                            decreaseInventoryAvailableQty(deliveryOrder, false);
                            decreaseInventoryQty(deliveryOrder, false);
                            this.CreateLog(deliveryOrder);
                        }
                        else if (newStatus == DeliveryOrderStatus.New)
                        {
                            decreaseInventoryAvailableQty(deliveryOrder, false);
                        }
                        //else if (newStatus == DeliveryOrderStatus.New || newStatus == DeliveryOrderStatus.Packed)
                        //{
                        //    //decreaseInventoryAvailableQty(deliveryOrder, false);
                        //}
                        break;
                    case DeliveryOrderStatus.New:
                        if (newStatus == DeliveryOrderStatus.Shipped)
                        {
                            decreaseInventoryQty(deliveryOrder, false);
                            this.CreateLog(deliveryOrder);
                        }
                        else if (newStatus == DeliveryOrderStatus.Void)
                        {
                            //this.decreaseInventoryQty(deliveryOrder, true);
                            decreaseInventoryAvailableQty(deliveryOrder, true);
                        }
                        break;
                    //case DeliveryOrderStatus.Packed:
                    //    if (newStatus == DeliveryOrderStatus.Shipped)
                    //    {
                    //       decreaseInventoryQty(deliveryOrder, false);
                    //       this.CreateLog(deliveryOrder);
                    //    }
                    //    else if (newStatus == DeliveryOrderStatus.Void)
                    //    {
                    //        //this.decreaseInventoryQty(deliveryOrder, true);
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
                //Update(deliveryOrder);
                new SalesOrderBFC().UpdateStatus(deliveryOrder.SalesOrderID);
                trans.Complete();
            }
        }

        public void OnApproved(long deliveryOrderID, string userName)
        {
            var deliveryOrder = RetrieveByID(deliveryOrderID);
            var salesOrder = new SalesOrderBFC().RetrieveByID(deliveryOrder.SalesOrderID);

            if (deliveryOrder != null)
            {
                var deliveryOrderDetails = RetrieveDetails(deliveryOrderID);

                foreach (var doDetail in deliveryOrderDetails)
                {
                    // Commenting this portion out. QtyAvailable decreases upon creation of SalesOrder
                    //var itemLocation = new ItemLocationBFC().RetrieveByProductIDWarehouseID(doDetail.ProductID, deliveryOrder.WarehouseID);
                    //if (itemLocation != null)
                    //{
                    //    itemLocation.QtyAvailable -= doDetail.Quantity;
                    //    //itemLocation.QtyOnHand += doDetail.Quantity;

                    //    new ItemLocationBFC().Update(itemLocation);
                    //}
                    //else
                    //    throw new Exception("error do itemlocation");

                    //else
                    //{
                    //    var newitemLocation = new ItemLocationModel();
                    //    newitemLocation.ProductID = doDetail.ProductID;
                    //    newitemLocation.WarehouseID = doDetail.WarehouseID;
                    //    newitemLocation.QtyOnHand = newitemLocation.QtyAvailable = pdDetail.Quantity;
                    //    new ItemLocationBFC().Create(newitemLocation);
                    //}
                }

                //var salesOrderDetails = new SalesOrderBFC().RetrieveDetails(deliveryOrder.SalesOrderID);

                //CalculateDeliveryOrderDetailPrice(deliveryOrderID, deliveryOrderDetails, salesOrderDetails);
                //new InvoiceBFC().CreateInvoiceByDeliveryOrder(deliveryOrderID, userName);
            }
        }

        public void voidFromOldQty(DeliveryOrderModel header, List<DeliveryOrderDetailModel> details)
        {
            var oldDetails = RetrieveDetails(header.ID);

            foreach (var detail in details)
            {
                detail.SelisihQuantity = -detail.Quantity;
            }

            //UpdateDOItemLocationBin(header, details);


        }

        public void OnVoid(long deliveryOrderID, string voidRemarks, string userName)
        {
            var deliveryOrder = RetrieveByID(deliveryOrderID);
            var deliveryOrderDetails = RetrieveDetails(deliveryOrderID);

            voidFromOldQty(deliveryOrder, deliveryOrderDetails);
        }

        public void ValidateShip(long deliveryID)
        {
            var deliveryOrder = new DeliveryOrderBFC().RetrieveByID(deliveryID);
            var deliveryOrderDetails = new DeliveryOrderBFC().RetrieveDetails(deliveryID);
            var soDetails = new SalesOrderBFC().RetrieveDetails(deliveryOrder.SalesOrderID);

            foreach (var deliveryOrderDetail in deliveryOrderDetails)
            {
                deliveryOrderDetail.UnitRate = new ProductBFC().GetUnitRate(deliveryOrderDetail.ConversionID);
                var itemLoc = new ItemLocationBFC().RetrieveByProductIDWarehouseID(deliveryOrderDetail.ProductID, deliveryOrder.WarehouseID);
                if (itemLoc != null)
                    deliveryOrderDetail.StockQty = itemLoc.QtyOnHand / deliveryOrderDetail.UnitRate;
                else
                    deliveryOrderDetail.StockQty = 0;

                if (itemLoc != null)
                    deliveryOrderDetail.StockAvailable = itemLoc.QtyAvailable / deliveryOrderDetail.UnitRate;
                else
                    deliveryOrderDetail.StockAvailable = 0;

                var query = from i in soDetails
                            where i.ProductID == deliveryOrderDetail.ProductID && i.ItemNo == deliveryOrderDetail.SalesOrderItemNo
                            select i;

                var spkDetail = query.FirstOrDefault();
                var doQty = deliveryOrderDetail.Quantity;

                if (deliveryOrderDetail.StockQty < doQty)
                    throw new Exception("Jumlah " + spkDetail.ProductCode + " yang dimasukkan melebihi jumlah di stok on hand");

            }
        }

        public void Validate(DeliveryOrderModel deliveryOrder, List<DeliveryOrderDetailModel> deliveryOrderDetails)
        {
            var obj = RetrieveByID(deliveryOrder.ID);
            var soDetails = new SalesOrderBFC().RetrieveDetails(deliveryOrder.SalesOrderID);

            var extDetails = new List<DeliveryOrderDetailModel>();

            if (obj != null)
                extDetails = RetrieveDetails(deliveryOrder.ID);

            foreach (var deliveryOrderDetail in deliveryOrderDetails)
            {
                var query = from i in soDetails
                            where i.ProductID == deliveryOrderDetail.ProductID && i.ItemNo == deliveryOrderDetail.SalesOrderItemNo
                            select i;

                var spkDetail = query.FirstOrDefault();
                var doQty = deliveryOrderDetail.Quantity;
                if (doQty > 0)
                {
                    if (spkDetail.OutstandingQuantity < doQty)
                        throw new Exception("Jumlah " + spkDetail.ProductCode + " yang dimasukkan melebihi jumlah yg tersisa");

                    if (spkDetail.StockAvailableHidden < doQty)
                        throw new Exception("Jumlah " + spkDetail.ProductCode + " yang dimasukkan melebihi jumlah di stok available");

                    if (spkDetail.StockQtyHidden < doQty)
                        throw new Exception("Jumlah " + spkDetail.ProductCode + " yang dimasukkan melebihi jumlah di stok on hand");
                }
            }
        }

        public void CreateBySalesOrder(DeliveryOrderModel deliveryOrder, long salesOrderID)
        {
            var salesOrder = new SalesOrderBFC().RetrieveByID(salesOrderID);

            if (salesOrder != null)
            {
                deliveryOrder.Code = SystemConstants.autoGenerated;//GetDeliveryOrderCode();
                
                deliveryOrder.SalesOrderID = salesOrderID;
                deliveryOrder.SalesOrderCode = salesOrder.Code;
                deliveryOrder.POCustomerNo = salesOrder.POCustomerNo;
                //deliveryOrder.POCustomerDate = salesOrder.POCustomerDate;
                deliveryOrder.CustomerCode = salesOrder.CustomerCode;
                deliveryOrder.CustomerName = salesOrder.CustomerName;
                deliveryOrder.WarehouseID = salesOrder.WarehouseID;

                var warehouse = new WarehouseBFC().RetrieveByID(salesOrder.WarehouseID);
                deliveryOrder.WarehouseName = warehouse.Name;

                var soDetails = new SalesOrderBFC().RetrieveDetails(salesOrderID);
                var deliveryOrderDetails = new List<DeliveryOrderDetailModel>();

                foreach (var soDetail in soDetails)
                {
                    if (soDetail.OutstandingQuantity > 0)
                    {
                        var detail = new DeliveryOrderDetailModel();
                        detail.isFulFill = false;
                        detail.SalesOrderItemNo = soDetail.ItemNo;
                        detail.Barcode = soDetail.Barcode;
                        detail.ProductID = soDetail.ProductID;
                        detail.ProductCode = soDetail.ProductCode;
                        detail.ProductName = soDetail.ProductName;
                        detail.ConversionName = soDetail.ConversionName;
                        detail.ConversionID = soDetail.ConversionID;
                        detail.QtySO = Convert.ToDouble(soDetail.Quantity);
                        detail.QtyDO = Convert.ToDouble(soDetail.CreatedDOQuantity);
                        detail.Quantity = soDetail.OutstandingQuantity;
                        detail.QtyHidden = soDetail.OutstandingQuantity;
                        //detail.StrQuantity = soDetail.OutstandingQuantity.ToString().Replace(",", "").Replace(".", ",");
                        detail.Remarks = soDetail.Remarks;

                        detail.UnitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                        var itemLoc = new ItemLocationBFC().RetrieveByProductIDWarehouseID(soDetail.ProductID, salesOrder.WarehouseID);
                        if (itemLoc != null)
                            detail.StockQty = itemLoc.QtyOnHand / detail.UnitRate;
                        else
                            detail.StockQty = 0;

                        if (itemLoc != null)
                            detail.StockAvailable = itemLoc.QtyAvailable / detail.UnitRate;
                        else
                            detail.StockAvailable = 0;

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

        public override void Create(DeliveryOrderModel header, List<DeliveryOrderDetailModel> details)
        {
            header.Code = GetDeliveryOrderCode(header);

            using (TransactionScope trans = new TransactionScope())
            {
                base.Create(header, details);

                OnUpdated(header.SalesOrderID, header.CreatedBy);

                PostAccounting(header.ID,header.Status);

                trans.Complete();
            }
            new SalesOrderBFC().UpdateStatus(header.SalesOrderID);
        }

        public override void Update(DeliveryOrderModel header, List<DeliveryOrderDetailModel> details)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                //Please dont change the location after base.Update
                //updateFromOldQty(header, details);

               
                //header.ModifiedDate = DateTime.Now;
                //OnUpdated(header.ID, header.CreatedBy);
                base.Update(header, details);

                new SalesOrderBFC().UpdateStatus(header.SalesOrderID);

                PostAccounting(header.ID,header.Status);

                trans.Complete();
            }
        }

        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public List<DeliveryOrderModel> RetrieveBySalesOrderID(long soID)
        {
            return new ABCAPOSDAC().RetrieveDeliveryOrderBySalesOrderID(soID);
        }

        public List<DeliveryOrderModel> RetrieveBySalesOrderID(long soID, DeliveryOrderStatus deliveryStatus)
        {
            return new ABCAPOSDAC().RetrieveDeliveryOrderBySalesOrderID(soID, deliveryStatus);
        }

        public List<DeliveryOrderDetailModel> RetrieveDetailsBySalesOrderID(long spkID)
        {
            return new ABCAPOSDAC().RetrieveDeliveryOrderDetailBySalesOrderID(spkID);
        }

        public List<DeliveryOrderModel> RetrieveByBSID(long bookingSalesID)
        {
            var soList = new SalesOrderBFC().RetrieveByBSID(bookingSalesID);
            var doList = new List<DeliveryOrderModel>();

            foreach (var so in soList)
            {
                var doList2 = new ABCAPOSDAC().RetrieveDOBySOID(so.ID);
                doList.AddRange(doList2);
            }

            return doList;
        }

        public List<DeliveryOrderModel> RetreiveDOByCustomerID(long customerID)
        {
            return new ABCAPOSDAC().RetreiveDOByCustomerID(customerID);
        }

        public List<DeliveryOrderModel> RetrieveDOByCustomerIDExpeditionIDStartDateEndDate(long customerID, long expeditionID, DateTime startDate, DateTime endDate)
        {
            return new ABCAPOSDAC().RetrieveDOByCustomerIDExpeditionIDStartDateEndDate(customerID, expeditionID, startDate, endDate);
        }

        public ABCAPOSReportEDSC.DeliveryOrderDTRow RetrievePrintOut(long deliveryOrderID)
        {
            return new ABCAPOSReportDAC().RetrieveDeliveryOrderPrintOut(deliveryOrderID);
        }

        public ABCAPOSReportEDSC.DeliveryOrderDetailDTDataTable RetrieveDetailPrintOut(long deliveryOrderID)
        {
            return new ABCAPOSReportDAC().RetrieveDeliveryOrderDetailPrintOut(deliveryOrderID);
        }

        #region Post to Accounting Result

        public void PostAccounting(long deliveryOrderID, int Status)
        {
            var deliveryOrder = RetrieveByID(deliveryOrderID);

            new ABCAPOSDAC().DeleteAccountingResults(deliveryOrderID, AccountingResultDocumentType.DeliveryOrder);

            if (Status != (int)MPL.DocumentStatus.Void)
                CreateAccountingResult(deliveryOrderID);
        }

        private void CreateAccountingResult(long deliveryOrderID)
        {
            var deliveryOrder = RetrieveByID(deliveryOrderID);
            var deliveryOrderDetails = RetrieveDetails(deliveryOrderID);

            var salesOrderDetails = new SalesOrderBFC().RetrieveDetails(deliveryOrder.SalesOrderID);

            decimal totalAmount = 0;

            var accountingResultList = new List<AccountingResultModel>();

            foreach (var deliveryOrderDetail in deliveryOrderDetails)
            {
                var soDetail = (from i in salesOrderDetails
                                where i.ProductID == deliveryOrderDetail.ProductID
                                select i).FirstOrDefault();

                if (soDetail != null)
                {
                    var product = new ProductBFC().RetrieveByID(deliveryOrderDetail.ProductID);

                    totalAmount += Convert.ToDecimal(deliveryOrderDetail.Quantity) * product.BasePrice;
                }
            }

            accountingResultList = AddToAccountingResultList(accountingResultList, deliveryOrder, (long)PostingAccount.PersediaanDalamPerjalanan, AccountingResultType.Debit, totalAmount, "Persediaan dalam perjalanan item fulfillment " + deliveryOrder.Code);

            accountingResultList = AddToAccountingResultList(accountingResultList, deliveryOrder, (long)PostingAccount.PersediaanBarangJadi, AccountingResultType.Credit, totalAmount, "Persediaan barang jadi item fulfillment " + deliveryOrder.Code);

            new AccountingResultBFC().Posting(accountingResultList);
        }

        private List<AccountingResultModel> AddToAccountingResultList(List<AccountingResultModel> resultList, DeliveryOrderModel obj, long accountID, AccountingResultType resultType, decimal amount, string remarks)
        {
            if (amount > 0)
            {
                var account = new AccountBFC().RetrieveByID(accountID);
                var result = new AccountingResultModel();

                result.DocumentID = obj.ID;
                result.DocumentType = (int)AccountingResultDocumentType.DeliveryOrder;
                result.Type = (int)resultType;
                result.Date = obj.Date;
                result.AccountID = account.ID;
                result.DocumentNo = obj.Code;
                result.Amount = amount;

                if (resultType == AccountingResultType.Debit)
                    result.DebitAmount = amount;
                else
                    result.CreditAmount = amount;

                result.Remarks = remarks;

                resultList.Add(result);
            }

            return resultList;
        }
        #endregion

        #region Notification

        public int RetrieveUnapprovedDeliveryOrderCount(CustomerGroupModel customerGroup)
        {
            return new ABCAPOSDAC().RetrieveUnapprovedDeliveryOrderCount(customerGroup);
        }

        public int RetrieveVoidDeliveryOrderCount(CustomerGroupModel customerGroup)
        {
            return new ABCAPOSDAC().RetrieveVoidDeliveryOrderCount(customerGroup);
        }

        #endregion

        //public List<DeliveryOrderModel> RetrieveBySOID(long salesOrderID)
        //{
        //    return new ABCAPOSDAC().RetrievePDByPOID(salesOrderID);
        //}
    }
}
