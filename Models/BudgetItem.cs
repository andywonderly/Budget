using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budget.Models
{
    public class BudgetItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float Amount { get; set; }
        public string Frequency { get; set; }
        public int HouseholdId { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
        public int CategoryId { get; set; }
        public bool Void { get; set; }
        public bool Deleted { get; set; }
        public virtual Household Household { get; set; }
    }

    public class BudgetItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float Amount { get; set; }
        public string Frequency { get; set; }
        public int HouseholdId { get; set; }
        public string HouseholdName { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool Void { get; set; }
        public bool Deleted { get; set; }
        public double Spent { get; set; }
        //public virtual Household Household { get; set; }

    }
}