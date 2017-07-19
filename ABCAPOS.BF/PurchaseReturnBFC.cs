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
using MPL.MVC;

namespace ABCAPOS.BF
{
    public class PurchaseReturnBFC : MasterDetailBFC<PurchaseReturn, v_PurchaseReturn, PurchaseReturnDetail, v_PurchaseReturnDetail, PurchaseReturnModel, PurchaseReturnDetailModel>
    {

        public override string GenerateID()
        {
            throw new NotImplementedException();
        }

        public string GetPurchaseReturnCode()
        {
            var prefixSetting = new PrefixSettingBFC().Retrieve();
            var purchaseReturnPrefix = "";

            if (prefixSetting != null)
                purchaseReturnPrefix = prefixSetting.PurchaseReturnPrefix;

            var code = new ABCAPOSDAC().RetrievePurchaseReturnMaxCode(purchaseReturnPrefix, 4);

            return code;
        }

        protected override GenericDetailDAC<PurchaseReturnDetail, PurchaseReturnDetailModel> GetDetailDAC()
        {
            return new GenericDetailDAC<PurchaseReturnDetail, PurchaseReturnDetailModel>("PurchaseReturnID", "ItemNo", false);
        }

        protected override GenericDetailDAC<v_PurchaseReturnDetail, PurchaseReturnDetailModel> GetDetailViewDAC()
        {
            return new GenericDetailDAC<v_PurchaseReturnDetail, PurchaseReturnDetailModel>("PurchaseReturnID", "ItemNo", false);
        }

        protected override GenericDAC<PurchaseReturn, PurchaseReturnModel> GetMasterDAC()
        {
            return new GenericDAC<PurchaseReturn, PurchaseReturnModel>("ID", false, "Code DESC");
        }

        protected override GenericDAC<v_PurchaseReturn, PurchaseReturnModel> GetMasterViewDAC()
        {
            return new GenericDAC<v_PurchaseReturn, PurchaseReturnModel>("ID", false, "Code DESC");
        }

        public override void Create(PurchaseReturnModel header, List<PurchaseReturnDetailModel> details)
        {
            header.Code = GetPurchaseReturnCode();

            using (TransactionScope trans = new TransactionScope())
            {
                base.Create(header, details);

                trans.Complete();
            }
            Approve(header.ID, header.CreatedBy);
        }

        public override void Update(PurchaseReturnModel header, List<PurchaseReturnDetailModel> details)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                base.Update(header, details);

                trans.Complete();
            }
        }

        public void CopyTransaction(PurchaseReturnModel header, long purchaseReturnID)
        {
        }

        public void Validate(PurchaseReturnModel obj, List<PurchaseReturnDetailModel> details)
        {
        }

        public void CreateByPurchaseOrder(PurchaseReturnModel purchaseReturn, long purchaseOrderID)
        {
            var purchaseOrder = new PurchaseOrderBFC().RetrieveByID(purchaseOrderID);

            if (purchaseOrder != null)
            {
                purchaseReturn.Code = GetPurchaseReturnCode();

                purchaseReturn.PurchaseOrderID = purchaseOrderID;
                purchaseReturn.PurchaseOrderCode = purchaseOrder.Code;
                purchaseReturn.VendorCode = purchaseOrder.VendorCode;
                purchaseReturn.VendorName = purchaseOrder.VendorName;
                purchaseReturn.DepartmentName = purchaseOrder.DepartmentName;
                purchaseReturn.WarehouseName = purchaseOrder.WarehouseName;
                purchaseReturn.POSupplierNo = purchaseOrder.POSupplierNo;
                
                var purchaseBills = new PurchaseBillBFC().RetrieveByPOID(purchaseOrder.ID);
                if (purchaseBills.Count > 0)
                {
                    purchaseReturn.SupplierFPNo = purchaseBills[0].SupplierFPNo;
                    purchaseReturn.PurchaseTax = purchaseBills[0].PurchaseTax;
                }
                //purchaseReturn.SupplierCode = purchaseOrder.SupplierCode;
                //purchaseReturn.SupplierName = purchaseOrder.SupplierName;
                //purchaseReturn.ShipTo = purchaseOrder.ShipTo;
                //purchaseReturn.TaxType = purchaseOrder.TaxType;

                var poDetails = new PurchaseOrderBFC().RetrieveDetails(purchaseOrderID);
                var purchaseReturnDetails = new List<PurchaseReturnDetailModel>();

                foreach (var poDetail in poDetails)
                {
                    if (poDetail.Quantity > 0)
                    {
                        var detail = new PurchaseReturnDetailModel();

                        detail.PurchaseOrderItemNo = poDetail.ItemNo;
                        detail.ProductID = poDetail.ProductID;
                        detail.Barcode = poDetail.Barcode;
                        detail.ProductCode = poDetail.ProductCode;
                        detail.ProductName = poDetail.ProductName;
                        detail.ConversionID = poDetail.ConversionID;
                        detail.ConversionName = poDetail.ConversionName;
                        detail.POQuantity = Convert.ToDouble(poDetail.Quantity);
                        detail.Quantity = 0;
                        detail.Remarks = poDetail.Remarks;

                        purchaseReturnDetails.Add(detail);
                    }
                }

                purchaseReturn.Details = purchaseReturnDetails;
            }

        }

        public void Approve(long purchaseReturnID, string userName)
        {
            var purchaseReturn = RetrieveByID(purchaseReturnID);

            purchaseReturn.Status = (int)MPL.DocumentStatus.Approved;
            purchaseReturn.ApprovedBy = userName;
            purchaseReturn.ApprovedDate = DateTime.Now;

            using (TransactionScope trans = new TransactionScope())
            {
                Update(purchaseReturn);
                OnApproved(purchaseReturnID, userName);

                trans.Complete();
            }
        }

        public void Void(long purchaseReturnID, string voidRemarks, string userName)
        {
            var purchaseReturn = RetrieveByID(purchaseReturnID);
            var Status = purchaseReturn.Status;

            purchaseReturn.Status = (int)MPL.DocumentStatus.Void;
            purchaseReturn.VoidRemarks = voidRemarks;
            purchaseReturn.ApprovedBy = "";
            purchaseReturn.ApprovedDate = SystemConstants.UnsetDateTime;

            using (TransactionScope trans = new TransactionScope())
            {
                new PurchaseReturnBFC().Update(purchaseReturn);

                if (Status >= 3)
                    OnVoid(purchaseReturnID);

                trans.Complete();
            }
        }

        public List<PurchaseReturnDetailModel> RetrieveDetailsByPRID(long prID)
        {
            return new ABCAPOSDAC().RetrievePurchaseReturnDetailByPRID(prID);
        }

        public void OnApproved(long prID, string userName)
        {
            var pd = RetrieveByID(prID);

            if (pd != null)
            {
                var purchaseOrder = new PurchaseOrderBFC().RetrieveByID(pd.PurchaseOrderID);

                if (purchaseOrder != null)
                {
                    var prDetails = RetrieveDetailsByPRID(prID);
                    var poDetails = new PurchaseOrderBFC().RetrieveDetails(purchaseOrder.ID);

                    foreach (var poDetail in poDetails)
                    {
                        var query = from i in prDetails
                                    where i.ProductID == poDetail.ProductID && i.PurchaseOrderItemNo == poDetail.ItemNo
                                    select i.Quantity;

                        poDetail.CreatedReturnQuantity += query.Sum();
                    }

                    new PurchaseOrderBFC().Update(purchaseOrder, poDetails);
                    new PurchaseOrderBFC().UpdateDetails(purchaseOrder.ID, poDetails);
                }
            }
        }

        public void OnVoid(long purchaseReturnID)
        {
        }

    }
}
