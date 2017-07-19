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
    public class AssemblyBuildBFC:MasterDetailBFC<AssemblyBuild,v_AssemblyBuild,AssemblyBuildDetail,v_AssemblyBuildDetail,AssemblyBuildModel,AssemblyBuildDetailModel>
    {

        private void IncreaseInventoryQty(AssemblyBuildModel header, List<AssemblyBuildDetailModel> details)
        {
            if (header != null)
            {
                var itemLocation = new ItemLocationBFC().RetrieveByProductIDWarehouseID(header.ProductID, header.WarehouseID);
                var unitRateHeader = new ProductBFC().GetUnitRate(header.StockUnitID);
                var qtyBaseUnitHeader = Convert.ToDouble(header.QtyActual) * unitRateHeader;
                if (itemLocation != null)
                {
                    itemLocation.QtyAvailable +=qtyBaseUnitHeader;
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

        private void DecreaseInventoryQty(AssemblyBuildModel header, List<AssemblyBuildDetailModel> details)
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

        #region PostingTableLog
        public void IncreaseLog(AssemblyBuildModel header,List<AssemblyBuildDetailModel>details)
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
                        decimal amount;

                        double QtyRemaining = baseQuantity;
                        do
                        {
                            var LogDetail = new LogDetailModel();
                            var container = new ContainerBFC().RetreiveByProductIDWarehouseID(detail.ProductDetailID, header.WarehouseID);
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
                                //new ContainerBFC().UpdateByAssemblyBuild(container, QtyRemaining, qty);

                                LogDetail.ContainerID = container.ID;
                                LogDetail.ProductID = detail.ProductDetailID;
                                LogDetail.MovingOutQty = qty;

                                if (container.Price == 0)
                                    container.Price = 1;

                                amount = Convert.ToDecimal(qty) * Convert.ToDecimal(container.Price);
                                header.TotalProject = header.TotalProject + amount;

                                LogDetails.Add(LogDetail);
                            }
                            else
                            {
                                var containerHeader = new ContainerModel();
                                containerHeader.ProductID = detail.ProductDetailID;
                                containerHeader.WarehouseID = header.WarehouseID;
                                containerHeader.Qty = QtyRemaining;
                                containerHeader.Price = 0;
                                new ContainerBFC().Create(containerHeader);

                                LogDetail.ContainerID = containerHeader.ID;
                                LogDetail.ProductID = detail.ProductDetailID;
                                LogDetail.MovingOutQty = QtyRemaining;
                                QtyRemaining = 0;

                                amount = Convert.ToDecimal(QtyRemaining) * 1;
                                header.TotalProject = header.TotalProject + amount;

                                LogDetails.Add(LogDetail);
                            }
                        } while (QtyRemaining > 0);
                    }

                    header.LogID = LogHeader.ID;
                    header.Hpp = header.TotalProject / header.QtyActual;

                    var unitRateheader = new ProductBFC().GetUnitRate(header.StockUnitID);
                    var baseQuantityheader = Convert.ToDouble(header.QtyActual) * unitRateheader;
                    var ContainerHeader = new ContainerModel();


                    ContainerHeader.ProductID = header.ProductID;
                    ContainerHeader.WarehouseID = header.WarehouseID;
                    ContainerHeader.Qty = baseQuantityheader;
                    ContainerHeader.Price = Convert.ToDouble(header.Hpp);
                    new ContainerBFC().Create(ContainerHeader);
                   
                    var LogDet = new LogDetailModel();

                    LogDet.ContainerID = ContainerHeader.ID;
                    LogDet.ProductID = header.ProductID;
                    LogDet.MovingInQty = baseQuantityheader;
                    LogDetails.Add(LogDet);
                    new LogBFC().Create(LogHeader, LogDetails);
                    header.LogID = LogHeader.ID;


                    //base.Update(header);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void DecreaseContainer(AssemblyBuildModel header)
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
                        var container = new ContainerBFC().RetreiveByLogIDproductIDdocType(header.LogID, detail.ProductID, (int)DocType.AssemblyBuild);
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
                                }
                            }
                        }
                        /* Undo Container and Log Moving Out */
                    }

                    /* Decrease Container and Log Moving IN*/
                    var containerHeader = new ContainerBFC().RetreiveByLogIDproductIDdocType(header.LogID, header.ProductID, (int)DocType.AssemblyBuild);

                    if (containerHeader != null)
                    {
                        var containerDetail = new ContainerBFC().RetreiveByContainerIDProductIDWarehouseID(containerHeader.ContainerID, header.ProductID, header.WarehouseID);
                        if (containerDetail != null)
                        {
                            if (containerHeader.Pemakaian > 0)
                            {
                                throw new Exception("Barang sudah ada yg terpakai,void tidak dapat dilakukan");
                            }
                            else if (containerHeader.Pemakaian == 0)
                            {
                                var ContainerHeader = new ContainerModel();
                                ContainerHeader.ID = containerHeader.ContainerID;
                                ContainerHeader.ProductID = header.ProductID;
                                ContainerHeader.WarehouseID = header.WarehouseID;
                                ContainerHeader.Qty = 0;
                                ContainerHeader.Price = 0;
                                new ContainerBFC().Update(ContainerHeader);
                            }
                        }
                    }
                    /* Decrease Container and Log Moving IN */
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
        #endregion

        private void UpdateWorkOrder(AssemblyBuildModel header, List<AssemblyBuildDetailModel> details, long workorderID)
        {
            var wo = new WorkOrderBFC().RetrieveByID(workorderID);

            wo.Status = (int)WorkOrderStatus.FullyBuild;


            var wodetails = new WorkOrderBFC().RetreiveByWorkorderID(header.WorkOrderID);

            foreach (var abdetail in wodetails)
            {
                var qtyDetail = RetreiveQtyByAssemblyID(header.ID, abdetail.ProductDetailID);
                abdetail.UsedInBuilt = qtyDetail.Qty;
            }

            wo.QtyBuilt = header.QtyBuild;

            new WorkOrderBFC().Update(wo, wodetails);
        }

        private void UpdateWorkOrderPartialy(AssemblyBuildModel header, List<AssemblyBuildDetailModel> details, long workorderID)
        {
            var wo = new WorkOrderBFC().RetrieveByID(workorderID);

            wo.Status = (int)WorkOrderStatus.PartialyBuild;


            var wodetails = new WorkOrderBFC().RetreiveByWorkorderID(header.WorkOrderID);

            foreach (var abdetail in wodetails)
            {
                var qtyDetail = RetreiveQtyByAssemblyID(header.ID, abdetail.ProductDetailID);
                if (qtyDetail != null)
                {
                    abdetail.UsedInBuilt = qtyDetail.Qty;
                }

                //if (qtyDetail == null)
                //    qtyDetail.Qty = 0;

              
            }

            wo.QtyBuilt = header.QtyBuild;

            new WorkOrderBFC().Update(wo, wodetails);
        }

        protected override GenericDetailDAC<AssemblyBuildDetail, AssemblyBuildDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<AssemblyBuildDetail, AssemblyBuildDetailModel>("AssemblyBuildID", "ItemNo", false);
        }

        protected override GenericDetailDAC<v_AssemblyBuildDetail, AssemblyBuildDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_AssemblyBuildDetail, AssemblyBuildDetailModel>("AssemblyBuildID", "ItemNo", false);
        }

        protected override GenericDAC<AssemblyBuild, AssemblyBuildModel> GetMasterDAC()
        {
            return new GenericDAC<AssemblyBuild, AssemblyBuildModel>("ID", false, "Date DESC");
        }

        protected override GenericDAC<v_AssemblyBuild, AssemblyBuildModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_AssemblyBuild, AssemblyBuildModel>("ID", false, "Date DESC");
        }

        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public override void Create(AssemblyBuildModel header,List<AssemblyBuildDetailModel>details)
        {
           

            using (TransactionScope trans = new TransactionScope())
            {
                
                header.Code = GetAssemblyBuildCode(header);

                header.QtyLost = header.QtyBuild - header.QtyActual;

                header.BatchNo = "ABCA" + Convert.ToString(DateTime.Now.ToString("ddmmyyyy"));

                base.Create(header, header.Details);

                this.UpdateWorkOrderPartialy(header,details,header.WorkOrderID);
               
                trans.Complete();
            }

           
        }

        public override void Update(AssemblyBuildModel header, List<AssemblyBuildDetailModel> details)
        {
            using(TransactionScope trans = new TransactionScope())
            {
                header.QtyLost = header.QtyBuild - header.QtyActual;

                //header.BatchNo = "ABCA" + header.Tanggal;

                base.Update(header, details);
                
                trans.Complete();
            }
           
        }


        public string GenerateCode(string prefix, int maxLength = 5)
        {
            string strNumber = this.RetrieveAll().Count > 0 ? this.RetrieveAll().Max(e => e.GetType().GetProperty("BatchNo").GetValue(e, null)).ToString().Split('-')[1] : "0";
            string code = (int.Parse(strNumber) + 1).ToString();
            while (code.Length < maxLength)
            {
                code = "0" + code;
            }
            return prefix + "-" + code;
        }


        public string GetAssemblyBuildCode(AssemblyBuildModel header)
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var AssemblyBuildPrefix = "";

            if (prefixSetting != null)
                AssemblyBuildPrefix = prefixSetting.AssemblyBuildPrefix;

            var warehouse = new WarehouseBFC().RetrieveByID(header.WarehouseID);
            var year = DateTime.Now.Year.ToString().Substring(2, 2);

            var prefix = AssemblyBuildPrefix + year + "-" + warehouse.Code + "-";
            var code = new ABCAPOSDAC().RetreiveAssemblyBuildMaxCode(prefix, 7);

            return code;
        }

        public void CreatedByWorkOrder(AssemblyBuildModel header, long workOrderID)
        {
            var workOrder = new WorkOrderBFC().RetrieveByID(workOrderID);

            if (workOrder != null)
            {
                header.Code = SystemConstants.autoGenerated;
                header.WorkOrderID = workOrder.ID;
                header.Date = workOrder.Date;
                header.WorkOrderCode = workOrder.Code;
                header.ProductID = workOrder.ProductID;
                header.QtyWO = workOrder.QtyWO;
                header.QtyBuild = workOrder.QtyWO;
                header.WoNotes = workOrder.Notes;

                var product = new ProductBFC().RetrieveByID(header.ProductID);
                header.ProductCode = product.Code;
                header.ProductName = product.ProductName;
                header.Class = product.Class;
                header.GroupWarna = product.GroupWarna;
                header.ItemProduct = product.ItemProduct;
                header.ItemBrand = product.ItemBrand;
               

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

                var abDetails = new WorkOrderBFC().RetreiveByWorkorderID(workOrderID).OrderBy(p=>p.ItemNo);

                var assemblyBuildDetails = new List<AssemblyBuildDetailModel>();

                foreach (var abDetail in abDetails)
                {
                    var detail = new AssemblyBuildDetailModel();
                    detail.ItemNo = abDetail.ItemNo;
                    detail.ProductDetailID = abDetail.ProductDetailID;
                    detail.ProductDetailCode = abDetail.ProductCode;
                    detail.ProductDetailName = abDetail.ProductName;
                    detail.ConversionID = abDetail.ConversionID;
                    detail.ConversionIDTemp = abDetail.ConversionID;
                    detail.ConversionName = abDetail.ConversionName;
                  
                    var qtyDetail = new WorkOrderBFC().RetreiveQtyByWorkOrderID(header.WorkOrderID, detail.ProductDetailID);
                    detail.Qty = qtyDetail.Qty;
                    detail.QtyWO = qtyDetail.Qty;
                    //detail.ConversionID = qtyDetail.ConversionID;
                    //detail.ConversionName = qtyDetail.ConversionName;

                    detail.UnitRate = new ProductBFC().GetUnitRate(detail.ConversionID);
                    var itemLoc = new ItemLocationBFC().RetrieveByProductIDWarehouseID(abDetail.ProductDetailID, workOrder.WarehouseID);

                    if (itemLoc != null)
                        detail.QtyOnHand = itemLoc.QtyOnHand / detail.UnitRate;
                    else
                        detail.QtyOnHand = 0;

                    if (itemLoc != null)
                        detail.QtyAvailable = itemLoc.QtyAvailable / detail.UnitRate;
                    else
                        detail.QtyAvailable = 0;

                    assemblyBuildDetails.Add(detail);
                }
                header.Details = assemblyBuildDetails;
                //header.Details.OrderBy(P => P.ItemNo);
            }
        }

        public void Approve(long assemblyBuildID, string userName)
        {
            var assemblyBuild = RetrieveByID(assemblyBuildID);

            var details = RetrieveDetails(assemblyBuildID);

            assemblyBuild.Status = (int)MPL.DocumentStatus.Approved;

            assemblyBuild.ApprovedBy = userName;

            assemblyBuild.ApprovedDate = DateTime.Now;

            assemblyBuild.QtyLost = assemblyBuild.QtyBuild - assemblyBuild.QtyActual;

            using (TransactionScope trans = new TransactionScope())
            {
                this.IncreaseInventoryQty(assemblyBuild, details);

                this.IncreaseLog(assemblyBuild, details);

                Update(assemblyBuild);

                this.UpdateWorkOrder(assemblyBuild, details,assemblyBuild.WorkOrderID);

                this.PostAccounting(assemblyBuildID, assemblyBuild.Status);

                trans.Complete();
            }

        }

        public void Void(long assemblyBuildID, string voidRemarks, string userName)
        {
            var obj = RetrieveByID(assemblyBuildID);

            var details = RetrieveDetails(assemblyBuildID);

            obj.VoidRemarks = voidRemarks;
            obj.Status = (int)AssemblyBuildStatus.Void;
           
            using(TransactionScope trans = new TransactionScope())
            {
                VoidWorkOrder(obj.WorkOrderID,assemblyBuildID);

                this.DecreaseContainer(obj);

                this.DecreaseLog(obj.LogID);

                this.DecreaseInventoryQty(obj, details);

                this.PostAccounting(assemblyBuildID, obj.Status);

                base.Update(obj);

                trans.Complete();
            }
           
        }

        public void VoidWorkOrder(long workOrderID, long assemblyBuildID)
        {
            var wo = new WorkOrderBFC().RetrieveByID(workOrderID);
            wo.Status = (int)WorkOrderStatus.Approved;
            wo.QtyBuilt = 0;

            var buildDetails = base.RetrieveDetails(assemblyBuildID);
            var details = new List<WorkOrderDetailModel>();
            foreach (var abdetail in buildDetails)
            {
                var woDetail = new WorkOrderBFC().RetreiveQtyByWorkOrderID(abdetail.WorkOrderID, abdetail.ProductDetailID);
                woDetail.UsedInBuilt = 0;
                //abdetail.UsedInBuilt = 0;
                details.Add(woDetail);
            }

            new WorkOrderBFC().Update(wo, details);
        }

        public AssemblyBuildDetailModel RetreiveQtyByAssemblyID(long AssemblyID, long productDetailID)
        {
            return new ABCAPOSDAC().RetreiveQtyByAssemblyID(AssemblyID, productDetailID);
        }

        public List<AssemblyBuildModel> RetreiveBuildByWOID(long workOrderID)
        {
            return new ABCAPOSDAC().RetreiveBuildByWOID(workOrderID);
        }

        public List<AssemblyBuildModel> RetreiveBuildByproductID(long productID)
        {
            return new ABCAPOSDAC().RetreiveBuildByproductID(productID);
        }

        public List<AssemblyBuildModel> RetreiveListBuild(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters,dynamic ViewBag)
        {
            if ((bool)ViewBag.AllowViewFG)
            {
                return new ABCAPOSDAC().RetrieveListBuildFG(startIndex, amount, sortParameter, selectFilters);
            }
            else
            {
                return new ABCAPOSDAC().RetrieveListBuild(startIndex, amount, sortParameter, selectFilters);
            }
        }

      

        public List<AssemblyBuildModel> RetreiveListBuildFG(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveListBuildFG(startIndex, amount, sortParameter, selectFilters);
        }

        public int RetreiveListCountBuildFG(List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetreiveListBuildCountFG(selectFilters);
        }

        public int RetreiveListCountBuild(List<SelectFilter> selectFilters,dynamic ViewBag)
        {
            if ((bool)ViewBag.AllowViewFG)
            {
                return new ABCAPOSDAC().RetreiveListBuildCountFG(selectFilters);
            }
            else
            {
                return new ABCAPOSDAC().RetreiveListBuildCount(selectFilters);
            }
           
        }

        #region Post to Accounting Result

        public void PostAccounting(long assemblyBuildID, int Status)
        {
            var assemblyBuild = RetrieveByID(assemblyBuildID);

            new ABCAPOSDAC().DeleteAccountingResults(assemblyBuildID, AccountingResultDocumentType.Assembly);

            if (Status != (int)MPL.DocumentStatus.Void)
                CreateAccountingResult(assemblyBuildID);
        }

        private void CreateAccountingResult(long assemblyBuildID)
        {
            var assemblyBuild = RetrieveByID(assemblyBuildID);
            //var assemblyBuildDetails = RetrieveDetails(assemblyBuildID);
            var assemblyBuildDetails = new LogBFC().RetrieveDetails(assemblyBuild.LogID);

            decimal totalAmount = 0;

            //decimal totalAmountSupporting = 0;

            decimal hppAmount = 0;

            totalAmount = Convert.ToDecimal(assemblyBuildDetails.Sum(p=>p.MovingOutQty * p.Price));
            hppAmount = Convert.ToDecimal(assemblyBuildDetails.Sum(p => p.MovingInQty * p.Price));
            //if (assemblyBuild.Hpp == 0)
            //    assemblyBuild.Hpp = 1;

            //hppAmount = assemblyBuild.QtyActual * assemblyBuild.Hpp;

            var accountingResultList = new List<AccountingResultModel>();

            //foreach (var assemblyBuildDetail in assemblyBuildDetails)
            //{
            //    if (assemblyBuildDetail.MovingOutQty > 0)
            //    {
            //        if (assemblyBuildDetail.Price == 0)
            //            assemblyBuildDetail.Price = 1;

            //        if (assemblyBuildDetail.ItemTypeID == (int)ItemTypeProduct.Supporting)
            //            totalAmountSupporting += Math.Round(Convert.ToDecimal(assemblyBuildDetail.MovingOutQty),4) * Math.Round(Convert.ToDecimal(assemblyBuildDetail.Price),4);
            //        else
            //            totalAmount += Math.Round(Convert.ToDecimal(assemblyBuildDetail.MovingOutQty), 4) * Math.Round(Convert.ToDecimal(assemblyBuildDetail.Price), 4);
            //    }
            //    else if (assemblyBuildDetail.MovingInQty > 0)
            //    {
            //        if (assemblyBuildDetail.Price == 0)
            //            assemblyBuildDetail.Price = 1;

            //        hppAmount = Math.Round(Convert.ToDecimal(assemblyBuildDetail.MovingInQty),4) * Math.Round(Convert.ToDecimal(assemblyBuildDetail.Price),4);
            //    }
            //}

            accountingResultList = AddToAccountingResultList(accountingResultList, assemblyBuild, (long)PostingAccount.PersediaanBarangJadi, AccountingResultType.Debit, Math.Round(hppAmount,4), "Persediaan - barang jadi  " + assemblyBuild.Code);

            accountingResultList = AddToAccountingResultList(accountingResultList, assemblyBuild, (long)PostingAccount.PersediaanBahanBaku, AccountingResultType.Credit, Math.Round(hppAmount,4), "Persediaan - bahan baku   " + assemblyBuild.Code);

            //accountingResultList = AddToAccountingResultList(accountingResultList, assemblyBuild, (long)PostingAccount.PersediaanBahanPembantu, AccountingResultType.Credit, Math.Round(totalAmountSupporting,4), "Persediaan - bahan pembantu  " + assemblyBuild.Code);

            new AccountingResultBFC().Posting(accountingResultList);
        }

        private List<AccountingResultModel> AddToAccountingResultList(List<AccountingResultModel> resultList, AssemblyBuildModel obj, long accountID, AccountingResultType resultType, decimal amount, string remarks)
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
        #endregion
    }
}
