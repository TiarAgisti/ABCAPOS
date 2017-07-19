using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class CashSalesModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        [Required]
        public DateTime Date { get; set; }

        public string Title { get; set; }
        public DateTime DeliveryDate { get; set; }
        [Required]
        public long CustomerID { get; set; }

        public string POCustomerNo { get; set; }
        public DateTime POCustomerDate { get; set; }

        public long ExpedisiID { get; set; }
        public string ExpedisiName { get; set; }

        public string SJReferenceNo { get; set; }
        public string SalesReference { get; set; }
        public long DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        [Required]
        public long WarehouseID { get; set; }
        [Required]
        public string WarehouseName { get; set; }
        [Required]
        public long EmployeeID { get; set; }
        [Required]
        public string EmployeeName { get; set; }
        public long PaymentMethodID { get; set; }
        public string PaymentMethodName { get; set; }
        public string ShipTo { get; set; }
        public long SalesmanID { get; set; }
        public int TaxType { get; set; }
        public int Currency { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal ConversionValue { get; set; }
        public string VoidRemarks { get; set; }
        public string Remarks { get; set; }
        public long PriceLevelID { get; set; }
        public string PriceLevelName { get; set; }
        [Required]
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
        public double Quantity { get; set; }
        
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

        [Required]
        public string CustomerCode { get; set; }
        [Required]
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string SalesName { get; set; }

        public decimal SubTotal { get; set; }
        public decimal TaxValue { get; set; }
        public decimal GrandTotal { get; set; }

        public decimal SOTotal { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CreatedPaymentAmount { get; set; }
        public decimal ApprovedPaymentAmount { get; set; }

        public bool IsPayable { get; set; }
        public string StatusDescription { get; set; }
        public int Flaging { get; set; }

        public long LogID { get; set; }

        public List<CashSalesDetailModel> Details { get; set; }

        public decimal USDGrandTotal
        {
            get
            {
                if (Currency == (int)Util.Currency.IDR)
                    return 0;
                else
                    return GrandTotal / ExchangeRate;
            }
        }

        public string CurrencyDescription
        {
            get
            {
                return Convert.ToString((Util.Currency)Currency);
            }
        }

        public CashSalesModel()
        {
            Date = DeliveryDate = POCustomerDate = DateTime.Now;

            ExchangeRate = 1;
            Currency = (int)Util.Currency.IDR;
            TaxType = (int)Util.TaxType.NonTax;
            Status = (int)MPL.DocumentStatus.New;
            Details = new List<CashSalesDetailModel>();
        }
    }
}
