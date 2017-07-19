using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class WorkOrderModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        public DateTime Date { get; set; }
        public long ProductID { get; set; }
        public long ProductDetailID { get; set; }
        [Required]
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        [Required]
        public decimal QtyWO { get; set; }
        public decimal QtyBuilt { get; set; }
        public long ConversionID { get; set; }
        public long ConversionIDTemp { get; set; }
        public string ConversionName { get; set; }
        public decimal ConversionValue { get; set; }

        public long WarehouseID

        {
            get
            {
                return 2;
            }
        }

        public string WarehouseName
        {
            get
            {
                return "Factory";
            }
        }

        public long DepartmentID
        {
            get
            {
                return 4;
            }
        }
        public string DepartmentName
        {
            get
            {
                return "Operation : Manufacturing";
            }
        }
       
        public long EmployeeID { get; set; }
        [Required]
        public string EmployeeName { get; set; }
        public int Status { get; set; }
        public string Notes { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
        public string VoidRemarks { get; set; }
        public List<WorkOrderDetailModel> Details { get; set; }
        public double QtyOnHand { get; set; }
        public double QtyAvailable { get; set; }
        public double UsedInBuilt { get; set; }
        public int ItemTypeID { get; set; }
        public long SalesOrderID { get; set; }
        public string SalesOrderCode { get; set; }
        public long CustomerID { get; set; }
        public string CustomerName { get; set; }

    
        public string StatusCreated
        {
            get
            {
                return CreatedBy + " - on :" + CreatedDate;
            }
        }

        public string StatusModified
        {
            get
            {
                return ModifiedBy + " - on :" + ModifiedDate;
            }
        }

        public string StatusDescription
        {
            get
            {
                if (Status == (int)WorkOrderStatus.New)
                    return "Pending Approval";
                else if (Status == (int)WorkOrderStatus.Approved)
                    return "Pending Build";
                else if (Status == (int)WorkOrderStatus.PartialyBuild)
                    return "Partially Build";
                else if (Status == (int)WorkOrderStatus.FullyBuild)
                    return "Fully Build";
                else if (Status == (int)WorkOrderStatus.Void)
                    return "Void";
                else
                    return "";
            }
        }

        public WorkOrderModel()
        {
            Date = DateTime.Now;
            Status = (int)MPL.DocumentStatus.New;
            Details = new List<WorkOrderDetailModel>();
        }

    }
}
