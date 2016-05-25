using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budget.Models
{
    public class ManageHouseholdViewModel
    {
        public string Name { get; set; }
        public int? Id { get; set; }
        public List<Account> Accounts { get; set; }
        public List<ApplicationUser> Members { get; set; }
        public List<Invitation> PendingInvitations { get; set; }
        public List<Category> Categories { get; set; }
    }
}