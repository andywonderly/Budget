using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Budget.Models
{
    public class Household
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string OwnerUserId { get; set; }
        public DateTimeOffset Created { get; set; }
        public virtual ICollection<ApplicationUser> Members { get; set; }
        public virtual ICollection<Account> Accounts { get; set; }
        public virtual ICollection<BudgetItem> BudgetItems { get; set; }
        public virtual ICollection<Category> Categories { get; set;}
        public bool Active { get; set; }



    }
}