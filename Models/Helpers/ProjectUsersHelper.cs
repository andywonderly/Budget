using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker2.Models.Helpers
{
    public class ProjectUsersHelper
    {

        ApplicationDbContext db = new ApplicationDbContext();

        public void AddUserToProject(int projectId, string userId)
        {

            ApplicationUser user = db.Users.Find(userId);  //get the user by userId
            Project project = db.Projects.First(p => p.Id == projectId); //get the project by projectId
            IEnumerable<ApplicationUser> projectUsers = project.ProjectUsers.ToList(); //get a list of all project users
            //bool userIsOnProject = projectUsers.Any(n => n.Id == user.Id);
            ProjectUsersHelper projectUsersHelper = new ProjectUsersHelper(); //instantiate the helper

            if (!projectUsersHelper.IsUserOnProject(project.Id, user.Id)) //only add user if they are not already on the project
            {
                project.ProjectUsers.Add(user);
                db.SaveChanges();
            }
            
        }

        public void RemoveUserFromProject(int projectId, string userId)
        {
            ApplicationUser user = db.Users.Find(userId);
            Project project = db.Projects.FirstOrDefault(p => p.Id == projectId);
            IEnumerable<ApplicationUser> projectUsers = project.ProjectUsers.ToList();
            //bool userIsOnProject = projectUsers.Any(n => n.Id == user.Id);
            ProjectUsersHelper projectUsersHelper = new ProjectUsersHelper();

            if (projectUsersHelper.IsUserOnProject(project.Id, user.Id)) //Only remove user if they are on the project
            {
                project.ProjectUsers.Remove(user);
                db.SaveChanges();
            }
        }

        public IList<string> ListProjectUsers(int projectId)
        {
            Project project = db.Projects.First(p => p.Id == projectId);
            IList<string> projectUserList = new List<string>();

            //projectUserList = project.Users.Where(x => x.Id == )

            foreach (var item in project.ProjectUsers)
                projectUserList.Add(item.DisplayName);
            
            return projectUserList;
        }

        public IList<string> ListNonProjectUsers(int projectId)
        {
            Project project = db.Projects.FirstOrDefault(p => p.Id == projectId);
            List<ApplicationUser> userList = db.Users.ToList(); //list of all users
            IList<string> nonUserDisplayNames = new List<string>();

            foreach (var item in project.ProjectUsers) //remove project users from all users to get non-project users
                userList.Remove(item);

            foreach (var item in userList) //add non-project user display names to nonUserDisplayNames
                nonUserDisplayNames.Add(item.DisplayName);

            return nonUserDisplayNames;
        }

        public List<Project> ListUserProjects(string userId)
        {
            ApplicationUser user = db.Users.Find(userId);

            //IEnumerable<Projects> projects = db.Projects.Where(x => x.ProjectUsers == user);
            List<Project> projectsList = user.Projects.ToList();
            return projectsList;
        }

        public bool IsUserOnProject(int projectId, string userId)
        {
            var project = db.Projects.FirstOrDefault(p => p.Id == projectId);
            var flag = project.ProjectUsers.Any(u => u.Id == userId.ToString());
            return (flag);
        }

        public List<string> ListAllUserDisplayNames()
        {
            var allUsers = db.Users.ToList();
            List<string> allUserDisplayNames = new List<string>();

            foreach (var item in allUsers)
                allUserDisplayNames.Add(item.DisplayName);

            return allUserDisplayNames;
        }


    }


}