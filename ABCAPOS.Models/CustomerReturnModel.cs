using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class CustomerReturnModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        // helper field for forms (not saved)
        public long PriceLevelID { get; set; }
        [Required]
        public long CustomerID { get; set; }
        [Required]
        public string CustomerCode { get; set; }
        [Required]
        public string CustomerName { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public int Status { get; set; }
        public string RefPO { get; set; }
        public string Title { get; set; }
        public string RefSO { get; set; }
        public string RefDO { get; set; }
        public DateTime SalesEffectiveDate { get; set; }
        public long LeadSourceID { get; set; }
        public long PartnerID { get; set; }
        public bool ExcludeCommisions { get; set; }
        public long SalesOrderID { get; set; }
        public string SalesOrderCode { get; set; }
        public long DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        [Required]
        public long WarehouseID { get; set; }
        [Required]
        public string WarehouseName { get; set; }

        public long SalesmanID { get; set; }
        public string SalesName { get; set; }

        public string ShipTo { get; set; }
        [Required]
        public long EmployeeID { get; set; }
        [Required]
        public string EmployeeName { get; set; }
        public int Currency { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal AltSalesTotal { get; set; }
        public int TaxType { get; set; }
        public decimal DiscountTotal { get; set; }

        public bool HasReceipt
        {
            get
            {
                if (ReceivedQuantity > 0)
                    return true;
                else
                    return false;
            }
        }
        public bool IsFullyReceived
        {
            get
            {
                if (ReceivedQuantity == this.Quantity && HasReceipt)
                    return true;
                else
                    return false;
            }
        }
        public bool HasCredit
        {
            get
            {
                if (CreditedQuantity > 0)
                    return true;
                else
                    return false;
            }
        }
        public bool IsFullyCredited
        {
            get
            {
                if (CreditedQuantity == this.Quantity && HasCredit)
                    return true;
                else
                    return false;
            }
        }

        public string VoidRemarks { get; set; }
        public string Remarks { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }

        public bool IsReceivable
        {
            get
            {
                if (ReceivedQuantity < this.Quantity && !IsFullyReceived)
                    return true;
                else
                    return false;
            }
        }
        public bool IsCreditable
        {
            get
            {
                if (ReceivedQuantity > CreditedQuantity && !IsFullyCredited)
                    return true;
                else
                    return false;
            }
        }

        //public decimal ConversionValue { get; set; }

        public decimal SubTotal
        {
            get
            {
                return this.Details.Sum(p => p.TotalAmount);
            }
        }
        public decimal TaxValue
        {
            get
            {
                return this.Details.Sum(p => p.TotalPPN);
            }
        }
        public decimal GrandTotal
        {
            get
            {
                return SubTotal + TaxValue;
            }
        }
        public double Quantity
        {
            get
            {
                return this.Details.Sum(p => p.Quantity);
            }
        }
        public double ReceivedQuantity
        {
            get
            {
                return this.Details.Sum(p => p.ReceivedQuantity);
            }
        }
        public double CreditedQuantity
        {
            get
            {
                return this.Details.Sum(p => p.CreditedQuantity);
            }
        }

        public string SalesReference { get; set; }

        public long ReferenceID1 { get; set; }
        public string ReferenceCode1 { get; set; }

        public long ReferenceID2 { get; set; }
        public string ReferenceCode2 { get; set; }

        public long ReferenceID3 { get; set; }
        public string ReferenceCode3 { get; set; }

        public long ReferenceID4 { get; set; }
        public string ReferenceCode4 { get; set; }

        public long ReferenceID5 { get; set; }
        public string ReferenceCode5 { get; set; }

        public long ReferenceID6 { get; set; }
        public string ReferenceCode6 { get; set; }

        //public decimal SOTotal { get; set; }

        public string StatusDescription
        {
            get
            {
                string receiptStatus = "";
                string creditStatus = "";
                string paymentStatus = "";

                if (this.Status == (int)MPL.DocumentStatus.Void)
                {
                    return "Void";
                }

                if (this.Status == (int)MPL.DocumentStatus.New)
                {
                    return "Pending Approval";
                }

                if (this.ReceivedQuantity == 0)
                {
                    receiptStatus = "Pending Fulfillment";
                }
                else if (IsFullyReceived)
                {
                    receiptStatus = "";
                }
                else
                {
                    receiptStatus = "Partially Fulfilled";
                }

                if (IsFullyCredited)
                {
                    creditStatus = "Fully Credited";
                }
                else if (CreditedQuantity == 0)
                {
                    creditStatus = "Pending Credit";
                }
                else
                {
                    creditStatus = "Partially Credited";
                }

                var combinator1 = "";
                if (receiptStatus.Length > 0 && creditStatus.Length > 0)
                    combinator1 = " / ";
                var combinator2 = "";
                if (creditStatus.Length > 0 && paymentStatus.Length > 0)
                    combinator2 = " / ";
                else if (creditStatus.Length == 0
                    && (receiptStatus.Length > 0 && paymentStatus.Length > 0))
                    combinator1 = " / ";

                return receiptStatus
                    + combinator1 + creditStatus
                    + combinator2 + paymentStatus;
            }
        }

        public List<CustomerReturnDetailModel> Details { get; set; }


        public string CurrencyDescription
        {
            get
            {
                return Convert.ToString((Util.Currency)Currency);
            }
        }

        public CustomerReturnModel()
        {
            Date = SalesEffectiveDate = DateTime.Now;

            ExchangeRate = 1;
            Currency = (int)Util.Currency.IDR;
            TaxType = (int)Util.TaxType.NonTax;
            Status = (int)MPL.DocumentStatus.New;
            Details = new List<CustomerReturnDetailModel>();

        }
    }
}
