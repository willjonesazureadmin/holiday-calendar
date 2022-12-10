using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace api.models
{
    public class CalendarEntry
    {

        public Guid filePrefix { get; private set; }

        public string fileContent { get; private set; }

        public string fileExtension
        {
            get
            {
                return this.fileContent.Split("/")[1];
            }
        }

        public string fileReference
        {
            get
            {
                return this.filePrefix + "." + this.fileExtension;
            }
        }

        public DateTime allowedDate
        {
            get
            {
                return new DateTime(this.year, this.month, this.day);
            }
        }

        public int month { get; private set; }

        public int year { get; private set; }

        public int day { get; private set; }

        public ImageType imageType { get; set; }

        public string containerName { get; set; }

        public CalendarEntry()
        {

        }

        public CalendarEntry(Calendar c, int day, string fileContent)
        {
            this.filePrefix = Guid.NewGuid();
            this.day = day;
            this.containerName = c.containerName;
            this.fileContent = fileContent;
            this.month = c.month;
            this.year = c.year;
            this.imageType = ImageType.None;
        }

        public CalendarEntry(Calendar c, ImageType imageType, string fileContent)
        {
            this.filePrefix = Guid.NewGuid();
            this.day = 1;
            this.containerName = c.containerName;
            this.fileContent = fileContent;
            this.month = c.month;
            this.year = c.year;
            this.imageType = imageType;
        }

        public CalendarEntry(IDictionary<string, string> metadata)
        {
            this.containerName = metadata[nameof(CalendarEntry.containerName)];
            this.filePrefix = Guid.Parse(metadata[nameof(CalendarEntry.filePrefix)]);
            this.day = int.Parse(metadata[nameof(CalendarEntry.day)]);
            this.month = int.Parse(metadata[nameof(CalendarEntry.month)]);
            this.year = int.Parse(metadata[nameof(CalendarEntry.year)]);
            this.fileContent = metadata[nameof(CalendarEntry.fileContent)];
            this.imageType = (ImageType)Enum.Parse(typeof(ImageType),metadata[nameof(CalendarEntry.imageType)]);
        }

        public Dictionary<string, string> GetMetadata()
        {
            Dictionary<string, string> metadata = new Dictionary<string, string>();
            metadata.Add("filePrefix", this.filePrefix.ToString());
            metadata.Add("allowedDate", this.allowedDate.ToShortDateString());
            metadata.Add("day", day.ToString());
            metadata.Add("month", month.ToString());
            metadata.Add("year", year.ToString());
            metadata.Add("fileContent", this.fileContent);
            metadata.Add("imageType", this.imageType.ToString());
            metadata.Add("containerName", this.containerName.ToString());
            return metadata;
        }

       

    }


}