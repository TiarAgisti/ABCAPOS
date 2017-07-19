using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using ABCAPOS.Util;

namespace ABCAPOS.Models
{
    public class SupplierModel
    {
        public long ID { get; set; }

        [Required(ErrorMessage = "Kode harus diisi")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Nama harus diisi")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Alamat harus diisi")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Telepon harus diisi")]
        public string Phone { get; set; }
        public string Fax { get; set; }
        [Required(ErrorMessage = "Kota harus diisi")]
        public string City { get; set; }
        [Required(ErrorMessage = "Contact Person harus diisi")]
        public string ContactPerson { get; set; }
        public long MaxAr { get; set; }
        public long MaxDay { get; set; }
        public long Discount { get; set; }
        public long CompanyID { get; set; }
        public long OrganizationID { get; set; }
        public DateTime EffectiveStartDate { get; set; }
        public DateTime EffectiveEndDate { get; set; }        
        public string TaxFileNumber { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }

        public long SupplierGroupID { get; set; }
        public string SupplierGroupName { get; set; }

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

        public SupplierModel()
        {
            IsActive = true;
        }
    }
}
