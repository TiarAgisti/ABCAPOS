using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class PurchaseDeliveryModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public DateTime DeliveryDate { get; set; }

        public long PurchaseOrderID { get; set; }
        public string PurchaseOrderCode { get; set; }
        public string PurchaseOrderTitle { get; set; }
        public long WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public DateTime PurchaseOrderDate { get; set; }
        public string Remarks { get; set; }
        public string VoidRemarks { get; set; }
        public int Status { get; set; }
        public int Currency { get; set; }
        public decimal ExchangeRate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
        public double CreatedPBQuantity { get; set; }
        public double Quantity { get; set; }

        public string VendorCode { get; set; }
        public string VendorName { get; set; }
        public string POSupplierNo { get; set; }

        public long SupplierID { get; set; }
        public long EmployeeID { get; set; }

        [Required]
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public long DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public long CurrencyID { get; set; }
        public string CurrencyName { get; set; }
        public bool IsBillable { get; set; }
        //public long LogStockID { get; set; }

        public long LogID { get; set; }
        public long ContainerID { get; set; }
        public List<PurchaseDeliveryDetailModel> Details { get; set; }
        //public List<LogStockDetailModel> LogStockDetails { get; set; }

        public string StatusDescription
        {
            get
            {
                if (Status == (int)PurchaseDeliveryStatus.New)
                    return ""; //return "Partially Received";
                else if (Status == (int)PurchaseDeliveryStatus.Fully)
                    return ""; //return "Fully Received";
                else if (Status == (int)PurchaseDeliveryStatus.Void)
                    return "Void";
                else
                    return "";
            }
        }

        public string VoidDescription { get; set; }

        public PurchaseDeliveryModel()
        {
            Date = DeliveryDate = DateTime.Now;
            Details = new List<PurchaseDeliveryDetailModel>();

            Status = (int)PurchaseDeliveryStatus.New;
        }
    }
}
