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
        public int? HouseholdId { get; set; }
        public string HouseholdName { get; set; }
        public DateTimeOffset Created { get; set; }
        public bool RespondedTo { get; set; }

    }

    public class InvitationViewModel
    {
        public int Id { get; set; }
        public string OwnerUserId { get; set; }
        public string OwnerUserName { get; set; }
        public string OwnerUserEmail { get; set; }
        public string InvitedEmail { get; set; }
        public string InvitedId { get; set; }
        public int? HouseholdId { get; set; }
        public string HouseholdName { get; set; }
        public DateTimeOffset Created { get; set; }
        public ApplicationUser SearchResult { get; set; }
        public bool AlreadyInvited { get; set; }
        public bool InvitedSuccess { get; set; }
        public bool HasAccount { get; set; }
        public bool AlreadyInHousehold { get; set; }

        
    }
}