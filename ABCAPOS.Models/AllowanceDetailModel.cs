using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class AllowanceDetailModel
    {
        public long AllowanceID { get; set; }
        public int ItemNo { get; set; }
        public long StaffID { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal TransportAllowance { get; set; }
        public decimal ActiveBonus { get; set; }
        public decimal PositionAllowance { get; set; }
        public decimal MealAllowance { get; set; }
        public decimal Bonus { get; set; }
        public decimal THR { get; set; }
        public decimal MealAllowanceExpense { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal PaidLoanAmount { get; set; }
        public int InstallmentOrderNo { get; set; }
        public string StaffName { get; set; }
        public bool isPaidLoan { get; set; }

        public decimal IncomeAmount
        {
            get
            {
                return BasicSalary + TransportAllowance + ActiveBonus + PositionAllowance + MealAllowance + Bonus + THR;
            }
        }

        public decimal DeductionAmount
        {
            get
            {
                if (isPaidLoan == true)
                {
                    return MealAllowanceExpense + PaidLoanAmount;
                }
                else
                {
                    return MealAllowanceExpense;
                }

            }
        }
    }
}
