﻿using System;
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
    }
}