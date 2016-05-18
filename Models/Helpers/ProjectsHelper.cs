using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker2.Models.Helpers
{
    public class ProjectsHelper
    {

        ApplicationDbContext db = new ApplicationDbContext();

        public bool IsUserOnProject(string userId, int projectId)
        {
            var project = db.Projects.FirstOrDefault(p=> p.Id == projectId);
            var flag = project.ProjectUsers.Any(u => u.Id == userId);
            return (flag);
        }

        public ICollection<Project> ListUserProjects(string userId)
        {
            //ApplicationUser user = db.Users.Find(userId);
            IEnumerable<Project> project = new List<Project>().Where(n => n.Id.ToString() == userId);
            ICollection<Project> projects = project.ToList();
            projects = project.ToList();
            return (projects);
        }
    }
}