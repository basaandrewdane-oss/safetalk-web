using System.Web;
using System.Web.Optimization;

namespace SafeTalkApp
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/bundles/directives").Include(
                    "~/Scripts/Holy-Scripts/Directives/materialSelect.js",
                    "~/Scripts/Holy-Scripts/Directives/passwordValidator.js",
                    "~/Scripts/Holy-Scripts/Directives/validateAge.js",
                    "~/Scripts/Holy-Scripts/Directives/materialTimepicker24.js",
                    "~/Scripts/Holy-Scripts/Directives/fileModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/safetalkapp").Include(
                "~/Scripts/Holy-Scripts/Module.js",
                "~/Scripts/Holy-Scripts/ApiHelper.js",
                "~/Scripts/Holy-Scripts/controllers/accountController.js",
                "~/Scripts/Holy-Scripts/controllers/adminController.js",
                "~/Scripts/Holy-Scripts/controllers/appointmentsController.js",
                "~/Scripts/Holy-Scripts/controllers/chatBotController.js",
                "~/Scripts/Holy-Scripts/controllers/consultationController.js",
                "~/Scripts/Holy-Scripts/controllers/dashboardController.js",
                "~/Scripts/Holy-Scripts/controllers/homeController.js",
                "~/Scripts/Holy-Scripts/controllers/paymentController.js",
                "~/Scripts/Holy-Scripts/controllers/reportsController.js",
                "~/Scripts/Holy-Scripts/controllers/resourceController.js",
                "~/Scripts/Holy-Scripts/services/accountService.js",
                "~/Scripts/Holy-Scripts/services/adminService.js",
                "~/Scripts/Holy-Scripts/services/appointmentService.js",
                "~/Scripts/Holy-Scripts/services/chatBotService.js",
                "~/Scripts/Holy-Scripts/services/consultationService.js",
                "~/Scripts/Holy-Scripts/services/dashboardService.js",
                "~/Scripts/Holy-Scripts/services/homeService.js",
                "~/Scripts/Holy-Scripts/services/paymentService.js",
                "~/Scripts/Holy-Scripts/services/reportsService.js",
                "~/Scripts/Holy-Scripts/services/resourceService.js"));
        }
    }
}
