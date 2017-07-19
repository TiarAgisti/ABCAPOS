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
    public class TransferOrderBFC : MasterDetailBFC<TransferOrder, v_TransferOrder, TransferOrderDetail, v_TransferOrderDetail, TransferOrderModel, TransferOrderDetailModel>
    {
        private void UpdateQuantities(TransferOrderModel transferOrder, bool isUndo)
        {
            var details = RetrieveDetails(transferOrder.ID);

            foreach (var detail in details)
            {

                var qtyCustomUnit = detail.Quantity;
                var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                var qtyBaseUnit = detail.Quantity * unitRate;

                var itemLocation = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductID, transferOrder.FromWarehouseID);
                if (itemLocation != null)
                {
                    if (isUndo)
                        itemLocation.QtyAvailable += qtyBaseUnit;
                    else
                        itemLocation.QtyAvailable -= qtyBaseUnit;
                    //else if (fulfillmentStatus == DeliveryOrderStatus.Void)
                    //{
                    //    itemLocation.QtyAvailable -= qtyBaseUnit;
                    //    itemLocation.QtyOnHand += qtyBaseUnit;
                    //}
                    new ItemLocationBFC().Update(itemLocation);
                }
                else
                {
                    if (isUndo)
                        new ItemLocationBFC().Create(detail.ProductID,
                            transferOrder.FromWarehouseID,
                            0,
                            -qtyBaseUnit);
                    else
                        new ItemLocationBFC().Create(detail.ProductID,
                            transferOrder.FromWarehouseID,
                            0,
                            qtyBaseUnit);

                    //else if (fulfillmentStatus == DeliveryOrderStatus.Void)
                    //{
                    //    itemLocation.QtyAvailable -= qtyBaseUnit;
                    //    itemLocation.QtyOnHand += qtyBaseUnit;
                    //}
                }
            }
        }

        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetTransferOrderCode(TransferOrderModel header)
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var transferOrderPrefix = "";

            if (prefixSetting != null)
                transferOrderPrefix = prefixSetting.TransferPrefix;

            var warehouse = new WarehouseBFC().RetrieveByID(header.FromWarehouseID);
            var year = DateTime.Now.Year.ToString().Substring(2, 2);

            var prefix = transferOrderPrefix + year + "-" + warehouse.Code + "-";

            var code = new ABCAPOSDAC().RetrieveTransferOrderMaxCode(prefix, 7);

            return code;
        }

        protected override GenericDetailDAC<TransferOrderDetail, TransferOrderDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<TransferOrderDetail, TransferOrderDetailModel>("TransferOrderID", "LineSequenceNumber", false);
        }

        protected override GenericDetailDAC<v_TransferOrderDetail, TransferOrderDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_TransferOrderDetail, TransferOrderDetailModel>("TransferOrderID", "LineSequenceNumber", false);
        }

        protected override GenericDAC<TransferOrder, TransferOrderModel> GetMasterDAC()
        {
            return new GenericDAC<TransferOrder, TransferOrderModel>("ID", false, "Date DESC");
        }

        protected override GenericDAC<v_TransferOrder, TransferOrderModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_TransferOrder, TransferOrderModel>("ID", false, "Date DESC");
        }

        public void CopyTransaction(TransferOrderModel header, long transferOrderID)
        {
            var transferOrder = RetrieveByID(transferOrderID);
            var transferOrderDetails = RetrieveDetails(transferOrderID);

            ObjectHelper.CopyProperties(transferOrder, header);

            header.Status = (int)TransferOrderStatus.New;

            var details = new List<TransferOrderDetailModel>();

            foreach (var transferOrderDetail in transferOrderDetails)
            {
                var detail = new TransferOrderDetailModel();

                ObjectHelper.CopyProperties(transferOrderDetail, detail);

                details.Add(detail);
            }

            header.Details = details;
        }

        public void UpdateStatus(long transferOrderID, string userName)
        {
            var order = RetrieveByID(transferOrderID);

            if (order.Status == (int)TransferOrderStatus.New
                || order.Status == (int)TransferOrderStatus.Void)
                goto end;

            if (order.QtyDelivered < order.QtyOrdered)
            {
                order.Status = (int)TransferOrderStatus.PendingFulfillment;
                goto end;
            }
            else
            {
                order.Status = (int)TransferOrderStatus.PendingReceipt;
            }

            if (order.QtyReceived < order.QtyOrdered)
            {

                goto end;
            }
            else
            {
                order.Status = (int)TransferOrderStatus.Received;
            }

            end:
            order.ModifiedBy = userName;
            order.ModifiedDate = DateTime.Now;

            Update(order);
        }

        public void UpdateDetails(long toID, List<TransferOrderDetailModel> toDetails)
        {
            var dac = new ABCAPOSDAC();
            var transferOrder = RetrieveByID(toID);

            using (TransactionScope trans = new TransactionScope())
            {
                //base.Update(purchaseOrder, poDetails);
                GetDetailDAC().DeleteByParentID(toID);
                dac.CreateTODetails(toID, toDetails);
                //GetDetailDAC().Create(poID, poDetails);

                trans.Complete();
            }
        }

        public void Validate(TransferOrderModel obj, List<TransferOrderDetailModel> details)
        {
            if (details.Count <= 0)
                throw new Exception("Must have at least 1 item to transfer");

            // note: can transfer to and from same WarehouseID, ie: bin transfer

            foreach (var detail in details)
            {
                if (detail.ProductID == 0)
                    throw new Exception("Product not chosen");

                if (detail.Quantity == 0)
                    throw new Exception("Qty Product cannot be zero");

                //TODO: -if time - add warning if value in "AMOUNT" != (qty * transferPrice)
            }
        }

        public void Approve(long toID, string userName)
        {
            var transferOrder = RetrieveByID(toID);

            transferOrder.Status = (int)TransferOrderStatus.PendingFulfillment;
            transferOrder.ApprovedBy = userName;
            transferOrder.ApprovedDate = DateTime.Now;
            using (TransactionScope trans = new TransactionScope())
            {
                Update(transferOrder);
                UpdateQuantities(transferOrder, false);
                trans.Complete();
            }
        }

        public void UndoQuantities(long toID)
        {
            var transferOrder = RetrieveByID(toID);
            UpdateQuantities(transferOrder, true);
        }

        public void Void(long transferOrderID, string voidRemarks, string userName)
        {
            var transferOrder = RetrieveByID(transferOrderID);
            var oldStatus = transferOrder.Status;
            transferOrder.Status = (int)TransferOrderStatus.Void;
            transferOrder.VoidRemarks = voidRemarks;
            transferOrder.ApprovedBy = userName;
            transferOrder.ApprovedDate = DateTime.Now;

            using (TransactionScope trans = new TransactionScope())
            {
                Update(transferOrder);
                UndoQuantities(transferOrder.ID);
                //UpdateQuantities(transferOrder, false);
                trans.Complete();
            }
        }

        public int RetrieveUncreatedTransferDeliveryCount(List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveUncreatedTransferDeliveryTOCount(selectFilters);
        }

        public List<TransferOrderModel> RetrieveUncreatedTransferDelivery(int startIndex, int? amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            if (amount == null)
                amount = SystemConstants.ItemPerPage;

            return new ABCAPOSDAC().RetrieveUncreatedTransferDeliveryTO(startIndex, (int)amount, sortParameter, selectFilters);
        }

        public int RetrieveUncreatedTransferReceiptCount(List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveUncreatedTransferReceiptTOCount(selectFilters);
        }

        public List<TransferOrderModel> RetrieveUncreatedTransferReceipt(int startIndex, int? amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            if (amount == null)
                amount = SystemConstants.ItemPerPage;

            return new ABCAPOSDAC().RetrieveUncreatedTransferReceiptTO(startIndex, (int)amount, sortParameter, selectFilters);
        }

        public List<TransferOrderModel> RetrieveAutoComplete(string key)
        {
            return new ABCAPOSDAC().RetrieveTransferOrderAutoComplete(key);
        }

        public TransferOrderModel RetrieveByCode(string transferCode)
        {
            return new ABCAPOSDAC().RetrieveTransferOrderByCode(transferCode);
        }

        public TransferOrderDetailModel RetreiveQtyReceivedTransferOrder(long transferOrderID, long productID)
        {
            return new ABCAPOSDAC().RetreiveQtyReceivedTransferOrder(transferOrderID, productID);
        }

        public ABCAPOSReportEDSC.SalesOrderDTRow RetrievePrintOut(long transferOrderID)
        {
            return new ABCAPOSReportDAC().RetrieveTransferOrderPrintOut(transferOrderID);
        }

        public ABCAPOSReportEDSC.SalesOrderDetailDTDataTable RetrieveDetailPrintOut(long salesOrderID)
        {
            return new ABCAPOSReportDAC().RetrieveTransferOrderDetailPrintOut(salesOrderID);
        }

        public TransferOrderDetailModel RetrieveQtyReceivedTransferReceipt(long transferOrderID, int itemNo, long productID)
        {
            return new ABCAPOSDAC().RetrieveQtyReceivedTransferReceipt(transferOrderID, itemNo, productID);
        }

        public List<TransferOrderModel> RetrieveTransferOrder(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveTransferOrder(startIndex, amount, sortParameter, selectFilters);
        }

        public int RetrieveTransferOrderCount(List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveTransferOrderCount(selectFilters);
        }

        #region Notification

        public int RetrieveTransferOrderUnDelivery()
        {
            return new ABCAPOSDAC().RetrieveTransferOrderUnDelivery();
        }

        #endregion

        //public override void Create(TransferOrderModel header, List<TransferOrderDetailModel> details)
        //{
        //    header.Code = GetTransferOrderCode(header);

        //    using (TransactionScope trans = new TransactionScope())
        //    {
        //        base.Create(header, details);
        //    }
        //}

    }
}
