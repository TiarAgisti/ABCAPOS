using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using ABCAPOS.Util;

namespace ABCAPOS.Models
{
    public class MonthlyDataModel
    {
        public string Month { get; set; }
        public string Amount { get; set; }
        public string color { get; set; }
    }
}
