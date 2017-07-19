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
    public class TransferReceiptBFC : MasterDetailBFC<TransferReceipt, v_TransferReceipt, TransferReceiptDetail, v_TransferReceiptDetail, TransferReceiptModel, TransferReceiptDetailModel>
    {
        private void UpdateInventory(long transferReceiptID, string userName, bool isUndo)
        {
            var receipt = RetrieveByID(transferReceiptID);

            if (receipt == null)
                return;

            var transferOrder = new TransferOrderBFC().RetrieveByID(receipt.TransferOrderID);
            var receiptDetails = RetrieveDetails(transferReceiptID);

            foreach (var receiptDetail in receiptDetails)
            {
                var qtyCustomUnit = receiptDetail.Quantity;
                var unitRate = new ProductBFC().GetUnitRate(receiptDetail.ConversionID);
                var qtyBaseUnit = receiptDetail.Quantity * unitRate;
                var itemLocation = new ItemLocationBFC().RetrieveByProductIDWarehouseID(receiptDetail.ProductID, receipt.ToWarehouseID);
                if (itemLocation != null)
                {
                    if (!isUndo)
                    {
                        itemLocation.QtyAvailable += qtyBaseUnit;
                        itemLocation.QtyOnHand += qtyBaseUnit;
                    }
                    else
                    {
                        itemLocation.QtyAvailable -= qtyBaseUnit;
                        itemLocation.QtyOnHand -= qtyBaseUnit;
                    }
                    new ItemLocationBFC().Update(itemLocation);
                }
                else
                {
                    if (!isUndo)
                        new ItemLocationBFC().Create(receiptDetail.ProductID,
                                  receipt.ToWarehouseID,
                                  qtyBaseUnit,
                                  qtyBaseUnit);
                    else
                        new ItemLocationBFC().Create(receiptDetail.ProductID,
                                receipt.ToWarehouseID,
                                -qtyBaseUnit,
                                -qtyBaseUnit);
                }
                if (receiptDetail.BinID != null && receiptDetail.BinID != 0)
                {
                    var binProduct = new BinProductWarehouseBFC().Retrieve(receiptDetail.BinID, receiptDetail.ProductID);
                    if (binProduct != null)
                    {
                        if (!isUndo)
                            binProduct.Quantity += qtyBaseUnit;
                        else
                            binProduct.Quantity += qtyBaseUnit;

                        new BinProductWarehouseBFC().Update(binProduct);
                    }
                }
            }
        }

        private void validasiQtyReceipt(TransferReceiptModel header, List<TransferReceiptDetailModel> details)
        {
            if (header != null && details.Count > 0)
            {
                foreach (var detail in details)
                {
                    var check = new TransferOrderBFC().RetrieveQtyReceivedTransferReceipt(header.TransferOrderID, detail.TransferOrderItemNo, detail.ProductID);
                    if (check != null)
                    {
                        if (check.Quantity < check.QtyReceived)
                        {
                            throw new Exception("Qty terima " + detail.ProductName + " tidak boleh lebih dari qty out standing");
                        }
                    }
                }
            }
        }

        #region PostingTableLog
        public void CreateLog(TransferReceiptModel header, List<TransferReceiptDetailModel> details)
        {
            try
            {
                var LogHeader = new LogModel();
                var LogDetails = new List<LogDetailModel>();

                LogHeader.Date = header.Date;
                LogHeader.WarehouseID = header.ToWarehouseID;
                LogHeader.DocType = (int)DocType.TransferReceipts;

                foreach (var detail in details)
                {
                    var LogDetail = new LogDetailModel();
                    var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                    var baseQuantity = detail.Quantity * unitRate;
                    var ContainerHeader = new ContainerModel();

                    var ContainerPrice = new ContainerBFC().RetreiveByProductIDWarehouseID(detail.ProductID, header.FromWarehouseID);
                    if (ContainerPrice != null)
                    {
                        ContainerHeader.Price = ContainerPrice.Price;
                    }
                    else
                    {
                        ContainerHeader.Price = 0;
                    }
                 
                    ContainerHeader.ProductID = detail.ProductID;
                    ContainerHeader.WarehouseID = header.ToWarehouseID;
                    ContainerHeader.Qty = baseQuantity;
                   

                    new ContainerBFC().Create(ContainerHeader);

                    LogDetail.ContainerID = ContainerHeader.ID;
                    LogDetail.ProductID = detail.ProductID;
                    LogDetail.MovingInQty = baseQuantity;
                    //LogDetail.MovingInValue = ContainerPrice.Price;
                    LogDetails.Add(LogDetail);
                }

                //LogHeader.Details = LogDetails;
                new LogBFC().Create(LogHeader, LogDetails);

                header.LogID = LogHeader.ID;
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }


        private void UpdateLog(TransferReceiptModel header, List<TransferReceiptDetailModel> details)
        {
            try
            {
                this.VoidLog(header);
                this.CreateLog(header, details);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        public void VoidLog(TransferReceiptModel header)
        {
            try
            {
                var LogHeader = new LogModel();
                var LogDetails = new List<LogDetailModel>();
                var Log = new LogBFC().RetrieveByID(header.LogID);
                var details = new LogBFC().RetrieveDetails(header.LogID);
                if (Log != null)
                {
                    var itemNo = 1;
                    foreach (var detail in details)
                    {
                        var LogDetail = new LogDetailModel();

                        var container = new ContainerBFC().RetreiveByLogIDproductIDdocType(header.LogID, detail.ProductID, (int)DocType.TransferReceipts);

                        if (container != null)
                        {
                            var containerDetail = new ContainerBFC().RetreiveByContainerIDProductIDWarehouseID(container.ContainerID, detail.ProductID, header.ToWarehouseID);
                            if (containerDetail != null)
                            {
                                if (container.Pemakaian > 0)
                                {
                                    throw new Exception("Barang sudah ada yg terpakai,void tidak dapat dilakukan");
                                }
                                else if (container.Pemakaian == 0)
                                {
                                    var containerHeader = new ContainerModel();
                                    containerHeader.ID = container.ContainerID;
                                    containerHeader.ProductID = detail.ProductID;
                                    containerHeader.WarehouseID = header.ToWarehouseID;
                                    containerHeader.Qty = 0;
                                    containerHeader.Price = 0;
                                    new ContainerBFC().Update(containerHeader);

                                    new ABCAPOSDAC().DeleteLogByLogIDContainerIDProductID(header.LogID, container.ContainerID, detail.ProductID);
                                    LogDetail.LogID = header.LogID;
                                    LogDetail.ItemNo = itemNo++;
                                    LogDetail.ContainerID = container.ContainerID;
                                    LogDetail.ProductID = detail.ProductID;
                                    LogDetail.MovingInQty = 0;
                                    //LogDetail.MovingInValue = 0;
                                    new ABCAPOSDAC().CreateLog(LogDetail);
                                }
                            }
                        }
                    }
                }
                LogHeader.ID = header.LogID;
                LogHeader.WarehouseID = header.ToWarehouseID;
                LogHeader.Date = header.Date;
                LogHeader.DocType = (int)MPL.DocumentStatus.Void;
                new LogBFC().Update(LogHeader);
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

        public string GetTransferReceiptCode(TransferReceiptModel header)
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var transferReceiptPrefix = "";

            if (prefixSetting != null)
                transferReceiptPrefix = prefixSetting.TransferReceiptPrefix;

            var warehouse = new WarehouseBFC().RetrieveByID(header.FromWarehouseID);
            var year = DateTime.Now.Year.ToString().Substring(2, 2);

            var prefix = transferReceiptPrefix + year + "-" + warehouse.Code + "-";

            var code = new ABCAPOSDAC().RetrieveTransferReceiptMaxCode(prefix, 7);

            return code;
        }

        protected override GenericDetailDAC<TransferReceiptDetail, TransferReceiptDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<TransferReceiptDetail, TransferReceiptDetailModel>("TransferReceiptID", "ItemNo", false);
        }

        protected override GenericDetailDAC<v_TransferReceiptDetail, TransferReceiptDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_TransferReceiptDetail, TransferReceiptDetailModel>("TransferReceiptID", "ItemNo", false);
        }

        protected override GenericDAC<TransferReceipt, TransferReceiptModel> GetMasterDAC()
        {
            return new GenericDAC<TransferReceipt, TransferReceiptModel>("ID", false, "Date DESC");
        }

        protected override GenericDAC<v_TransferReceipt, TransferReceiptModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_TransferReceipt, TransferReceiptModel>("ID", false, "Date DESC");
        }

        public override void Create(TransferReceiptModel header, List<TransferReceiptDetailModel> details)
        {
            header.Code = GetTransferReceiptCode(header);

            using (TransactionScope trans = new TransactionScope())
            {
                this.CreateLog(header, details);

                base.Create(header, details);

                //this.validasiQtyReceipt(header, details);

                UpdateInventory(header.ID, header.CreatedBy, false);

                trans.Complete();
            }
            new TransferOrderBFC().UpdateStatus(header.TransferOrderID, header.CreatedBy);
        }

       

        public override void Update(TransferReceiptModel header, List<TransferReceiptDetailModel> details)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                UpdateInventory(header.ID, header.CreatedBy, true);

                this.UpdateLog(header, details);

                base.Update(header, details);

                UpdateInventory(header.ID, header.CreatedBy, false);

                new TransferOrderBFC().UpdateStatus(header.TransferOrderID, header.CreatedBy);

                trans.Complete();
            }
        }

        public void Void(long transferReceiptID, string voidRemarks, string userName)
        {
            var transferDelivery = RetrieveByID(transferReceiptID);
            var oldStatus = transferDelivery.Status;
            var details = RetrieveDetails(transferReceiptID);

            transferDelivery.Status = (int)MPL.DocumentStatus.Void;
            transferDelivery.VoidRemarks = voidRemarks;
            transferDelivery.ApprovedBy = "";
            transferDelivery.ApprovedDate = SystemConstants.UnsetDateTime;

            using (TransactionScope trans = new TransactionScope())
            {
                UpdateInventory(transferReceiptID, userName, true);

                Update(transferDelivery);

                this.VoidLog(transferDelivery);

                trans.Complete();
            }
        }

        public void OnVoid(long transferReceiptID, string voidRemarks, string userName)
        {
            // TODO: not implemented yet
            //throw new NotImplementedException();
            var transferReceipt = new TransferReceiptBFC().RetrieveByID(transferReceiptID);

            //var transferReceiptDetails = new TransferReceiptBFC().RetrieveDetails(transferReceiptID);

            UpdateInventory(transferReceipt.ID, transferReceipt.CreatedBy, true);
        }

        public void Validate(TransferReceiptModel receipt, List<TransferReceiptDetailModel> receiptDetails)
        {
            var originalTransferDetails = new TransferOrderBFC().RetrieveDetails(receipt.TransferOrderID);

            foreach (var receiptDetail in receiptDetails)
            {
                var originalTransferDetail = (from i in originalTransferDetails
                                              where i.ProductID == receiptDetail.ProductID &&
                                                   i.ItemNo == receiptDetail.TransferOrderItemNo
                                              select i).FirstOrDefault();
                if (receiptDetail.Quantity < originalTransferDetail.Quantity)
                    throw new Exception("Transfer orders cannot be partially fulfilled / received");
                else if (receiptDetail.Quantity > originalTransferDetail.Quantity)
                    throw new Exception("The amount spcified for " + receiptDetail.ProductCode + " is higher than the amount in the original Transfer Order.");
            }
        }

        public void PreparedByFromTransferDelivery(TransferReceiptModel receipt, long tranferDeliveryID)
        {
            var transferDelivery = new TransferDeliveryBFC().RetrieveByID(tranferDeliveryID);
            if (transferDelivery != null)
            {
                receipt.Code = SystemConstants.autoGenerated;// GetTransferReceiptCode();
                receipt.TransferOrderID = transferDelivery.TransferOrderID;
                receipt.Date = DateTime.Now;
                receipt.FromWarehouseID = transferDelivery.FromWarehouseID;
                receipt.FromWarehouseName = transferDelivery.FromWarehouseName;
                receipt.ToWarehouseID = transferDelivery.ToWarehouseID;
                receipt.ToWarehouseName = transferDelivery.ToWarehouseName;
                receipt.TransferOrderCode = transferDelivery.TransferOrderCode;
                receipt.SupplierRef = transferDelivery.Code;

                var transferDetails = new TransferDeliveryBFC().RetrieveDetails(tranferDeliveryID);
                var transferDeliveryDetails = new List<TransferReceiptDetailModel>();

                foreach (var transferDetail in transferDetails)
                {
                    var detail = new TransferReceiptDetailModel();
                    detail.TransferOrderItemNo = transferDetail.ItemNo;
                    detail.ProductID = transferDetail.ProductID;
                    detail.ProductCode = transferDetail.ProductCode;
                    detail.ProductName = transferDetail.ProductName;

                    var QtytransferOrder = new TransferOrderBFC().RetreiveQtyReceivedTransferOrder(transferDelivery.TransferOrderID, transferDetail.ProductID);
                    detail.QtyShipped = QtytransferOrder.Quantity - QtytransferOrder.QtyReceived;

                    detail.Quantity = detail.QtyShipped;

                    detail.ConversionName = transferDetail.ConversionName;
                    detail.ConversionID = transferDetail.ConversionID;
                    detail.UnitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                    detail.ToWarehouseName = transferDelivery.ToWarehouseName;

                    var itemLoc = new ItemLocationBFC().RetrieveByProductIDWarehouseID(transferDetail.ProductID, transferDelivery.ToWarehouseID);
                    if (itemLoc != null)
                        detail.StockQty = itemLoc.QtyOnHand / detail.UnitRate;
                    else
                        detail.StockQty = 0;

                    var product = new ProductBFC().RetrieveByID(detail.ProductID);
                    if (product.UseBin)
                    {
                        detail.BinID = new BinBFC().RetrieveDefaultBinID(product.ID, receipt.ToWarehouseID);
                    }

                    if (detail.QtyShipped > 0)
                        transferDeliveryDetails.Add(detail);

                    //transferDeliveryDetails.Add(detail);
                }
                receipt.Details = transferDeliveryDetails;
            }
        }

        public void PreFillFromTransferOrder(TransferReceiptModel receipt, long transferOrderID)
        {
            // assume Delivery contents = Transfer Order contents exactly
            var transferOrder = new TransferOrderBFC().RetrieveByID(transferOrderID);
            if (transferOrder == null)
                return;

            var deliveries = new TransferDeliveryBFC().RetrieveShippedByTransferOrderID(transferOrderID);
            if (deliveries == null || deliveries.Count == 0)
                return;

            receipt.Code = SystemConstants.autoGenerated;// GetTransferReceiptCode();
            receipt.TransferOrderID = transferOrderID;
            receipt.Date = DateTime.Now;
            receipt.FromWarehouseID = transferOrder.FromWarehouseID;
            receipt.FromWarehouseName = transferOrder.FromWarehouseName;
            receipt.ToWarehouseID = transferOrder.ToWarehouseID;
            receipt.ToWarehouseName = transferOrder.ToWarehouseName;
            receipt.TransferOrderCode = transferOrder.Code;

            //transferDelivery.PostingPeriod = transferOrder.PostingPeriod; TODO: fix Posting Period

            var transferDetails = new TransferOrderBFC().RetrieveDetails(transferOrderID);
            var transferDeliveryDetails = new List<TransferReceiptDetailModel>();

            foreach (var transferDetail in transferDetails)
            {
                var detail = new TransferReceiptDetailModel();
                detail.TransferOrderItemNo = transferDetail.ItemNo;
                detail.ProductID = transferDetail.ProductID;
                detail.ProductCode = transferDetail.ProductCode;
                detail.ProductName = transferDetail.ProductName;

                //var QtytransferOrder = new TransferOrderBFC().RetreiveQtyReceivedTransferOrder(transferOrderID, transferDetail.ProductID);
                detail.QtyShipped = transferDetail.Quantity - transferDetail.QtyReceived;

                detail.Quantity = detail.QtyShipped;

                detail.ConversionName = transferDetail.ConversionName;
                detail.ConversionID = transferDetail.ConversionID;
                detail.UnitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                detail.ToWarehouseName = transferOrder.ToWarehouseName;

                var itemLoc = new ItemLocationBFC().RetrieveByProductIDWarehouseID(transferDetail.ProductID, transferOrder.ToWarehouseID);
                if (itemLoc != null)
                    detail.StockQty = itemLoc.QtyOnHand / detail.UnitRate;
                else
                    detail.StockQty = 0;

                var product = new ProductBFC().RetrieveByID(detail.ProductID);
                if (product.UseBin)
                {
                    detail.BinID = new BinBFC().RetrieveDefaultBinID(product.ID, receipt.ToWarehouseID);
                }

                if(detail.QtyShipped > 0)
                    transferDeliveryDetails.Add(detail);

                //transferDeliveryDetails.Add(detail);
            }
            receipt.Details = transferDeliveryDetails;
        }

        public List<TransferReceiptModel> RetrieveByTransferOrderID(long toID)
        {
            return new ABCAPOSDAC().RetrieveTransferReceiptByTransferOrderID(toID);
        }

        public List<TransferReceiptDetailModel> RetrieveDetailsByTransferOrderID(long spkID)
        {
            return new ABCAPOSDAC().RetrieveTransferReceiptDetailByTransferOrderID(spkID);
        }

        /// <summary>
        /// Update Inventory (add to available and on hand)
        /// </summary>
        /// <param name="transferReceiptID"></param>
        /// <param name="userName"></param>
        /// <param name="isUndo">true if subtracting, false if Regular operation</param>
    }
}
