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
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Azure.Storage.Sas;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using System.Reflection;

namespace api
{


    public class holiday_calendar_api
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ReadWriteContext _readwritecontext;

        public holiday_calendar_api(ReadWriteContext readwritecontext, BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            this._readwritecontext = readwritecontext;
            this._blobServiceClient = blobServiceClient;
        }
        [FunctionName("holiday_calendar_get")]
        public IActionResult hc_get(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "calendars")] HttpRequest req, ILogger log)
        {
            log.LogInformation("Get Calendar Requested");
            var calendar = GetCalendar();
            return new OkObjectResult(calendar);
        }

        [FunctionName("holiday_calendar_get_id")]
        public async Task<IActionResult> hc_get_id(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "calendars/{id}")] HttpRequest req, Guid Id, ILogger log)
        {
            log.LogInformation("Get Calendar By Id Requested");
            var c = await GetCalendar(Id);

            if (c != null)
            {
                c.GetImages(this._blobServiceClient);
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
            int days = data?.noOfEntries;
            int month = data?.month;
            int year = data?.year;
            CalendarType type = data?.calendarType;
            var calendar = CreateCalendar(type, days, month, year);
            return new OkObjectResult(calendar);
        }

        [FunctionName("holiday_calendar_patch_image")]
        public async Task<IActionResult> hc_patch_image_update_reference(
            [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "calendars/{id}/image/{imageType}/$value")] HttpRequest req, Guid id, string imageType, ILogger log)
        {
            log.LogInformation("Calendar Image Update Reference Requested");
            string contentType = req.ContentType;
            Stream uploadStream = new MemoryStream();
            Stream resizeStream = new MemoryStream();
            await req.Body.CopyToAsync(uploadStream);


            if (contentType == "image/jpeg" || contentType == "image/png" || contentType == "image/bmp" || contentType == "image/gif")
            {


                IImageFormat format;
                var c = await GetCalendar(id);
                if (c != null)
                {
                    uploadStream.Position = 0;
                    using (var image = Image.Load(uploadStream, out format))
                    {

                        image.Mutate(x => x.Resize(480, 480));
                        image.Save(resizeStream, format);
                    }
                    await c.UploadImage(_blobServiceClient, imageType, resizeStream, contentType);
                    _readwritecontext.Entry(c).State = EntityState.Modified;
                    _readwritecontext.SaveChanges();
                    return new OkObjectResult(c);
                }
                else return null;

            }

            return new NotFoundObjectResult(null);
        }

        [FunctionName("holiday_calendar_add_entries")]
        public async Task<IActionResult> hce_add(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "calendars/{id}/entries")] HttpRequest req, Guid Id, ILogger log)
        {
            var calendar = await CreateCalendarEntriesAsync(Id);
            return new OkObjectResult(calendar);
        }

        [FunctionName("holiday_calendar_get_entries")]
        public async Task<IActionResult> hce_get(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "calendars/{id}/entries")] HttpRequest req, Guid id, ILogger log)
        {
            log.LogInformation("Calendar Entries Requested By Calendar Id");
            var calendarEntry = await GetCalenderEntryByCalendarId(id);
            return new OkObjectResult(calendarEntry);
        }

        [FunctionName("holiday_calendar_entry_get_by_id")]
        public async Task<IActionResult> hce_get_id(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "entries/{id}")] HttpRequest req, Guid id, ILogger log)
        {
            log.LogInformation("Calendar Entry Requested");
            var calendarEntry = await GetCalenderEntry(id);
            return new OkObjectResult(calendarEntry);
        }

        [FunctionName("holiday_calendar_entry_reveal")]
        public async Task<IActionResult> hce_reveal_image_reveal(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "entries/{id}/reveal")] HttpRequest req, Guid id, ILogger log)
        {
            log.LogInformation("Calendar Image Reveal URL Requested");
            var ce = await RevealCalendarEntry(id);
            if (ce.Id != Guid.Empty)
            {
                return new OkObjectResult(ce);
            }
            return new NotFoundObjectResult(null);

        }


        [FunctionName("holiday_calendar_entry_patch_image")]
        public async Task<IActionResult> hce_patch_image(
            [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "entries/{id}/image/$value")] HttpRequest req, Guid id, ILogger log)
        {
            log.LogInformation("Calendar Entry Image Update Requested");
            string contentType = req.ContentType;
            Stream uploadStream = new MemoryStream();

            await req.Body.CopyToAsync(uploadStream);
            if (contentType == "image/jpeg" || contentType == "image/png")
            {
                var ce = await GetCalenderEntry(id);
                var c = await GetCalendar(ce.CalendarId);
                if (ce.Id != null)
                {
                    ce.UploadImage(_blobServiceClient, uploadStream, c.ContainerName);
                    _readwritecontext.Entry(ce).State = EntityState.Modified;
                    _readwritecontext.SaveChanges();
                    return new OkObjectResult(ce);
                }
                else return null;
            }
            else
            {
                return new NotFoundObjectResult(null);
            }
        }

        private async Task<Calendar> GetCalendar(Guid Id)
        {
            var c = await _readwritecontext.Calendar.Where(c => c.Id == Id).FirstOrDefaultAsync();
            return c;
        }

        private List<Calendar> GetCalendar()
        {
            var c = _readwritecontext.Calendar.ToList();
            return c;
        }

        private async Task<CalendarEntry> GetCalenderEntry(Guid id)
        {
            var ce = await _readwritecontext.CalendarEntry.Where(a => a.Id == id).FirstOrDefaultAsync();
            if (ce.Id != null)
            {
                var c = await _readwritecontext.Calendar.FirstOrDefaultAsync(a => a.Id == ce.CalendarId);
                if (c.Id != null)
                {
                    ce.SetDefaultImage(this._blobServiceClient, c.ContainerName, c.DefaultReference);
                }
                return ce;
            }
            return null;
        }

        private async Task<List<CalendarEntry>> GetCalenderEntryByCalendarId(Guid id)
        {
            var c = await _readwritecontext.Calendar.Where(a => a.Id == id).FirstOrDefaultAsync();
            if (c.Id == null)
            {
                return null;
            }
            var cel = _readwritecontext.CalendarEntry.Where(a => a.CalendarId == id).ToList();
            foreach (var ce in cel)
            {
                ce.SetDefaultImage(this._blobServiceClient, c.ContainerName, c.DefaultReference);
            }
            return cel;
        }

        private Calendar CreateCalendar(CalendarType _type, int _days, int _month, int _year)
        {
            var calendar = new Calendar(_days, _month, _year, _type);
            _readwritecontext.Add(calendar);
            _readwritecontext.SaveChanges();
            return calendar;
        }


        private async Task<List<CalendarEntry>> CreateCalendarEntriesAsync(Guid CalendarId)
        {
            var calendar = await _readwritecontext.Calendar.Where(a => a.Id == CalendarId).FirstOrDefaultAsync();
            var cel = await _readwritecontext.CalendarEntry.Where(a => a.CalendarId == calendar.Id).ToListAsync();
            if (calendar.Id != null)
            {
                if (calendar.DefaultReference == null || calendar.TooEarlyReference == null)
                {
                    return null;
                }
                else
                {
                    for (int i = (cel.Count + 1); i <= calendar.noOfEntries; i++)
                    {
                        var nce = new CalendarEntry(i.ToString(), new DateTime(calendar.year, calendar.month, i), 0, calendar.CalendarType, calendar.Id, calendar.ContainerName);
                        cel.Add(nce);
                        _readwritecontext.Add(nce);
                    }
                    _readwritecontext.SaveChanges();
                    return cel;
                }
            }
            else
            {
                return null;
            }
        }

        private async Task<CalendarEntry> RevealCalendarEntry(Guid id)
        {
            var ce = await GetCalenderEntry(id);
            var c = await GetCalendar(ce.CalendarId);
            if (ce.Id != null)
            {
                ce.RevealEntry(_blobServiceClient, c.ContainerName, c.TooEarlyReference, c.MissingReference);
            }
            return ce;
        }

    }
}