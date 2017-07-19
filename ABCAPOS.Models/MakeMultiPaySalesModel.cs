using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class MakeMultiPaySalesModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        public long CustomerID { get; set; }
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
        public long DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public bool IsBonus { get; set; }
        public bool IsDeposit { get; set; }

        public decimal TotalPayment { get; set; }
      
        public string Account { get; set; }
        [Required]
        public long AccountID { get; set; }
        [Required]
        public string AccountName { get; set; }

        public string VoidRemarks { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
        
        [Required]
        public string WarehouseName { get; set; }
        [Required]
        public string CustomerName { get; set; }
        public string CurrencyName { get; set; }
        public decimal TotalAmount { get; set; }
        public int PaymentMethodInvoice { get; set; }

        // helper for Javascript
        public decimal AmountHelp { get; set; }
        public decimal AmountChecked { get; set; }
        public long No { get; set; }

        public string PaymentMethodInvoiceDesc { get; set; }

        public List<MakeMultiPaySalesDetailModel> Details { get; set; }

        public string CurrencyDescription
        {
            get
            {
                return CurrencyName;
            }
        }

        public string StatusCreated
        {
            get
            {
                return CreatedBy + " - on :" + CreatedDate;
            }
        }

        public string StatusModified
        {
            get
            {
                return ModifiedBy + " - on :" + ModifiedDate;
            }
        }

        public MakeMultiPaySalesModel()
        {
            Date = PaymentEventDate = DateTime.Now;
            PaymentMethodInvoice = 1;
            ExchangeRate = 1;
            CurrencyID = (int)Util.Currency.IDR;
            Status = (int)MPL.DocumentStatus.New;
            Details = new List<MakeMultiPaySalesDetailModel>();
        }
    }
}
