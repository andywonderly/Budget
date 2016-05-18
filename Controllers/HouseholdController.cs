using Budget.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTrackerForTemplate.Controllers
{
    public class HomeController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

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
            if(currentUser.DefaultHouseholdId != null)
                model.ActiveHousehold = db.Households.Find(currentUser.DefaultHouseholdId);
            else
                model.ActiveHousehold = currentUser.Households.FirstOrDefault();

            model.Transactions = db.Transactions.Where(n => n.OwnerUserId == currentUser.Id).ToList();
            model.Invitations = db.Invitations.Where(n => n.InvitedId == currentUser.Id || n.InvitedEmail == currentUser.Email).ToList();

            model.Notifications = new List<Notification>();

            if (model.ActiveHousehold != null)
                model.Notifications = db.Notifications.Where(n => n.HouseholdId == model.ActiveHousehold.Id).ToList();

            if (currentUserId != null)
                model.Notifications.AddRange(db.Notifications.Where(n => n.UserId == currentUserId));

            model.Invitations = db.Invitations.Where(n => n.InvitedId == currentUserId || n.InvitedEmail == currentUser.Email).ToList();

            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}