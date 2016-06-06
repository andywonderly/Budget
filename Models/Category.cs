using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budget.Models
{
    public class Category
    {
        public int Id { get; set; }
        //public int HouseholdId { get; set; }
        public int Household_Id { get; set; }
        public string Name { get; set; }
        public bool Deleted { get; set; }
        public bool Stock { get; set; }
        public virtual Household Household { get;set; }
        public double BudgetAmount { get; set; }
        public double Spent { get; set; }
        public bool BudgetDefined { get; set; }
    }

    public class CategoryViewModel
    {
        public int Id { get; set; }
        public int HouseholdId { get; set; }
        public string Name { get; set; }
        public List<Category> Categories { get; set; }
        public double BudgetAmount { get; set; }
        public double Spent { get; set; }
    }

    public class EditCategoryViewModel
    {
        public int Id { get; set; }
        public int HouseholdId { get; set; }
        public string Name { get; set; }
        public bool Stock { get; set; }
        public double BudgetAmount { get; set; }
        public double Spent { get; set; }
    }
}