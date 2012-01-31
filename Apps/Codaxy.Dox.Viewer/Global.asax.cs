using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Codaxy.Common.Logging;
using Codaxy.Dextop;
using System.IO;

namespace Codaxy.Dox.Viewer
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            
            routes.IgnoreRoute("rpc.ashx");
            routes.IgnoreRoute("poll.ashx");
            routes.IgnoreRoute("lpoll.ashx");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            Logger logger = CreateLogger();
            try
            {
                var app = new Application(logger);
                app.Initialize();

                Codaxy.Dox.Viewer.Application.RegisterApplication(app);

                logger.Info("Application started.");
            }
            catch (Exception ex)
            {
                logger.Exception("Application start error!", ex);
                throw;
            }
        }

        protected void Application_End()
        {
            try
            {
                DextopApplication.GetApplication().Dispose();
            }
            catch
            {
            }
        }

        private Logger CreateLogger()
        {
            try
            {
                var logFileName = DextopUtil.MapPath("~/log.txt");
                try
                {
                    if (File.Exists(logFileName))
                        File.Delete(logFileName);
                }
                catch
                {
                }
                TextFileLogAppender log = new TextFileLogAppender(logFileName);
                return new Logger(log, "");
            }
            catch
            {
                return null;
            }
        }
    }
}