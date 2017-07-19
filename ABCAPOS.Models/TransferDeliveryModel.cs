using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class TransferDeliveryModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        public long TransferOrderID { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public string PostingPeriod { get; set; }
        public string POCustomer { get; set; }
        [Required]
        public long StaffID { get; set; }
        public string Remarks { get; set; }
        public long DriverID { get; set; }
        public DateTime? SJReceiveDate { get; set; }
        public bool HasSJKembali { get; set; }
        public bool IsPrinted { get; set; }
        public string VoidRemarks { get; set; }
        public int Status { get; set; }
        public int Flaging { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
        
        public string TransferOrderCode { get; set; }
        public string TransferOrderTitle { get; set; }
        [Required]
        public string StaffName { get; set; }

        public long FromWarehouseID { get; set; }
        public string FromWarehouseName { get; set; }

        public long ToWarehouseID { get; set; }
        public string ToWarehouseName { get; set; }
        
        public string DriverName { get; set; }

        public double Quantity { get; set; }

        public long LogID { get; set; }

        public List<TransferDeliveryDetailModel> Details { get; set; }

        public string StatusDescription
        {
            get
            {
                if (Status == (int)DeliveryOrderStatus.New)
                    return "Picked";
                else if (Status == (int)DeliveryOrderStatus.Packed)
                    return "Packed";
                else if (Status == (int)DeliveryOrderStatus.Shipped)
                    return "Shipped";
                else if (Status == (int)MPL.DocumentStatus.Void)
                    return "Void";
                else
                    return "";
            }
        }

        public TransferDeliveryModel()
        {
            Date = DateTime.Now;
            SJReceiveDate = DateTime.Now;

            Status = (int)MPL.DocumentStatus.New;

            Details = new List<TransferDeliveryDetailModel>();
        }
    }
}
