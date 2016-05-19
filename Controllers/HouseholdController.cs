using Budget;
using Budget.Models;
using Microsoft.AspNet.Identity;
using SendGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static Budget.Models.Extensions;


namespace BugTrackerForTemplate.Controllers
{
    public class HouseholdController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        //[AuthorizeHouseholdRequired]
        public ActionResult Index()
        {
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);

            Household household = db.Households.Find(currentUser.HouseholdId);

            List<ApplicationUser> members = new List<ApplicationUser>();

            foreach (var item in household.Members)
            {
                members.Add(new ApplicationUser { DisplayName = item.DisplayName, Id = item.Id });
            }

            ManageHouseholdViewModel model = new ManageHouseholdViewModel
            {
                Name = household.Name,
                Id = household.Id,
                Members = members.ToList(),
                Accounts = household.Accounts.ToList(),
                PendingInvitations = db.Invitations.Where(n => n.HouseholdId == household.Id).ToList(),
            };



            return View(model);
        }

        [Authorize]
        public ActionResult Invite(int id)
        {
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);
            Household household = db.Households.Find(currentUser.HouseholdId);

            InvitationViewModel model = new InvitationViewModel
            {
                OwnerUserId = currentUserId,
            };

            return View();

        }

        [Authorize]
        [HttpPost]
        public ActionResult Invite([Bind(Include="InviteeEmail")] InvitationViewModel model)
        {
            return RedirectToAction("InviteConfirm", new { email = model.InvitedEmail });
        }

        [Authorize]
        public ActionResult InviteConfirm(string email)
        {
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);
            Household household = db.Households.Find(currentUser.HouseholdId);

            List<ApplicationUser> searchResult = db.Users.Where(n => n.Email == email).ToList();

            InvitationViewModel model = new InvitationViewModel();

            foreach(var item in searchResult)
            {
                model.SearchResult.Add(new ApplicationUser
                {
                    DisplayName = item.DisplayName,
                    Id = item.Id,
                    Email = item.Email
                });
            }

            return View(model);

        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> InviteConfirm([Bind(Include = "InvitedId")] InvitationViewModel model)
        {
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);
            ApplicationUser invited = db.Users.Find(model.InvitedId);
            Household household = db.Households.Find(currentUser.HouseholdId);



            Invitation invitation = new Invitation
            {
                InvitedEmail = invited.Email,
                InvitedId = invited.Id,
                OwnerUserId = currentUserId,
                HouseholdId = household.Id,
                HouseholdName = household.Name,
                Created = DateTimeOffset.Now,
                RespondedTo = false,
                
            };

            if(db.Invitations.Any(n => n.InvitedEmail == invitation.InvitedEmail))
            {
                model.AlreadyInvited = true;
                return RedirectToAction("Invite", "HouseholdController", model);
            };

            db.Invitations.Add(invitation);

            model.InvitedSuccess = true;
            db.SaveChanges();


            //new EmailService.SendAsync(new IdentityMessage
            //{
            //    Destination = invitation.InvitedEmail,
            //    Subject = "Budget app: you have been invited to a household.",
            //    Body = "You have been invited to the household " +
            //            household.Name + " by the user " +
            //            db.Users.First(n => n.Id == invitation.OwnerUserId).DisplayName +
            //            ".  Please log in to <a href='https://awonderly-budget.azurewebsites.net'>Budget</a> " +
            //            "using this email address to respond to " +
            //            "this invitation.",
            //});

            var EmailInvitation = new IdentityMessage
            {
                Body = "You have been invited to the household " +
                        household.Name + " by the user " +
                        db.Users.First(n => n.Id == invitation.OwnerUserId).DisplayName +
                        ".  Please log in to <a href='https://awonderly-budget.azurewebsites.net'>Budget</a> " +
                        "using this email address to respond to " +
                        "this invitation.",
                Subject = "Budget app: you have been invited to a household.",
                Destination = invitation.InvitedEmail,
            };

            var SendService = new EmailService();
            await SendService.SendAsync(EmailInvitation);


            return RedirectToAction("Invite", "HouseholdController", model);
        }
        
    }


}