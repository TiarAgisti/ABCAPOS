using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class UnitDetailModel
    {
        public long ID { get; set; }
        public long UnitID { get; set; }
        public string Name { get; set; }
        public string PluralName { get; set; }
        public string Abbreviation { get; set; }
        public string PluralAbbreviation { get; set; }
        public double Rate { get; set; }
        public bool IsBase { get; set; }
        public long BaseID { get; set; }
        public bool IsActive { get; set; }
        public int ItemNo { get; set; }

        public string UnitType { get; set; }
    }
}
