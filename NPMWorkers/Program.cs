using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NPMWorkers.Data;
using NPMWorkers.Repositories;
using NPMWorkers.Services;

namespace NPMWorkers
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
                    IConfiguration configuration = hostContext.Configuration;
                    // Entity framework
                    services.AddDbContext<ERADBContext>(options => options.UseSqlServer("name=ConnectionStrings:era"), ServiceLifetime.Singleton);
                    services.AddDbContext<NPMDBContext>(options => options.UseSqlServer("name=ConnectionStrings:npm"), ServiceLifetime.Singleton);

                    // Service workers
                    services.AddHostedService<AutoPostingWorker>();
                    services.AddSingleton<IPracticeRepository, PracticeRepository>();
                    services.AddSingleton<IERAImportRepository, ERAImportRepository>();
                }).UseWindowsService();
    }
}
