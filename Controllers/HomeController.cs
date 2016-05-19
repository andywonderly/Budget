using Budget.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Budget.Models.Extensions;

namespace BugTrackerForTemplate.Controllers
{
    public class HomeController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        //[AuthorizeHouseholdRequired]
        public ActionResult Index()
        {
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);
            ApplicationUser modelUser = new ApplicationUser
            {
                Id = currentUser.Id,
                DisplayName = currentUser.DisplayName
            };
            
            DashboardViewModel model = new DashboardViewModel();

            model.User = modelUser;
            model.Household = currentUser.Household;

            model.Transactions = db.Transactions.Where(n => n.OwnerUserId == currentUser.Id).ToList();
            model.Invitations = db.Invitations.Where(n => n.InvitedId == currentUser.Id || n.InvitedEmail == currentUser.Email).ToList();

            model.Notifications = new List<Notification>();

            if (model.Household != null)
                model.Notifications = db.Notifications.Where(n => n.HouseholdId == model.Household.Id).ToList();

            if (currentUserId != null)
                model.Notifications.AddRange(db.Notifications.Where(n => n.UserId == currentUserId));

            model.Invitations = db.Invitations.Where(n => n.InvitedId == currentUserId || n.InvitedEmail == currentUser.Email).ToList();

            return View(model);
        }

        [Authorize]
        public ActionResult Create()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        //[Authorize]
        //[HttpPost]
        //public ActionResult Create(Household household)

        //public ActionResult Contact()
        //{
        //    ViewBag.Message = "Your contact page.";

        //    return View();
        //}

        [Authorize]
        public ActionResult CreateJoinHousehold()
        {
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);

            CreateJoinViewModel model = new CreateJoinViewModel();

            model.User = new ApplicationUser { DisplayName = currentUser.DisplayName, Id = currentUser.Id };


            List<Invitation> Invitations = new List<Invitation>();
            Invitations = db.Invitations.Where(n => n.InvitedId == currentUser.Id || n.InvitedEmail == currentUser.Email).ToList();

            model.Invitations = new List<InvitationViewModel>();

            foreach(var item in Invitations)
            {
                model.Invitations.Add(new InvitationViewModel
                { 
                    /*OwnerUserId = item.OwnerUserId,*/
                    OwnerUserName = db.Users.Find(item.OwnerUserId).DisplayName,
                    OwnerUserEmail = db.Users.Find(item.OwnerUserId).Email,
                    HouseholdId = item.HouseholdId,
                    HouseholdName = db.Households.Find(item.HouseholdId).Name,
                    Created = db.Households.Find(item.HouseholdId).Created
                });
            }

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult CreateJoinHousehold(CreateJoinViewModel model)
        {
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult CreateHousehold([Bind(Include = "Name")] CreateJoinViewModel model)
        {
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);

            //Create new household
            Household household = new Household
            {
                Name = model.Name,
                OwnerUserId = currentUserId,
                Created = DateTimeOffset.Now
            };

            //Add current user
            household.Members = new List<ApplicationUser>();
            household.Members.Add(currentUser);

            //Add it to the database
            db.Households.Add(household);
            db.SaveChanges();

            //Set the current user's household Id to the household that was just created & added
            currentUser.HouseholdId = db.Households.FirstOrDefault(u => u.OwnerUserId == currentUserId).Id;


            ViewBag.Message = "Household successfully created and joined.";
            return RedirectToAction("Index");
        }
    }


}