using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BusinessApplication.ViewModels
{
    public class PermissionVM
    {
        public DateTime? ModifiedOn { get; set; }
        public string BP_USER { get; set; }
        public string JOB_NO { get; set; }
        public bool BP_READ { get; set; }
        public bool BP_WRITE { get; set; }
        public bool MN_READ { get; set; }
        public bool MN_WRITE { get; set; }
        public bool PJ_READ { get; set; }
        public bool PJ_WRITE { get; set; }
    }
}