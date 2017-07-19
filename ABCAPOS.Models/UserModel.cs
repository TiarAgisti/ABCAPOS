using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace ABCAPOS.Models
{
    public class UserModel
    {
        [Required]
        public string UserID { get; set; }
        [Required]
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public bool IsActive { get; set; }

        public UserModel()
        {
            IsActive = true;
        }
    }
}
