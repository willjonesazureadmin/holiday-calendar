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

namespace api
{
  

    public  class holiday_calendar_api
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ReadWriteContext _readwritecontext;
        public holiday_calendar_api(ReadWriteContext readwritecontext, BlobServiceClient blobServiceClient)
        {
            this._readwritecontext = readwritecontext;
            this._blobServiceClient = blobServiceClient;
        }

        [FunctionName("holiday_calendar_add")]
        public async Task<IActionResult> hc_add(
            [HttpTrigger(AuthorizationLevel.Function, "post","get", Route = "calendar/add")] HttpRequest req,
            ILogger log)
        {

            var calendar = InvokeCalendar();

            return new OkObjectResult(calendar);
        }
        
        [FunctionName("holiday_calendar_get")]
        public async Task<IActionResult> hc_get(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "calendar")] HttpRequest req,
            ILogger log)
        {
            var calendar = await GetCalendar();

            return new OkObjectResult(calendar);
        }

        private async Task<List<CalendarEntry>> GetCalendar()
        {
            var c = await _readwritecontext.CalendarEntry.ToListAsync();
            return c;


        }

        [FunctionName("holiday_calendar_entry_get")]
        public static async Task<IActionResult> hce_get(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "calendar/{day}")] HttpRequest req, int day,
            ILogger log)
        {
            log.LogInformation("Calendar Entry Requested");

            var calendarEntry = ReturnCalendarEntry(day);

            return new OkObjectResult(calendarEntry);
        }

        [FunctionName("holiday_calendar_entry_update_image")]
        public async Task<IActionResult> hce_post_image(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "calendar/{id}/image")] HttpRequest req, Guid id,
            ILogger log)
        {
            log.LogInformation("Calendar Entry Requested");
            var formdata = await req.ReadFormAsync();
                var file = req.Form.Files["file"];

                var ce = await _readwritecontext.CalendarEntry.FirstOrDefaultAsync(a => a.Id == id);
                if(ce.Id != null)
                {
                    _blobServiceClient.
                }
                
                ce.imageUrl 

            return new OkObjectResult(calendarEntry);
        }

        private List<CalendarEntry> InvokeCalendar()
        {
            List<CalendarEntry> cel = new List<CalendarEntry>();
            var counter = 24;
            for(int i =1; i<= counter; i++)
            {
                var ce = new CalendarEntry(i.ToString(), new DateTime(DateTime.UtcNow.Year, 12, i),  0);
                cel.Add(ce);
                _readwritecontext.Add(ce);
            }
            _readwritecontext.SaveChanges();
            return cel;
        }

        private static CalendarEntry ReturnCalendarEntry(int day)
        {
            var ce = new CalendarEntry(day.ToString(), new DateTime(DateTime.UtcNow.Year, 12, day), 0);
            ce.RevealEntry();
            return ce;
        }
        
    }
}
