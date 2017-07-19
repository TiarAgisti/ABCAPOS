using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class PurchaseBillModel
    {
        public long ID { get; set; }
        [Required]
        public string Code { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime PaymentEventDate { get; set; }

        public long WarehouseID { get; set; }
        public long PurchaseOrderID { get; set; }
        public string PurchaseOrderCode { get; set; }
        public string PurchaseOrderTitle { get; set; }
        public long PurchaseDeliveryID { get; set; }
        public string PurchaseDeliveryCode { get; set; }
        public string WarehouseName { get; set; }
        public string SupplierInvNo { get; set; }
        public string SupplierFPNo { get; set; }
        public bool PurchaseTax { get; set; }
        public decimal Amount
        {
            get;
            set;
        }
        public decimal TaxAmount{ get; set; }
        public decimal CreatedPaymentAmount { get; set; }
        public decimal ApprovedPaymentAmount { get; set; }

        public DateTime PurchaseOrderDate { get; set; }
        public string Remarks { get; set; }
        public string VoidRemarks { get; set; }
        public int TermOfPayment { get; set; }
        public string TermOfPaymentName { get; set; }
        public int Status { get; set; }
        public int Currency { get; set; }
        public decimal ExchangeRate { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }

        public long VendorID { get; set; }
        public string VendorCode { get; set; }
        public string VendorName { get; set; }
        public string TaxNumber { get; set; }
        public string POSupplierNo { get; set; }

        public long EmployeeID { get; set; }
        [Required]
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public double Quantity { get; set; }
        public long CurrencyID { get; set; }
        public string CurrencyName { get; set; }

        public decimal DiscountTaken { get; set; }
        public double DiscountAmount { get; set; }
        public decimal BeforePaymentAmount { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal CreditedAmount { get; set; }
        public string StatusDesc { get; set; }

        public List<PurchaseBillDetailModel> Details { get; set; }

        public string StatusDescription
        {
            get
            {
                if (OutstandingAmount <= 0)
                    return "Paid In Full";
                else if (Status == 0)
                    return "Void";
                else
                    return "Open";
            }
        }

        public string VoidDescription { get; set; }

        public double GrandTotal { get; set; }

        public decimal OutstandingAmount
        {
            get
            {
                return Convert.ToDecimal(GrandTotal) - PaymentAmount - CreditedAmount;
            }
        }

        public PurchaseBillModel()
        {
            Date = DueDate = PaymentEventDate = DateTime.Now;
            Details = new List<PurchaseBillDetailModel>();

            Status = (int)PurchaseBillStatus.New;
        }
    }
}
