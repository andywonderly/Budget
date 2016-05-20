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
        public async Task<ActionResult> InviteConfirm([Bind(Include="InvitedEmail, HouseholdId")] InvitationViewModel model)
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
                ViewBag.Message = "The invitation has been submitted, and its recipient will see it "
                                + "upon his or her next login.";
            }



            if (!model.AlreadyInvited && !model.AlreadyInHousehold)
            {
                Invitation invitation = new Invitation
                {
                    InvitedEmail = model.InvitedEmail,
                    OwnerUserId = currentUserId,
                    HouseholdId = household.Id,
                    HouseholdName = household.Name,
                    Created = DateTimeOffset.Now,
                    RespondedTo = false,
                };

                db.Invitations.Add(invitation);
                db.SaveChanges();


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


            }

            return View(model);
        }

        [Authorize]
        public ActionResult LeaveHousehold()
        {
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);
            Household household = db.Households.Find(currentUser.HouseholdId);


            if(household.Members.Count == 1)
                ViewBag.Message = "Note: you are the only member of this household.  Leaving it will "
                                + "cause it to be deleted.";
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult LeaveHousehold(int Id)
        {

            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);
            Household household = db.Households.Find(currentUser.HouseholdId);

            household.Members.Remove(currentUser);

            if (household.Members.Count > 0)
            {
                db.Households.Attach(household);
            } else
            {
                db.Households.Remove(household);
            }

            db.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult Accept (int id)
        {
            Invitation invitation = new Invitation();
            invitation.Id = id;
            invitation.HouseholdName = db.Households.Find(id).Name;

            return View(invitation);
        }



        [Authorize]
        
        public ActionResult AcceptConfirm ([Bind(Include="Id")] InvitationViewModel model)
        {
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);
            //declare current and new households
            Household currentHousehold = db.Households.Find(currentUser.HouseholdId);
            Household newHousehold = db.Households.Find(model.Id);

            //Remove from current and add to new.  And update HouseholdId
            currentHousehold.Members.Remove(currentUser);
            newHousehold.Members.Add(currentUser);
            currentUser.HouseholdId = model.Id;
            db.Users.Attach(currentUser);


            //Set the invitation respondedTo to true and attach to db.Invitations
            Invitation invitation = db.Invitations.Find(model.Id);
            invitation.RespondedTo = true;
            db.Invitations.Attach(invitation);

            db.SaveChanges();

            return RedirectToAction("Index", "Household");

        }

        [Authorize]
        public ActionResult Dismiss(int id)
        {
            Invitation invitation = db.Invitations.Find(id);
            invitation.RespondedTo = true;
           
            db.Invitations.Attach(invitation);
            db.Entry(invitation).Property("RespondedTo").IsModified = true;
            db.SaveChanges();

            return RedirectToAction("CreateJoinHousehold", "Home");
        }



    }


}