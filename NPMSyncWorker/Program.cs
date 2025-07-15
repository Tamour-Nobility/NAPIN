using NPMSyncWorker.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NPMSyncWorker.Repositories.Interfaces;

namespace NPMSyncWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddTransient<IUnitofWork, DapperUnitofWork>();
                    services.AddTransient<IExternalSynchronization, InboxHealthSyncService>();
                }).UseWindowsService();
    }
}
