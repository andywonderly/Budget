using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budget.Models
{
    public class Account
    {
        public int Id { get; set; }
        public string OwnerUserId { get; set; }
        public DateTimeOffset Created { get; set; }
        public int HouseholdId { get; set; }
        public string Name { get; set; }
        public float Balance { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
        //public virtual ICollection<Notification> Notifications { get; set; }
        public bool Active { get; set; }

    }

    public class AccountViewModel
    {
        public int Id { get; set; }
        public string OwnerUserId { get; set; }
        public DateTimeOffset Created { get; set; }
        public int HouseholdId { get; set; }
        public string Name { get; set; }
        public float Balance { get; set; }
        public virtual List<Transaction> Transactions { get; set; }
        //public virtual ICollection<Notification> Notifications { get; set; }
        public List<TransactionViewModel> TransactionViewModels { get; set; }
        public bool Active { get; set; }
    }
}