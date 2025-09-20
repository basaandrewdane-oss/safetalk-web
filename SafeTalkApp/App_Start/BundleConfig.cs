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
                "~/Scripts/Holy-Scripts/Controllers/AccountController.js",
                "~/Scripts/Holy-Scripts/Controllers/AdminController.js",
                "~/Scripts/Holy-Scripts/Controllers/AppointmentsController.js",
                "~/Scripts/Holy-Scripts/Controllers/ChatBotController.js",
                "~/Scripts/Holy-Scripts/Controllers/ConsultationController.js",
                "~/Scripts/Holy-Scripts/Controllers/HomeController.js",
                "~/Scripts/Holy-Scripts/Controllers/PaymentController.js",
                "~/Scripts/Holy-Scripts/Controllers/ResourceController.js",
                "~/Scripts/Holy-Scripts/Services/AccountService.js",
                "~/Scripts/Holy-Scripts/Services/AdminService.js",
                "~/Scripts/Holy-Scripts/Services/AppointmentService.js",
                "~/Scripts/Holy-Scripts/Services/ChatBotService.js",
                "~/Scripts/Holy-Scripts/Services/ConsultationService.js",
                "~/Scripts/Holy-Scripts/Services/HomeService.js",
                "~/Scripts/Holy-Scripts/Services/PaymentService.js",
                "~/Scripts/Holy-Scripts/Services/ResourceService.js"));
        }
    }
}
