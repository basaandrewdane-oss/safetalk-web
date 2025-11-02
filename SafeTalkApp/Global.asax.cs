using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace SafeTalkApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                   path: Server.MapPath("~/App_Data/Logs/log-.txt"), // logs will go to /App_Data/Logs/
                   rollingInterval: RollingInterval.Day,
                   retainedFileCountLimit: 10, // keep last 10 days
                   shared: true)
                .CreateLogger();
            Log.Information("✅ SafeTalk Application started.");
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error()
        {
            var ex = Server.GetLastError();
            Log.Error(ex, "Unhandled exception occurred.");
        }
    }
}
