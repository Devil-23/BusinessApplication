using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BusinessApplication.ViewModels
{
    public class IncomeVM
    {
        public bool IsSelected { get; set; }
        public string GRP { get; set; }
        public string ID { get; set; }
        public string JobNo { get; set; }
        public string Type { get; set; }
        public string DESCRIPTION { get; set; }
        public string Unit { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Amount { get; set; }
        public decimal? JAN { get; set; }
        public decimal? FEB { get; set; }
        public decimal? MAR { get; set; }
        public decimal? APR { get; set; }
        public decimal? MAY { get; set; }
        public decimal? JUN { get; set; }
        public decimal? JUL { get; set; }
        public decimal? AUG { get; set; }
        public decimal? SEP { get; set; }
        public decimal? OCT { get; set; }
        public decimal? NOV { get; set; }
        public decimal? DEC { get; set; }
        public decimal? EST__QTY { get; set; }
        public decimal? TOTAL { get; set; }
        public List<decimal?> MonthlyValues { get; set; }
        public decimal? GetMonthValue(int month)
        {
            switch (month)
            {
                case 1: return JAN;
                case 2: return FEB;
                case 3: return MAR;
                case 4: return APR;
                case 5: return MAY;
                case 6: return JUN;
                case 7: return JUL;
                case 8: return AUG;
                case 9: return SEP;
                case 10: return OCT;
                case 11: return NOV;
                case 12: return DEC;
                default: return 0;
            }
        }

    }
}