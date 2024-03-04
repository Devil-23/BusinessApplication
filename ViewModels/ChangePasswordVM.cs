using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BusinessApplication.ViewModels
{
    public class ChangePasswordVM
    {
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Old Password Required")]
        public string OldPassword { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "New Password Required")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Confirm Password Required")]
        [Compare("NewPassword", ErrorMessage = "Not Matching With New Password !")]
        public string ConfirmPassword { get; set; }
    }
}