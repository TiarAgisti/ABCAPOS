using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.EDM
{
    public partial class v_Rpt_PurchaseBillOverdue
    {
        public decimal GrandTotal
        {
            get
            {
                return Convert.ToDecimal(Amount + TaxAmount);
            }
        }

        public string StatusDescription
        {
            get
            {
                return "Open";
            }
        }
    }
}
