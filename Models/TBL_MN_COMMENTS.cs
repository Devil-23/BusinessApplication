//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BusinessApplication.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class TBL_MN_COMMENTS
    {
        public decimal ID_ { get; set; }
        public string NT_USER { get; set; }
        public string JOB_NO { get; set; }
        public string ITM_ID { get; set; }
        public string ITM_TYPE { get; set; }
        public Nullable<int> ITM_YEAR { get; set; }
        public Nullable<int> ITM_MONTH { get; set; }
        public string ITM_COMMENT { get; set; }
        public Nullable<System.DateTime> ITM_TIME { get; set; }
        public Nullable<decimal> ITM_ADJUST { get; set; }
    }
}
