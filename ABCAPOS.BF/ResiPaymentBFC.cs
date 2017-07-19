using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.DA;
using ABCAPOS.EDM;
using ABCAPOS.BF.GenericBFC;
using ABCAPOS.Models;
using ABCAPOS.Util;
using MPL.Business;

namespace ABCAPOS.BF
{
    public class ResiPaymentBFC:PSIMasterDetailBFC<ResiPayment,v_ResiPayment,ResiPaymentDetail,v_ResiPaymentDetail,ResiPaymentModel,ResiPaymentDetailModel>
    {
        private void UpdateStatusResiBill(ResiPaymentModel header,int status)
        {
            var details = base.RetrieveDetails(header.ID);
            foreach (var detail in details)
            {
                var resi = new ResiBillBFC().RetrieveByID(detail.ResiBillID);
                if (resi != null)
                {
                    if (status == (int)MPL.DocumentStatus.Void)
                    {
                        resi.Status = (int)ResiBillStatus.PendingPayment;
                    }
                    else
                    {
                        if (resi.OutStandingAmount == 0)
                        {
                            resi.Status = (int)ResiBillStatus.FullyPayment;
                        }
                        else
                        {
                            resi.Status = (int)ResiBillStatus.PartialyPayment;
                        }
                    }
                    new ResiBillBFC().Update(resi);
                }
            }
            
        }

        public void PrepareResiBillByID(ResiPaymentModel header, long resiBillID)
        {
            var obj = new ResiBillBFC().RetrieveByID(resiBillID);
            if (obj != null)
            {
                header.ExpeditionID = obj.ExpeditionID;
                header.ExpeditionName = obj.ExpeditionName;
                header.AmountHelp = obj.TotalAmount - obj.PaymentAmount;

                var payableResi = new ResiBillBFC().RetrieveResiBillByExpeditionID(obj.ExpeditionID);
                var resiDetails = new List<ResiPaymentDetailModel>();
                //var resiDetailsByCustomer = new List<ResiPaymentDetailModel>();

                foreach (var resiBill in payableResi)
                {
                    var detail = new ResiPaymentDetailModel();
                    detail.ResiBillID = resiBill.ID;
                    detail.ResiBillCode = resiBill.Code;
                    detail.Amount = resiBill.TotalAmount;
                    detail.OriginalAmount = resiBill.TotalAmount;
                    detail.PaymentAmount = resiBill.PaymentAmount;
                    detail.AmountDue = detail.OriginalAmount - detail.PaymentAmount;
                    detail.ResiBillDate = resiBill.Date;

                    if (detail.ResiBillID == resiBillID)
                    {
                        detail.AmountStr = detail.AmountDue.ToString("N2");
                    }

                    resiDetails.Add(detail);

                }
                header.Details = resiDetails;
            }
        }

        public void ApproveResiPayment(string key,string userName,int roleID)
        {
            base.Approve(key,userName,roleID);
            var header = base.RetrieveByID(Convert.ToInt64(key));
            if(header != null)
            {
                this.UpdateStatusResiBill(header, header.Status);
            }
        }

        public void VoidResiPayment(ResiPaymentModel header, string userName)
        {
            var obj = base.RetrieveByID(header.ID);
            if (obj != null)
            {
                obj.VoidRemarks = header.VoidRemarks;
                obj.Status = (int)ResiBillStatus.Void;
                obj.VoidedBy = userName;
                obj.VoidedDate = DateTime.Now;
                base.Update(obj);
                this.UpdateStatusResiBill(obj, obj.Status);
            }
        }

        public override void Create(ResiPaymentModel header, List<ResiPaymentDetailModel> details)
        {
            var itemNo = 1;
            var detailsResi = new List<ResiPaymentDetailModel>();

            foreach (var detail in header.Details)
            {
                detail.ItemNo = itemNo++;
                detailsResi.Add(detail);
            }

            foreach (var detailResi in header.ResiDetails)
            {
                detailResi.ItemNo = itemNo++;
                detailsResi.Add(detailResi);
            }

            details = detailsResi;

            base.Create(header, details);
        }

        public ResiPaymentModel RetrieveResiPaymentCode(string resipaymentCode)
        {
            return new ABCAPOSDAC().RetrieveResiPaymentCode(resipaymentCode);
        }

        public List<ResiPaymentDetailModel> RetrieveResiPaymentDetailByResiBillID(long resibillID)
        {
            return new ABCAPOSDAC().RetrieveResiPaymentDetailByResiBillID(resibillID);
        }
    }
}
