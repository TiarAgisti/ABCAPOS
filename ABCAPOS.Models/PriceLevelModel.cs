using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class PriceLevelModel
    {
        public long ID { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
