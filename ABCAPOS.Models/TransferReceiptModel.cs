using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class TransferReceiptModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        public long TransferOrderID { get; set; }
        public string SupplierRef { get; set; }
        //public long TransferDeliveryID { get; set; }
        public long PostingPeriodID { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public string Memo { get; set; }
        public long StaffID { get; set; }
        public long DepartmentID { get; set; }
        public string VoidRemarks { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }

        public string TransferOrderCode { get; set; }
        public string TransferOrderTitle { get; set; }

        public string StaffName { get; set; }

        public long FromWarehouseID { get; set; }
        public string FromWarehouseName { get; set; }

        public long ToWarehouseID { get; set; }
        public string ToWarehouseName { get; set; }

        public long LogID { get; set; }
        public List<TransferReceiptDetailModel> Details { get; set; }

        //public string StatusDescription
        //{
        //    get
        //    {
        //        if (Status == (int)DeliveryOrderStatus.New)
        //            return "Picked";
        //        else if (Status == (int)DeliveryOrderStatus.Packed)
        //            return "Packed";
        //        else if (Status == (int)DeliveryOrderStatus.Shipped)
        //            return "Shipped";
        //        else if (Status == (int)MPL.DocumentStatus.Void)
        //            return "Void";
        //        else
        //            return "";
        //    }
        //}

        public TransferReceiptModel()
        {
            Date = DateTime.Now;

            Status = (int)MPL.DocumentStatus.New;

            Details = new List<TransferReceiptDetailModel>();
        }
    }
}
