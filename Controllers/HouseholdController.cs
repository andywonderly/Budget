using Budget;
using Budget.Models;
using Microsoft.AspNet.Identity;
using SendGrid;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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

        [Authorize]
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

            List<Invitation> temp = model.PendingInvitations.ToList();

            foreach(var item in temp)
            {
                if(item.RespondedTo == true)
                {
                    model.PendingInvitations.Remove(item);
                }
            }

            model.Categories = household.Categories.ToList();
            ViewBag.CategoryId = new SelectList(model.Categories, "Id", "Name");

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
                if (item.HouseholdId == household.Id && item.RespondedTo == false)
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
        [ValidateAntiForgeryToken]
        public ActionResult LeaveHousehold(int Id)
        {

            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);
            Household household = db.Households.Find(currentUser.HouseholdId);

            household.Members.Remove(currentUser);
            currentUser.HouseholdId = null;

            if(household.Members.Count == 0)
            {
                //Soft-delete the household.  This may not actually be necessary since the display
                //of the household to the user is tied to their HouseholdId.  Aka, there is no program
                //logic yet that does anything with the bool Active.
                household.Active = false;

                //After removing the household, we want to find all invitations to that household
                //and set the RespondedTo bool to True so that the invitations are not displayed, and
                //more importantly, cannot be accepted
                List<Invitation> invitations = new List<Invitation>();
                invitations = db.Invitations.Where(n => n.HouseholdId == household.Id).ToList();
                
                foreach(var item in invitations)
                {
                    item.RespondedTo = true;
                    db.Entry(item).Property("RespondedTo").IsModified = true;  
                }
            }

            db.Entry(currentUser).State = EntityState.Modified;
            db.Entry(household).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult Accept (int id)
        {
            Invitation invitation = db.Invitations.Find(id);
            //invitation.Id = id;
            invitation.HouseholdName = db.Households.Find(invitation.HouseholdId).Name;
            
            return View(invitation);
        }



        [Authorize]   
        public ActionResult AcceptConfirm ([Bind(Include="Id")] InvitationViewModel model)
        {
            //***SEND INVITATION AND HOUSEHOLD ID FROM FORM?  TO CONFIRM THAT YOU WERE INVITED
            //TO THE HOUSEHOLD YOU ARE JOINING

            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);
            Invitation invitation = db.Invitations.Find(model.Id);
            //declare current and new households
            //Household currentHousehold = db.Households.Find(currentUser.HouseholdId);
            Household newHousehold = db.Households.Find(invitation.HouseholdId);

            //Remove from current and add to new.  And update HouseholdId
            if (currentUser.HouseholdId != null)
            {
                Household currentHousehold = db.Households.Find(currentUser.HouseholdId);
                currentHousehold.Members.Remove(currentUser);
            }

            newHousehold.Members.Add(currentUser);
            currentUser.HouseholdId = newHousehold.Id;

            db.Entry(currentUser).State = EntityState.Modified;
            //db.Users.Attach(currentUser);

            db.Entry(newHousehold).State = EntityState.Modified;
            //db.Households.Attach(newHousehold);


            //Set the invitation respondedTo to true and attach to db.Invitations
            //Invitation invitation = db.Invitations.Find(model.Id);
            invitation.RespondedTo = true;
            db.Entry(invitation).State = EntityState.Modified;
            //db.Invitations.Attach(invitation);

            db.SaveChanges();

            return RedirectToAction("Index", "Household");

        }

        [Authorize]
        public ActionResult Dismiss(int id)
        {
            Invitation invitation = db.Invitations.Find(id);
            invitation.RespondedTo = true;

            db.Entry(invitation).State = EntityState.Modified;
            //db.Invitations.Attach(invitation);
            
            db.SaveChanges();

            return RedirectToAction("CreateJoinHousehold", "Home");
        }

        [Authorize]
        public ActionResult NewTransaction([Bind(Include = "Amount, AccountId, CategoryId, Expenditure")] TransactionViewModel model)
        {
            //Send:
            //Categories
            //
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);
            Household household = db.Households.Find(currentUser.HouseholdId);
            Account account = db.Accounts.Find(model.AccountId);

            if (model.Expenditure)
            {
                model.Amount *= -1;
            }


            account.Balance = account.Balance + model.Amount;

            Transaction transaction = new Transaction
            {
                Amount = model.Amount,
                AccountId = model.AccountId,
                Balance = account.Balance,
                CategoryId = model.CategoryId,
                Description = model.Description,
                OwnerUserId = currentUser.Id,
                Expenditure = model.Expenditure,
            };


            account.Transactions.Add(transaction);
            db.Entry(account).State = EntityState.Modified;
            db.Transactions.Add(transaction);

            db.SaveChanges();


            return RedirectToAction("Index", "Household");
        }

        //POST ACTION NEEDED!

        [Authorize]
        public ActionResult CreateAccount([Bind(Include = "Name, Balance, HouseholdId")] Account account)
        {
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);
            Household household = db.Households.Find(currentUser.HouseholdId);


            if (ModelState.IsValid)
            {
                account.OwnerUserId = currentUser.Id;
            }

            db.Accounts.Add(account);
            household.Accounts.Add(account);
            db.SaveChanges();

            return RedirectToAction("Index", "Household");


        }



    }


}