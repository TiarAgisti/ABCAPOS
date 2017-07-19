using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABCAPOS.Util;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class StaffModel
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string JobTitle { get; set; }
        public long SupervisorID { get; set; }
        public string SupervisorName { get; set; }

        public string Phone { get; set; }
        public string Email { get; set; }

        public long DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public long WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        
        public decimal BasicSalary { get; set; }
        public decimal TransportAllowance { get; set; }
        public decimal ActiveBonus { get; set; }
        public decimal PositionAllowance { get; set; }
        public decimal MealAllowance { get; set; }
        public decimal Bonus { get; set; }
        public decimal THR { get; set; }
        public decimal MealAllowanceExpense { get; set; }
        public decimal LoanAmount { get; set; }
        public int InstallmentCount { get; set; }
        public int LastInstallmentNo { get; set; }
        public int RoleID { get; set; }
        
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }

        public string UserName { get; set; }
        public bool IsSalesRep { get; set; }

        public string RoleDescription
        {
            get
            {
                if (RoleID == (int)StaffType.Courier)
                    return "Kurir";
                else if (RoleID == (int)StaffType.Operator)
                    return "Operator";
                else if (RoleID == (int)StaffType.Others)
                    return "Others";
                else
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

        public string IsInactiveDescription
        {
            get
            {
                if (IsActive)
                    return "No";
                else
                    return "Yes";
            }
        }

        public StaffModel()
        {
            RoleID = (int)StaffType.Others;

            IsActive = true;
        }
    }
}

