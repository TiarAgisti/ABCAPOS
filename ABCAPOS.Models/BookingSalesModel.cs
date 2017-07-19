using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class BookingSalesModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        public string BookingNo { get; set; }

        [Required]
        public DateTime Date { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public long CustomerID { get; set; }
        [Required]
        public string CustomerName { get; set; }
        public string Title { get; set; }
        [Required]
        public long WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public string VendorCode { get; set; }
        [Required]
        public string VendorName { get; set; }
        public string TaxNumber { get; set; }
        public bool HasSO { get; set; }
        public bool IsSOFulfilled { get; set; }
        public bool IsSaleable { get; set; }
        public string VoidRemarks { get; set; }
        public string Remarks { get; set; }
        public int Status { get; set; }
        public long PriceLevelID { get; set; }
        public string PriceLevelName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
        public double Quantity { get; set; }
        public double CreatedSOQuantity { get; set; }
        public double CreatedDOQuantity { get; set; }
        public double CreatedInvQuantity { get; set; }

        public decimal ConversionValue { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxValue { get; set; }
        public decimal GrandTotal { get; set; }

        public string StatusDescription { get; set; }
        public string StatusDesc { get; set; }
        [Required]
        public string EmailTo { get; set; }

        public List<BookingSalesDetailModel> Details { get; set; }

        public BookingSalesModel()
        {
            Date = DateFrom = DateTo = DateTime.Now;
            Status = (int)MPL.DocumentStatus.New;
            Details = new List<BookingSalesDetailModel>();
        }


        
    }
}
