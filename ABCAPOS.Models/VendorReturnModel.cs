using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class VendorReturnModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
        public long CurrencyID { get; set; }
        [Required]
        public decimal ExchangeRate { get; set; }
        public long PurchaseOrderID { get; set; }
        public string SupplierReference { get; set; }

        public string Title { get; set; }
        public long SupplierID { get; set; }

        public long DepartmentID { get; set; }
        public long ClassID { get; set; }
        [Required]
        public long WarehouseID { get; set; }
        public long ItemProductID { get; set; }
        public string SupplierFPNo { get; set; }

        public bool IsFPPembelian { get; set; }
        public long GroupWarnaID { get; set; }

        public long EmployeeID { get; set; }

        public string VendorCode { get; set; }
        [Required]
        public string VendorName { get; set; }
        public string WarehouseName { get; set; }

        public string TaxNumber { get; set; }
        [Required]
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }

        public string POSupplierNo { get; set; }
        public DateTime POSupplierDate { get; set; }

        public string ShipTo { get; set; }
        public int TaxType { get; set; }
        public decimal CreatedPaymentAmount { get; set; }
        public decimal ApprovedPaymentAmount { get; set; }

        public string CurrencyName { get; set; }

        public bool IsDeliveryFulfilled { get; set; }
        public bool HasDelivery { get; set; }


        public bool IsCreditFulfilled { get; set; }
        public bool HasCredit { get; set; }


        public string VoidRemarks { get; set; }
        public string Remarks { get; set; }
        public int Status { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
        public double Quantity { get; set; }
        public double CreatedDeliveryQuantity { get; set; }
        public double CreatedCreditQuantity { get; set; }

        public bool IsDeliverable { get; set; }
        public bool IsCreditable
        {
            get
            {
                if (CreatedCreditQuantity < CreatedDeliveryQuantity)
                    return true;
                else
                    return false;
            }
        }
        public bool IsPayable { get; set; }
        public String StatusDescription { get; set; }

        public string PurchaseOrderCode { get; set; }

        [Required]
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string ContactPerson { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxValue { get; set; }
        public decimal GrandTotal { get; set; }

        public List<VendorReturnDetailModel> Details { get; set; }

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

        public string CurrencyDescription
        {
            get
            {
                return Convert.ToString((Util.Currency)CurrencyID);
            }
        }

        public VendorReturnModel()
        {
            Date = POSupplierDate = DateTime.Now;

            ExchangeRate = 1;
            CurrencyID = (int)Util.Currency.IDR;
            TaxType = (int)Util.TaxType.NonTax;
            Status = (int)MPL.DocumentStatus.New;
            Details = new List<VendorReturnDetailModel>();
        }
    }
}
