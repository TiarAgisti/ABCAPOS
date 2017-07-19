using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class HomeNotificationModel
    {
        public string Module { get; set; }
        public long CustomerGroupID { get; set; }

        public List<HomeNotificationDetailModel> Details { get; set; }

        public HomeNotificationModel()
        {
            Details = new List<HomeNotificationDetailModel>();
        }
    }
}
