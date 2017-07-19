using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class InventoryAdjustmentModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        public long CustomerID { get; set; }
        public long AccountID { get; set; }
        public decimal EstValue { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public long PostingPeriodID { get; set; }
        public string Title { get; set; }
        
        public long DepartmentID { get; set; }
        [Required]
        public long WarehouseID { get; set; }
        [Required]
        public long StaffID { get; set; }

        public int Status { get; set; }
        public string VoidRemarks { get; set; }

        public bool HasCounted { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
         
        public string DepartmentDesc { get; set; }
        public string WarehouseName { get; set; }
        [Required]
        public string StaffName { get; set; }
        public string CustomerName { get; set; }

        public long LogID { get; set; }

        public List<InventoryAdjustmentDetailModel> Details { get; set; }

        public InventoryAdjustmentModel()
        {
            Date = DateTime.Now;
            Details = new List<InventoryAdjustmentDetailModel>();
        }

        public string StatusDescription { get; set; }
    }
}
