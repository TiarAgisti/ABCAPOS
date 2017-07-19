using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class VendorModel
    {
        public long ID { get; set; }
        [Required]
        public string Code { get; set; }
        [Required]
        public string Name { get; set; }
        public string Duplicate { get; set; }
        public string ContactPerson { get; set; }

        //public string FirstName { get; set; }
        //public string LastName { get; set; }
        public string Category { get; set; }
        public string Email { get; set; }
        public string BillingAddress { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string City { get; set; }
        public string WebAddress { get; set; }
        public string Account { get; set; }
        //public string Terms { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal Balance { get; set; }
        public decimal UnbilledOrders { get; set; }
        
        // largely unused section
        public long CompanyID { get; set; }
        public long OrganizationID { get; set; }
        public DateTime EffectiveStartDate { get; set; }
        public DateTime EffectiveEndDate { get; set; }

        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }

        public string TaxNumber { get; set; }
        public long TermsID { get; set; }
        public string TermsName { get; set; } // from v_Vendor
        public long CurrencyID { get; set; }
        public string CurrencyName { get; set; } // from v_Vendor

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

        public VendorModel()
        {
            IsActive = true;
        }
    }
}
