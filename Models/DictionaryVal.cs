using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BusinessApplication.Models
{
    [MetadataType(typeof(DictionaryVal))]
    public partial class TBL_BP_DICTIONARY
    { }
    public class DictionaryVal
    {
        [Required(ErrorMessage = "Year is Required")]
        public int BP_YEAR { get; set; }
        [Required(ErrorMessage = "Job Number is Required")]
        [StringLength(8,ErrorMessage = "Job Number should be of 8 Digits",MinimumLength =8)]
        public string JOB_NO { get; set; }
        [Required(ErrorMessage = "Start Date is Required")]
        [DataType(DataType.Date, ErrorMessage = "Date only")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public Nullable<System.DateTime> SDATE { get; set; }
        [Required(ErrorMessage = "Finish Date is Required")]
        [DataType(DataType.Date, ErrorMessage = "Date only")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public Nullable<System.DateTime> FDATE { get; set; }
        [Required(ErrorMessage = "Job Title is Required")]

        public string JOB_TITLE { get; set; }
        public string DIVISION { get; set; }
        [Required(ErrorMessage = "Job Leader Name is Required")]

        public string BP_MANAGER { get; set; }
        public Nullable<int> MN_PROJECTION { get; set; }
        [Required(ErrorMessage = "Contract Value is Required")]
//        [RegularExpression(@"^[a-zA-Z0-9]{1,18}$", ErrorMessage = "Contract value should be alphanumberic")]
        public Nullable<decimal> JOB_VOWD { get; set; }
        public Nullable<int> MN_CLOSE { get; set; }
        public Nullable<int> BP_OFFSHORE { get; set; }

        public string CLIENT { get; set; }

        public string LOCATION { get; set; }
        [Required(ErrorMessage = "OT Percent is Required")]

        public Nullable<decimal> OT_PERCENT { get; set; }
        [Required(ErrorMessage = "Fuel Percent is Required")]

        public Nullable<decimal> FUEL_PERCENT { get; set; }
        [Required(ErrorMessage = "Staff is Required")]

        public Nullable<decimal> FOOD_STAFF { get; set; }
        [Required(ErrorMessage = "Worker is Required")]

        public Nullable<decimal> FOOD_WORKER { get; set; }

        public string JOBTP { get; set; }
    }
}