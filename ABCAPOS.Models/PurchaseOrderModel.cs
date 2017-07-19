using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class PurchaseOrderModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        public long BookingOrderID { get; set; }
        public string BookingOrderCode { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public DateTime DueDate { get; set; }
        
        public string Title { get; set; }
        public long SupplierID { get; set; }

        [Required]
        public long WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public long DepartmentID { get; set; }
        public long EmployeeID { get; set; }

        public string VendorCode { get; set; }
        [Required]
        public string VendorName { get; set; }
        public int Terms { get; set; }
        public string TaxNumber { get; set; }
        [Required]
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }

        public string POSupplierNo { get; set; }
        public DateTime POSupplierDate { get; set; }
        public long PaymentMethodID { get; set; }
        public string PaymentMethodName { get; set; }
        public string ShipTo { get; set; }
        public int TaxType { get; set; }
        public long CurrencyID { get; set; }
        public string CurrencyName { get; set; }
        [Required]
        public decimal ExchangeRate { get; set; }
        public bool HasPD { get; set; }
        public bool IsPDFulfilled { get; set; }
        public bool HasPB { get; set; }
        public bool IsPBFulfilled { get; set; }

        public bool CopyCurrencyValueToMaster { get; set; }

        public decimal CostExpedition { get; set; }
        public decimal ConversionValue { get; set; }
        public string Goods { get; set; }
        public string Shipment { get; set; }
        public string VoidRemarks { get; set; }
        public string Remarks { get; set; }
        public int Status { get; set; }
        public decimal CreatedPaymentAmount { get; set; }
        public decimal ApprovedPaymentAmount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
        public double Quantity { get; set; }
        public double CreatedPDQuantity { get; set; }
        public double CreatedPBQuantity { get; set; }

        public bool IsReceivable { get; set; }
        public bool IsBillable { get; set; }
        public bool IsPayable { get; set; }
        public String StatusDescription { get; set; }

        [Required]
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string ContactPerson { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxValue { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal Discount { get; set; }

        //[Required]
        public string EmailTo { get; set; }
        public long TermsID { get; set; }
        public string TermsOfName { get; set; }

        public decimal POTotal { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal POTotalDollar { get; set; }
        public decimal DiscountTotalDollar { get; set; }

        public List<PurchaseOrderDetailModel> Details { get; set; }

        public decimal OutstandingAmount
        {
            get
            {
                return GrandTotal - CreatedPaymentAmount;
            }
        }

        public decimal USDGrandTotal
        {
            get
            {
                if (CurrencyID == (int)Util.Currency.IDR)
                    return 0;
                else
                    return GrandTotal / ExchangeRate;
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

        //public string StatusDescription
        //{
        //    get
        //    {
        //        var ReceiptStat = "";
        //        var BillStat = "";
        //        var PayStat = "";
                
        //        // Receipt
        //        if (Status == (int)MPL.DocumentStatus.New)
        //            return "Pending Approval";
        //        else if (Status == (int)MPL.DocumentStatus.Void)
        //            return "Void";

        //        if (ReceiptStatus == (int)GenericStatus.None)
        //            ReceiptStat = "Pending Receipt";
        //        else if (ReceiptStatus == (int)GenericStatus.Partial)
        //            ReceiptStat = "Partially Received";
        //        else
        //            ReceiptStat = "";

        //        // Billing
        //        var combinator1 = "";

        //        if (ReceiptStatus == (int)GenericStatus.None)
        //            BillStat = "";
        //        else if (BillStatus == (int)GenericStatus.Full)
        //            BillStat = "Fully Billed";
        //        else 
        //            BillStat = "Pending Billing";

        //        if (ReceiptStat.Length > 0 && BillStat.Length > 0)
        //        {
        //            combinator1 = " / ";
        //        }

        //        // Payment
        //        var combinator2 = "";

        //        if (BillStatus == (int)GenericStatus.None)
        //            PayStat = "";
        //        else if (PaymentStatus == (int)GenericStatus.Full)
        //            PayStat = "Paid in Full";
        //        else
        //            PayStat = "Pending Payment";

        //        if (PayStat.Length > 0 && BillStat.Length > 0)
        //        {
        //            combinator2 = " / ";
        //        }

        //        return (ReceiptStat + combinator1 + BillStat + combinator2 + PayStat);
        //    }
        //}

        public string CurrencyDescription
        {
            get
            {
                return Convert.ToString((Util.Currency)CurrencyID);
            }
        }

        public PurchaseOrderModel()
        {
            Date = POSupplierDate = DueDate = DateTime.Now;

            ExchangeRate = 1;
            CurrencyID = (int)Util.Currency.IDR;
            TaxType = (int)Util.TaxType.NonTax;
            Status = (int)MPL.DocumentStatus.New;
            Details = new List<PurchaseOrderDetailModel>();
        }
    }
}
