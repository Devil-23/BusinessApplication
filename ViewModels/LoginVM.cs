using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BusinessApplication.ViewModels
{
    public class LoginVM
    {
        public string User { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}