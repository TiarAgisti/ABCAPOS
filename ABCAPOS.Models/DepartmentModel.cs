using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class DepartmentModel
    {
        public long ID { get; set; }
        public string Code { get; set; }
        [Required]
        public string Name { get; set; }
        public string DepartmentDesc { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public long SubDepartmentOf { get; set; }
        public string SubDepartmentOfDesc { get; set; }

        public DepartmentModel()
        {
            IsActive = true;
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
    }
}
