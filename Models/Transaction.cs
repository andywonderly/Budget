using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budget.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string OwnerUserId { get; set; }
        public float Amount { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public int AccountId { get; set; }
        public float Balance { get; set; }
        public bool Reconciled { get; set; }
        public bool Expenditure { get; set; }
        public bool Void { get; set; }
        public DateTimeOffset Created { get; set; }
    }

    public class TransactionViewModel
    {
        public int Id { get; set; }
        public string OwnerUserId { get; set; }
        public string OwnerUserName { get; set; }
        public float Amount { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public string Category { get; set; }
        public int AccountId { get; set; }
        public float Balance { get; set; }
        public bool Reconciled { get; set; }
        public bool Expenditure { get; set; }
        public List<Category> Categories { get; set; }
        public DateTimeOffset Created { get; set; }
    }
}