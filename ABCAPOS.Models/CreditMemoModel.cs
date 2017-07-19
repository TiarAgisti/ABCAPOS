using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using ABCAPOS.Util;

namespace ABCAPOS.Models
{
    public class CreditMemoModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        // helper field for forms (not saved)
        public long CustomerReturnID { get; set; }
        public long PriceLevelID { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public long CustomerID { get; set; }
        public string RefSO { get; set; }
        public string Title { get; set; }
        public string RefDO { get; set; }
        public string RefInv { get; set; }
        public string DeliveryOrderCodeList { get; set; }
        public string SalesOrderCodeList { get; set; }
        public DateTime SalesEffectiveDate { get; set; }
        public bool ExcludeCommisions { get; set; }
        public long DepartmentID { get; set; }
        public long WarehouseID { get; set; }
        public long SalesmanID { get; set; }
        public long EmployeeID { get; set; }
        public string ShipTo { get; set; }
        public bool Bonus { get; set; }
        public long Currency { get; set; }
        public long CurrencyID { get { return Currency; } } // TODO: credit memo currency fix
        public string CurrencyName { get { return "Rupiah"; } } // TODO: credit memo currency fix
        public decimal ExchangeRate { get; set; }
        public decimal FlatDiscount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Adjustment { get; set; }

        public string VoidRemarks { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }

        public decimal AmountApplied { get; set; }
        public string CustomerReturnCode { get; set; }
        public string CustomerReturnTitle { get; set; }
        public string DepartmentName { get; set; }
        public string SalesOrderCode { get; set; }
        public string SalesOrderTitle { get; set; }
        public string SalesReference { get; set; }
        [Required]
        public string CustomerCode { get; set; }
        [Required]
        public string CustomerName { get; set; }
        [Required]
        public string WarehouseName { get; set; }
        public string EmployeeName { get; set; }

        public long AccountID { get; set; }
        public string AccountUserCode { get; set; }
        public string AccountName { get; set; }

        public decimal TotalApplied { get; set; }
        public decimal Total { get; set; }

        public decimal TotalUnapplied
        {
            get
            {
                return Total - TotalApplied;
            }
        }
        public string StatusDescription
        {
            get
            {
                if (this.TotalApplied == 0)
                {
                    return "Not Applied";
                }
                else if (this.TotalApplied >= this.Total)
                {
                    return "Fully Applied";
                }
                else
                {
                    return "Partially Applied";
                }
            }
        }

        public String StatusDesc { get; set; }

        public List<CreditMemoDetailModel> Details { get; set; }

        public decimal GrandTotal
        {
            get
            {
                return SubTotal + TaxTotal;
            }
        }

        public decimal SubTotal
        {
            get
            {
                return this.Details.Sum(p => p.TotalAmount);
            }
        }

        public decimal TaxTotal
        {
            get
            {
                return this.Details.Sum(p => p.TotalPPN);
            }
        }


        public CreditMemoModel()
        {
            Date = SalesEffectiveDate = DateTime.Now;
            Details = new List<CreditMemoDetailModel>();
            Status = (int)InvoiceStatus.New;
        }
    }
}
