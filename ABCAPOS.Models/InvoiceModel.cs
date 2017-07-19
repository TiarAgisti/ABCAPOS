using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using ABCAPOS.Util;

namespace ABCAPOS.Models
{
    public class InvoiceModel
    {
        //Table Manual
        public long ID { get; set; }
        public string Code { get; set; }
        public long SalesOrderID { get; set; }
        public string DeliveryOrderCodeList { get; set; }
        public string ResiCodeList { get; set; }
        public DateTime Date { get; set; }
        [Required]
        public DateTime DueDate { get; set; }
        public string ReceiptNo { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CreatedPaymentAmount { get; set; }
        public decimal ApprovedPaymentAmount { get; set; }
        public string VoidRemarks { get; set; }
        public string Remarks { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
        public bool HasMultiple { get; set; }
        public int Flaging { get; set; }
        public long EmployeeIDAsDireksi { get; set; }
        public string POCustomerNo { get; set; }
        //public long No { get; set; }

        //readonly
        public string SalesOrderCode { get; set; }
        public string SalesOrderTitle { get; set; }
        public string SalesReference { get; set; }
        public long CustomerID { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public long WarehouseID { get; set; }
        [Required]
        public string WarehouseName { get; set; }
        public string DepartmentName { get; set; }
        public long PaymentMethodID { get; set; }
        public string PaymentMethodName { get; set; }
        public string EmployeeNameAsDireksi { get; set; }
        public bool IsMultipleFulfilled { get; set; }
        public long CurrencyID { get; set; }
        public string CurrencyName { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal OriginalAmount { get; set; }
        //public decimal ShippingTotal { get; set; }
        public string StatusDesc { get; set; }

        public List<InvoiceDetailModel> Details { get; set; }
        public List<InvoiceResiDetailModel> ResiDetails { get; set; }

        public decimal PaymentAmount { get; set; }
        public decimal CreditedAmount { get; set; }
        public double ShippingAmount { get; set; }

        public decimal GrandTotal
        {
            get
            {
                return Amount + TaxAmount + Convert.ToDecimal(ShippingAmount);
            }
        }

        public decimal OutstandingAmount
        {
            get;
            set;
        }

        public string StatusDescription
        {
            get
            {
                if ( PaymentAmount == 0 && CreditedAmount == 0 && Status != (int)MPL.DocumentStatus.Void) //Math.Round(OutstandingAmount, 0) > 0
                    return InvoiceStatus.New.ToString();
                else if ((PaymentAmount > 0 && Math.Round(OutstandingAmount, 0) > 0) && Status != (int)MPL.DocumentStatus.Void) //|| CreditedAmount > 0)//(Status == (int)InvoiceStatus.Approved && Math.Round(OutstandingAmount, 0) > 0)
                    return "Unpaid";
                else if (Math.Round(OutstandingAmount, 0) <= 0 && Status != (int)MPL.DocumentStatus.Void)
                    return "Lunas / Paid In Full";
                else if (Status == (int)InvoiceStatus.Void)
                    return InvoiceStatus.Void.ToString();
                else
                    return "";
            }
        }

        public int DayOverDue { get; set; }

        public InvoiceModel()
        {
            Date = DateTime.Now;
            DueDate = Date.AddDays(30);
            Status = (int)InvoiceStatus.New;
        }
    }
}
