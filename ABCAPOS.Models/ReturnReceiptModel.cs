using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class ReturnReceiptModel
    {
        public long ID { get; set; }
        public long CustomerID { get; set; }
        public long CustomerReturnID { get; set; }
        public string Code { get; set; }
        public string SupplierReference {get; set; }
        [Required]
        public DateTime Date { get; set; }
        public string Title {get; set; }
        public long EmployeeID { get; set; }
        public long CurrencyID {get; set; }

        public long DepartmentID { get; set; }
        public string VoidRemarks { get; set; }
        public int Status { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
 
        public string CustomerReturnCode { get; set; }
        public string CustomerReturnTitle { get; set; }
        public DateTime CustomerReturnDate {get; set; }
        public int Currency { get; set; }
        public decimal ExchangeRate { get; set; }
        public string CurrencyName{get; set; }
        public string DepartmentName { get; set; }
        [Required]
        public string EmployeeName { get; set; }
        public string CustomerName { get; set; }

        public long WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public bool IsCreditable { get; set; }

        public long LogStockID { get; set; }

        public long LogID { get; set; }

        public List<ReturnReceiptDetailModel> Details { get; set; }

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

        public ReturnReceiptModel()
        {
            Date = DateTime.Now;
            Details = new List<ReturnReceiptDetailModel>();

            Status = (int)PurchaseDeliveryStatus.New;
        }
    }
}
