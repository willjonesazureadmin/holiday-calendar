using System;
using System.Collections.Generic;


namespace api.models
{
    public class Calendar
    {
        public Guid id { get; set; }
        public string containerName
        {
            get
            {
                return id.ToString();
            }
        }
        public String calendarName { get {
            return calendarType.ToString() + " " + month.ToString() + " " + year.ToString();
        } }
        public CalendarType calendarType { get; set; }

        public DateTime calendarDate { get {
            return new DateTime(this.year, this.month, 1);
        }}

        public int month { get; private set; }

        public int year { get; private set; }

        public int noOfEntries { get; private set; }

        public String tooEarlyReference { get; set; }
        public String defaultReference { get; set; }

        public String missingReference { get; set; }

        public Calendar()
        {
        }

        public Calendar(int noOfEntries, int month, int year, CalendarType _CalendarType, string TooEarlyReference, string DefaultReference, string MissingReference, Guid id)
        {
            this.calendarType = _CalendarType;
            this.month = month;
            this.year =year;
            this.noOfEntries = noOfEntries;
            this.tooEarlyReference = TooEarlyReference;
            this.missingReference = MissingReference;
            this.defaultReference = DefaultReference;
            this.id = id;
        }

        public Calendar(IDictionary<string, string> metadata)
        {
            this.noOfEntries =  int.Parse(metadata[nameof(Calendar.noOfEntries)]);
            this.month = int.Parse(metadata[nameof(Calendar.month)]);
            this.year = int.Parse(metadata[nameof(Calendar.year)]);
            this.calendarType = (CalendarType)Enum.Parse(typeof(CalendarType), metadata[nameof(Calendar.calendarType)]);
            this.tooEarlyReference = metadata[nameof(Calendar.tooEarlyReference)];
            this.missingReference = metadata[nameof(Calendar.missingReference)];
            this.defaultReference = metadata[nameof(Calendar.defaultReference)];
            this.id = Guid.Parse(metadata[nameof(Calendar.id)]);
        }

        public IDictionary<string,string> GetMetaData()
        {
            Dictionary<string, string> metadata = new Dictionary<string, string>();
            metadata.Add("noOfEntries", noOfEntries.ToString());
            metadata.Add("month", month.ToString());
            metadata.Add("year", year.ToString());
            metadata.Add("calendarType", calendarType.ToString());
            metadata.Add("tooEarlyReference", tooEarlyReference);
            metadata.Add("defaultReference", defaultReference);
            metadata.Add("missingReference", missingReference);
            metadata.Add("id", id.ToString());
            return metadata;
        }

       
    }


}
