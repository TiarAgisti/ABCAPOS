using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class CompanySettingModel
    {
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string Regency { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        public string PostCode { get; set; }
        [Required]
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Fax { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        [Required]
        public string OwnerName { get; set; }
        public string OwnerEmail { get; set; }
        public string TaxFileNumber { get; set; }
        public string ImageName { get; set; }
        public string BankName1 { get; set; }
        public string BankAddress1 { get; set; }
        public string AccountNumber1 { get; set; }
        public string AccountName1 { get; set; }
        public string BranchAccount1 { get; set; }
        public string BankName2 { get; set; }
        public string BankAddress2 { get; set; }
        public string AccountNumber2 { get; set; }
        public string AccountName2 { get; set; }
        public string BranchAccount2 { get; set; }

    }
}
