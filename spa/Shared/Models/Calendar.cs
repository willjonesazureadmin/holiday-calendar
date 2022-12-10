using System;
using System.ComponentModel.DataAnnotations;

namespace spa.Shared.Models
{
    public class Calendar
    {
        public Guid id { get; set; }
        public string containerName { get; set;}
        public String calendarName { get; set;}
        public CalendarType calendarType { get; set; }

        public DateTime calendarDate  { get; set;}
        public int month { get;  set; }

        public int year { get;  set; }

        public int noOfEntries { get;  set; }

        public String tooEarlyReference { get; set; }
        public String defaultReference { get; set; }

        public String missingReference { get; set; }

    }
}
