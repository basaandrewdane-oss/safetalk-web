using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

[assembly: OwinStartup(typeof(SafeTalkApp.Startup))]
namespace SafeTalkApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Enable cookie authentication
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "ApplicationCookie",
                LoginPath = new PathString("/Account/Login"), // redirect here if not authenticated
                ExpireTimeSpan = TimeSpan.FromMinutes(30),
                SlidingExpiration = true,
            });
            app.MapSignalR(); // Map SignalR hubs   
        }
    }
}