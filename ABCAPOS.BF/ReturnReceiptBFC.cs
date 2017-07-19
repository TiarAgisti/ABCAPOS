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
    public class ReturnReceiptBFC : MasterDetailBFC<ReturnReceipt, v_ReturnReceipt, ReturnReceiptDetail, v_ReturnReceiptDetail, ReturnReceiptModel, ReturnReceiptDetailModel>
    {
        #region PostingTableLog
        public void CreateLog(ReturnReceiptModel header, List<ReturnReceiptDetailModel> details)
        {
            try
            {
                var LogHeader = new LogModel();
                var LogDetails = new List<LogDetailModel>();

                LogHeader.Date = header.Date;
                LogHeader.WarehouseID = header.WarehouseID;
                LogHeader.DocType = (int)DocType.ReturnReceipt;

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

        private void UpdateLog(ReturnReceiptModel header, List<ReturnReceiptDetailModel> details)
        {
            try
            {
                var LogHeader = new LogModel();
                var Log = new LogBFC().RetrieveByID(header.LogID);
                if (Log != null)
                {
                    LogHeader.Date = header.Date;
                    LogHeader.WarehouseID = header.WarehouseID;
                    LogHeader.DocType = (int)DocType.ReturnReceipt;

                    new LogBFC().Update(LogHeader);

                    var itemNo = 1;

                    foreach (var detail in details)
                    {

                        var LogDetail = new LogDetailModel();
                        var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                        var baseQuantity = detail.Quantity * unitRate;

                        var container = new ContainerBFC().RetreiveByLogIDproductIDdocType(header.LogID, detail.ProductID, (int)DocType.ReturnReceipt);
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
                throw (ex);
            }

        }

        public void VoidLog(ReturnReceiptModel header, List<ReturnReceiptDetailModel> details)
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

                        var container = new ContainerBFC().RetreiveByLogIDproductIDdocType(header.LogID, detail.ProductID, (int)DocType.ReturnReceipt);

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
                throw (ex);
            }
        }
        #endregion

        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        #region Retrieve

        public string GetReturnReceiptCode()
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var ReturnReceiptPrefix = "";

            if (prefixSetting != null)
                ReturnReceiptPrefix = prefixSetting.CustomerReturnReceiptPrefix;

            var code = new ABCAPOSDAC().RetrieveReturnReceiptMaxCode(ReturnReceiptPrefix, 5);

            return code;
        }

        protected override GenericDetailDAC<ReturnReceiptDetail, ReturnReceiptDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<ReturnReceiptDetail, ReturnReceiptDetailModel>("ReturnReceiptID", "ItemNo", false);
        }

        protected override GenericDetailDAC<v_ReturnReceiptDetail, ReturnReceiptDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_ReturnReceiptDetail, ReturnReceiptDetailModel>("ReturnReceiptID", "ItemNo", false);
        }

        protected override GenericDAC<ReturnReceipt, ReturnReceiptModel> GetMasterDAC()
        {
            return new GenericDAC<ReturnReceipt, ReturnReceiptModel>("ID", false, "Date DESC");
        }

        protected override GenericDAC<v_ReturnReceipt, ReturnReceiptModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_ReturnReceipt, ReturnReceiptModel>("ID", false, "Date DESC");
        }

        public List<ReturnReceiptModel> RetrieveByCustomerReturnID(long customerReturnID)
        {
            return new ABCAPOSDAC().RetrieveReturnReceiptByCustomerReturnID(customerReturnID);
        }

        public List<ReturnReceiptDetailModel> RetrieveDetailsByCustomerReturnID(long poID)
        {
            return new ABCAPOSDAC().RetrieveReturnReceiptDetailByCustomerReturnID(poID);
        }

        #endregion 
        
        public override void Create(ReturnReceiptModel header, List<ReturnReceiptDetailModel> details)
        {
            header.Code = GetReturnReceiptCode();

            using (TransactionScope trans = new TransactionScope())
            {
                this.CreateLog(header, details);

                base.Create(header, details);

                OnUpdated(header.ID);

                OnApproved(header.ID, header.CreatedBy);

                trans.Complete();
            }
         
        }

        public override void Update(ReturnReceiptModel header, List<ReturnReceiptDetailModel> details)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                UndoOnApproved(header.ID, header.CreatedBy);

                this.UpdateLog(header, details);

                base.Update(header, details);

                OnApproved(header.ID, header.CreatedBy);

                trans.Complete();
            }
        }

        public void CreateByCustomerReturn(ReturnReceiptModel pd, long poID)
        {
            var customerReturn = new CustomerReturnBFC().RetrieveByID(poID);

            if (customerReturn != null)
            {
                pd.Code = GetReturnReceiptCode();

                pd.CustomerReturnID = poID;
                pd.CustomerReturnCode = customerReturn.Code;
                pd.CustomerReturnDate = customerReturn.Date;
                pd.CustomerReturnTitle = customerReturn.Title;
                pd.CustomerName = customerReturn.CustomerName;
                pd.WarehouseID = customerReturn.WarehouseID;

                var warehouse = new WarehouseBFC().RetrieveByID(customerReturn.WarehouseID);
                pd.WarehouseName = warehouse.Name;

                var department = new DepartmentBFC().RetrieveByID(customerReturn.DepartmentID);
                if (department != null)
                    pd.DepartmentName = department.DepartmentDesc;
                pd.Currency = customerReturn.Currency;
                pd.CurrencyName = customerReturn.CurrencyDescription;
                pd.ExchangeRate = customerReturn.ExchangeRate;

                var poDetails = new CustomerReturnBFC().RetrieveDetails(poID);
                var pdDetails = new List<ReturnReceiptDetailModel>();

                foreach (var poDetail in poDetails)
                {
                    if (poDetail.OutstandingQuantity > 0)
                    {
                        var detail = new ReturnReceiptDetailModel();

                        detail.CustomerReturnItemNo = poDetail.ItemNo;
                        detail.ProductID = poDetail.ProductID;
                        detail.Barcode = poDetail.Barcode;
                        detail.ProductCode = poDetail.ProductCode;
                        detail.ProductName = poDetail.ProductName;
                        detail.ConversionName = poDetail.ConversionName;
                        detail.ConversionID = poDetail.ConversionID;
                        detail.QtyPO = Convert.ToDouble(poDetail.Quantity);
                        detail.QtyReceive = Convert.ToDouble(poDetail.ReceivedQuantity);
                        detail.QtyRemain = Convert.ToDouble(poDetail.Quantity - poDetail.ReceivedQuantity);
                        detail.Quantity = Convert.ToDouble(poDetail.Quantity - poDetail.ReceivedQuantity);
                        detail.StrQuantity = Convert.ToDouble(poDetail.Quantity - poDetail.ReceivedQuantity).ToString().Replace(",", "").Replace(".", ",");
                        detail.Remarks = poDetail.Remarks;

                        var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                        if (unitRate == null)
                        {
                            unitRate = 1;
                        }

                        var itemLoc = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductID, pd.WarehouseID);
                        if (itemLoc != null)
                        {
                            detail.StockQty = itemLoc.QtyOnHand / unitRate;
                            detail.StockQtyAvailable = itemLoc.QtyAvailable / unitRate;
                        }
                        else
                        {
                            detail.StockQty = 0;
                            detail.StockQtyAvailable = 0;
                        }
                          

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

        public void Validate(ReturnReceiptModel pd, List<ReturnReceiptDetailModel> pdDetails)
        {
            var obj = RetrieveByID(pd.ID);
            var poDetails = new CustomerReturnBFC().RetrieveDetails(pd.CustomerReturnID);

            var extDetails = new List<ReturnReceiptDetailModel>();

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
                            where i.ProductID == pdDetail.ProductID && i.ItemNo == pdDetail.CustomerReturnItemNo
                            select i;

                var poDetail = query.FirstOrDefault();
                var pdQty = pdDetail.Quantity;

                if (obj != null)
                {
                    var extDetail = (from i in extDetails
                                     where i.ProductID == pdDetail.ProductID && i.ItemNo == pdDetail.CustomerReturnItemNo
                                     select i).FirstOrDefault();

                    poDetail.ReceivedQuantity -= extDetail.Quantity;
                }

                if (poDetail.OutstandingQuantity < pdQty)
                    throw new Exception("Jumlah Penerimaan tidak boleh lebih dari Remaining");
            }
        }

        public void OnUpdated(long pdID)
        {
            var pd = RetrieveByID(pdID);

            if (pd != null)
            {
                var customerReturn = new CustomerReturnBFC().RetrieveByID(pd.CustomerReturnID);

                if (customerReturn != null)
                {
                    var pdDetails = RetrieveDetailsByCustomerReturnID(customerReturn.ID);
                    var poDetails = new CustomerReturnBFC().RetrieveDetails(customerReturn.ID);

                    var fulfilledCount = 0;

                    foreach (var poDetail in poDetails)
                    {
                        var query = from i in pdDetails
                                    where i.ProductID == poDetail.ProductID && i.CustomerReturnItemNo == poDetail.ItemNo
                                    select i.Quantity;

                        poDetail.ReceivedQuantity = query.Sum();

                        if (poDetail.Quantity == poDetail.ReceivedQuantity)
                            fulfilledCount += 1;
                    }

                    new CustomerReturnBFC().Update(customerReturn);
                    new CustomerReturnBFC().UpdateDetails(customerReturn.ID, poDetails);
                }
            }
        }

        public void OnApproved(long returnReceiptID, string userName)
        {
            var returnReceipt = RetrieveByID(returnReceiptID);
            var customerReturn = new CustomerReturnBFC().RetrieveByID(returnReceipt.CustomerReturnID);

            if (returnReceipt != null)
            {
                var returnReceiptDetails = RetrieveDetails(returnReceiptID);

                foreach (var pdDetail in returnReceiptDetails)
                {
                    var unitRate = new ProductBFC().GetUnitRate(pdDetail.ConversionID);
                    var baseQuantity = pdDetail.Quantity * unitRate;
                    var itemLocation = new ItemLocationBFC().RetrieveByProductIDWarehouseID(pdDetail.ProductID, returnReceipt.WarehouseID);
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
                        newitemLocation.WarehouseID = returnReceipt.WarehouseID;
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

        public void UndoOnApproved(long returnReceiptID, string userName)
        {
            var returnReceipt = RetrieveByID(returnReceiptID);
            var customerReturn = new CustomerReturnBFC().RetrieveByID(returnReceipt.CustomerReturnID);

            if (returnReceipt != null)
            {
                var returnReceiptDetails = RetrieveDetails(returnReceiptID);

                foreach (var pdDetail in returnReceiptDetails)
                {
                    var unitRate = new ProductBFC().GetUnitRate(pdDetail.ConversionID);
                    var baseQuantity = pdDetail.Quantity * unitRate;
                    var itemLocation = new ItemLocationBFC().RetrieveByProductIDWarehouseID(pdDetail.ProductID, returnReceipt.WarehouseID);
                    if (itemLocation != null)
                    {
                        itemLocation.QtyAvailable -= baseQuantity;
                        itemLocation.QtyOnHand -= baseQuantity;

                        new ItemLocationBFC().Update(itemLocation);
                    }
                    else
                    {
                        var newitemLocation = new ItemLocationModel();
                        newitemLocation.ProductID = pdDetail.ProductID;
                        newitemLocation.WarehouseID = returnReceipt.WarehouseID;
                        newitemLocation.QtyOnHand = newitemLocation.QtyAvailable = -baseQuantity;
                        new ItemLocationBFC().Create(newitemLocation);
                    }

                    if (pdDetail.BinID != null || pdDetail.BinID != 0)
                    {
                        var binProduct = new BinProductWarehouseBFC().Retrieve(pdDetail.BinID, pdDetail.ProductID);
                        if (binProduct != null)
                        {
                            binProduct.Quantity -= baseQuantity;
                            new BinProductWarehouseBFC().Update(binProduct);
                        }
                    }
                }
            }
        }

        public void OnVoid(long returnReceiptID, string voidRemarks, string userName)
        {
            var returnReceipt = RetrieveByID(returnReceiptID);
            var returnReceiptDetails = RetrieveDetails(returnReceiptID);

            this.VoidLog(returnReceipt, returnReceiptDetails);

            voidFromOldQty(returnReceipt, returnReceiptDetails);
        }

        public void UpdateRRItemLocationBin(ReturnReceiptModel header, List<ReturnReceiptDetailModel> details)
        {

            foreach (var detail in details)
            {
                var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                var baseSelisihQuantity = detail.SelisihQuantity * unitRate;
                var itemLocation = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductID, header.WarehouseID);
                if (itemLocation != null)
                {
                    itemLocation.QtyAvailable -= baseSelisihQuantity;
                    itemLocation.QtyOnHand -= baseSelisihQuantity;

                    new ItemLocationBFC().Update(itemLocation);
                }

                if (detail.BinID != null || detail.BinID != 0)
                {
                    var binProduct = new BinProductWarehouseBFC().Retrieve(detail.BinID, detail.ProductID);
                    if (binProduct != null)
                    {
                        binProduct.Quantity -= baseSelisihQuantity;
                        new BinProductWarehouseBFC().Update(binProduct);
                    }
                }
            }
        }

        public void voidFromOldQty(ReturnReceiptModel header, List<ReturnReceiptDetailModel> details)
        {
            var oldDetails = RetrieveDetails(header.ID);

            foreach (var detail in details)
            {
                detail.SelisihQuantity = detail.Quantity;
            }

            UpdateRRItemLocationBin(header, details);

        }
        
        public void Void(long returnReceiptID, string voidRemarks, string userName)
        {
            var returnReceipt = RetrieveByID(returnReceiptID);
            var oldStatus = returnReceipt.Status;
            
            using (TransactionScope trans = new TransactionScope())
            {
                OnVoid(returnReceiptID, voidRemarks, userName);

                returnReceipt.Status = (int)MPL.DocumentStatus.Void;
                returnReceipt.VoidRemarks = voidRemarks;
                returnReceipt.ApprovedDate = DateTime.Now;
                returnReceipt.ApprovedBy = userName;
                Update(returnReceipt);

                trans.Complete();
            }

        }

        //#region Post to Accounting Result

        //public void PostAccounting(long returnReceiptID)
        //{
        //    var returnReceipt = RetrieveByID(returnReceiptID);

        //    new ABCAPOSDAC().DeleteAccountingResults(returnReceiptID, AccountingResultDocumentType.ReturnReceipt);

        //    if (returnReceipt!=null && returnReceipt.Status != (int)MPL.DocumentStatus.Void)
        //        CreateAccountingResult(returnReceiptID);
        //}

        //private void CreateAccountingResult(long returnReceiptID)
        //{
        //    var returnReceipt = RetrieveByID(returnReceiptID);
        //    var returnReceiptDetails = RetrieveDetails(returnReceiptID);

        //    var accountingResultList = new List<AccountingResultModel>();

        //    decimal totalAmount = 0;
        //    decimal basePriceTotal = 0;

        //    var customerReturn = new CustomerReturnBFC().RetrieveByID(returnReceipt.CustomerReturnID);
        //    var customerReturnDetails = new CustomerReturnBFC().RetrieveDetails(returnReceipt.CustomerReturnID);

        //    foreach (var returnDeliveryDetail in returnReceiptDetails)
        //    {
        //        var returnDetail = (from i in customerReturnDetails
        //                            where i.ProductID == returnDeliveryDetail.ProductID
        //                            select i).FirstOrDefault();

        //        if (returnDetail != null)
        //        {
        //            var product = new ProductBFC().RetrieveByID(returnDeliveryDetail.ProductID);

        //            totalAmount += Convert.ToDecimal(returnDeliveryDetail.Quantity) * (returnDetail.Total / Convert.ToDecimal(returnDetail.Quantity));

        //            basePriceTotal += Convert.ToDecimal(returnDeliveryDetail.Quantity) * product.BasePrice;
        //        }
        //    }

        //    totalAmount = totalAmount * returnReceipt.ExchangeRate;

        //    accountingResultList = AddToAccountingResultList(accountingResultList, returnReceipt, (long)PostingAccount.ReturPenjualan, AccountingResultType.Debit, totalAmount, "Retur penjualan return receipt " + returnReceipt.Code);

        //    accountingResultList = AddToAccountingResultList(accountingResultList, returnReceipt, (long)PostingAccount.PiutangDagang, AccountingResultType.Credit, totalAmount, "Piutang dagang return receipt " + returnReceipt.Code);

        //    accountingResultList = AddToAccountingResultList(accountingResultList, returnReceipt, (long)PostingAccount.PersediaanBarangJadi, AccountingResultType.Debit, basePriceTotal, "Persediaan barang jadi return receipt " + returnReceipt.Code);

        //    accountingResultList = AddToAccountingResultList(accountingResultList, returnReceipt, (long)PostingAccount.HargaPokokPenjualan, AccountingResultType.Credit, basePriceTotal, "Harga pokok penjualan return receipt " + returnReceipt.Code);

        //    new AccountingResultBFC().Posting(accountingResultList);
        //}

        //private List<AccountingResultModel> AddToAccountingResultList(List<AccountingResultModel> resultList, ReturnReceiptModel obj, long accountID, AccountingResultType resultType, decimal amount, string remarks)
        //{
        //    if (amount > 0)
        //    {
        //        var account = new AccountBFC().RetrieveByID(accountID);
        //        var result = new AccountingResultModel();

        //        result.DocumentID = obj.ID;
        //        result.DocumentType = (int)AccountingResultDocumentType.ReturnReceipt;
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
    }
}
