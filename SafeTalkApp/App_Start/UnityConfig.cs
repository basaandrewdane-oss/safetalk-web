using SafeTalkApp.Models;
using SafeTalkApp.Services;
using System;
using System.Configuration;
using System.Net.Http;
using Unity;
using Unity.AspNet.Mvc;
using Unity.Injection;

namespace SafeTalkApp
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public static class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container =
          new Lazy<IUnityContainer>(() =>
          {
              var container = new UnityContainer();
              RegisterTypes(container);
              return container;
          });

        /// <summary>
        /// Configured Unity Container.
        /// </summary>
        public static IUnityContainer Container => container.Value;
        #endregion

        /// <summary>
        /// Registers the type mappings with the Unity container.
        /// </summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>
        /// There is no need to register concrete types such as controllers or
        /// API controllers (unless you want to change the defaults), as Unity
        /// allows resolving a concrete type even if it was not previously
        /// registered.
        /// </remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            // NOTE: To load from web.config uncomment the line below.
            // Make sure to add a Unity.Configuration to the using statements.
            // container.LoadConfiguration();

            // TODO: Register your type's mappings here.
            // container.RegisterType<IProductRepository, ProductRepository>();
            container.RegisterType<ISafeTalkAppContext, SafeTalkAppContext>(new PerRequestLifetimeManager());
            container.RegisterType<IAccountService, AccountService>();
            container.RegisterType<IEmailService, EmailService>();
            container.RegisterType<IHomeService, HomeService>();
            container.RegisterType<IAdminService, AdminService>();
            container.RegisterType<IAppointmentService, AppointmentService>();
            container.RegisterType<IChatBotService, ChatBotService>();
            container.RegisterType<IConsultationService, ConsultationService>();
            container.RegisterType<IPaymentService, PaymentService>();
            container.RegisterType<IPayPalService, PayPalService>();
            container.RegisterType<ITranscriptionService, TranscriptionService>();
            container.RegisterType<IReportsService, ReportsService>();
            container.RegisterType<IDashboardService, DashboardService>();
            container.RegisterType<IResourceService, ResourceService>();
            container.RegisterType<IProfileService, ProfileService>();
            container.RegisterType<IAvailabilityService, AvailabilityService>();

            var httpClient = new HttpClient();
            container.RegisterInstance<HttpClient>(httpClient);

            
            string apiKey = ConfigurationManager.AppSettings["CohereApiKey"];
            container.RegisterInstance<string>("CohereApiKey", apiKey);

            string assemblyApiKey = ConfigurationManager.AppSettings["AssemblyAIKey"];
            container.RegisterInstance<string>("AssemblyAIKey", assemblyApiKey);
        }
    }
}