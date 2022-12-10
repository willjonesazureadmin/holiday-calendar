using System;

namespace spa.Shared.Models
{
    public class CalendarEntry
    {
        public Guid filePrefix { get; set; }

        public string fileContent { get; set; }

        public string fileExtension { get; set; }

        public string fileReference { get; set; }
        public DateTime allowedDate { get; set; }
        public int month { get; set; }

        public int year { get; set; }

        public int day { get; set; }

        public ImageType imageType { get; set; }

        public string containerName { get; set; }

        public string imageUrl { get; set; }

        public CalendarEntry()
        {
            this.imageUrl = "/loading.gif";
        }
    }
}
