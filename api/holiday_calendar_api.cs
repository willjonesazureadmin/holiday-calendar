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

namespace api
{
    public static class holiday_calendar_api
    {
        
        [FunctionName("holiday_calendar_get")]
        public static async Task<IActionResult> hc_get(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "calendar")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Full Calendar Requested");

            var calendar = InvokeCalendar();

            return new OkObjectResult(calendar);
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

        private static List<CalendarEntry> InvokeCalendar()
        {
            List<CalendarEntry> ce = new List<CalendarEntry>();
            var counter = 24;
            for(int i =1; i<= counter; i++)
            {
                ce.Add(new CalendarEntry(i.ToString(), new DateTime(DateTime.UtcNow.Year, 12, i),  0));
            }
            return ce;
        }

        private static CalendarEntry ReturnCalendarEntry(int day)
        {
            var ce = new CalendarEntry(day.ToString(), new DateTime(DateTime.UtcNow.Year, 12, day), 0);
            ce.RevealEntry();
            return ce;
        }
        
    }
}
