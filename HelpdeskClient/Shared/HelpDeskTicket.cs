using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HelpdeskClient.Shared
{
   public class HelpDeskTicket
    {
        public int Id { get; set; }
        [Required]
        public string TicketStatus { get; set; }
        [Required]
        public DateTime TicketDate { get; set; }
        [Required, StringLength(50, MinimumLength = 2, ErrorMessage ="Açıklama {1} ile {0} arasında olmalı")]
        public string TicketDescription { get; set; }
        [Required,EmailAddress]
        public string TicketRequesterEmail { get; set; }
        public string TicketGuid { get; set; }
        public List<HelpDeskTicketDetail> HelpDeskTicketDetails { get; set; }
    }
}
