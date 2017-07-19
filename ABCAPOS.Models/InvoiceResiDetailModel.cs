using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class InvoiceResiDetailModel
    {
        //Table Manual
        public long InvoiceID { get; set; }
        public int ItemNo { get; set; }
        public long ResiID { get; set; }
        public double Amount { get; set; }

        //Read Only
        public string InvoiceCode { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime ResiDate { get; set; }
        public string ResiCode { get; set; }
        public string ExpeditionName { get; set; }
    }
}
