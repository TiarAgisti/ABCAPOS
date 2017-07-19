using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class BillCreditModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        [Required]
        public long VendorID { get; set; }
        //public decimal Total { get; set; }
        //public decimal SubTotal {get; set; }
        public long VendorReturnID { get; set; }
        public long CurrencyID { get; set; }
        [Required]
        public decimal ExchangeRate { get; set; }
        public string TaxNumber { get; set; }

        //public decimal TaxAmount 

        [Required]
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string SupplierReference { get; set; }

        public long DepartmentID { get; set; }

        [Required]
        public long WarehouseID { get; set; }
        public bool IsFakturPajak { get; set; }
        public string SupplierFPNo { get; set; }

        public decimal TotalApplied { get; set; }
        public decimal TotalUnapplied
        {
            get
            {
                return Total - TotalApplied;
            }
        }

        public string VoidRemarks { get; set; }

        public long AccountID { get; set; }
        public string AccountUserCode { get; set; }
        public string AccountName { get; set; }

        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
        public string StatusDescription
        {
            get
            {
                if (this.TotalApplied == 0 && this.Status == 1)
                {
                    return "Not Applied";
                }
                else if (this.TotalApplied == this.Total)
                {
                    return "Fully Applied";
                }
                else if (this.TotalApplied == 0 && this.Status == 0)
                {
                    return "Void";
                }
                else
                {
                    return "Partially Applied";
                }
            }
        }

        public string VendorReturnCode { get; set; }
        [Required]
        public string VendorCode { get; set; }
        [Required]
        public string VendorName { get; set; }
        [Required]
        public string WarehouseName { get; set; }
        public string CurrencyName { get; set; }
        public string DepartmentName { get; set; }

        public List<BillCreditDetailModel> Details { get; set; }

        public decimal SubTotal
        {
            get
            {
                return this.Details.Sum(p => p.TotalAmount);
            }
        }

        public decimal TaxAmount
        {
            get
            {
                return this.Details.Sum(p => p.TotalPPN);
            }
        }

        public decimal Total { get; set; }
        //{
        //    get
        //    {
        //        return this.Details.Sum(p => p.Total);
        //    }
        //}

        public string CurrencyDescription
        {
            get
            {
                return CurrencyName;
            }
        }

        public BillCreditModel()
        {
            Date = DateTime.Now;

            ExchangeRate = 1;
            CurrencyID = (int)Util.Currency.IDR;
            Status = (int)MPL.DocumentStatus.New;
            Details = new List<BillCreditDetailModel>();
        }
    }
}
