using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Models.GenericModel;

namespace ABCAPOS.Models
{
    public class ResiBillDetailModel:PSIDetailModel
    {
        //Table Manual
        public long CustomerID { get; set; }
        public long ResiID { get; set; }
        public double Amount { get; set; }

        //Read Only
        public string ResiCode { get; set; }
        public DateTime ResiDate { get; set; }
        public string CustomerName { get; set; }

        //For tab Bill
        public string ResiBillCode { get; set; }
        public DateTime ResiBillDate { get; set; }

      


    }
}
    