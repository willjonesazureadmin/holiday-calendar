using System;
using System.ComponentModel.DataAnnotations;

namespace spa.Shared.Models
{
    public class Calendar
    {
        public Guid Id { get; set; }
        public String CalendarName { get; set; }

        public CalendarType CalendarType { get; set; }

        [Required]
        [Range(1,12, ErrorMessage = "Value must be between {1} and {2}")]
        public int month { get; set; }

        [Required]
        [Range(2022,2050, ErrorMessage = "Value must be between {1} and {2}")]
        public int year { get; set; }

        [Required]
        [Range(1,24, ErrorMessage = "Value must be between {1} and {2}")]
        public int noOfEntries { get; set; }

        public String TooEarlyReference {get; set; }
        public String DefaultReference {get; set; }

        public String MissingReference {get; set; }

    }
}
