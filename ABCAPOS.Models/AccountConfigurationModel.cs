using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class AccountConfigurationModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool Modifiable { get; set; }
        public bool Active { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }

        public List<AccountConfigurationDetailModel> Details { get; set; }
        public List<AccountConfigurationDetailModel> CreditDetails { get; set; }

        public string ActiveDescription
        {
            get
            {
                if (Active)
                    return "Aktif";
                else
                    return "Non-aktif";
            }
        }
        
        public AccountConfigurationModel()
        {
            Modifiable = true;
            Active = true;

            Details = new List<AccountConfigurationDetailModel>();
            CreditDetails = new List<AccountConfigurationDetailModel>();
        }
    }
}
