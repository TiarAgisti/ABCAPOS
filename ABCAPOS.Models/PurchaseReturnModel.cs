using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class PurchaseReturnModel
    {
        public long ID { get; set; }
        public long PurchaseOrderID { get; set; }
        public string Code { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string VoidRemarks { get; set; }
        public string Remarks { get; set; }
        public string DepartmentName { get; set; }
        public string WarehouseName { get; set; }

        public decimal Amount { get; set; }
        public int Status { get; set; }
        public int StatusPosition { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }

        [Required]
        public string PurchaseOrderCode { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string POSupplierNo { get; set; }
        public string SupplierFPNo { get; set; }
        public bool PurchaseTax { get; set; }
        public string VendorCode { get; set; }
        public string VendorName { get; set; }
        public string ShipTo { get; set; }
        public string ContactPerson { get; set; }
        public string SupplierPhone { get; set; }
        public string PaymentMethodName { get; set; }
        public int TaxType { get; set; }
        public decimal Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal GrandTotal
        {
            get
            {
                return Price + Price;
            }
        }
        public List<PurchaseReturnDetailModel> Details { get; set; }

        public string StatusDescription
        {
            get
            {
                if (Status == (int)MPL.DocumentStatus.New)
                    return MPL.DocumentStatus.New.ToString();
                else if (Status == (int)MPL.DocumentStatus.Approved)
                    return MPL.DocumentStatus.Approved.ToString();
                else if (Status == (int)MPL.DocumentStatus.Void)
                    return MPL.DocumentStatus.Void.ToString();
                else
                    return "";
            }
        }

        public string StatusPositionDesc
        {
            get
            {
                if (StatusPosition == (int)PurchaseReturnPosition.Returned)
                    return "Returned";
                else if (StatusPosition == (int)PurchaseReturnPosition.Recycled)
                    return "Defect";
                else
                    return "";
            }
        }

        public PurchaseReturnModel()
        {
            Date = DateTime.Now;
            Status = (int)MPL.DocumentStatus.New;
            StatusPosition = 0;

            Details = new List<PurchaseReturnDetailModel>();
        }
    }
}
