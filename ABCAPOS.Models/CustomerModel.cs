using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using ABCAPOS.Util;

namespace ABCAPOS.Models
{
    public class CustomerModel
    {
        public long ID { get; set; }

        [Required(ErrorMessage="ID is required")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Company name is required")]
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string City { get; set; }
        public string Email { get; set; }


        public string ContactPerson { get; set; }
               
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }

        public int Category { get; set; }
        public string CompanyName { get; set; }
        public bool IsIndividual { get; set; }
        public long SalesRep { get; set; }
        public string SalesRepName { get; set; }
        public string Stage { get; set; }
        [Required(ErrorMessage="Status is required")]
        public string Status { get; set; }
        public string Territory { get; set; }
        public string LeadSource { get; set; }
        public string Partner { get; set; }
        public DateTime EndDate { get; set; }
        public decimal ReminderDays { get; set; }
        public string AltContact { get; set; }
        public string OfficePhone { get; set; }
        public string Comments { get; set; }
        public string BillingAddress1 { get; set; }
        public string BillingAddress2 { get; set; }
        public string Province { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public string ShippingAddress { get; set; }
        public string Account { get; set; }
        public int TermsID { get; set; }
        public string TermsName { get; set; }
        public long PriceLevelID { get; set; }
        public string PriceLevelName { get; set; }
        public string ResaleNumber { get; set; }
        public decimal Balance { get; set; }
        public decimal UnbilledOrder { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal OverdueBalance { get; set; }
        public int DaysOverdue { get; set; }
        public bool OnCreditHold { get; set; }
        public bool OverrideCreditHold { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateFirstSale { get; set; }
        public DateTime DateProspect { get; set; }
        public string Duplicate { get; set; }

        public long ParentID { get; set; }
        public string ParentName { get; set; }

        public bool BlackList { get; set; }
        public string Notes { get; set; }
        public bool IsCoverExpeditionByABCA { get; set; }

        // readonly values (from View)
        public decimal InvoiceBalance { get; set; }

        // helper variables for Form
        public string StrCreditLimit { get; set; }

        public string CategoryName
        {
            get
            {
                if (Category == (int)CustomerCategory.Individual)
                    return "INDIVIDUAL";
                else if (Category == (int)CustomerCategory.Agent)
                        return "AGENT";
                else if (Category == (int)CustomerCategory.Industry)
                        return "INDUSTRIAL";

                return "";
            }
        }

        public string OnCreditHoldDescription
        {
            get
            {
                if (OnCreditHold)
                    return "YES";
                else
                    return "NO";
            }
        }

        public string BlackListDescription
        {
            get
            {
                if (BlackList)
                    return "YES";
                else
                    return "NO";
            }
        }

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

        public string IsCoverExpeditionDesc
        {
            get
            {
                if (IsCoverExpeditionByABCA)
                    return "Yes";
                else
                    return "No";
            }
        }

        public CustomerModel()
        {
            Type = (int)CustomerType.Retail;
            IsActive = true;
        }

        //unused legacy fields
        public long CustomerGroupID { get; set; }
        public string CustomerGroupName { get; set; }
        public long MaxAr { get; set; }
        public long MaxDay { get; set; }
        public long Discount { get; set; }
        public long CompanyID { get; set; }
        public long OrganizationID { get; set; }
        public DateTime EffectiveStartDate { get; set; }
        public DateTime EffectiveEndDate { get; set; }
        public bool MAP { get; set; }
        public int Type { get; set; }
        public string TaxFileNumber { get; set; }

        public long WarehouseID { get; set; }
        public string WarehouseName { get; set; }
    }
}
