using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace api.models
{
    public class Calendar
    {
        public Guid Id { get; set; }
        public String CalendarName { get; set; }
        public String ContainerName { get {
            return Id.ToString();
        } }
        public CalendarType CalendarType { get; set; }

        public int month { get; private set; }

        public int year { get; private set; }

        public int noOfEntries { get; private set; }

        public String TooEarlyReference { get; set; }
        public String DefaultReference { get; set; }

        public String MissingReference { get; set; }

        public Calendar()
        {
        }

        public Calendar(int days, int month, int year, CalendarType _CalendarType)
        {
            this.CalendarName = _CalendarType.ToString() + " " + month.ToString() + " " + year.ToString();
            this.CalendarType = _CalendarType;
            this.month = month;
            this.year = year;
            this.noOfEntries = days;           
        }

        public async Task UploadImage(BlobServiceClient _client,  string imageType, Stream content, string contentType)
        {
            var enumImageType = (ImageType)Enum.Parse(typeof(ImageType),imageType);
            var blobName = this.Id + imageType + "." + contentType.Split('/').Last();
            BlobContainerClient _containerClient = _client.GetBlobContainerClient(this.ContainerName);
            _containerClient.CreateIfNotExists();
            CancellationToken ct = new CancellationToken();
        
            IProgress<long> ph = new Progress<long>( progress => { 
                Console.WriteLine(progress.ToString());
            });
            content.Position = 0;
            var _blobclient = _containerClient.GetBlobClient(blobName);
            await _blobclient.UploadAsync(content, null, null, null, progressHandler:ph);

            // uploadTask.GetAwaiter();
            // while(!uploadTask.IsCompleted)
            // {
            //     Console.WriteLine(DateTime.Now.ToString());
            //     var status = uploadTask.GetAwaiter();                
            //     Console.WriteLine(status.ToString());
            //     uploadTask.GetAwaiter();
            // }
            
            if ( enumImageType == ImageType.TooEarly )  this.TooEarlyReference = blobName;
            if ( enumImageType == ImageType.Default ) this.DefaultReference = blobName;
            if ( enumImageType == ImageType.Missing ) this.MissingReference = blobName;

        }

    }

 
}
