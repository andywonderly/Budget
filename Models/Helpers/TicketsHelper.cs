using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker2.Models.Helpers
{
    public class TicketsHelper
    {
        ApplicationDbContext db = new ApplicationDbContext();

        //These helper functions return a select list.  They take in a ticket that presumably
        //has whatever property it is generating a list for.  The overall point of these helpers
        //is to have the default selected item be the current property value.  For example,
        //if you are listing ticket priorities 1 through 5, and the priority of the ticket in question
        //is 2, then we want 2 to be the selected value when the list appears.

        public List<SelectListItem> TicketPrioritiesSelectList(Ticket ticket)
        {
            SelectListItem item2 = new SelectListItem();
            List<SelectListItem> ticketPriorityList = new List<SelectListItem>();

            foreach (var item in db.TicketPriorities)
            {
                bool isSelected = false;

                if (item.Id.ToString() == ticket.TicketStatusId)
                    isSelected = true;

                item2 = new SelectListItem
                {
                    Selected = isSelected,
                    Text = item.Name,
                    Value = item.Id.ToString()
                };

                ticketPriorityList.Add(item2);

            }

            return ticketPriorityList;
        }

        public List<SelectListItem> TicketStatusesSelectList(Ticket ticket)
        {
            SelectListItem item2 = new SelectListItem();
            List<SelectListItem> ticketStatusList = new List<SelectListItem>();

            foreach (var item in db.TicketStatuses)
            {
                bool isSelected = false;

                if (item.Id.ToString() == ticket.TicketStatusId)
                    isSelected = true;

                item2 = new SelectListItem
                {
                    Selected = isSelected,
                    Text = item.Name,
                    Value = item.Id.ToString()
                };

                ticketStatusList.Add(item2);

            }

            return ticketStatusList;
        }

        public List<SelectListItem> TicketTypesSelectList(Ticket ticket)
        {
            SelectListItem item2 = new SelectListItem();
            List<SelectListItem> ticketTypeList = new List<SelectListItem>();

            foreach (var item in db.TicketTypes)
            {
                bool isSelected = false;

                if (item.Id.ToString() == ticket.TicketTypeId)
                    isSelected = true;

                item2 = new SelectListItem
                {
                    Selected = isSelected,
                    Text = item.Name,
                    Value = item.Id.ToString()
                };

                ticketTypeList.Add(item2);

            }

            return ticketTypeList;

        }

    }
}