﻿using Budget.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Budget.Models.Extensions;

namespace BugTrackerForTemplate.Controllers
{
    public class HomeController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize]
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
            model.Invitations = db.Invitations.Where(n => n.InvitedEmail == currentUser.Email && n.RespondedTo == false && db.Households.Any( m => m.Id == n.HouseholdId)).ToList();

            model.Notifications = new List<Notification>();

            if (model.Household != null)
                model.Notifications = db.Notifications.Where(n => n.HouseholdId == model.Household.Id).ToList();

            if (currentUserId != null)
                model.Notifications.AddRange(db.Notifications.Where(n => n.UserId == currentUserId));

   

            //model.Invitations = db.Invitations.Where(n => n.InvitedId == currentUserId || n.InvitedEmail == currentUser.Email).ToList();

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


            List<Invitation> invitations = new List<Invitation>();
            invitations = db.Invitations.Where(n => n.InvitedEmail == currentUser.Email && n.RespondedTo == false && db.Households.Any(m => m.Id == n.HouseholdId)).ToList();

            model.Invitations = new List<InvitationViewModel>();


            foreach (var item in invitations)
            {
                //model.Invitations.Add(item);

                InvitationViewModel temp = new InvitationViewModel();

                /*OwnerUserId = item.OwnerUserId,*/
                temp.Id = item.Id;
                temp.InvitedEmail = item.InvitedEmail;
                temp.OwnerUserName = db.Users.Find(item.OwnerUserId).DisplayName;
                temp.OwnerUserEmail = db.Users.Find(item.OwnerUserId).Email;
                temp.HouseholdId = item.HouseholdId;
                temp.HouseholdName = db.Households.Find(item.HouseholdId).Name;
                temp.Created = db.Households.Find(item.HouseholdId).Created;


                model.Invitations.Add(temp);
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
                Created = DateTimeOffset.Now,
                Active = true,
               
            };

            //Add current user
            household.Members = new List<ApplicationUser>();
            household.Members.Add(currentUser);
            db.Households.Add(household);
            db.SaveChanges();


            //Add the stock categories
            List<Category> stock = db.Categories.Where(s => s.Stock == true).ToList();
            //currentUser.HouseholdId = db.Households.FirstOrDefault(u => u.OwnerUserId == currentUserId && u.Created == household.Created).Id;
            Household newHousehold = db.Households.Find(currentUser.HouseholdId);
            newHousehold.Categories = new List<Category>();
            currentUser.HouseholdId = newHousehold.Id;
            foreach(var item in stock)
            {
                Category temp = new Category
                {
                    
                    Household_Id = household.Id,
                    Name = item.Name,
                    Deleted = false,
                    Stock = false,
                };

                db.Categories.Add(temp);
                newHousehold.Categories.Add(temp);
                //db.SaveChanges();
            }

            db.Entry(newHousehold).State = EntityState.Modified;
            db.SaveChanges();
            //Do this so that the categories are assigned an id that we can access in
            //the following foreach loop.

            Household newHousehold2 = db.Households.Find(currentUser.HouseholdId);

            foreach (var item in newHousehold2.Categories)
            {
                BudgetItem temp = new BudgetItem
                {
                    Name = item.Name,
                    Amount = 0,
                    CategoryId = item.Id,
                    Created = DateTimeOffset.Now,
                    Void = false,
                    Deleted = false,
                    HouseholdId = item.Household_Id,
                    BudgetSet = false,

                };

                db.BudgetItems.Add(temp);
                newHousehold.BudgetItems.Add(temp);
            }



            //Set the current user's household Id to the household that was just created & added
            //currentUser.HouseholdId = db.Households.FirstOrDefault(u => u.OwnerUserId == currentUserId && u.Created == household.Created).Id;
            db.Entry(currentUser).State = EntityState.Modified;
            db.Entry(newHousehold2).State = EntityState.Modified;
            db.SaveChanges();

            ViewBag.Message = "Household successfully created and joined.";
            //return RedirectToAction("Index","Household");
            return RedirectToAction("InitialAccount", new { id = newHousehold2.Id });
        }

        [Authorize]
        public ActionResult InitialAccount(int id)
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult InitialAccount([Bind(Include ="Name, Balance")] AccountViewModel model)
        {
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);


            return RedirectToAction("Index", "Household");
        }
    }


}