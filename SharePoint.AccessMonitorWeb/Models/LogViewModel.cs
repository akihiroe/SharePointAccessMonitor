using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SharePoint.AccessMonitorWeb.Models
{
    public class LogViewModel
    {
        public List<LogItem> Logs { get; set; }

        public LogViewModel()
        {
            if (HttpContext.Current != null)
            {
                lock (HttpContext.Current.Application)
                {
                    Logs = (List<LogItem>)HttpContext.Current.Application["Logs"];
                }
            }
        }

        public void Write(LogItem item)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<MonitorHub>();
            hubContext.Clients.All.update(new
            {
                Timestamp = DateTime.Now,
                Message = item.Message,
                User = item.User,
            });            
        }
    }
}