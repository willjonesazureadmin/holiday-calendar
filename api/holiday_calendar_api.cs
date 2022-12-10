using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using api.models;
using System.Collections.Generic;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System.Linq;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using Azure.Storage.Blobs.Models;
using Azure;

namespace api
{


    public class holiday_calendar_api
    {
        private readonly BlobServiceClient _storageServiceClient;

        public holiday_calendar_api(BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            this._storageServiceClient = blobServiceClient;
        }
        [FunctionName("holiday_calendar_get")]
        public IActionResult hc_get(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "calendars")] HttpRequest req, ILogger log)
        {
            log.LogInformation("Get Calendar Requested");
            try
            {
                var calendars = GetCalendars();
                return new OkObjectResult(calendars);
            }
            catch
            {
                return new BadRequestObjectResult(null);
            }

        }

        [FunctionName("holiday_calendar_get_id")]
        public IActionResult hc_get_id(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "calendars/{calendarId}")] HttpRequest req, Guid calendarId, ILogger log)
        {
            log.LogInformation("Get Calendar By Id Requested");
            var c = GetCalendarById(calendarId);

            if (c != null)
            {
                return new OkObjectResult(c);
            }
            return new NotFoundObjectResult(null);
        }

        [FunctionName("holiday_calendar_add")]
        public async Task<IActionResult> hc_add(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "calendars")] HttpRequest req, ILogger log)
        {
            log.LogInformation("Add Calendar Requested");
            string requestBody = String.Empty;
            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string noe = data?.noOfEntries;
            string month = data?.month;
            string year = data?.year;
            string calType = data?.calendarType;
            var c = AddCalendar(noe, month, year, calType);
            return new OkObjectResult(c);
        }


        [FunctionName("holiday_calendar_get_calendar_entry_by_id")]
        public IActionResult holiday_calendar_get_calendar_entry_by_id(
                          [HttpTrigger(AuthorizationLevel.Function, "get", Route = "calendars/{calendarId}/entries")] HttpRequest req, Guid calendarId, ILogger log)
        {
            log.LogInformation("Get All Calendar Images");

            try
            {
                var c = GetCalendarById(calendarId);
                {
                    if (c != null)
                    {
                        List<CalendarEntry> cel = GetCalendarEntriesByCalendarId(c, ImageType.None);
                        return new OkObjectResult(cel);
                    }
                    else
                    {
                        return new NotFoundObjectResult(null);
                    }
                }

            }
            catch
            {
                return new BadRequestObjectResult(null);
            }
        }

      [FunctionName("holiday_calendar_placeholder_get_image")]
        public IActionResult holiday_calendar_placeholder_get_image(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "calendars/{calendarId}/placeholders/{imageType}/image")] HttpRequest req, Guid calendarId, string imageType, ILogger log)
        {
            log.LogInformation("Calendar Placeholder Image URL Requested");
            var c = GetCalendarById(calendarId);
            if (c != null)
            {
                var it = (ImageType)Enum.Parse(typeof(ImageType), imageType);
                var ce = GetCalendarImageByType(c, it, "", false);
                if(ce != null)
                {
                return new OkObjectResult(GetImageSAS(ce));
                }
                else
                {
                    return new NotFoundObjectResult(null);
                }
            }
            else
            {
                return new NotFoundObjectResult(null);
            }

        }

        [FunctionName("holiday_calendar_patch_placeholder_images")]
        public async Task<IActionResult> holiday_calendar_patch_placeholder_images(
            [HttpTrigger(AuthorizationLevel.Function, "patch", "post", Route = "calendars/{calendarId}/placeholders/{imageType}/$value")] HttpRequest req, Guid calendarId, string imageType, ILogger log)
        {
            log.LogInformation("Calendar Image Update Reference Requested");
            string contentType = req.ContentType;
            Stream uploadStream = new MemoryStream();
            Stream resizeStream = new MemoryStream();
            await req.Body.CopyToAsync(uploadStream);


            if (contentType == "image/jpeg" || contentType == "image/png" || contentType == "image/bmp" || contentType == "image/gif" )
            {

                var it = (ImageType)Enum.Parse(typeof(ImageType), imageType);
                IImageFormat format;
                var c = GetCalendarById(calendarId);
                if (c != null)
                {
                    var ce = GetCalendarImageByType(c, it, contentType, true);
                    if (ce != null)
                    {
                        uploadStream.Position = 0;
                        using (var image = Image.Load(uploadStream, out format))
                        {                 

                            image.Mutate(x => x.Resize(600, 600, KnownResamplers.Lanczos3));
                            image.Save(resizeStream, format);
                        }
                        UploadImage(ce, uploadStream);
                        c = UpdateImageReference(c, it, ce.fileReference);

                        return new OkObjectResult(ce);
                    }
                    else return null;
                }
            }

            return new NotFoundObjectResult(null);
        }




        [FunctionName("holiday_calendar_entry_get_by_id")]
        public IActionResult hce_get_id(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "calendars/{calendarId}/entries/{dayno}")] HttpRequest req, Guid calendarId, int dayno, ILogger log)
        {
            try
            {
                var c = GetCalendarById(calendarId);
                {
                    if (c != null)
                    {
                        var ce = GetCalendarEntryByDayTag(c, c.containerName, dayno, null, false);
                        if (ce != null)
                        {
                            return new OkObjectResult(ce);
                        }
                        else
                        {
                            return new NotFoundObjectResult(null);
                        }

                    }
                    else
                    {
                        return new NotFoundObjectResult(null);
                    }
                }

            }
            catch
            {
                return new BadRequestObjectResult(null);
            }
        }

        [FunctionName("holiday_calendar_entry_reveal")]
        public IActionResult hce_reveal_image_reveal(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "calendars/{calendarId}/entries/{dayno}/reveal")] HttpRequest req, Guid calendarId, int dayno, ILogger log)
        {
            log.LogInformation("Calendar Image Reveal URL Requested");
            var c = GetCalendarById(calendarId);
            if (c != null)
            {
                var ce = GetCalendarEntryByDayTag(c, c.containerName, dayno, null, false);
                if(ce != null)
                {
                return new OkObjectResult(Reveal(c, ce, dayno));
                }
                else
                {
                    return new NotFoundObjectResult(null);
                }
            }
            else
            {
                return new NotFoundObjectResult(null);
            }

        }

        [FunctionName("holiday_calendar_entry_actual")]
        public IActionResult hce_reveal_image_actual(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "calendars/{calendarId}/entries/{dayno}/actual")] HttpRequest req, Guid calendarId, int dayno, ILogger log)
        {
            log.LogInformation("Calendar Image actual URL Requested");
            var c = GetCalendarById(calendarId);
            if (c != null)
            {
                var ce = GetCalendarEntryByDayTag(c, c.containerName, dayno, null, false);
                if(ce != null)
                {
                return new OkObjectResult(GetImageSAS(ce));
                }
                else
                {
                    return new NotFoundObjectResult(null);
                }
            }
            else
            {
                return new NotFoundObjectResult(null);
            }

        }

        [FunctionName("holiday_calendar_entry_default")]
        public IActionResult holiday_calendar_entry_default(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "calendars/{calendarId}/entries/{dayno}/default")] HttpRequest req, Guid calendarId, int dayNo, ILogger log)
        {
            log.LogInformation("Calendar Image Default URL Requested");
            var c = GetCalendarById(calendarId);
            var ce = GetCalendarEntryByDayTag(c, c.containerName, dayNo, null, false);
            if (c != null)
            {
                return new OkObjectResult(Default(c, ce, dayNo));
            }
            else
            {
                return new NotFoundObjectResult(null);
            }

        }


        [FunctionName("holiday_calendar_entry_patch_image")]
        public async Task<IActionResult> holiday_calendar_entry_patch_image(
            [HttpTrigger(AuthorizationLevel.Function, "patch","post", Route = "calendars/{calendarId}/entries/{dayno}/$value")] HttpRequest req, Guid calendarId, int dayNo, ILogger log)
        {
            log.LogInformation("Calendar Entry Image Update Requested");
            string contentType = req.ContentType;
            Stream uploadStream = new MemoryStream();
            Stream resizeStream = new MemoryStream();
            await req.Body.CopyToAsync(uploadStream);


            if (contentType == "image/jpeg" || contentType == "image/png" || contentType == "image/bmp" || contentType == "image/gif")
            {


                IImageFormat format;
                var c = GetCalendarById(calendarId);
                if (c != null)
                {
                    var ce = GetCalendarEntryByDayTag(c, c.containerName, dayNo, contentType, true);
                    uploadStream.Position = 0;
                    using (var image = Image.Load(uploadStream, out format))
                    {

                        image.Mutate(x => x.Resize(480, 480));
                        image.Save(resizeStream, format);
                    }
                    UploadImage(ce, uploadStream);
                    return new OkObjectResult(ce);

                }
                else
                {
                    return new BadRequestObjectResult(null);
                }

            }
            else
            {
                return new BadRequestObjectResult(null);
            }
        }

        #region Private Methods


        private CalendarEntry GetCalendarImageByType(Calendar calendar, ImageType imageType, string contentType, bool createnew = false)
        {
            try
            {
                string query = $"@container = '{calendar.containerName}' AND \"imageType\" = '{imageType}'";
                var resultSet = _storageServiceClient.FindBlobsByTags(query).FirstOrDefault();
                if (resultSet != null)
                {
                    var blob = _storageServiceClient.GetBlobContainerClient(resultSet.BlobContainerName).GetBlobClient(resultSet.BlobName);
                    var tags = blob.GetTags().Value;
                    return new CalendarEntry(tags.Tags);
                }
                else if (createnew == true)
                {
                    return new CalendarEntry(calendar, imageType, contentType);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private string Default(Calendar c, CalendarEntry ce, int dayno)
        {
            if (ce != null)
            {
                {
                    return GetImageSAS(GetCalendarImageByType(c, ImageType.Default, null, false));
                }
            }
            else
            {
                return GetImageSAS(GetCalendarImageByType(c, ImageType.Missing, null, false));
            }
        }

        private string Reveal(Calendar c, CalendarEntry ce, int dayno)
        {
            if (ce != null)
            {
                if (ce.allowedDate <= DateTime.Now)
                {
                    return GetImageSAS(ce);
                }
                else
                {
                    return GetImageSAS(GetCalendarImageByType(c, ImageType.TooEarly, null, false));
                }
            }
            else
            {
                return GetImageSAS(GetCalendarImageByType(c, ImageType.Missing, null, false));
            }

        }

        private string GetImageSAS(CalendarEntry ce)
        {
            var _blobContainerClient = _storageServiceClient.GetBlobContainerClient(ce.containerName);
            return _blobContainerClient.GetBlobClient(ce.fileReference).GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.Now.AddMinutes(1)).AbsoluteUri.ToString();
        }

        private CalendarEntry GetCalendarEntryByDayTag(Calendar calendar, string containerName, int dayNo, string contentType, bool createnew = false)
        {
            try
            {
                string query = $"@container = '{containerName}' AND \"day\" = '{dayNo.ToString()}' AND \"imageType\" = 'None'";
                var resultSet = _storageServiceClient.FindBlobsByTags(query).FirstOrDefault();
                if (resultSet != null)
                {
                    var blob = _storageServiceClient.GetBlobContainerClient(resultSet.BlobContainerName).GetBlobClient(resultSet.BlobName);
                    var tags = blob.GetTags().Value;
                    return new CalendarEntry(tags.Tags);
                }
                else if (createnew == true)
                {
                    return new CalendarEntry(calendar, dayNo, contentType);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private Calendar AddCalendar(string noOfEntries, string month, string year, string type)
        {
            Calendar c = new Calendar(int.Parse(noOfEntries), int.Parse(month), int.Parse(year), (CalendarType)Enum.Parse(typeof(CalendarType), type), null, null, null, Guid.NewGuid());
            IDictionary<string, string> metadata = c.GetMetaData();
            this._storageServiceClient.CreateBlobContainer(c.containerName.ToString(), Azure.Storage.Blobs.Models.PublicAccessType.None, metadata, default);
            return GetCalendarById(c.id);
        }


        private Calendar GetCalendarById(Guid Id)
        {

            try
            {
                var r = new List<Calendar>();
                var result = _storageServiceClient.GetBlobContainers(Azure.Storage.Blobs.Models.BlobContainerTraits.Metadata, Id.ToString(), default).FirstOrDefault();
                if (result != null)
                {
                    return new Calendar(result.Properties.Metadata);
                }
                else
                {
                    return null;
                }


            }
            catch (RequestFailedException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }

        }

        private CalendarEntry GetPlaceHolderImageByName(Calendar c, string name)
        {
            try
            {
                var _containerClient = _storageServiceClient.GetBlobContainerClient(c.containerName);
                var _blobClient = _containerClient.GetBlobClient(name);

                if (_blobClient.Exists())
                {
                    return new CalendarEntry(_blobClient.GetTags().Value.Tags);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private List<Calendar> GetCalendars()
        {
            try
            {
                var cl = new List<Calendar>();
                var result = _storageServiceClient.GetBlobContainers(BlobContainerTraits.Metadata, null, default).ToList();

                foreach (var r in result)
                {
                    cl.Add(new Calendar(r.Properties.Metadata));
                }
                return cl;

            }
            catch (RequestFailedException e)
            {
                Console.WriteLine(e.Message);

                throw;
            }
        }


        private List<CalendarEntry> GetCalendarEntriesByCalendarId(Calendar c, ImageType imageType)
        {
            List<CalendarEntry> cel = new List<CalendarEntry>();
            try
            {
                string query = $"@container = '{c.containerName}' AND \"imageType\" = '{imageType.ToString()}'";
                var resultSet = _storageServiceClient.FindBlobsByTags(query).ToList();
                if (resultSet != null)
                {
                    foreach (var r in resultSet)
                    {
                        var blob = _storageServiceClient.GetBlobContainerClient(r.BlobContainerName).GetBlobClient(r.BlobName);
                        var tags = blob.GetTags().Value;
                        cel.Add(new CalendarEntry(tags.Tags));
                    }
                    return cel;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }



        private void UploadImage(CalendarEntry ce, Stream resizeStream)
        {
            try
            {
                BlobContainerClient _blobContainerClient = this._storageServiceClient.GetBlobContainerClient(ce.containerName);
                resizeStream.Position = 0;
                BlobClient _blobClient = _blobContainerClient.GetBlobClient(ce.fileReference);
                _blobClient.Upload(resizeStream, true);
                IDictionary<string, string> meta = ce.GetMetadata();
                _blobClient.SetTags(meta);
            }
            catch
            {

            }        
        }

        private Calendar UpdateImageReference(Calendar c, ImageType it, string imageReference)
        {
            switch (it)
            {
                case ImageType.TooEarly:
                    c.tooEarlyReference = imageReference;
                    break;
                case ImageType.Default:
                    c.defaultReference = imageReference;
                    break;
                case ImageType.Missing:
                    c.missingReference = imageReference;
                    break;
            }

            return UpdateCalendarTags(c);
        }

        private Calendar UpdateCalendarTags(Calendar c)
        {
            var _containerClient = _storageServiceClient.GetBlobContainerClient(c.containerName);
            _containerClient.SetMetadata(c.GetMetaData(), null, default);

            return new Calendar(_containerClient.GetProperties().Value.Metadata);



        }

        #endregion
    }

}