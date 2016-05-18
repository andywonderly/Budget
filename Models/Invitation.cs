using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budget.Models
{
    public class Invitation
    {
        public int Id { get; set; }
        public string OwnerUserId { get; set; }
        public string InvitedId { get; set; }
        public string InvitedEmail { get; set; }
        public string HouseholdId { get; set; }
        public DateTimeOffset Created { get; set; }
        public bool RespondedTo { get; set; }

    }
}