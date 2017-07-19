using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Models.GenericModel;

namespace ABCAPOS.Models
{
    public class ResiDetailModel:PSIDetailModel
    {
        //Table Only
        public long ResiID { get; set; }
        public long DeliveryOrderID { get; set; }

        //Read Only
        public string ResiCode { get; set; }
        public string DeliveryOrderCode { get; set; }
        public DateTime DeliveryOrderDate { get; set; }
        public string CustomerName { get; set; }
    }
}
