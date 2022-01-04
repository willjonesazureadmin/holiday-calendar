using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace api.models
{
    public class CalendarEntry
    {
        public Guid Id { get; private set; }
        public string dayNo { get; private set; }
        public DateTime dateTime { get; private set; }

        public string imageUrl { get; private set; }
        public int Score { get; private set; }

        public CalendarEntry()
        {
            
        }
        public CalendarEntry(string _dayNo, DateTime _dateTime, int _score)
        {
            this.dayNo = _dayNo;
            this.dateTime = _dateTime;
            this.imageUrl = Settings.doorUrl;
            this.Score = _score;
        }

        public void RevealEntry()
        {
            if(this.dateTime > DateTime.Now) {this.imageUrl = Settings.earlyUrl; this.dayNo = "Too Early" ;}
            else { this.imageUrl = "revealed.jpg"; }
        }
    }
}