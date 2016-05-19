using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budget.Models
{
    public class DashboardViewModel
    {
        public ApplicationUser User { get; set; }
        public Household Household { get; set; }
        public List<Invitation> Invitations { get; set; }
        public List<Transaction> Transactions { get; set; }
        public List<Notification> Notifications { get; set; }

    }
}