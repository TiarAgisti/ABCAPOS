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
    public class AssemblyUnBuildBFC:MasterDetailBFC<AssemblyUnBuild,v_AssemblyUnBuild,AssemblyUnBuildDetail,v_AssemblyUnBuildDetail,AssemblyUnBuildModel,AssemblyUnBuildDetailModel>
    {
        private void IncreaseInventoryQty(AssemblyUnBuildModel header, List<AssemblyUnBuildDetailModel> details)
        {
            if (header != null)
            {
                var itemLocation = new ItemLocationBFC().RetrieveByProductIDWarehouseID(header.ProductID, header.WarehouseID);
                var unitRateHeader = new ProductBFC().GetUnitRate(header.StockUnitID);
                var qtyBaseUnitHeader = Convert.ToDouble(header.QtyActual) * unitRateHeader;
                if (itemLocation != null)
                {
                    itemLocation.QtyAvailable -= qtyBaseUnitHeader;
                    itemLocation.QtyOnHand -= qtyBaseUnitHeader;
                    new ItemLocationBFC().Update(itemLocation);
                }
                else
                {
                    new ItemLocationBFC().Create(header.ProductID,
                      header.WarehouseID, -qtyBaseUnitHeader, -qtyBaseUnitHeader);
                }

                foreach (var detail in details)
                {
                    var itemLocationDetail = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductDetailID, header.WarehouseID);
                    var unitRatedetail = new ProductBFC().GetUnitRate(detail.ConversionID);
                    var qtyBaseUnitdetail = Convert.ToDouble(detail.Qty) * unitRatedetail;
                    if (itemLocationDetail != null)
                    {
                        itemLocationDetail.QtyOnHand += qtyBaseUnitdetail;
                        itemLocationDetail.QtyAvailable += qtyBaseUnitdetail;
                        new ItemLocationBFC().Update(itemLocationDetail);
                    }
                    else
                    {
                        new ItemLocationBFC().Create(detail.ProductDetailID,
                          header.WarehouseID, qtyBaseUnitdetail, qtyBaseUnitdetail);
                    }
                }
            }
        }

        private void DecreaseInventoryQty(AssemblyUnBuildModel header, List<AssemblyUnBuildDetailModel> details)
        {
            if (header != null)
            {
                var itemLocation = new ItemLocationBFC().RetrieveByProductIDWarehouseID(header.ProductID, header.WarehouseID);
                var unitRateHeader = new ProductBFC().GetUnitRate(header.StockUnitID);
                var qtyBaseUnitHeader = Convert.ToDouble(header.QtyActual) * unitRateHeader;
                if (itemLocation != null)
                {
                    itemLocation.QtyAvailable += qtyBaseUnitHeader;
                    itemLocation.QtyOnHand += qtyBaseUnitHeader;
                    new ItemLocationBFC().Update(itemLocation);
                }
                else
                {
                    new ItemLocationBFC().Create(header.ProductID,
                      header.WarehouseID, qtyBaseUnitHeader, qtyBaseUnitHeader);
                }

                foreach (var detail in details)
                {
                    var itemLocationDetail = new ItemLocationBFC().RetrieveByProductIDWarehouseID(detail.ProductDetailID, header.WarehouseID);
                    var unitRatedetail = new ProductBFC().GetUnitRate(detail.ConversionID);
                    var qtyBaseUnitdetail = Convert.ToDouble(detail.Qty) * unitRatedetail;
                    if (itemLocationDetail != null)
                    {
                        itemLocationDetail.QtyOnHand -= qtyBaseUnitdetail;
                        itemLocationDetail.QtyAvailable -= qtyBaseUnitdetail;
                        new ItemLocationBFC().Update(itemLocationDetail);
                    }
                    else
                    {
                        new ItemLocationBFC().Create(detail.ProductDetailID,
                          header.WarehouseID, -qtyBaseUnitdetail, -qtyBaseUnitdetail);
                    }
                }
            }
        }

        private void DecreaseContainer(AssemblyUnBuildModel header)
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

                        /* Undo Container and Log Moving Out */
                        var container = new ContainerBFC().RetreiveByLogIDproductIDdocType(header.LogID, detail.ProductID, (int)DocType.AssemblyUnBuild);
                        if (container != null)
                        {
                            var containerDetail = new ContainerBFC().RetreiveByContainerIDProductIDWarehouseID(container.ContainerID, detail.ProductID, header.WarehouseID);
                            if (containerDetail != null)
                            {
                                var LogDetail = new LogBFC().RetreiveByLogIDContainerIDProductIDWarehouseID(header.LogID, container.ContainerID, detail.ProductID, header.WarehouseID);
                                if (LogDetail != null)
                                {
                                    if (LogDetail.MovingOutQty > 0)
                                    {
                                        var ContainerHeader = new ContainerModel();
                                        ContainerHeader.ID = container.ContainerID;
                                        ContainerHeader.ProductID = detail.ProductID;
                                        ContainerHeader.WarehouseID = header.WarehouseID;
                                        ContainerHeader.Qty = containerDetail.Qty + LogDetail.MovingOutQty;
                                        ContainerHeader.Price = containerDetail.Price;
                                        new ContainerBFC().Update(ContainerHeader);
                                    }

                                    if (LogDetail.MovingInQty > 0)
                                    {
                                        var ContainerHeader = new ContainerModel();
                                        ContainerHeader.ID = container.ContainerID;
                                        ContainerHeader.ProductID = detail.ProductID;
                                        ContainerHeader.WarehouseID = header.WarehouseID;
                                        ContainerHeader.Qty = containerDetail.Qty - LogDetail.MovingInQty;
                                        ContainerHeader.Price = containerDetail.Price;
                                        new ContainerBFC().Update(ContainerHeader);
                                    }

                                 
                                }
                            }
                        }
                        /* Undo Container and Log Moving Out */
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
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

        private void IncreaseLog(AssemblyUnBuildModel header, List<AssemblyUnBuildDetailModel> details)
        {
            try
            {
                if (header != null && details != null)
                {
                    var LogHeader = new LogModel();
                    var LogDetails = new List<LogDetailModel>();

                    LogHeader.WarehouseID = header.WarehouseID;
                    LogHeader.Date = header.Date;
                    LogHeader.DocType = (int)DocType.AssemblyBuild;

                    foreach (var detail in details)
                    {
                        var unitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                        var baseQuantity = detail.Qty * unitRate;

                        var LogDetail = new LogDetailModel();
                        var container = new ContainerBFC().RetreiveByProductIDWarehouseID(detail.ProductDetailID, header.WarehouseID);
                        if (container != null)
                        {
                            var containerHeader = new ContainerModel();
                            containerHeader.ID = container.ID;
                            containerHeader.ProductID = container.ProductID;
                            containerHeader.WarehouseID = container.WarehouseID;
                            containerHeader.Qty = container.Qty + baseQuantity;
                            containerHeader.Price = container.Price;
                            new ContainerBFC().Update(containerHeader);
                            //new ContainerBFC().UpdateByAssemblyBuild(container, QtyRemaining, qty);

                            LogDetail.ContainerID = container.ID;
                            LogDetail.ProductID = detail.ProductDetailID;
                            LogDetail.MovingInQty = baseQuantity;

                            LogDetails.Add(LogDetail);
                        }
                        else
                        {
                            var containerHeader = new ContainerModel();
                            containerHeader.ProductID = detail.ProductDetailID;
                            containerHeader.WarehouseID = header.WarehouseID;
                            containerHeader.Qty = baseQuantity;
                            containerHeader.Price = 0;
                            new ContainerBFC().Create(containerHeader);

                            LogDetail.ContainerID = containerHeader.ID;
                            LogDetail.ProductID = detail.ProductDetailID;
                            LogDetail.MovingInQty = baseQuantity;

                            LogDetails.Add(LogDetail);
                        }
                    }

                    var unitRateheader = new ProductBFC().GetUnitRate(header.StockUnitID);
                    var baseQuantityheader = Convert.ToDouble(header.QtyActual) * unitRateheader;
                    double QtyRemaining = baseQuantityheader;
                    do
                    {
                        var LogDet = new LogDetailModel();
                        var cnHeader = new ContainerBFC().RetreiveByProductIDWarehouseID(header.ProductID, header.WarehouseID);
                        if (cnHeader != null)
                        {
                            double qty = (QtyRemaining > cnHeader.Qty) ? cnHeader.Qty : QtyRemaining;
                            var ContainerHeader = new ContainerModel();
                            ContainerHeader.ID = cnHeader.ID;
                            ContainerHeader.ProductID = cnHeader.ProductID;
                            ContainerHeader.WarehouseID = cnHeader.WarehouseID;
                            ContainerHeader.Qty = cnHeader.Qty - qty; ;
                            new ContainerBFC().Update(ContainerHeader);

                            LogDet.ContainerID = ContainerHeader.ID;
                            LogDet.ProductID = header.ProductID;
                            LogDet.MovingOutQty = qty;
                            LogDetails.Add(LogDet);
                        }
                        else
                        {
                            var containerHeader = new ContainerModel();
                            containerHeader.ProductID = header.ProductID;
                            containerHeader.WarehouseID = header.WarehouseID;
                            containerHeader.Qty = -QtyRemaining;
                            containerHeader.Price = 0;
                            new ContainerBFC().Create(containerHeader);

                            LogDet.ContainerID = containerHeader.ID;
                            LogDet.ProductID = header.ProductID;
                            LogDet.MovingOutQty = QtyRemaining;
                            QtyRemaining = 0;

                            LogDetails.Add(LogDet);
                        }
                    } while (QtyRemaining > 0);

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

        protected override GenericDetailDAC<AssemblyUnBuildDetail, AssemblyUnBuildDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<AssemblyUnBuildDetail, AssemblyUnBuildDetailModel>("AssemblyUnBuildID","ItemNo",false);
        }

        protected override GenericDetailDAC<v_AssemblyUnBuildDetail, AssemblyUnBuildDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_AssemblyUnBuildDetail, AssemblyUnBuildDetailModel>("AssemblyUnBuildID","ItemNo",false);
        }

        protected override GenericDAC<AssemblyUnBuild, AssemblyUnBuildModel> GetMasterDAC()
        {
            return new GenericDAC<AssemblyUnBuild, AssemblyUnBuildModel>("ID",false,"Date Desc");
        }

        protected override GenericDAC<v_AssemblyUnBuild, AssemblyUnBuildModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_AssemblyUnBuild, AssemblyUnBuildModel>("ID",false,"Date Desc");
        }

        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetAssemblyUnBuildCode(AssemblyUnBuildModel header)
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var AssemblyUnBuildPrefix = "";

            if (prefixSetting != null)
                AssemblyUnBuildPrefix = prefixSetting.AssemblyUnBuildPrefix;

            var warehouse = new WarehouseBFC().RetrieveByID(header.WarehouseID);
            var year = DateTime.Now.Year.ToString().Substring(2, 2);

            var prefix = AssemblyUnBuildPrefix + year + "-" + warehouse.Code + "-";
            var code = new ABCAPOSDAC().RetreiveUnBuildMaxCode(prefix, 7);

            return code;
        }

        public override void Create(AssemblyUnBuildModel header, List<AssemblyUnBuildDetailModel> details)
        {
            header.Code = this.GetAssemblyUnBuildCode(header);
            base.Create(header, details);
        }

        public override void Update(AssemblyUnBuildModel header, List<AssemblyUnBuildDetailModel> details)
        {
            base.Update(header, details);
        }

        public void PrepareUnBuildByFormulasi(AssemblyUnBuildModel header, long productID)
        {
            var formulasi = new FormulasiBFC().RetrieveByID(productID);
            if (formulasi != null)
            {
                header.ProductID = formulasi.ProductID;
                var product = new ProductBFC().RetrieveByID(header.ProductID);
                header.ProductCode = product.Code;
                header.ProductName = product.ProductName;
                header.Class = product.Class;
                header.ItemProduct = product.ItemProduct;
                header.ItemBrand = product.ItemBrand;
                header.GroupWarna = product.GroupWarna;

                var unit = new UnitBFC().GetUnitDetailByID(product.StockUnitID);
                if (unit == null)
                {
                    //var UnitDefault = new UnitBFC().GetUnitByID(product.UnitTypeID);
                    header.UnitName = "";
                }
                else
                {
                    header.UnitName = unit.Name;
                }

                var unbuildDetail = new FormulasiBFC().RetreiveByProductID(productID).OrderBy(p => p.ItemNo);
                var details = new List<AssemblyUnBuildDetailModel>();

                foreach (var unbuildDetails in unbuildDetail)
                {
                    var detail = new AssemblyUnBuildDetailModel();
                    var productDetail = new ProductBFC().RetrieveByID(unbuildDetails.ProductDetailID);
                    detail.ItemNo = unbuildDetails.ItemNo;
                    detail.ProductDetailID = unbuildDetails.ProductDetailID;
                    detail.ProductDetailCode = unbuildDetails.ProductCode;
                    detail.ProductDetailName = unbuildDetails.ProductName;

                    detail.Qty = unbuildDetails.Qty;
                    detail.StockQtyHidden = unbuildDetails.Qty;

                    detail.ConversionID = unbuildDetails.ConversionID;
                    detail.ConversionIDTemp = unbuildDetails.ConversionID;
                    detail.ConversionName = unbuildDetails.ConversionName;
                    //detail.ItemTypeID = productDetail.ItemTypeID;

                    detail.UnitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                    var itemLoc = new ItemLocationBFC().RetrieveByProductIDWarehouseID(unbuildDetails.ProductDetailID, header.WarehouseID);
                    if (itemLoc != null)
                        detail.QtyOnHand = itemLoc.QtyOnHand / detail.UnitRate;
                    else
                        detail.QtyOnHand = 0;

                    if (itemLoc != null)
                        detail.QtyAvailable = itemLoc.QtyAvailable / detail.UnitRate;
                    else
                        detail.QtyAvailable = 0;

                    if (product.UseBin)
                    {
                        detail.BinID = new BinBFC().RetrieveDefaultBinID(product.ID, header.WarehouseID);
                    }

                    details.Add(detail);
                }
                header.Details = details;
            }
            else
            {
                header.ProductID = productID;

                var product = new ProductBFC().RetrieveByID(productID);
                header.ProductCode = product.Code;
                header.ProductName = product.ProductName;

                var unit = new UnitBFC().GetUnitDetailByID(product.StockUnitID);
                header.UnitName = unit.Name;
            }
        }

        public void Approve(long assemblyUnBuildID, string userName)
        {
            try
            {
                var UnBuild = RetrieveByID(assemblyUnBuildID);

                var details = RetrieveDetails(assemblyUnBuildID);

                UnBuild.Status = (int)AssemblyBuildStatus.Fully;

                UnBuild.ApprovedBy = userName;

                UnBuild.ApprovedDate = DateTime.Now;

                this.IncreaseInventoryQty(UnBuild, details);

                this.IncreaseLog(UnBuild, details);

                this.PostAccounting(assemblyUnBuildID, UnBuild.Status);

                base.Update(UnBuild);
            }
            catch (Exception ex)
            {
                throw;
            }
         
        }

        public void Void(long unBuildID, string voidRemarks, string userName)
        {
            var obj = RetrieveByID(unBuildID);

            var details = RetrieveDetails(unBuildID);

            obj.VoidRemarks = voidRemarks;
            obj.Status = (int)MPL.DocumentStatus.Void;

            this.DecreaseContainer(obj);

            this.DecreaseLog(obj.LogID);

            this.DecreaseInventoryQty(obj, details);

            this.PostAccounting(unBuildID, obj.Status);

            base.Update(obj);

        }


        public List<AssemblyUnBuildModel> RetreiveListUnBuild(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters, dynamic ViewBag)
        {
            if ((bool)ViewBag.AllowViewFG)
            {
                return new ABCAPOSDAC().RetreiveListUnBuildFG(startIndex, amount, sortParameter, selectFilters);
            }
            else
            {
                return new ABCAPOSDAC().RetreiveListUnBuild(startIndex, amount, sortParameter, selectFilters);
            }
        }

        public int RetreiveListCountBuild(List<SelectFilter> selectFilters, dynamic ViewBag)
        {
            if ((bool)ViewBag.AllowViewFG)
            {
                return new ABCAPOSDAC().RetreiveListBuildCountFG(selectFilters);
            }
            else
            {
                return new ABCAPOSDAC().RetreiveListUnBuildCount(selectFilters);
            }
        }

        #region Posting Accounting
        private List<AccountingResultModel> AddToAccountingResultList(List<AccountingResultModel> resultList, AssemblyUnBuildModel obj, long accountID, AccountingResultType resultType, decimal amount, string remarks)
        {
            if (amount > 0)
            {
                var account = new AccountBFC().RetrieveByID(accountID);
                var result = new AccountingResultModel();

                result.DocumentID = obj.ID;
                result.DocumentType = (int)AccountingResultDocumentType.Assembly;
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

        private void CreateAccountingResult(long UnBuildID)
        {
            var unbuild = RetrieveByID(UnBuildID);
            var unbuildDetails = RetrieveDetails(UnBuildID);

            decimal totalAmount = 0;

            decimal totalAmountSupporting = 0;

            decimal Amount = 0;

            var ProductJadi = new LogBFC().RetreivePriceByLogIDProductIDWarehouseID(unbuild.LogID, unbuild.ProductID, unbuild.WarehouseID);
            if (ProductJadi != null)
            {
                if (ProductJadi.Price == 0)
                    ProductJadi.Price = 1;

                Amount = unbuild.QtyActual *  Convert.ToDecimal(ProductJadi.Price);
            }

            var accountingResultList = new List<AccountingResultModel>();
            foreach (var unbuildDetail in unbuildDetails)
            {
                var product = new LogBFC().RetreivePriceByLogIDProductIDWarehouseID(unbuild.LogID, unbuildDetail.ProductDetailID, unbuild.WarehouseID);
                if (product != null)
                {
                    if (product.Price == 0)
                        product.Price = 1;

                    if (product.ItemTypeID == (int)ItemTypeProduct.Supporting)
                        totalAmountSupporting += Convert.ToDecimal(unbuildDetail.Qty) * Convert.ToDecimal(product.Price);
                    else
                        totalAmount += Convert.ToDecimal(unbuildDetail.Qty) * Convert.ToDecimal(product.Price);
                }
            }

            accountingResultList = AddToAccountingResultList(accountingResultList, unbuild, (long)PostingAccount.PersediaanBahanBaku, AccountingResultType.Debit, totalAmount, "Persediaan - bahan baku   " + unbuild.Code);

            accountingResultList = AddToAccountingResultList(accountingResultList, unbuild, (long)PostingAccount.PersediaanBahanPembantu, AccountingResultType.Debit, totalAmountSupporting, "Persediaan - bahan pembantu   " + unbuild.Code);

            accountingResultList = AddToAccountingResultList(accountingResultList, unbuild, (long)PostingAccount.PersediaanBarangJadi, AccountingResultType.Credit, Amount, "Persediaan - barang jadi   " + unbuild.Code);

        }

        public void PostAccounting(long unBuildID,int Status)
        {
            var unBuild = RetrieveByID(unBuildID);

            new ABCAPOSDAC().DeleteAccountingResults(unBuildID, AccountingResultDocumentType.Assembly);

            if (Status != (int)MPL.DocumentStatus.Void)
                CreateAccountingResult(unBuildID);
        }
        #endregion

    }
}
