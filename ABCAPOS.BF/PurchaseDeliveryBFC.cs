using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using ABCAPOS.DA;
using ABCAPOS.Models;
using System.Transactions;
using ABCAPOS.EDM;
using MPL.Business;

namespace ABCAPOS.BF
{
    public class PurchaseDeliveryBFC : MasterDetailBFC<PurchaseDelivery, v_PurchaseDelivery, PurchaseDeliveryDetail, v_PurchaseDeliveryDetail, PurchaseDeliveryModel, PurchaseDeliveryDetailModel>
    {
        #region PostingTableLog

        public void CreateLog(PurchaseDeliveryModel header,List<PurchaseDeliveryDetailModel>details)
        {
            try
            {
                var LogHeader = new LogModel();
                var LogDetails = new List<LogDetailModel>();

                LogHeader.Date = header.Date;
                LogHeader.WarehouseID = header.WarehouseID;
                LogHeader.DocType = (int)DocType.PurchaseDelivery;

                foreach (var detail in details)
                {
                    var LogDetail = new LogDetailModel();
                    var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                    var baseQuantity = detail.Quantity * unitRate;

                    var ContainerHeader = new ContainerModel();
                    ContainerHeader.ProductID = detail.ProductID;
                    ContainerHeader.WarehouseID = header.WarehouseID;
                    ContainerHeader.Qty = baseQuantity;
                    ContainerHeader.Price = Convert.ToDouble(detail.AssetPrice);

                    new ContainerBFC().Create(ContainerHeader);

                    LogDetail.ContainerID = ContainerHeader.ID;
                    LogDetail.ProductID = detail.ProductID;
                    LogDetail.MovingInQty = baseQuantity;
                    //LogDetail.MovingInValue = Convert.ToDouble(detail.AssetPrice);
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

        private void UpdateLog(PurchaseDeliveryModel header, List<PurchaseDeliveryDetailModel> details)
        {
            try
            {
                var LogHeader = new LogModel();
                var Log = new LogBFC().RetrieveByID(header.LogID);
                if (Log != null)
                {
                    LogHeader.Date = header.Date;
                    LogHeader.WarehouseID = header.WarehouseID;
                    LogHeader.DocType = (int)DocType.PurchaseDelivery;

                    new LogBFC().Update(LogHeader);

                    var itemNo = 1;

                    foreach (var detail in details)
                    {

                        var LogDetail = new LogDetailModel();
                        var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                        var baseQuantity = detail.Quantity * unitRate;

                        var container = new ContainerBFC().RetreiveByLogIDproductIDdocType(header.LogID, detail.ProductID, (int)DocType.PurchaseDelivery);
                        if (container != null)
                        {

                            var containerDetail = new ContainerBFC().RetreiveByContainerIDProductIDWarehouseID(container.ContainerID, detail.ProductID, header.WarehouseID);

                            if (containerDetail != null)
                            {
                                if (baseQuantity >= container.Pemakaian)
                                {
                                    var containerHeader = new ContainerModel();
                                    containerHeader.ID = container.ContainerID;
                                    containerHeader.ProductID = detail.ProductID;
                                    containerHeader.WarehouseID = header.WarehouseID;
                                    containerHeader.Qty = baseQuantity - container.Pemakaian;
                                    containerHeader.Price = Convert.ToDouble(detail.AssetPrice);
                                    new ContainerBFC().Update(containerHeader);

                                    new ABCAPOSDAC().DeleteLogByLogIDContainerIDProductID(header.LogID, container.ContainerID, detail.ProductID);
                                    LogDetail.LogID = header.LogID;
                                    LogDetail.ItemNo = itemNo++;
                                    LogDetail.ContainerID = container.ContainerID;
                                    LogDetail.ProductID = detail.ProductID;
                                    LogDetail.MovingInQty = baseQuantity - container.Pemakaian;
                                    //LogDetail.MovingInValue = Convert.ToDouble(detail.AssetPrice);
                                    new ABCAPOSDAC().CreateLog(LogDetail);
                                }
                                else
                                {
                                    throw new Exception("Jumlah qty yg diedit kurang dari barang yg sudah terpakai");
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

        public void VoidLog(PurchaseDeliveryModel header, List<PurchaseDeliveryDetailModel> details)
        {
            try
            {
                var LogHeader = new LogModel();
                var LogDetails = new List<LogDetailModel>();
                var Log = new LogBFC().RetrieveByID(header.LogID);
                if (Log != null)
                {
                    var itemNo = 1;
                    foreach (var detail in details)
                    {
                        var LogDetail = new LogDetailModel();
                        var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                        var baseQuantity = detail.Quantity * unitRate;

                        var container = new ContainerBFC().RetreiveByLogIDproductIDdocType(header.LogID, detail.ProductID, (int)DocType.PurchaseDelivery);

                        if (container != null)
                        {
                            var containerDetail = new ContainerBFC().RetreiveByContainerIDProductIDWarehouseID(container.ContainerID, detail.ProductID, header.WarehouseID);
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
                                    containerHeader.WarehouseID = header.WarehouseID;
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
                    LogHeader.ID = header.LogID;
                    LogHeader.WarehouseID = header.WarehouseID;
                    LogHeader.Date = header.Date;
                    LogHeader.DocType = (int)MPL.DocumentStatus.Void;
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

        public string GetPurchaseDeliveryCode(PurchaseDeliveryModel header)
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var PurchaseDeliveryPrefix = "";

            if (prefixSetting != null)
                PurchaseDeliveryPrefix = prefixSetting.PurchaseDeliveryPrefix;

            var warehouse = new WarehouseBFC().RetrieveByID(header.WarehouseID);
            var year = DateTime.Now.Year.ToString().Substring(2, 2);

            var prefix = PurchaseDeliveryPrefix + year + "-" + warehouse.Code + "-";

            var code = new ABCAPOSDAC().RetrievePurchaseDeliveryMaxCode(prefix, 7);

            return code;
        }

        protected override GenericDetailDAC<PurchaseDeliveryDetail, PurchaseDeliveryDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<PurchaseDeliveryDetail, PurchaseDeliveryDetailModel>("PurchaseDeliveryID", "ItemNo", false);
        }

        protected override GenericDetailDAC<v_PurchaseDeliveryDetail, PurchaseDeliveryDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_PurchaseDeliveryDetail, PurchaseDeliveryDetailModel>("PurchaseDeliveryID", "ItemNo", false);
        }

        protected override GenericDAC<PurchaseDelivery, PurchaseDeliveryModel> GetMasterDAC()
        {
            return new GenericDAC<PurchaseDelivery, PurchaseDeliveryModel>("ID", false, "Date DESC");
        }

        protected override GenericDAC<v_PurchaseDelivery, PurchaseDeliveryModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_PurchaseDelivery, PurchaseDeliveryModel>("ID", false, "Date DESC");
        }

        public List<PurchaseDeliveryModel> RetrieveByPOID(long purchaseOrderID)
        {
            return new ABCAPOSDAC().RetrievePDByPOID(purchaseOrderID);
        }

        public List<PurchaseDeliveryModel> RetrieveByBOID(long bookingOrderID)
        {
            var poList = new PurchaseOrderBFC().RetrieveByBOID(bookingOrderID);
            var pdList = new List<PurchaseDeliveryModel>();

            foreach (var po in poList)
            {
                var pdList2 = new ABCAPOSDAC().RetrievePDByPOID(po.ID);
                pdList.AddRange(pdList2);
            }

            return pdList;
        }

        public override void Create(PurchaseDeliveryModel header, List<PurchaseDeliveryDetailModel> details)
        {
            header.Code = GetPurchaseDeliveryCode(header);

            using (TransactionScope trans = new TransactionScope())
            {
                this.CreateLog(header, details);

                base.Create(header, details);
                OnUpdated(header.PurchaseOrderID);

                PostAccounting(header.ID);

                trans.Complete();
            }
            OnApproved(header.ID, header.CreatedBy);
            new PurchaseOrderBFC().UpdateStatus(header.PurchaseOrderID);
        }

        public override void Update(PurchaseDeliveryModel header, List<PurchaseDeliveryDetailModel> details)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                

                //Please dont change the location after base.Update
                updateFromOldQty(header, details);

                this.UpdateLog(header, details);

                base.Update(header, details);
                //OnEditApproved(header.ID, header.ModifiedBy);

                OnUpdated(header.PurchaseOrderID);

                PostAccounting(header.ID);

                trans.Complete();
            }
            new PurchaseOrderBFC().UpdateStatus(header.PurchaseOrderID);
        }

        public void Void(long purchaseDeliveryID, string voidRemarks, string userName)
        {
            var purchaseDelivery = RetrieveByID(purchaseDeliveryID);
            var oldStatus = purchaseDelivery.Status;
            

            using (TransactionScope trans = new TransactionScope())
            {
                OnVoid(purchaseDeliveryID, voidRemarks, userName);

                purchaseDelivery.Status = (int)MPL.DocumentStatus.Void;
                purchaseDelivery.VoidRemarks = voidRemarks;
                purchaseDelivery.ApprovedBy = userName;
                purchaseDelivery.ApprovedDate = DateTime.Now;

                Update(purchaseDelivery);

                new PurchaseOrderBFC().UpdateStatus(purchaseDelivery.PurchaseOrderID);

                PostAccounting(purchaseDeliveryID);

                OnUpdated(purchaseDelivery.PurchaseOrderID);

                trans.Complete();
            }
        }

        public void CreateByPurchaseOrder(PurchaseDeliveryModel pd, long poID)
        {
            var purchaseOrder = new PurchaseOrderBFC().RetrieveByID(poID);

            if (purchaseOrder != null)
            {
                pd.Code = SystemConstants.autoGenerated;
                // GetPurchaseDeliveryCode();

                pd.PurchaseOrderID = poID;
                pd.PurchaseOrderCode = purchaseOrder.Code;
                pd.PurchaseOrderDate = purchaseOrder.Date;
                pd.PurchaseOrderTitle = purchaseOrder.Title;
                pd.VendorCode = purchaseOrder.VendorCode;
                pd.VendorName = purchaseOrder.VendorName;
                pd.POSupplierNo = purchaseOrder.POSupplierNo;
                pd.WarehouseID = purchaseOrder.WarehouseID;


                var warehouse = new WarehouseBFC().RetrieveByID(purchaseOrder.WarehouseID);
                pd.WarehouseName = warehouse.Name;

                var department = new DepartmentBFC().RetrieveByID(purchaseOrder.DepartmentID);
                if (department != null)
                    pd.DepartmentName = department.DepartmentDesc;

                pd.CurrencyID = purchaseOrder.CurrencyID;
                pd.CurrencyName = purchaseOrder.CurrencyName;
                pd.ExchangeRate = purchaseOrder.ExchangeRate;

                var poDetails = new PurchaseOrderBFC().RetrieveDetails(poID);
                var pdDetails = new List<PurchaseDeliveryDetailModel>();

                foreach (var poDetail in poDetails)
                {
                    if (poDetail.OutstandingQuantity > 0)
                    {
                        var detail = new PurchaseDeliveryDetailModel();

                        detail.PurchaseOrderItemNo = poDetail.ItemNo;
                        detail.ProductID = poDetail.ProductID;
                        detail.Barcode = poDetail.Barcode;
                        detail.ProductCode = poDetail.ProductCode;
                        detail.ProductName = poDetail.ProductName;
                        detail.ConversionName = poDetail.ConversionName;
                        detail.ConversionID = poDetail.ConversionID;
                        detail.QtyPO = Convert.ToDouble(poDetail.Quantity);
                        detail.QtyReceive = Convert.ToDouble(poDetail.CreatedPDQuantity);
                        detail.QtyRemain = Convert.ToDouble(poDetail.Quantity - poDetail.CreatedPDQuantity);
                        detail.Quantity = Convert.ToDouble(poDetail.Quantity - poDetail.CreatedPDQuantity);
                        detail.StrQuantity = detail.Quantity.ToString();
                        detail.Remarks = poDetail.Remarks;
                        detail.AssetPrice = poDetail.AssetPrice;

                        var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                        var itemLoc = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductID, pd.WarehouseID);
                        if (itemLoc != null)
                            detail.StockQty = itemLoc.QtyOnHand / unitRate;
                        else
                            detail.StockQty = 0;

                        var product = new ProductBFC().RetrieveByID(detail.ProductID);
                        if (product.UseBin)
                        {
                            detail.BinID = new BinBFC().RetrieveDefaultBinID(product.ID, pd.WarehouseID);
                        }

                        pdDetails.Add(detail);
                    }
                }

                pd.Details = pdDetails;
            }

        }

        public void Validate(PurchaseDeliveryModel pd, List<PurchaseDeliveryDetailModel> pdDetails)
        {
            var obj = RetrieveByID(pd.ID);
            var poDetails = new PurchaseOrderBFC().RetrieveDetails(pd.PurchaseOrderID);

            var extDetails = new List<PurchaseDeliveryDetailModel>();

            if (obj != null)
                extDetails = RetrieveDetails(obj.ID);

            foreach (var pdDetail in pdDetails)
            {
                if (!string.IsNullOrEmpty(pdDetail.StrQuantity))
                    pdDetail.Quantity = Convert.ToDouble(pdDetail.StrQuantity);
                else
                    pdDetail.Quantity = 0;

                if (pdDetail.Quantity == 0)
                {
                    var product = new ProductBFC().RetrieveByID(pdDetail.ProductID);

                    throw new Exception("Product Qty " + product.Code + " cannot be zero");
                }

                var query = from i in poDetails
                            where i.ProductID == pdDetail.ProductID && i.ItemNo == pdDetail.PurchaseOrderItemNo
                            select i;

                var poDetail = query.FirstOrDefault();
                var pdQty = pdDetail.Quantity;

                //if (obj != null)
                //{
                //    var extDetail = (from i in extDetails
                //                     where i.ProductID == pdDetail.ProductID && i.ItemNo == pdDetail.PurchaseOrderItemNo
                //                     select i).FirstOrDefault();

                //    poDetail.CreatedPDQuantity -= extDetail.Quantity;
                //}

                if (poDetail.OutstandingQuantity < pdQty)
                    throw new Exception("Jumlah Penerimaan tidak boleh lebih dari Remaining");

            }
        }

        public void UpdateValidation(PurchaseDeliveryModel pd, List<PurchaseDeliveryDetailModel> newDetails)
        {
            var obj = RetrieveByID(pd.ID);
            var poDetails = new PurchaseOrderBFC().RetrieveDetails(pd.PurchaseOrderID);

            var extDetails = new List<PurchaseDeliveryDetailModel>();

            if (obj != null)
                extDetails = RetrieveDetails(obj.ID);

            foreach (var newDetail in newDetails)
            {
                newDetail.Remarks = "";
                if (!string.IsNullOrEmpty(newDetail.StrQuantity))
                    newDetail.Quantity = Convert.ToDouble(newDetail.StrQuantity);
                else
                    newDetail.Quantity = 0;

                var poData = from i in poDetails
                            where i.ProductID == newDetail.ProductID && i.ItemNo == newDetail.PurchaseOrderItemNo
                            select i;

                var oldData = from i in extDetails
                              where i.PurchaseOrderItemNo == newDetail.PurchaseOrderItemNo
                              select i;

                var poDetail = poData.FirstOrDefault();
                var oldDetail = oldData.FirstOrDefault();

                if (poDetail.OutstandingQuantity + oldDetail.Quantity < newDetail.Quantity)
                {
                    throw new Exception("Jumlah Penerimaan baru tidak boleh lebih dari Remaining");
                }
            }

            
        }

        public void OnUpdated(long poID)
        {
            var purchaseOrder = new PurchaseOrderBFC().RetrieveByID(poID);

            if (purchaseOrder != null)
            {
                var pdDetails = RetrieveDetailsByPOID(purchaseOrder.ID);
                var poDetails = new PurchaseOrderBFC().RetrieveDetails(purchaseOrder.ID);

                var fulfilledCount = 0;

                foreach (var poDetail in poDetails)
                {
                    if (poDetail.Quantity == poDetail.CreatedPDQuantity)
                        fulfilledCount += 1;
                }

                if (pdDetails.Count == 0)
                    purchaseOrder.HasPD = false;
                else
                    purchaseOrder.HasPD = true;

                if (fulfilledCount == poDetails.Count)
                {
                    purchaseOrder.IsPDFulfilled = true;
                }
                else
                {
                    purchaseOrder.IsPDFulfilled = false;
                }

                new PurchaseOrderBFC().Update(purchaseOrder);
                new PurchaseOrderBFC().UpdateDetails(purchaseOrder.ID, poDetails);
                //new PurchaseDeliveryBFC().Update(pd);
            }
            //var pd = RetrieveByID(pdID);

            //if (pd != null)
            //{
               
            //}
        }

        public void OnApproved(long purchaseDeliveryID, string userName)
        {
            var purchaseDelivery = RetrieveByID(purchaseDeliveryID);
            var purchaseOrder = new PurchaseOrderBFC().RetrieveByID(purchaseDelivery.PurchaseOrderID);

            if (purchaseDelivery != null)
            {
                var purchaseDeliveryDetails = RetrieveDetails(purchaseDeliveryID);

                foreach (var pdDetail in purchaseDeliveryDetails)
                {
                    var unitRate = new ProductBFC().GetUnitRate(pdDetail.ConversionID);
                    var baseQuantity = pdDetail.Quantity * unitRate;
                    var itemLocation = new ItemLocationBFC().RetrieveByProductIDWarehouseID(pdDetail.ProductID, purchaseDelivery.WarehouseID);
                    if (itemLocation != null)
                    {
                        itemLocation.QtyAvailable += baseQuantity;
                        itemLocation.QtyOnHand += baseQuantity;

                        new ItemLocationBFC().Update(itemLocation);
                    }
                    else
                    {
                        var newitemLocation = new ItemLocationModel();
                        newitemLocation.ProductID = pdDetail.ProductID;
                        newitemLocation.WarehouseID = purchaseDelivery.WarehouseID;
                        newitemLocation.QtyOnHand = newitemLocation.QtyAvailable = baseQuantity;
                        new ItemLocationBFC().Create(newitemLocation);
                    }
                    if (pdDetail.BinID != null || pdDetail.BinID != 0)
                    {
                        var binProduct = new BinProductWarehouseBFC().Retrieve(pdDetail.BinID, pdDetail.ProductID);
                        if (binProduct != null)
                        {
                            binProduct.Quantity += baseQuantity;
                            new BinProductWarehouseBFC().Update(binProduct);
                        }
                    }

                }
            }
        }

        public void voidFromOldQty(PurchaseDeliveryModel header, List<PurchaseDeliveryDetailModel> details)
        {
            var oldDetails = RetrieveDetails(header.ID);

            foreach (var detail in details)
            {
                detail.SelisihQuantity = -detail.Quantity;
            }

            UpdatePDItemLocationBin(header, details);

        }

        public void OnVoid(long purchaseDeliveryID, string voidRemarks, string userName)
        {
            var purchaseDelivery = RetrieveByID(purchaseDeliveryID);
            var purchaseDeliveryDetails = RetrieveDetails(purchaseDeliveryID);

            this.VoidLog(purchaseDelivery, purchaseDeliveryDetails);

            voidFromOldQty(purchaseDelivery, purchaseDeliveryDetails);
        }

        public void UpdatePDItemLocationBin(PurchaseDeliveryModel header, List<PurchaseDeliveryDetailModel> details)
        {

            foreach (var detail in details)
            {
                var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                var baseSelisihQuantity = detail.SelisihQuantity * unitRate;
                var itemLocation = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductID, header.WarehouseID);
                if (itemLocation != null)
                {
                    itemLocation.QtyAvailable += baseSelisihQuantity;
                    itemLocation.QtyOnHand += baseSelisihQuantity;

                    new ItemLocationBFC().Update(itemLocation);
                }
              
                if (detail.BinID != null || detail.BinID != 0)
                {
                    var binProduct = new BinProductWarehouseBFC().Retrieve(detail.BinID, detail.ProductID);
                    if (binProduct != null)
                    {
                        binProduct.Quantity += baseSelisihQuantity;
                        new BinProductWarehouseBFC().Update(binProduct);
                    }
                }
            }
        }

        public void updateFromOldQty(PurchaseDeliveryModel header, List<PurchaseDeliveryDetailModel> details)
        {
            var oldDetails = RetrieveDetails(header.ID);

            foreach (var detail in details)
            {
                var oldDetail = from i in oldDetails
                                where i.ProductID == detail.ProductID && i.ItemNo == detail.ItemNo
                                select i;

                var oldPdDetail = oldDetail.FirstOrDefault();

                detail.SelisihQuantity = detail.Quantity - oldPdDetail.Quantity;


            }
            UpdatePDItemLocationBin(header, details);
        }

        public List<PurchaseDeliveryDetailModel> RetrieveDetailsByPOID(long poID)
        {
            return new ABCAPOSDAC().RetrievePurchaseDeliveryDetailByPOID(poID);
        }

        #region Post to Accounting Result
        private void CreateAccountingResult(long purchaseDeliveryID)
        {
            var purchaseDelivery = RetrieveByID(purchaseDeliveryID);
            var purchaseDeliveryDetails = RetrieveDetails(purchaseDeliveryID);

            var purchaseOrderDetails = new PurchaseOrderBFC().RetrieveDetails(purchaseDelivery.PurchaseOrderID);

            decimal rawMaterialTotalAmount = 0;
            decimal extraMaterialTotalAmount = 0;
            decimal otherMaterialTotalAmount = 0;

            var accountingResultList = new List<AccountingResultModel>();

            foreach (var purchaseDeliveryDetail in purchaseDeliveryDetails)
            {
                var poDetail = (from i in purchaseOrderDetails
                                where i.ProductID == purchaseDeliveryDetail.ProductID
                                select i).FirstOrDefault();

                if (poDetail != null)
                {
                    var product = new ProductBFC().RetrieveByID(purchaseDeliveryDetail.ProductID);

                    if (product.ItemTypeID == (int)ItemTypeProduct.RawMaterial)
                        rawMaterialTotalAmount += Convert.ToDecimal(purchaseDeliveryDetail.Quantity) * (poDetail.Total / Convert.ToDecimal(poDetail.Quantity));
                    else if (product.ItemTypeID == (int)ItemTypeProduct.Supporting)
                        extraMaterialTotalAmount += Convert.ToDecimal(purchaseDeliveryDetail.Quantity) * (poDetail.Total / Convert.ToDecimal(poDetail.Quantity));
                    else if (product.ItemTypeID == (int)ItemTypeProduct.NonInventory)
                        otherMaterialTotalAmount += Convert.ToDecimal(purchaseDeliveryDetail.Quantity) * (poDetail.Total / Convert.ToDecimal(poDetail.Quantity));
                }
            }

            rawMaterialTotalAmount = rawMaterialTotalAmount * purchaseDelivery.ExchangeRate;
            extraMaterialTotalAmount = extraMaterialTotalAmount * purchaseDelivery.ExchangeRate;
            otherMaterialTotalAmount = otherMaterialTotalAmount * purchaseDelivery.ExchangeRate;

            accountingResultList = AddToAccountingResultList(accountingResultList, purchaseDelivery, (int)PostingAccount.PersediaanBahanBaku, AccountingResultType.Debit, rawMaterialTotalAmount, "bahan baku item receipt " + purchaseDelivery.Code);

            accountingResultList = AddToAccountingResultList(accountingResultList, purchaseDelivery, (int)PostingAccount.PersediaanBahanPembantu, AccountingResultType.Debit, extraMaterialTotalAmount, "Bahan pembantu item receipt " + purchaseDelivery.Code);

            accountingResultList = AddToAccountingResultList(accountingResultList, purchaseDelivery, (int)PostingAccount.CadanganBiayaLainnya, AccountingResultType.Debit, otherMaterialTotalAmount, "Cadangan biaya lainnya item receipt " + purchaseDelivery.Code);

            decimal totalAmount = rawMaterialTotalAmount + extraMaterialTotalAmount + otherMaterialTotalAmount;
            accountingResultList = AddToAccountingResultList(accountingResultList, purchaseDelivery, (int)PostingAccount.HutangDagangYangBelumDifakturkan, AccountingResultType.Credit, totalAmount, "Hutang dagang yang belum difakturkan item receipt " + purchaseDelivery.Code);

            new AccountingResultBFC().Posting(accountingResultList);
        }

        private List<AccountingResultModel> AddToAccountingResultList(List<AccountingResultModel> resultList, PurchaseDeliveryModel obj, long accountID, AccountingResultType resultType, decimal amount, string remarks)
        {
            if (amount > 0)
            {
                var account = new AccountBFC().RetrieveByID(accountID);
                var result = new AccountingResultModel();

                result.DocumentID = obj.ID;
                result.DocumentType = (int)AccountingResultDocumentType.PurchaseDelivery;
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

        public void PostAccounting(long purchaseDeliveryID)
        {
            var purchaseDelivery = RetrieveByID(purchaseDeliveryID);

            new ABCAPOSDAC().DeleteAccountingResults(purchaseDeliveryID, AccountingResultDocumentType.PurchaseDelivery);

            if (purchaseDelivery != null)
            {
                if (purchaseDelivery.Status != (int)MPL.DocumentStatus.Void)
                    CreateAccountingResult(purchaseDeliveryID);
            }

           
        }


        #endregion
    }
}
