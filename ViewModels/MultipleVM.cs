using BusinessApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BusinessApplication.ViewModels
{
    public class MultipleVM
    {
        private MultipleVM model;
        public List<ClientVM> CVM { get; set; }
        public TBL_BP_CLIENT Client { get; set; }
        public TBL_BP_LOCATION Loc { get; set; }
        public List<LocationVM> LVM{ get; set; }
        public TBL_BP_USERMASTER UserTbl { get; set; }
        public TBL_BP_DICTIONARY DictVM { get; set; }
        public List<IncomeVM> InList { get; set; }
        public List<IncomeVM> VInList { get; set; }
   
        public List<IncomeVM> AInList { get; set; }
        public List<IncomeVM> IncomeList { get; set; }
       // public IEnumerable<TBL_BP_RESOURCE_> Reso_List { get; set; }
        public IEnumerable<TBL_BP_DICTIONARY> DictListVM { get; set; }
        public PROC_BP_SHOW_RESOURCE_Result MaterialIH { get; set; }
        public IEnumerable<PROC_BP_SHOW_RESOURCE_Result> MaterialsIH { get; set; }
        public IEnumerable<TBL_BP_RESOURCE_> ResourceTbl { get; set; }
        public IEnumerable<TBL_BP_RESOURCE> BPResoTbl { get; set; }
        public TBL_BP_RESOURCE_ ResoTbl { get; set; }
        public TBL_BP_RESOURCE BResoTbl { get; set; }
      
        public IEnumerable<TBL_BP_CLIENT> ClientListVM{ get; set; }
        public IEnumerable<TBL_BP_LOCATION> LocListVM { get; set; }
        public IEnumerable<PROC_BP_SHOW_MASTER_Result> GlobalList { get; set; }
        public IEnumerable<PROC_BP_SHOW_PERMISSION_Result> PermissionList { get; set; }
        public List<PermissionVM> PermList { get; set; }
        public TBL_BP_REVISION ReviTbl { get; set; }
        public List<bool> chkPart { get; set; }
        public PROC_MN_REPT_GE4MONTH_Result MonthRpt { get; set; }
        public IEnumerable<PROC_MN_REPT_GE4MONTH_Result> MonthRptList { get; set; }

        public ChangePasswordVM changePassword { get; set; }
    }
}