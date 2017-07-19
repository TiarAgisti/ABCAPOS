using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Models.GenericModel;
using ABCAPOS.Util;

namespace ABCAPOS.Models
{
    public class ResiModel:PSIHeaderModel
    {
        //Table Only
        public long ExpeditionID { get; set; }
        public long CustomerID { get; set; }
        public long WarehouseID { get; set; }
        public DateTime Date { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string SalesOrderCodeList { get; set; }
        public string Remarks { get; set; }
        public bool IsHasInvoice { get; set; }
        public bool IsHasBill { get; set; }
        
        //Read Only
        public string ExpeditionName { get; set; }
        public string CustomerName { get; set; }
        public string BillingAddress1 { get; set; }
        public double Amount { get; set; }
        public string WarehouseName { get; set; }
        public bool IsCoverExpeditionByABCA { get; set; }

        public List<ResiDetailModel> Details { get; set; }
        public List<ResiPriceDetailModel> ResiPriceDetails { get; set; }
        //public List<ResiPaymentDetailModel> ResiPaymentDetails { get; set; }

        public string StatusDescription
        {
            get
            {
                if (this.Status == (int)ResiStatus.New)
                    return "New";
                else if (this.Status == (int)ResiStatus.PendingBilling)
                    return "Pending Billing";
                else if (this.Status == (int)ResiStatus.FullBilling)
                    return "Fully Billing";
                else
                    return "Void";
            }
        }

        public double SubTotal { get; set; }
        public double GrandTotal { get; set; }
        public double PaymentAmount { get; set; }
        public double OutStandingAmount { get; set; }


        public ResiModel()
        {
            Details = new List<ResiDetailModel>();
            ResiPriceDetails = new List<ResiPriceDetailModel>();
            this.Date = base.ID == 0 ? DateTime.Now : this.Date;
            //this.DateFrom = base.ID == 0 ? DateTime.Now : this.DateFrom;
            //this.DateTo = base.ID == 0 ? DateTime.Now : this.DateTo;
        }

    }
}
