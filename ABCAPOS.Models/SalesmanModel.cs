using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class SalesmanModel
    {
        public long ID { get; set; }
        [Required]
        public string Code { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string City { get; set; }
        public string State { get; set; }
        [Required]
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Phone3 { get; set; }
        [Required]
        public string Position { get; set; }
        public string UserID { get; set; }
        public string Signature { get; set; }
        public long CompanyID { get; set; }
        public long OrganizationID { get; set; }
        public DateTime EffectiveStartDate { get; set; }
        public DateTime EffectiveEndDate { get; set; }
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

        public SalesmanModel()
        {
            IsActive = true;
        }
    }
}
