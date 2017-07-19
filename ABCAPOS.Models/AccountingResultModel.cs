using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class AccountingResultModel
    {
        public long ID { get; set; }
        public long DocumentID { get; set; }
        public int DocumentDetailItemNo { get; set; }
        public int DocumentType { get; set; }
        public int Type { get; set; }
        public DateTime Date { get; set; }
        public long AccountID { get; set; }
        public string DocumentNo { get; set; }
        public decimal Amount { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public string Remarks { get; set; }
        public string AccountCode { get; set; }
        public string AccountUserCode { get; set; }
        public string AccountName { get; set; }
        public string AccountDescription { get; set; }
    }
}
