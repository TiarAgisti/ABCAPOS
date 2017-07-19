using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.EDM
{
    public partial class v_Rpt_SalesOrderDetail
    {
        public decimal Total
        {
            get
            {
                return Convert.ToDecimal(Price - Discount) * Convert.ToDecimal(Quantity);
            }
        }
    }
}
