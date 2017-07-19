using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Models.GenericModel;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class ResiPaymentModel:PSIHeaderModel
    {
        //Table Manual
        public long ExpeditionID { get; set; }
        public DateTime Date { get; set; }
        //public long CurrencyID { get; set; }
        public long CurrencyID
        {
            get
            {
                return 1;
            }
        }

        public double ExchangeRate { get; set; }
        public string Title { get; set; }
        public long WarehouseID { get; set; }
        public DateTime PaymentEventDate { get; set; }
        public bool IsFP { get; set; }
        public long PaymentMethodID { get; set; }
        public long AccountID { get; set; }

        //Read Only
        public string ExpeditionName { get; set; }
        public string CurrencyName
        {
            get
            {
                return "IDR";
            }
        }

        public double TotalAmount { get; set; }
        public string WarehouseName { get; set; }
        public string PaymentMethodName { get; set; }
        public string AccountName { get; set; }

        // helper for Javascript
        public double AmountHelp { get; set; }

        public List<ResiPaymentDetailModel> Details { get; set; }
        public List<ResiPaymentDetailModel> ResiDetails { get; set; }

        public string StatusDescription
        {
            get
            {
                if (this.Status == (int)MPL.DocumentStatus.New)
                    return "New";
                else if (this.Status == (int)MPL.DocumentStatus.Approved)
                    return "Approved";
                else if (this.Status == (int)MPL.DocumentStatus.Void)
                    return "Void";
                else
                    return "";

            }
        }

        public ResiPaymentModel()
        {
            Details = new List<ResiPaymentDetailModel>();
            ResiDetails = new List<ResiPaymentDetailModel>();
            this.Date = base.ID == 0 ? DateTime.Now : this.Date;
            this.PaymentEventDate = base.ID == 0 ? DateTime.Now : this.PaymentEventDate;
        }
    }
}
