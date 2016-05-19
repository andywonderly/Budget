using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budget.Models
{
    public class CreateJoinViewModel
    {
        public List<InvitationViewModel> Invitations { get; set; }
        public ApplicationUser User { get; set; }
        public string Name { get; set; }
        public DateTimeOffset Created { get; set; }


    }
}