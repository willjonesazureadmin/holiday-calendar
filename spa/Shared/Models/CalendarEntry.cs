using System;

namespace spa.Shared.Models
{
    public class CalendarEntry
    {
        public Guid CalendarId {get; set;}
        public Guid Id { get; set; }
        public string dayNo { get; set; }
        public DateTime dateTime { get; set; }

        public string imageUrl { get; set; }
        public int Score { get; set; }
    }
}
