using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class MakeMultiPayModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        public string LastCode { get; set; }
        public long VendorID { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public string CheckNo { get; set; }
        [Required]
        public long CurrencyID { get; set; }
        [Required]
        public decimal ExchangeRate { get; set; }
        public string Title { get; set; }
        [Required]
        public long WarehouseID { get; set; }
        public DateTime PaymentEventDate { get; set; }
        public bool IsFP { get; set; }

        public string VoidRemarks { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
        public long PaymentMethodID { get; set; }

        [Required]
        public string WarehouseName { get; set; }
        [Required]
        public string VendorName { get; set; }
        public string CurrencyName { get; set; }
        public decimal TotalAmount { get; set; }

        public long AccountID { get; set; }

        //ReadOnly (from sql View)
        public string PaymentMethodName { get; set; }
        public string AccountName { get; set; }

        // helper for Javascript
        public decimal AmountHelp { get; set; }

        public List<MakeMultiPayDetailModel> Details { get; set; }

        public string CurrencyDescription
        {
            get
            {
                return CurrencyName;
            }
        }

        public MakeMultiPayModel()
        {
            Date = PaymentEventDate = DateTime.Now;

            ExchangeRate = 1;
            CurrencyID = (int)Util.Currency.IDR;
            Status = (int)MPL.DocumentStatus.New;
            Details = new List<MakeMultiPayDetailModel>();
        }
    }
}
