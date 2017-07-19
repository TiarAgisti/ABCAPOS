using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Models.GenericModel;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class ResiPaymentDetailModel:PSIDetailModel
    {
        //Table Manual
        public long ResiBillID { get; set; }
        public double DiscountTaken { get; set; }
        public double Amount { get; set; }
        public int ResiBillItemNo { get; set; }
        
        //Read Only
        public double Subtotal { get; set; }
        public double PaymentAmount { get; set; }
        public double OriginalAmount { get; set; }
        public double AmountDue { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentCode { get; set; }
        public DateTime PaymentEventDate { get; set; }
        public int Status { get; set; }

        public string AmountStr { get; set; }
        public DateTime ResiBillDate { get; set; }
        public string ResiBillCode { get; set; }

        //field for sub tab payment in resi
        public long ResiPaymentID
        {
            get
            {
                return HeaderID;
            }
        }
    }
}
