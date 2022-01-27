using System;

namespace holiday_calendar.spa.Shared.Models
{
    public class Calendar
    {
        public Guid Id { get; set; }
        public String CalendarName { get; set; }
        public String ContainerName { get; set; }

        public int month { get; set; }

        public int year { get; set; }

        public int noOfEntries { get; set; }

        public String TooEarlyReference { get; set; }
        public String DefaultReference { get; set; }

        public String MissingReference { get; set; }
    }
}
