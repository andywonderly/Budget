using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budget.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTimeOffset DateTime { get; set; }
        public string ActionUrl { get; set; }
        public bool Unread { get; set; }
        public string UserId { get; set; }
        public int HouseholdId { get; set; }
        public bool Expired { get; set; }
    }
}