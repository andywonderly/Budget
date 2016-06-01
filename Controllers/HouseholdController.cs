using Budget;
using Budget.Models;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
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
                Accounts = household.Accounts.Where(n => n.Active == true).ToList(),
                PendingInvitations = db.Invitations.Where(n => n.HouseholdId == household.Id).ToList(),
            };

            List<Invitation> temp = model.PendingInvitations.ToList();

            foreach (var item in temp)
            {
                if (item.RespondedTo == true)
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
        public async Task<ActionResult> InviteConfirm([Bind(Include = "InvitedEmail, HouseholdId")] InvitationViewModel model)
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
            foreach (var item in outstanding)
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


            if (household.Members.Count == 1)
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

            if (household.Members.Count == 0)
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

                foreach (var item in invitations)
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
        public ActionResult Accept(int id)
        {
            Invitation invitation = db.Invitations.Find(id);
            //invitation.Id = id;
            invitation.HouseholdName = db.Households.Find(invitation.HouseholdId).Name;

            return View(invitation);
        }



        [Authorize]
        public ActionResult AcceptConfirm([Bind(Include = "Id")] InvitationViewModel model)
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
        public ActionResult NewTransaction([Bind(Include = "Name, Amount, AccountId, CategoryId, Expenditure")] TransactionViewModel model)
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
                Name = model.Name,
                Amount = model.Amount,
                AccountId = model.AccountId,
                Balance = account.Balance,
                CategoryId = model.CategoryId,
                Description = model.Description,
                OwnerUserId = currentUser.Id,
                Expenditure = model.Expenditure,
                Void = false,
                Created = DateTimeOffset.Now,
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
                account.Active = true;
            }

            db.Accounts.Add(account);
            household.Accounts.Add(account);
            db.SaveChanges();

            return RedirectToAction("Index", "Household");


        }

        [Authorize]
        public ActionResult EditAccount(int id)
        {
            Account model = db.Accounts.Find(id);
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult EditAccount([Bind(Include = "Id, Name, Balance")] Account model)
        {
            Account account = db.Accounts.Find(model.Id);
            account.Name = model.Name;
            account.Balance = model.Balance;
            db.Entry(account).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index", "Household");

        }

        [Authorize]
        public ActionResult DeleteAccount(int id)
        {
            Account model = db.Accounts.Find(id);

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult DeleteAccount([Bind(Include ="Id")] Account model)
        {
            Account account = db.Accounts.Find(model.Id);
            account.Active = false;
            db.Entry(account).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index", "Household");
        }

        //[Authorize]
        //public void _EditAccountConfirm([Bind(Include ="Name, Id")] Account model)
        //{
        //    Account account = db.Accounts.Find(model.Id);
        //    account.Name = model.Name;
        //    db.Entry(account).State = EntityState.Modified;
        //    db.SaveChanges();
        //}

        [Authorize]
        public ActionResult _AccountName(int id)
        {
            Account model = db.Accounts.Find(id);
            return View(model);
        }

        public ActionResult GetChart()
        {

            var data = new[] { new {label = "2008", value = 20},
            new { label = "2008", value = 5 },
            new { label = "2010", value = 7 },
            new { label = "2011", value = 10 },
            new { label = "2012", value = 20}};

            return Content(JsonConvert.SerializeObject(data),
            "application/json");
        }

        [Authorize]
        public ActionResult _DeleteAccount(int id)
        {
            Account model = db.Accounts.Find(id);
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult _DeleteAccount([Bind(Include = "Id")] Account model)
        {

            Account account = db.Accounts.Find(model.Id);
            account.Active = false;
            db.Entry(account).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index", "Household");
        }

        [Authorize]
        public ActionResult _ListTransactions(int id)
        {
            //List<Transaction> transactions = new List<Transaction>();
            Account account = db.Accounts.Find(id);
            Household household = db.Households.Find(account.HouseholdId);
            List<Transaction> transactions = account.Transactions.ToList();
            List<Transaction> transactions2 = transactions.ToList();

            foreach (var item in transactions)
            {
                if (item.Void == true)
                    transactions2.Remove(item);
            }

            AccountViewModel model = new AccountViewModel();
            model.TransactionViewModels = new List<TransactionViewModel>();

            foreach (var item in transactions2)
            {
                TransactionViewModel temp = new TransactionViewModel()
                {
                    Id = item.Id,
                    Amount = item.Amount,
                    OwnerUserName = db.Users.Find(item.OwnerUserId).DisplayName,
                    Balance = item.Balance,
                    Category = household.Categories.FirstOrDefault(n => n.Id == item.CategoryId).Name,
                    Reconciled = item.Reconciled,
                    Created = item.Created,

                };

                model.TransactionViewModels.Add(temp);
            }

            return View(model);
        }

        [Authorize]
        public ActionResult EditTransaction(int id)
        {
            Transaction transaction = db.Transactions.Find(id);
            Account account = db.Accounts.Find(transaction.AccountId);
            Household household = db.Households.Find(account.HouseholdId);

            TransactionViewModel model = new TransactionViewModel
            {
                Amount = transaction.Amount,
                Balance = transaction.Balance,
                CategoryId = transaction.CategoryId,
                Description = transaction.Description,
                Reconciled = transaction.Reconciled,
                Void = transaction.Void,
                Expenditure = transaction.Expenditure,
                Name = transaction.Name,
                Categories = household.Categories.ToList(),
                AccountId = transaction.AccountId,
            };

            var x = household.Categories.ToList();

            //var categoryIdSelected = household.Categories.Find(transaction.CategoryId);
            //ViewBag.CategoryId = new SelectList(household.Categories, "Id", "Name", model.CategoryId);
            //ViewBag.CatId = CatId;

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult EditTransaction([Bind(Include = "Name,Id,Amount,CategoryId,AccountId")] TransactionViewModel model)
        {
            Transaction transaction = db.Transactions.Find(model.Id);
            Account account = db.Accounts.Find(transaction.AccountId);

            account.Balance = account.Balance + (model.Amount - transaction.Amount);

            transaction.Name = model.Name;
            transaction.Balance = transaction.Balance + (model.Amount - transaction.Amount);
            transaction.Amount = model.Amount;


            //transaction.CategoryId = model.CategoryId;
            db.Entry(account).State = EntityState.Modified;
            db.Entry(transaction).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index", "Household");
        }

        [Authorize]
        public ActionResult VoidTransaction(int id)
        {
            Transaction transaction = db.Transactions.Find(id);
            TransactionViewModel model = new TransactionViewModel
            {
                Id = transaction.Id,
                Name = transaction.Name,
            };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult VoidTransaction([Bind(Include ="Id")] TransactionViewModel model)
        {
            Transaction transaction = db.Transactions.Find(model.Id);
            transaction.Void = true;

            Account account = db.Accounts.Find(transaction.AccountId);
            account.Balance = account.Balance - transaction.Amount;

            db.Entry(account).State = EntityState.Modified;
            db.Entry(transaction).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index", "Household");
        }

        [Authorize]
        public ActionResult DeleteTransaction(int id)
        {
            Transaction transaction = db.Transactions.Find(id);
            TransactionViewModel model = new TransactionViewModel
            {
                Id = transaction.Id,
                Name = transaction.Name,
            };


            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult DeleteTransaction([Bind(Include ="Id")] TransactionViewModel model)
        {
            Transaction transaction = db.Transactions.Find(model.Id);
            db.Transactions.Remove(transaction);

            Account account = db.Accounts.Find(transaction.AccountId);
            account.Balance = account.Balance - transaction.Amount;

            db.Entry(account).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index", "Household");
        }

        [Authorize]
        public ActionResult EditCategories(int id)
        {
            Household household = db.Households.Find(id);
            CategoryViewModel model = new CategoryViewModel();
            model.Categories = household.Categories.ToList();
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult EditCategories([Bind(Include ="Id")] CategoryViewModel model)
        {
            return RedirectToAction("Index", "Household");
        }

        [Authorize]
        public ActionResult EditSingleCategory([Bind(Include ="Id,HouseholdId")] CategoryViewModel model)
        {
            Household household = db.Households.Find(model.HouseholdId);
            Category category = household.Categories.FirstOrDefault(n => n.Id == model.Id);
            model.Name = category.Name;
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult EditSingleCategory([Bind(Include ="Id,Name,HouseholdId")] CategoryViewModel model)
        {
            Household household = db.Households.Find(model.HouseholdId);
            Category category = household.Categories.FirstOrDefault(n => n.Id == model.Id);
            string oldCategoryName = category.Name;
            category.Name = model.Name;





            List<Transaction> transactions = account.Transactions.Where(n => n.CategoryId == )

            db.Entry(transaction).State = EntityState.Modified;
            db.SaveChanges();


            return RedirectToAction("EditCategories", "Household", new { });
        }

    }


}