using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class MultipleInvoicingModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public string Title { get; set; }
        public long CustomerID { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string BillingAddress1 { get; set; }
        public string Remarks { get; set; }
        public string VoidRemarks { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CreatedPaymentAmount { get; set; }
        public decimal ApprovedPaymentAmount { get; set; }
        public string DeliveryOrderCodeList { get; set; }
        public string SalesOrderCodeList { get; set; }
        public string InvoiceCode { get; set; }

        public decimal SubTotal { get; set; }
        public decimal TaxValue { get; set; }
        public decimal GrandTotal { get; set; }

        public decimal SubTotalItem { get; set; }
        public decimal TaxValueItem { get; set; }
        public decimal GrandTotalItem { get; set; }

        public int Status { get; set; }
        public DateTime DueDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }

        public List<MultipleInvoicingDetailModel> Details { get; set; }
        public List<MultipleInvoiceItemModel> ItemDetails { get; set; }

        public MultipleInvoicingModel()
        {
            //Date = DateTime.Now;

            Details = new List<MultipleInvoicingDetailModel>();
            ItemDetails = new List<MultipleInvoiceItemModel>();
        }
    }
}
