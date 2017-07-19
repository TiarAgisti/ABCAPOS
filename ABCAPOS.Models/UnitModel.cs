using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class UnitModel
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }

        public string BaseUnitName { get; set; }
        public string BaseUnitID { get; set; }
        public List<UnitDetailModel> Details { get; set; }
    }
}
