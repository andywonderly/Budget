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
        public ActionResult Invite(int Id)
        {

            InvitationViewModel model = new InvitationViewModel();
            model.HouseholdId = Id;
            return View(model);

        }

        [Authorize]
        public ActionResult InviteConfirm([Bind(Include="InvitedEmail, HouseholdId")] InvitationViewModel model)
        {
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);
            Household household = db.Households.Find(currentUser.HouseholdId);
            //ApplicationUser invited = new ApplicationUser();

            //Check against existing household members
            List<ApplicationUser> members = household.Members.ToList();

            foreach (var item in members)
            {
                if (item.Email == model.InvitedEmail)
                {
                    model.AlreadyInHousehold = true;
                    ViewBag.Message = "This user is already a member of your household.";
                    return View(model);
                }
            }

            //Find any existing invitations to this user
            List<Invitation> outstanding = db.Invitations.Where(n => n.InvitedEmail == model.InvitedEmail).ToList();

            //Test each invitation for being for the current household and whether it's been responded to
            foreach(var item in outstanding)
            {
                if (item.HouseholdId == household.Id || item.RespondedTo == false)
                    model.AlreadyInvited = true;
                ViewBag.Message = "This user already has an outstanding invitation to your household.";
            }

            if (!model.AlreadyInvited)
            {
                Invitation invitation = new Invitation
                {
                    InvitedEmail = model.InvitedEmail,
                    OwnerUserId = currentUserId,
                    HouseholdId = model.HouseholdId,
                    HouseholdName = household.Name,
                    Created = DateTimeOffset.Now,
                    RespondedTo = false,
                };

                ViewBag.Message = "The invitation has been submitted, and its recipient will see it "
                                + "upon his or her next login.";
            }

            return View(model);
        }


        [Authorize]
        [HttpPost]
        public async Task<ActionResult> InviteConfirm([Bind(Include = "Email")] InvitationViewModel model)
        {
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);
            Household household = db.Households.Find(currentUser.HouseholdId);
            ApplicationUser invited = new ApplicationUser();

            if (model.HasAccount)
                invited = db.Users.Find(model.InvitedId);


            Invitation invitation = new Invitation
            {
                InvitedEmail = model.SearchResult.Email,
                OwnerUserId = currentUserId,
                HouseholdId = household.Id,
                HouseholdName = household.Name,
                Created = DateTimeOffset.Now,
                RespondedTo = false,
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