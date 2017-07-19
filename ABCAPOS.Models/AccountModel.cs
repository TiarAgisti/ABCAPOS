using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using ABCAPOS.Util;

namespace ABCAPOS.Models
{
    public class AccountModel
    {
        public long ID { get; set; }
        [Required]
        public string Code { get; set; }
        [Required]
        public string UserCode { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public long CategoryID { get; set; }
        public string CategoryDescription { get; set; }
        public int GroupID { get; set; }
        public int InvoicePaymentAccount { get; set; }
        public decimal BeginningBalance { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string DisplayName { get; set; }

        public string FullDescription
        {
            get
            {
                return UserCode + " - " + Description;
            }
        }

        public string GroupDescription
        {
            get
            {
                if (GroupID == (int)AccountGroup.IncomeExpense)
                    return "Neraca";
                else if (GroupID == (int)AccountGroup.ProfitAndLoss)
                    return "Laba (Rugi)";

                return "";
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

        public bool InvoicePaymentAccountBack
        {
            get
            {
                return InvoicePaymentAccount == 1 ? true : false;
            }
        }

        public AccountModel()
        {
            GroupID = (int)AccountGroup.IncomeExpense;
            IsActive = true;
        }
    }
}
