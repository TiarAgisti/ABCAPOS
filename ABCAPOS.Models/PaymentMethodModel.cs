using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using ABCAPOS.Util;

namespace ABCAPOS.Models
{
    public class PaymentMethodModel
    {
        public long ID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int Type { get; set; }
        public int TypeDescription { get; set; }

        [Required]
        public string AccountID { get; set; }
        public string AccountName { get; set; }
        public string Description { get; set; }
        public bool IsAccountBank { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }

        public string IsActiveDescription
        {
            get
            {
                if (IsActive)
                    return "Aktif";
                else
                    return "Tidak Aktif";
            }
        }

        public PaymentMethodModel()
        {
            IsActive = true;
        }
    }
}
