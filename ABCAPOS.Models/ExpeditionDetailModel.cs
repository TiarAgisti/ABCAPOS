using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using ABCAPOS.Models.GenericModel;

namespace ABCAPOS.Models
{
    public class ExpeditionDetailModel:PSIDetailModel
    {
        //public long ExpeditionID { get; set; }

        public string UnitName { get; set; }
        public double Price { get; set; }
        public string ExpeditionName { get; set; }
    }
}
