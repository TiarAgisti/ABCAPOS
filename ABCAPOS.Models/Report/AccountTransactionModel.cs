using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class AccountTransactionModel
    {
        public long ID { get; set; }
        public long AccountID { get; set; }
        public DateTime Date { get; set; }
        public string Remarks { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal Balance { get; set; }
        public string AccountCode { get; set; }
        public string AccountUserCode { get; set; }
        public string AccountName { get; set; }
        public string AccountDescription { get; set; }

        public decimal DebitBalance
        {
            get
            {
                if (Balance >= 0)
                    return Balance;
                else
                    return 0;
            }
        }

        public decimal CreditBalance
        {
            get
            {
                if (Balance < 0)
                    return -1 * Balance;
                else
                    return 0;
            }
        }
    }
}
