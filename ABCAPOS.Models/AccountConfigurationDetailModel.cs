using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class AccountConfigurationDetailModel
    {
        public long ConfigurationID { get; set; }
        public int ItemNo { get; set; }
        public long AccountID { get; set; }
        public int Type { get; set; }
        public string AccountCode { get; set; }
        public string AccountUserCode { get; set; }
        public string AccountName { get; set; }
        public string AccountDescription { get; set; }
    }
}
