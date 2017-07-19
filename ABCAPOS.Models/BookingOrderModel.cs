using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class BookingOrderModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        public string BookingNo { get; set; }

        [Required]
        public DateTime Date { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public long SupplierID { get; set; }
        [Required]
        public long WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public string VendorCode { get; set; }
        [Required]
        public string VendorName { get; set; }
        public string TaxNumber { get; set; }
        public bool HasPO { get; set; }
        public bool IsPOFulfilled { get; set; }
        public bool IsPurchaseable { get; set; }
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
        public double CreatedPOQuantity { get; set; }
        public double CreatedPDQuantity { get; set; }
        public double CreatedPBQuantity { get; set; }

        public decimal ConversionValue { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxValue { get; set; }
        public decimal GrandTotal { get; set; }
        public string StatusDescription { get; set; }
        //{
        //    get
        //    {
        //        if (Status == (int)MPL.DocumentStatus.New)
        //            return "New";
        //        else if (Status == (int)MPL.DocumentStatus.Approved)
        //            return "Approved";
        //        else if (Status == (int)MPL.DocumentStatus.Void)
        //            return "Void";
        //        else
        //            return "";
        //    }
        //}

        public List<BookingOrderDetailModel> Details { get; set; }
        
        public BookingOrderModel()
        {
            Date =  DateFrom = DateTo = DateTime.Now;
            Status = (int)MPL.DocumentStatus.New;
            Details = new List<BookingOrderDetailModel>();
        }
    }
}
