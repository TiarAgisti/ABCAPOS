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
using System.Transactions;

namespace ABCAPOS.BF
{
    public class ResiBFC:PSIMasterDetailBFC<Resi,v_Resi,ResiDetail,v_ResiDetail,ResiModel,ResiDetailModel>
    {
        private void UpdateDeliveryOrder(ResiModel header,int status)
        {
            var details = base.RetrieveDetails(header.ID);
            foreach (var detail in details)
            {
                var deliveryOrder = new DeliveryOrderBFC().RetrieveByID(detail.DeliveryOrderID);
                if (deliveryOrder != null)
                {
                    if (status == (int)ResiStatus.Void)
                    {
                        deliveryOrder.HasResi = false;
                        new DeliveryOrderBFC().Update(deliveryOrder);
                    }
                    else
                    {
                        deliveryOrder.HasResi = true;
                        new DeliveryOrderBFC().Update(deliveryOrder);
                    }
                }
            }
        }
        public override void Create(ResiModel header, List<ResiDetailModel> details)
        {
            var check = this.RetrieveResiCode(header.Code);
            if (check == null)
            {
                base.Create(header, details);
                this.UpdateDeliveryOrder(header, header.Status);
            }
            else
            {
                throw new Exception("Nomer Resi sudah pernah di input,silahkan input nomer resi yg berbeda");
            }
        }
        public void SetDateFromDateTo(ResiModel header)
        {
            string month = DateTime.Now.Month.ToString();
            string year = DateTime.Now.Year.ToString();
            DateTime startDate = Convert.ToDateTime(month + "/1/" + year);
            DateTime endDate = (startDate.AddMonths(1)).AddDays(-1);
            header.DateFrom = startDate;
            header.DateTo = endDate;
        }
        public void PrepareByCustomerID(ResiModel header, long customerID,long expeditionID, string dateFrom, string dateTo, string date,string resiCode)
        {
            var customer = new CustomerBFC().RetrieveByID(customerID);
            var expedition = new ExpeditionBFC().RetrieveByID(expeditionID);

            if (string.IsNullOrEmpty(dateFrom))
            {
                this.SetDateFromDateTo(header);
            }
            else
            {
                DateTime startDate = Convert.ToDateTime(dateFrom);
                DateTime endDate = Convert.ToDateTime(dateTo);
                DateTime dateTrans = Convert.ToDateTime(date);

                header.Date = dateTrans;
                header.DateFrom = startDate;
                header.DateTo = endDate;
            }

            if (customer != null && expedition !=null)
            {
                header.ExpeditionID = expeditionID;
                header.ExpeditionName = expedition.Name;
                header.Code = resiCode;

                header.CustomerID = customerID;
                header.CustomerName = customer.Name;
                header.BillingAddress1 = customer.BillingAddress1;
                var details = new List<ResiDetailModel>();
                var doDetails = new DeliveryOrderBFC().RetrieveDOByCustomerIDExpeditionIDStartDateEndDate(customerID,expeditionID,header.DateFrom,header.DateTo);
                foreach (var doDetail in doDetails)
                {
                    var detail = new ResiDetailModel();
                    detail.DeliveryOrderID = doDetail.ID;
                    detail.DeliveryOrderCode = doDetail.Code;
                    detail.DeliveryOrderDate = doDetail.Date;
                    detail.CustomerName = doDetail.CustomerName;

                    details.Add(detail);
                }
                header.Details = details;

                var resiprices = new ExpeditionBFC().RetrieveDetails(header.ExpeditionID);
                var priceDetails = new List<ResiPriceDetailModel>();
                foreach (var priceDetail in resiprices)
                {
                    var resiPrice = new ResiPriceDetailModel();
                    resiPrice.ExpeditionItemNo = priceDetail.ItemNo;
                    resiPrice.UnitName = priceDetail.UnitName;
                    resiPrice.Price = priceDetail.Price;

                    priceDetails.Add(resiPrice);
                }
                header.ResiPriceDetails = priceDetails;
            }
        }
        public void ApproveResi(string key, string userName, int roleID)
        {
            base.Approve(key, userName, roleID);
            var obj = base.RetrieveByID(Convert.ToUInt64(key));
            if (obj != null)
            {
                this.UpdateDeliveryOrder(obj, obj.Status);
            }
        }
        public void VoidResi(ResiModel header, string userName)
        {
            var obj = base.RetrieveByID(header.ID);
            if (obj != null)
            {
                obj.VoidRemarks = header.VoidRemarks;
                obj.Status = (int)ResiStatus.Void;
                obj.VoidedBy = userName;
                obj.VoidedDate = DateTime.Now;
                base.Update(obj);
                this.UpdateDeliveryOrder(obj, obj.Status);
            }
        }
        public ResiModel RetrieveResiCode(string resiCode)
        {
            return new ABCAPOSDAC().RetrieveResiCode(resiCode);
        }
        public List<ResiModel> RetrieveResiByCustomerID(long customerID)
        {
            return new ABCAPOSDAC().RetrieveResiByCustomerID(customerID);
        }
        public List<ResiModel> RetrieveUncreatedResiBill(int startIndex, int? amount, string sortParameter, List<SelectFilter> selectFilters)
        {
            if (amount == null)
                amount = SystemConstants.ItemPerPage;

            return new ABCAPOSDAC().RetrieveUncreatedResiBillResi(startIndex, (int)amount, sortParameter, selectFilters);
        }
        public int RetrieveUncreatedResiBillCount(List<SelectFilter> selectFilters)
        {
            return new ABCAPOSDAC().RetrieveUncreatedResiBillResiCount(selectFilters);
        }
        public void UpdateStatusResi(long ResiID, int statusbaru)
        {
            var Resi = RetrieveByID(ResiID);
            Resi.Status = statusbaru;
            using (TransactionScope trans = new TransactionScope())
            {
                Update(Resi);
                trans.Complete();
            }
        }

    }
}
