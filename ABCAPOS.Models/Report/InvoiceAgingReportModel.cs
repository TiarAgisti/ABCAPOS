using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class InvoiceAgingReportModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        public long CustomerID { get; set; }
        public string CustomerName { get; set; }
        public DateTime Date { get; set; }
        public DateTime DueDate { get; set; }
        public int Aging { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public decimal Aging1 { get; set; }
        public decimal Aging2 { get; set; }
        public decimal Aging3 { get; set; }
        public decimal Aging4 { get; set; }
        public decimal Aging5 { get; set; }

        public decimal NoDueYet
        {
            get
            {
                if (Aging < 0)
                    return OutstandingAmount;
                else
                    return 0;
            }
        }
    }
}
