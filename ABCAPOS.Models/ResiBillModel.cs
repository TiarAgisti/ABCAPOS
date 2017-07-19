using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Models.GenericModel;
using ABCAPOS.Util;

namespace ABCAPOS.Models
{
    public class ResiBillModel:PSIHeaderModel
    {
        //Table Manual
        //public long ResiID { get; set; }
        public long ExpeditionID { get; set; }
        public DateTime Date { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime PaymentEventDate { get; set; }
        public int TermOfPaymentID { get; set; }
        public long CurrencyID { get; set; }
        public string Remarks { get; set; }

        public List<ResiBillDetailModel> Details { get; set; }

        //Read Only
        public decimal ExchangeRate { get; set; }
        public string CurrencyName { get; set; }
        public string ExpeditionName { get; set; }
        public double TotalAmount { get; set; }
        public double TotalPriceAmount { get; set; }
        public string TermsOfPaymentName { get; set; }
        public string StatusDesc { get; set; }
        public int Status { get; set; }
        public string ResiCode { get; set; }
        public double OutStandingAmount { get; set; }
        public double PaymentAmount { get; set; }

        //for tab payment
        public double BeforePaymentAmount { get; set; }
        public double DiscountTaken { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentCode { get; set; }
       
        public ResiBillModel()
        {
            this.Date = DateTime.Now;
            this.DueDate = DateTime.Now;
            //Details = new List<ResiBillDetailModel>();
        }


    }
}
