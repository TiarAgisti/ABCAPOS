using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class TransferOrderModel
    {
        public long ID { get; set; }
        public string Code { get; set; }

        [Required]
        public DateTime Date { get; set; }
        public string PostingPeriod {get; set; }
        public string Title { get; set; }

        [Required]
        public long FromWarehouseID { get; set; }
        [Required]
        public string FromWarehouseName { get; set; }
        
        [Required]
        public long ToWarehouseID { get; set; }
        [Required]
        public string ToWarehouseName { get; set; }

        public long DepartmentID { get; set; }
        public string DepartmentName { get; set; }

        public long StaffID { get; set; }
        public string StaffName { get; set; }
        [Required]
        public int Status { get; set; }
        public bool IsFirmed { get; set; }

        public decimal SubTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal GrandTotal { get; set; }

        //public bool HasDO { get; set; }
        //public bool IsShipped { get; set; }
        //public bool HasItemReceipt { get; set; }
        //public bool IsReceived { get; set; } // property may be redundant, one item receipt only

        // read only (from view)
        public double QtyOrdered { get; set; }
        public double QtyDelivered { get; set; }
        public double QtyReceived { get; set; }

        public string VoidRemarks { get; set; }
        
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }

        
        public List<TransferOrderDetailModel> Details { get; set; }

        public TransferOrderModel()
        {
            Date = DateTime.Now;
            Details = new List<TransferOrderDetailModel>();
        }
        [Required]
        public string StatusDescription
        {
            get
            {
                switch (Status)
                {
                    case (int)TransferOrderStatus.New:
                        return "Pending Approval";
                    case (int)TransferOrderStatus.PendingFulfillment:
                        return "Pending Fulfillment";
                    case (int)TransferOrderStatus.PendingReceipt:
                        return "Partially Fullfillment";
                    case (int)TransferOrderStatus.Received:
                        return "Received";
                    case (int)TransferOrderStatus.Void:
                    default:
                        return "Invalid";
                }
            }
        }
    }
}
