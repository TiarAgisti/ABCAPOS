using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class VendorReturnDeliveryModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        public long VendorReturnID { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public DateTime SJReceiveDate { get; set; }
        public string ReceivedBy { get; set; }
        public string Sender { get; set; }
        public bool IsRead { get; set; }
        public string ShipTo { get; set; }
        [Required]
        public long EmployeeID { get; set; }
        [Required]
        public string EmployeeName { get; set; }
        public string VoidRemarks { get; set; }
        public string Remarks { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
        public string VendorReturnCode { get; set; }
        public string VendorReturnTitle { get; set; }
        public string POSupplierNo { get; set; }
        public DateTime POSupplierDate { get; set; }
        public string VendorCode { get; set; }
        public string VendorName { get; set; }
        [Required]
        public long WarehouseID { get; set; }
        [Required]
        public string WarehouseName { get; set; }
        public long DriverID { get; set; }
        public string DriverName { get; set; }
        public int Flaging { get; set; }
        public int SJReturn { get; set; }

        public bool HasPayment { get; set; }
        public double CreatedInvQuantity { get; set; }
        public double Quantity { get; set; }

        public long LogID { get; set; }
        public List<VendorReturnDeliveryDetailModel> Details { get; set; }

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

        public VendorReturnDeliveryModel()
        {
            Date = SJReceiveDate = DateTime.Now;
            Status = (int)MPL.DocumentStatus.New;

            Details = new List<VendorReturnDeliveryDetailModel>();
        }
    }
}
