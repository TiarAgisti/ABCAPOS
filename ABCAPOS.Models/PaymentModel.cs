using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;

namespace ABCAPOS.Models
{
    public class PaymentModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        public long InvoiceID { get; set; }
        public DateTime Date { get; set; }
        public long PaymentMethodID { get; set; }
        public decimal Amount { get; set; }
        public string VoidRemarks { get; set; }
        public string Remarks { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
        public string InvoiceCode { get; set; }
        public long DeliveryOrderID { get; set; }
        public string DeliveryOrderCode { get; set; }
        public long SalesOrderID { get; set; }
        public string SalesOrderCode { get; set; }
        public string SalesOrderTitle { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string PaymentMethodName { get; set; }
        public decimal SisaAmount { get; set; }
        public decimal InvoiceAmount { get; set; }
        public long WarehouseID { get; set; }

        public double OutstandingAmount
        {
            get
            {
                return Convert.ToDouble(InvoiceAmount - Amount);
            }
        }
        public string StatusDescription
        {
            get
            {
                if (Status == (int)MPL.DocumentStatus.New)
                    return MPL.DocumentStatus.New.ToString();
                else if (Status == (int)MPL.DocumentStatus.Approved)
                    return MPL.DocumentStatus.Approved.ToString();
                else if (Status == (int)MPL.DocumentStatus.Void)
                    return MPL.DocumentStatus.Void.ToString();
                else
                    return "";
            }
        }

        public PaymentModel()
        {
            Date = DateTime.Now;
            Status = (int)MPL.DocumentStatus.New;
        }
    }
}
