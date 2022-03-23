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
        public String ContainerName
        {
            get
            {
                return Id.ToString();
            }
        }
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

        public async Task UploadImage(BlobServiceClient _client, string imageType, Stream content, string contentType)
        {
            var enumImageType = (ImageType)Enum.Parse(typeof(ImageType), imageType);
            var blobName = this.Id + imageType + "." + contentType.Split('/').Last();
            BlobContainerClient _containerClient = _client.GetBlobContainerClient(this.ContainerName);
            _containerClient.CreateIfNotExists();
            IProgress<long> ph = new Progress<long>(progress =>
            {
                Console.WriteLine(progress.ToString());
            });
            content.Position = 0;
            var _blobclient = _containerClient.GetBlobClient(blobName);
            await _blobclient.UploadAsync(content, null, null, null, progressHandler: ph);

            switch (enumImageType)
            {
                case ImageType.TooEarly:
                    this.TooEarlyReference = blobName;
                    break;
                case ImageType.Default:
                    this.DefaultReference = blobName;
                    break;
                case ImageType.Missing:
                    this.MissingReference = blobName;
                    break;
            }

        }

        public void GetImages(BlobServiceClient _client)
        {
            BlobContainerClient _blobclient = _client.GetBlobContainerClient(this.ContainerName);
            if(this.TooEarlyReference != null && this.TooEarlyReference != "") this.TooEarlyReference = _blobclient.GetBlobClient(TooEarlyReference).GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.Now.AddMinutes(1)).AbsoluteUri.ToString(); else {this.TooEarlyReference = "pending.png";}
            if(this.DefaultReference != null && this.DefaultReference != "") this.DefaultReference = _blobclient.GetBlobClient(DefaultReference).GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.Now.AddMinutes(1)).AbsoluteUri.ToString();  else {this.DefaultReference = "pending.png";}
            if(this.MissingReference != null && this.MissingReference != "") this.MissingReference = _blobclient.GetBlobClient(MissingReference).GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.Now.AddMinutes(1)).AbsoluteUri.ToString();  else {this.MissingReference = "pending.png";}



        }

    }


}
