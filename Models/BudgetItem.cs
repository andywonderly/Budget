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
        public double Amount { get; set; }
        public string Frequency { get; set; }
        public int HouseholdId { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
        public int CategoryId { get; set; }
        public bool Void { get; set; }
        public bool Deleted { get; set; }
        public virtual Household Household { get; set; }
        public bool BudgetSet { get; set; }
    }

    public class BudgetItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
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
        public SpentBudgeted SpentBudgeted { get; set; }
        //public virtual Household Household { get; set; }
        public int SpentPercent { get; set; }
        public int UnspentPercent { get; set; }
        public string SpentPercentPx { get; set; }
        public string UnspentPercentPx { get; set; }
        

    }

    public class SpentBudgeted
    {
        public double Spent { get; set; }
        public double Budgeted { get; set; }
    }

    public class EditBudgetViewModel
    {
        public int Id { get; set; }
        public double Amount { get; set; }
        public int HouseholdId { get; set; }
    }
}