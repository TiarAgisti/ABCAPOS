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
using ABCAPOS.BF.GenericBFC;

namespace ABCAPOS.BF
{
    public class ResiBillBFC : PSIMasterDetailBFC<ResiBill, v_ResiBill, ResiBillDetail, v_ResiBillDetail, ResiBillModel, ResiBillDetailModel>
    {
        private void UpdateStatusResi(List<ResiBillDetailModel> details,int status)
        {
            foreach (ResiBillDetailModel detail in details)
            {
                ResiModel resi = new ResiBFC().RetrieveByID(detail.ResiID);
                if (resi != null)
                {
                    resi.Status = status;
                    new ResiBFC().Update(resi);
                }
            }
        }
        public override string GenerateID()
        {
            throw new NotImplementedException();
        }
        public void PrepareByResi(ResiBillModel rb, long resiID)
        {
            var Resi = new ResiBFC().RetrieveByID(resiID);
            if (Resi != null)
            {

                rb.ExpeditionID = Resi.ExpeditionID;
                rb.ExpeditionName = Resi.ExpeditionName;
                rb.ExchangeRate = 1;


                var rbDetails = new ResiBFC().RetrieveAll().Where(e => e.Status == (int)ResiStatus.PendingBilling && e.ExpeditionID == Resi.ExpeditionID);
                var ResiDetails = new List<ResiBillDetailModel>();
                foreach (var rbDetail in rbDetails)
                {
                    var detail = new ResiBillDetailModel();
                    detail.ResiID = rbDetail.ID;
                    detail.ResiCode = rbDetail.Code;
                    detail.Amount = rbDetail.Amount;
                    detail.CustomerID = rbDetail.CustomerID;
                    detail.CustomerName = rbDetail.CustomerName;
                    rb.Details.Add(detail);

                }
                rb.TotalPriceAmount = rbDetails.Sum(e => e.Amount);
                
          }
            
        }
        public override void Create(ResiBillModel header, List<ResiBillDetailModel> details)
        {
            base.Create(header, details);
            this.UpdateStatusResi(details, (int)ResiStatus.FullBilling);
        }
        public void Approve(long ResiBillID, string userName)
        {

            var ResiBill = RetrieveByID(ResiBillID);

            var details = RetrieveDetails(ResiBillID);

            ResiBill.Status = (int)ResiBillStatus.PendingPayment;

            ResiBill.ApprovedBy = userName;

            ResiBill.ApprovedDate = DateTime.Now;

            Update(ResiBill);              
        }
        public int RetreiveListCountBuild(List<SelectFilter> selectFilters, dynamic ViewBag)
        {
            if ((bool)ViewBag.AllowViewFG)
            {
                return new ABCAPOSDAC().RetreiveListBuildCountResi(selectFilters);
            }
            else
            {
                return new ABCAPOSDAC().RetreiveListBuildCount(selectFilters);
            }

        }
        public void Void(long ResiBillID, string voidRemarks, string userName)
        {
            var obj = RetrieveByID(ResiBillID);

            var details = RetrieveDetails(ResiBillID);

            obj.VoidRemarks = voidRemarks;
            obj.Status = (int)ResiBillStatus.Void;

            using (TransactionScope trans = new TransactionScope())
            {
                
                base.Update(obj);

                this.UpdateStatusResi(details, (int)ResiStatus.PendingBilling);

                trans.Complete();
            }

        }
        public int RetrieveCount(List<SelectFilter> selectFilters, bool showVoidDocuments)
        {
            return new ABCAPOSDAC().RetrieveResiBillCount(selectFilters, showVoidDocuments);
        }
        public List<ResiBillModel> Retrieve(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters, bool showVoidDocuments)
        {
            return new ABCAPOSDAC().RetrieveResiBill(startIndex, amount, sortParameter, selectFilters, showVoidDocuments);
        }
        public List<ResiBillModel> RetrieveUncreatedResiPayment(int startIndex, int amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveUncreatedResiPayment(startIndex, amount, sortParameter, selectFilters);
        }
        public int RetrieveUncreatedResiPaymentCount(List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveUncreatedResiPaymentCount(selectFilters);
        }
        public List<ResiBillModel> RetrieveResiBillByExpeditionID(long expeditionID)
        {
            return new ABCAPOSDAC().RetrieveResiBillByExpeditionID(expeditionID);
        }
        public List<ResiBillDetailModel> RetrieveResiBillDetailByResiID(long resiID)
        {
            return new ABCAPOSDAC().RetrieveResiBillDetailByResiID(resiID);
        }
    }
}