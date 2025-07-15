using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Cors;
using NPMAPI.App_Start;
using NPMAPI.Filters;
using NPMAPI.Helpers;
using NPMAPI.Repositories;
using NPMAPI.Services;
using Unity;
using Unity.Injection;

namespace NPMAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            //Enable CORS
            //string origin = "http://192.168.10.33,http://localhost:4300,http://localhost:4200,http://nobilitypm.infotechparadigm.com";
            string origin = "*";
            EnableCorsAttribute cors = new EnableCorsAttribute(origin, "*", "*");
            config.EnableCors(cors);
            //Dependency Injection
            var container = new UnityContainer();
            // Inject dependency here
            container.RegisterType<IRepository, Repository>();
            container.RegisterType<IUserManagementSetup, UserManagementService>();
            container.RegisterType<ICompanyRepository, CompanyService>();
            container.RegisterType<IDashboardRepository, DashboardService>();
            container.RegisterType<IAlertRepository, AlertService>();
            container.RegisterType<IDemographicRepository, DemographicService>();
            container.RegisterType<IInsuranceSetupRepository, InsuranceSetupService>();
            container.RegisterType<IPracticeRepository, PracticeService>();
            container.RegisterType<IReportRepository, ReportService>();
            container.RegisterType<ISetupRepository, SetupService>();
            container.RegisterType<ISubmissionRepository, SubmissionService>();
            container.RegisterType<IEncryption, EncryptionService>();
            container.RegisterType<ISchedulerRepository, SchedulerService>();
            container.RegisterType<IFileHandler, FileHandlerService>();
            container.RegisterType<IEligibility, EligibilityService>();
            container.RegisterType<IERAImport, ERAImportService>();
            container.RegisterType<IFTP, FTPService>();
            container.RegisterType<IPDFRepository, PDFService>();
            container.RegisterType<IPatientAttachment, PatientAttachmentService>();
            container.RegisterType<IPatientBilling, InboxHealthService>();
            container.RegisterType<IVendorRepository, VendorService>();
            container.RegisterType<IPaymentsRepository, PaymentsService>();
            container.RegisterType<IDeltaSyncRepository, DeltaSyncService>();
            container.RegisterType<IScrubberRepository, ScrubberService>();
            container.RegisterType<IReferralPhysicianRepository, ReferralPhysiciansService>();
            container.RegisterType<IUBClaim, UBClaim>();
            container.RegisterType<IClaimStatusService, ClaimStatusService>();
            container.RegisterType<IClaimService, ClaimStatus>();
            container.RegisterType<IGenerateBatch276Service, GenerateBatch276Service>();
            container.RegisterType<IClaimBatch276, ClaimBatch276>();
            container.RegisterType<ITicketTrackerRrepo, TTService>();
            container.RegisterType<ITicketAttachment , TicketAttachmentsService>();
            //container.RegisterType<IDemographicRepository, DemographicService>(new InjectionConstructor(new ResolvedParameter<IScrubberRepository>()));
            //IDemographicRepository serviceB = container.Resolve<IDemographicRepository>();


            config.DependencyResolver = new UnityResolver(container);
            // Web API routes
            config.MapHttpAttributeRoutes();
            // JWT Message Handler
            config.MessageHandlers.Add(new TokenValidationHandler());
            config.MessageHandlers.Add(new EncryptionHandler());
           // config.MessageHandlers.Add(new DynamicConnections());
            config.Filters.Add(new NLogExceptionLogger());
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("multipart/form-data"));
        }
    }
}
