using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Helpdesk.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace HelpdeskClient.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HelpDeskController : ControllerBase
    {
        private HelpdeskContext helpdeskContext;

        public HelpDeskController(HelpdeskContext helpdeskContext)
        {
            this.helpdeskContext = helpdeskContext;
        }
        [Authorize(Roles = "Admins")]
        [HttpGet]
        public object Get()
        {
            StringValues Skip;
            StringValues Take;
            StringValues OrderBy;

            var totalRecordCount = helpdeskContext.HelpDeskTickets.Count();
            int skip = (Request.Query.TryGetValue("$skip", out Skip)) ? Convert.ToInt32(Skip[0]) : 0;
            int top = (Request.Query.TryGetValue("$top", out Take)) ? Convert.ToInt32(Take[0]) : totalRecordCount;
            string orderBy = (Request.Query.TryGetValue($"orderby", out OrderBy)) ? OrderBy.ToString() : "TicketDate";

            if (orderBy.EndsWith(" desc"))
            {
                orderBy = orderBy.Replace(" desc", "");

                return new
                {
                    Items = helpdeskContext.HelpDeskTickets
                    .OrderByDescending(orderBy)
                    .Skip(skip)
                    .Take(top),
                    Count = totalRecordCount
                };

            }
            else
            {
                System.Reflection.PropertyInfo prop =
                    typeof(HelpDeskTickets).GetProperty(orderBy);

                return new
                {
                    Items = helpdeskContext.HelpDeskTickets
                    .OrderBy(orderBy)
                    .Skip(skip)
                    .Take(top),
                    Count = totalRecordCount
                };
            }

        }
        [HttpPost]
        [AllowAnonymous]
        public Task Post(HelpDeskTickets newHelpDeskTickets)
        {
            // Add a new Help Desk Ticket
            helpdeskContext.HelpDeskTickets.Add(newHelpDeskTickets);
            helpdeskContext.SaveChanges();

            return Task.FromResult(newHelpDeskTickets);
        }
        [HttpPut]
        [AllowAnonymous]
        public Task
            PutAsync(HelpDeskTickets UpdatedHelpDeskTickets)
        {
            // Get the existing record
            // Note: Caller must have the TicketGuid
            var existingTicket =
                helpdeskContext.HelpDeskTickets
                .Where(x => x.TicketGuid ==
                UpdatedHelpDeskTickets.TicketGuid)
                .FirstOrDefault();

            if (existingTicket != null)
            {
                existingTicket.TicketDate =
                    UpdatedHelpDeskTickets.TicketDate;

                existingTicket.TicketDescription =
                    UpdatedHelpDeskTickets.TicketDescription;

                existingTicket.TicketGuid =
                    UpdatedHelpDeskTickets.TicketGuid;

                existingTicket.TicketRequesterEmail =
                    UpdatedHelpDeskTickets.TicketRequesterEmail;

                existingTicket.TicketStatus =
                    UpdatedHelpDeskTickets.TicketStatus;

                // Insert any new TicketDetails
                if (UpdatedHelpDeskTickets.HelpDeskTicketDetails != null)
                {
                    foreach (var item in
                        UpdatedHelpDeskTickets.HelpDeskTicketDetails)
                    {
                        if (item.Id == 0)
                        {
                            // Create New HelpDeskTicketDetails record
                            HelpDeskTicketDetails newHelpDeskTicketDetails =
                                new HelpDeskTicketDetails();
                            newHelpDeskTicketDetails.HelpDeskTicketId =
                                UpdatedHelpDeskTickets.Id;
                            newHelpDeskTicketDetails.TicketDetailDate =
                                DateTime.Now;
                            newHelpDeskTicketDetails.TicketDescription =
                                item.TicketDescription;

                            helpdeskContext.HelpDeskTicketDetails
                                .Add(newHelpDeskTicketDetails);
                        }
                    }
                }

                helpdeskContext.SaveChanges();
            }
            else
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        [Authorize(Roles = "Admins")]
        [HttpDelete]
        public Task
           Delete(
           string HelpDeskTicketGuid)
        {
            // Get the existing record
            var ExistingTicket =
                helpdeskContext.HelpDeskTickets.Include(x => x.HelpDeskTicketDetails)
                .Where(x => x.TicketGuid == HelpDeskTicketGuid)
                .FirstOrDefault();

            if (ExistingTicket != null)
            {
                // Delete the Help Desk Ticket
                helpdeskContext.HelpDeskTickets.Remove(ExistingTicket);
                helpdeskContext.SaveChanges();
            }
            else
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }

    public static class IQueryableExtensions
    {
        public static IOrderedQueryable<T> OrderBy<T>(
            this IQueryable<T> source, string propertyName)
        {
            return source.OrderBy(ToLambda<T>(propertyName));
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(
            this IQueryable<T> source, string propertyName)
        {
            return source.OrderByDescending(ToLambda<T>(propertyName));
        }

        private static Expression<Func<T, object>> ToLambda<T>(
            string propertyName)
        {
            var parameter = Expression.Parameter(typeof(T));
            var property = Expression.Property(parameter, propertyName);
            var propAsObject = Expression.Convert(property, typeof(object));

            return Expression.Lambda<Func<T, object>>(propAsObject, parameter);
        }
    }


}

