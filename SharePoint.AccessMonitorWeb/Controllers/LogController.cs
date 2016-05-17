using Microsoft.AspNet.SignalR;
using Microsoft.SharePoint.Client;
using SharePoint.AccessMonitorWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SharePoint.AccessMonitorWeb.Controllers
{
    public class LogController : Controller
    {
        public ActionResult Write(string user, string title, string url)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<MonitorHub>();
            hubContext.Clients.All.update(new
            {
                Timestamp = DateTime.Now,
                User = user,
                Title = title,
                Url = url,
            });
            return new HttpStatusCodeResult(200);
        }
    }
}