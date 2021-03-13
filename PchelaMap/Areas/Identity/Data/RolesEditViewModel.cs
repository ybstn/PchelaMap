using System;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
namespace PchelaMap.Areas.Identity.Data
{
    public class RolesEditViewModel
    {
       public string userID { get; set; }
        public string userEmail { get; set; }
        public string userName { get; set; }
        public List<IdentityRole> AllRoles { get; set; }
        public IList<string> userRoles { get; set; }
        public string userRole { get; set; }
        public RolesEditViewModel()
        {
            AllRoles = new List<IdentityRole>();
            userRoles = new List<string>();
        }
    }
}
