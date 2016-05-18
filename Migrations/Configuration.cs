namespace Budget.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Budget.Models;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            context.Categories.AddOrUpdate(
                p => p.Name,
                new Category { Name = "Utilities" },
                new Category { Name = "Groceries" },
                new Category { Name = "TV-Internet" }
            );

            var roleManager = new RoleManager<IdentityRole>(
            new RoleStore<IdentityRole>(context));

            if (!context.Users.Any(r => r.DisplayName == "Admin"))
            {
                roleManager.Create(new IdentityRole { Name = "Admin" });
            }

            if (!context.Users.Any(r => r.DisplayName == "User"))
            {
                roleManager.Create(new IdentityRole { Name = "User" });
            }



            var userManager = new UserManager<ApplicationUser>(
                    new UserStore<ApplicationUser>(context));

            if (!context.Users.Any(u => u.Email == "andywonderly@gmail.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "andywonderly@gmail.com",
                    Email = "andywonderly@gmail.com",
                    FirstName = "Andrew",
                    LastName = "Wonderly",
                    DisplayName = "Andy"
                }, "clickboom");
            }

            var userId = userManager.FindByEmail("andywonderly@gmail.com").Id;
            userManager.AddToRole(userId, "Admin");


        }
    }
}
