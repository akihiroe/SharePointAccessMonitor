using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SharePoint.AccessMonitorWeb.Models
{
    public class LogItem
    {
        public DateTime Timestamp { get; set; }
        public string User { get; set; }

        public string Message { get; set; }
    }
}