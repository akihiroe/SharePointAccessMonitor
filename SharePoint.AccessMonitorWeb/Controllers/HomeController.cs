using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Web;
using System.Web.Mvc;

namespace SharePoint.AccessMonitorWeb.Controllers
{
    public class HomeController : Controller
    {
        [SharePointContextFilter]

        public ActionResult Index()
        {
            return View();
        }

        private static ClientContext CreateContext(string siteUrl, string user, string password)
        {
            var securePassword = new SecureString();
            foreach (char c in password)
            {
                securePassword.AppendChar(c);
            }

            var onlineCredentials = new SharePointOnlineCredentials(user, securePassword);
            var context = new ClientContext(siteUrl);
            context.Credentials = onlineCredentials;
            return context;

        }


        private string GetUserScript(ClientContext ctx)
        {
            var userCustomActions = ctx.Site.UserCustomActions;

            ctx.Load(userCustomActions);
            ctx.ExecuteQuery();
            var action = userCustomActions.Where(x => x.Title == userScriptName).FirstOrDefault();
            if (action == null) return null;
            return action.ScriptBlock;
        }

        private void SetUserScript(ClientContext ctx, string script)
        {
            var userCustomActions = ctx.Site.UserCustomActions;

            ctx.Load(userCustomActions);
            ctx.ExecuteQuery();
            var action = userCustomActions.Where(x => x.Title == userScriptName).FirstOrDefault();
            if (action == null)
            {
                action = userCustomActions.Add();
                action.Location = "ScriptLink";
                action.Title = userScriptName; ;
            }
            action.ScriptBlock = script??"";
            action.Sequence = 1000;
            action.Update();
            ctx.ExecuteQuery();

            //空の場合削除する
            if (string.IsNullOrWhiteSpace(script))
            {

                action.DeleteObject();
                ctx.Load(action);
                ctx.ExecuteQuery();
                return;
            }
        }


        private const string userScriptName = "AccessMonitor";


        public ActionResult Manage()
        {
            return View();
        }


        [HttpPost]
        [SharePointContextFilter]
        public ActionResult Manage(string user, string password, string spHostUrl, string command)
        {
            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
            {
                var actionUrl = Url.Action("Write", "Log", null, this.Request.Url.Scheme, null);
                var script = @"
(function () {
    this.send = function () {
        if (!SP.ClientContext) {
            setTimeout(this.send, 100);
            return;
        }

        var ctx = new SP.ClientContext.get_current();
        var website = ctx.get_web();
        var currentUser = website.get_currentUser();
        ctx.load(currentUser);
        ctx.executeQueryAsync(function () {
            var uid = currentUser.get_email();
            var r = new XMLHttpRequest(); 
            r.open('POST', '" + actionUrl +@"', true);
            r.setRequestHeader( 'Content-Type', 'application/x-www-form-urlencoded' );
            r.send('title=' + encodeURIComponent(document.title) + '&url='+ encodeURIComponent( location.pathname + location.search + location.hash) + '&user=' + uid );
        },
        function () {
           var r = new XMLHttpRequest(); 
            r.open('POST', '" + actionUrl +@"', true);
            r.setRequestHeader( 'Content-Type', 'application/x-www-form-urlencoded' );
            r.send('title=' + encodeURIComponent(document.title) + '&url='+ encodeURIComponent( location.pathname + location.search + location.hash) + '&user=');
         });
    }
    setTimeout(this.send, 0);
})();
";
                using (var ctx = CreateContext(spHostUrl, user, password))
                {
                    if (command == "モニター終了")
                    {
                        SetUserScript(ctx, "");
                    }
                    else
                    {
                        SetUserScript(ctx, script);
                    }
                }
            }
            return View();
        }
    }
}
