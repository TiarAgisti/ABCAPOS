using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class ApplyBillCreditModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        public long BillCreditID { get; set; }
        public long VendorID { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public long CurrencyID { get; set; }
        [Required]
        public decimal ExchangeRate { get; set; }
        public string Title { get; set; }
        public string VoidRemarks { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }

        public string BillCreditCode { get; set; }
        public string VendorName { get; set; }
        public string CurrencyName { get; set; }
        public decimal TotalAmount { get; set; }

        // Amount of credit that can be used (in case of void / edit)
        public decimal CeilingAmount { get; set; }

        // Helper field for Display / Javascript purposes
        public decimal CreditRemaining { get; set; }

        public List<ApplyBillCreditDetailModel> Details { get; set; }

        public string CurrencyDescription
        {
            get
            {
                return CurrencyName;
            }
        }

        public ApplyBillCreditModel()
        {
            Date = DateTime.Now;

            ExchangeRate = 1;
            CurrencyID = (int)Util.Currency.IDR;
            Status = (int)MPL.DocumentStatus.New;
            Details = new List<ApplyBillCreditDetailModel>();
        }
    }
}
