using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Models.GenericModel;

namespace ABCAPOS.Models
{
    public class ResiPriceDetailModel:PSIDetailModel
    {
        //Table Manual
        //public long ResiID { get; set; }
        public int ExpeditionItemNo { get; set; }
        public double Price { get; set; }
        public double Qty { get; set; }

        //Read Only
        public double TotalAmount { get; set; }
        public string UnitName { get; set; }
    }
}
