using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Blobs;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.models
{
    public class CalendarEntry
    {
        public Guid CalendarId {get; private set;}
        public Guid Id { get; private set; }
        public string dayNo { get; private set; }
        public DateTime dateTime { get; private set; }

        public CalendarType calendarType {get; private set;}
        private string imageReference { get; set;}

        [NotMapped]
        public string imageUrl { get; private set; }
        public int Score { get; private set; }

        public CalendarEntry()
        {
            
        }
        public CalendarEntry(string _dayNo, DateTime _dateTime, int _score, CalendarType _calendarType, Guid _calendarId, string _containerName)
        {
            this.dayNo = _dayNo;
            this.dateTime = _dateTime;
            this.Score = _score;
            this.calendarType = _calendarType;
            this.CalendarId = _calendarId;
            this.imageReference = _containerName;
        }

        public string ImageSASCreate(BlobServiceClient _client, string imageName, string container)
        {
            BlobContainerClient _blobclient = _client.GetBlobContainerClient(container);
            return _blobclient.GetBlobClient(imageName).GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Create, DateTimeOffset.Now.AddMinutes(1)).AbsoluteUri.ToString();
        }

        public void UploadImage(BlobServiceClient _client, Stream content, string container)
        {
            BlobContainerClient _blobclient = _client.GetBlobContainerClient(container);
            content.Position = 0;
            _blobclient.UploadBlob(this.Id.ToString(),content);
        }


        public void RevealEntry(BlobServiceClient _client, string container, string defaultImageRef, string backupImageRef)
        {
            BlobContainerClient _blobclient = _client.GetBlobContainerClient(container);
            if(this.dateTime > DateTime.Now) { this.imageUrl = _blobclient.GetBlobClient(defaultImageRef).GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.Now.AddMinutes(1)).AbsoluteUri.ToString(); this.dayNo = "Too Early" ;}            
            else { 
                if (this.imageReference == null)
                {
                    this.imageUrl = _blobclient.GetBlobClient(backupImageRef).GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.Now.AddMinutes(1)).AbsoluteUri.ToString(); this.dayNo = "Missing!" ; 
                }
                else
                {
                    this.imageUrl = _blobclient.GetBlobClient(container).GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.Now.AddMinutes(1)).AbsoluteUri.ToString(); 
                }
            }
        }

        public void SetDefaultImage(BlobServiceClient _client, string containerName, string imageName)
        {
            BlobContainerClient _blobclient = _client.GetBlobContainerClient(containerName);
            this.imageUrl = _blobclient.GetBlobClient(imageName).GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.Now.AddMinutes(1)).AbsoluteUri.ToString();
        }

        
    }

    public class ImageReference
    {
        public string ImageName { get; set; }

        public string ContainerName { get; set; }
    }
    
}