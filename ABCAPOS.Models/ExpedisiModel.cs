using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class ExpedisiModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
