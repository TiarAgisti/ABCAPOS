using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class WarehouseModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        [Required]
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string PIC { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }

        public WarehouseModel()
        {
            IsActive = true;
        }

        public string IsInactiveDescription{
            get{
                if (IsActive)
                    return "NO";
                else
                    return "YES";
            }
        }
    }
}
