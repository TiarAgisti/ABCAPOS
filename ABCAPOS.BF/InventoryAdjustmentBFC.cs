using ABCAPOS.DA;
using ABCAPOS.EDM;
using ABCAPOS.Models;
using ABCAPOS.Util;
using MPL;
using MPL.Business;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace ABCAPOS.BF
{
    public class InventoryAdjustmentBFC : MasterDetailBFC<InventoryAdjustment, v_InventoryAdjustment, InventoryAdjustmentDetail, v_InventoryAdjustmentDetail, InventoryAdjustmentModel, InventoryAdjustmentDetailModel>
    {
        private void UpdateQuantities(long documentID, bool isRegular)
        {
            var master = RetrieveByID(documentID);

            if (master != null)
            {
                var details = RetrieveDetails(documentID);

                foreach (var detail in details)
                {
                    var itemQtyInCustomUnit = detail.Quantity;
                    var itemQtyAvailable = detail.QuantityAvailable;
                    var itemUnitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                    var itemQtyInBaseUnit = itemQtyInCustomUnit * itemUnitRate;
                    var itemQtyAvailableBaseUnit = itemQtyAvailable * itemUnitRate;

                    var itemLocation = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductID, master.WarehouseID);
                    if (itemLocation != null)
                    {
                        if (isRegular)
                        {
                            itemLocation.QtyAvailable += itemQtyAvailableBaseUnit;
                            itemLocation.QtyOnHand += itemQtyInBaseUnit;
                        }
                        else
                        {
                            itemLocation.QtyAvailable -= itemQtyAvailableBaseUnit;
                            itemLocation.QtyOnHand -= itemQtyInBaseUnit;
                        }
                        new ItemLocationBFC().Update(itemLocation);
                    }
                    else
                    {
                        var newitemLocation = new ItemLocationModel();
                        newitemLocation.ProductID = detail.ProductID;
                        newitemLocation.WarehouseID = master.WarehouseID;
                        if (isRegular)
                        {
                            newitemLocation.QtyOnHand = itemQtyInBaseUnit;
                            newitemLocation.QtyAvailable = itemQtyAvailableBaseUnit;
                        }
                        else
                        {
                            newitemLocation.QtyOnHand = itemQtyInBaseUnit;
                            newitemLocation.QtyAvailable = itemQtyAvailableBaseUnit;
                        }
                           
                        new ItemLocationBFC().Create(newitemLocation);
                    }
                    if (detail.BinID != 0)
                    {
                        var binProduct = new BinProductWarehouseBFC().Retrieve(detail.BinID, detail.ProductID);
                        if (binProduct != null)
                        {
                            if (isRegular)
                                binProduct.Quantity += itemQtyInBaseUnit;
                            else
                                binProduct.Quantity -= itemQtyInBaseUnit;
                            new BinProductWarehouseBFC().Update(binProduct);
                        }
                    }
                }
            }
        }

        #region PostingTableLog
        public void IncreaseLog(InventoryAdjustmentModel header, List<InventoryAdjustmentDetailModel> details)
        {
            try
            {
                if (header != null && details != null)
                {
                    var LogHeader = new LogModel();
                    var LogDetails = new List<LogDetailModel>();

                    LogHeader.WarehouseID = header.WarehouseID;
                    LogHeader.Date = header.Date;
                    LogHeader.DocType = (int)DocType.InventoryAdjustment;

                    foreach (var detail in details)
                    {
                        var itemUnitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                        var itemQtyInBaseUnit = detail.Quantity * itemUnitRate;

                        if (detail.Quantity < 0)
                        {
                            var QtyRemaining = -(itemQtyInBaseUnit);
                            do
                            {
                                var LogDetail = new LogDetailModel();
                                var container = new ContainerBFC().RetreiveByProductIDWarehouseID(detail.ProductID, header.WarehouseID);
                                if (container != null)
                                {
                                    double qty = (QtyRemaining > container.Qty) ? container.Qty : QtyRemaining;
                                    var containerHeader = new ContainerModel();
                                    containerHeader.ID = container.ID;
                                    containerHeader.ProductID = container.ProductID;
                                    containerHeader.WarehouseID = container.WarehouseID;
                                    QtyRemaining = QtyRemaining - qty;
                                    containerHeader.Qty = container.Qty - qty;
                                    containerHeader.Price = container.Price;
                                    new ContainerBFC().Update(containerHeader);

                                    LogDetail.ContainerID = container.ID;
                                    LogDetail.ProductID = detail.ProductID;
                                    LogDetail.MovingOutQty = qty;
                                    LogDetail.MovingOutValue = container.Price;

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
                                    LogDetail.MovingOutValue = 0;
                                    QtyRemaining = 0;

                                    LogDetails.Add(LogDetail);
                                }
                            }while (QtyRemaining > 0);
                        }
                        else if (detail.Quantity > 0)
                        {
                            var LogDetail = new LogDetailModel();
                            var container = new ContainerBFC().RetreiveByProductIDWarehouseID(detail.ProductID, header.WarehouseID);
                            if (container != null)
                            {
                                var containerHeader = new ContainerModel();
                                containerHeader.ID = container.ID;
                                containerHeader.ProductID = container.ProductID;
                                containerHeader.WarehouseID = container.WarehouseID;
                                containerHeader.Qty = container.Qty + itemQtyInBaseUnit;
                                containerHeader.Price = container.Price;
                                new ContainerBFC().Update(containerHeader);

                                LogDetail.ContainerID = container.ID;
                                LogDetail.ProductID = detail.ProductID;
                                LogDetail.MovingInQty = itemQtyInBaseUnit;
                                LogDetail.MovingInValue = container.Price;

                                LogDetails.Add(LogDetail);
                            }
                            else
                            {
                                var containerHeader = new ContainerModel();
                                containerHeader.ProductID = detail.ProductID;
                                containerHeader.WarehouseID = header.WarehouseID;
                                containerHeader.Qty = itemQtyInBaseUnit;
                                containerHeader.Price = 0;
                                new ContainerBFC().Create(containerHeader);

                                LogDetail.ContainerID = containerHeader.ID;
                                LogDetail.ProductID = detail.ProductID;
                                LogDetail.MovingInQty = itemQtyInBaseUnit;
                                LogDetail.MovingOutValue = 0;

                                LogDetails.Add(LogDetail);
                            }
                        }
                    }
                    new LogBFC().Create(LogHeader, LogDetails);
                    header.LogID = LogHeader.ID;
                    base.Update(header);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void DecreaseLog(InventoryAdjustmentModel header, List<InventoryAdjustmentDetailModel> details)
        {
            try
            {
                if (header != null && details != null)
                {
                    var Log = new LogBFC().RetrieveByID(header.LogID);
                    if (Log != null)
                    {
                        var LogHeader = new LogModel();
                        var LogDetails = new List<LogDetailModel>();
                        var itemNo = 1;
                        foreach (var detail in details)
                        {
                            var itemUnitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                            var itemQtyInBaseUnit = detail.Quantity * itemUnitRate;
                            if (detail.Quantity < 0)
                            {
                                var LogDetail = new LogDetailModel();
                                var qty = -(itemQtyInBaseUnit);
                                var container = new ContainerBFC().RetreiveByLogIDproductIDdocType(header.LogID, detail.ProductID, (int)DocType.InventoryAdjustment);
                                if (container != null)
                                {
                                    var containerDetail = new ContainerBFC().RetreiveByContainerIDProductIDWarehouseID(container.ContainerID, detail.ProductID, header.WarehouseID);
                                    if (containerDetail != null)
                                    {
                                        var containerHeader = new ContainerModel();
                                        containerHeader.ID = container.ContainerID;
                                        containerHeader.ProductID = detail.ProductID;
                                        containerHeader.WarehouseID = header.WarehouseID;
                                        containerHeader.Qty = containerDetail.Qty + qty;
                                        containerHeader.Price = containerDetail.Price;
                                        new ContainerBFC().Update(containerHeader);

                                        new ABCAPOSDAC().DeleteLogByLogIDContainerIDProductID(header.LogID, container.ContainerID, detail.ProductID);
                                        LogDetail.LogID = header.LogID;
                                        LogDetail.ItemNo = itemNo++;
                                        LogDetail.ContainerID = container.ContainerID;
                                        LogDetail.ProductID = detail.ProductID;
                                        LogDetail.MovingInQty = 0;
                                        //LogDetail.MovingInValue = 0;
                                        LogDetail.MovingOutQty = 0;
                                        //LogDetail.MovingOutValue = 0;
                                        new ABCAPOSDAC().CreateLog(LogDetail);
                                    }
                                }
                            }
                            else if (detail.Quantity > 0)
                            {
                                var LogDetail = new LogDetailModel();
                                var container = new ContainerBFC().RetreiveByLogIDproductIDdocType(header.LogID, detail.ProductID, (int)DocType.InventoryAdjustment);
                                if (container != null)
                                {
                                    var containerDetail = new ContainerBFC().RetreiveByContainerIDProductIDWarehouseID(container.ContainerID, detail.ProductID, header.WarehouseID);
                                    if (containerDetail != null)
                                    {
                                        var containerHeader = new ContainerModel();
                                        containerHeader.ID = container.ContainerID;
                                        containerHeader.ProductID = detail.ProductID;
                                        containerHeader.WarehouseID = header.WarehouseID;
                                        containerHeader.Qty = containerDetail.Qty - itemQtyInBaseUnit;
                                        containerHeader.Price = containerDetail.Price;
                                        new ContainerBFC().Update(containerHeader);

                                        new ABCAPOSDAC().DeleteLogByLogIDContainerIDProductID(header.LogID, container.ContainerID, detail.ProductID);
                                        LogDetail.LogID = header.LogID;
                                        LogDetail.ItemNo = itemNo++;
                                        LogDetail.ContainerID = container.ContainerID;
                                        LogDetail.ProductID = detail.ProductID;
                                        LogDetail.MovingInQty = 0;
                                        //LogDetail.MovingInValue = 0;
                                        LogDetail.MovingOutQty = 0;
                                        //LogDetail.MovingOutValue = 0;
                                        new ABCAPOSDAC().CreateLog(LogDetail);
                                    }
                                }
                            }
                        }
                        LogHeader.ID = header.LogID;
                        LogHeader.WarehouseID = header.WarehouseID;
                        LogHeader.Date = header.Date;
                        LogHeader.DocType = (int)MPL.DocumentStatus.Void;
                        new LogBFC().Update(LogHeader);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetInventoryAdjustmentCode(InventoryAdjustmentModel header)
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var defaultPrefix = "";

            if (prefixSetting != null)
                defaultPrefix = prefixSetting.AdjustmentPrefix;

            var warehouse = new WarehouseBFC().RetrieveByID(header.WarehouseID);
            var year = DateTime.Now.Year.ToString().Substring(2, 2);

            var prefix = defaultPrefix + year + "-" + warehouse.Code + "-";

            var code = new ABCAPOSDAC().RetrieveInventoryAdjustmentMaxCode(prefix, 7);

            return code;
        }

        protected override GenericDetailDAC<InventoryAdjustmentDetail, InventoryAdjustmentDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<InventoryAdjustmentDetail, InventoryAdjustmentDetailModel>("InventoryAdjustmentID", "ItemNo", false);
        }

        protected override GenericDetailDAC<v_InventoryAdjustmentDetail, InventoryAdjustmentDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_InventoryAdjustmentDetail, InventoryAdjustmentDetailModel>("InventoryAdjustmentID", "ItemNo", false);
        }

        protected override GenericDAC<InventoryAdjustment, InventoryAdjustmentModel> GetMasterDAC()
        {
            return new GenericDAC<InventoryAdjustment, InventoryAdjustmentModel>("ID", false, "Date DESC");
        }

        protected override GenericDAC<v_InventoryAdjustment, InventoryAdjustmentModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_InventoryAdjustment, InventoryAdjustmentModel>("ID", false, "Date DESC");
        }

        public override void Create(InventoryAdjustmentModel header, List<InventoryAdjustmentDetailModel> details)
        {
            header.Code = GetInventoryAdjustmentCode(header);

            base.Create(header, details);

            //UpdateQuantities(header.ID, true);
            //using (TransactionScope trans = new TransactionScope())
            //{
                

                //trans.Complete();
            //}
        }

        public override void Update(InventoryAdjustmentModel header, List<InventoryAdjustmentDetailModel> details)
        {
            header.HasCounted = true;

            base.Update(header, details);

            //UpdateQuantities(header.ID, true);
            //using (TransactionScope trans = new TransactionScope())
            //{
               

            //    trans.Complete();
            //}
        }

        //public void UndoQuantities(long documentID)
        //{
        //    var inventoryAdjustment = RetrieveByID(documentID);

        //    if (inventoryAdjustment.CreatedBy != "MIGRATION" || inventoryAdjustment.HasCounted)
        //        UpdateQuantities(documentID, false);
        //}
        public void CopyTransaction(InventoryAdjustmentModel header, long inventoryAdjustmentID)
        {
            var inventoryAdjustment = RetrieveByID(inventoryAdjustmentID);
            var inventoryAdjustmentDetails = RetrieveDetails(inventoryAdjustmentID);

            ObjectHelper.CopyProperties(inventoryAdjustment, header);

            header.Status = (int)MPL.DocumentStatus.New;

            var details = new List<InventoryAdjustmentDetailModel>();

            foreach (var inventoryAdjustmentDetail in inventoryAdjustmentDetails)
            {
                var detail = new InventoryAdjustmentDetailModel();
                ObjectHelper.CopyProperties(inventoryAdjustmentDetail, detail);
                details.Add(detail);
            }

            header.Details = details;
        }

        public void Validate(InventoryAdjustmentModel obj, List<InventoryAdjustmentDetailModel> details)
        {
            var productIDs = new List<long>();

            foreach (var detail in details)
            {
                if (productIDs.Contains(detail.ProductID))
                    throw new Exception("Cannot adjust the same item more than once for the same location");
                else
                    productIDs.Add(detail.ProductID);

                if (detail.ProductID == 0)
                    throw new Exception("Product not chosen");

                //if (detail.Quantity == 0)
                //    throw new Exception("Adjustment quantity cannot be zero");

                //if (detail.NewQty == 0)
                //    throw new Exception("New quantity cannot be zero");
                if (Math.Round(detail.QtyOnHandHidden + detail.Quantity, 0) < 0)
                    throw new Exception("New quantity cannot less than zero");
            }
        }

        public void Approve(long inventoryAdjustmentID, string userName)
        {
            var inventoryAdjustment = RetrieveByID(inventoryAdjustmentID);

            var details = RetrieveDetails(inventoryAdjustmentID);

            inventoryAdjustment.Status = (int)MPL.DocumentStatus.Approved;

            inventoryAdjustment.ApprovedBy = userName;

            inventoryAdjustment.ApprovedDate = DateTime.Now;

            using (TransactionScope trans = new TransactionScope())
            {
                UpdateQuantities(inventoryAdjustment.ID, true);

                Update(inventoryAdjustment);

                this.IncreaseLog(inventoryAdjustment, details);

                trans.Complete();
            }

        }

        public void Void(long inventoryAdjustmentID, string voidRemarks, string userName)
        {
            var inventoryAdjustment = RetrieveByID(inventoryAdjustmentID);
            var details = RetrieveDetails(inventoryAdjustmentID);
            var oldStatus = inventoryAdjustment.Status;
            inventoryAdjustment.Status = (int)MPL.DocumentStatus.Void;
            inventoryAdjustment.VoidRemarks = voidRemarks;
            inventoryAdjustment.ApprovedBy = userName;
            inventoryAdjustment.ApprovedDate = DateTime.Now;

            using (TransactionScope trans = new TransactionScope())
            {
                UpdateQuantities(inventoryAdjustmentID, false);

                this.DecreaseLog(inventoryAdjustment, details);

                Update(inventoryAdjustment);

                trans.Complete();
            }
        }

        #region Notification

        //public int RetrieveUnapprovedPurchaseOrderCount()
        //{
        //    return new ABCAPOSDAC().RetrieveUnapprovedInventoryAdjustmentCount();
        //}

        //public int RetrieveThisMonthInventoryAdjustmentCount()
        //{
        //    return new ABCAPOSDAC().RetrieveThisMonthInventoryAdjustmentCount();
        //}

        #endregion

    }
}
