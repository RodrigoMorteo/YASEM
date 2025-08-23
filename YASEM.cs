using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YASEM.CLI;
using YASEM.Core.Configuration;
using YASEM.Core.Interfaces;

namespace YASEM
{
    // This is the Composition Root of the YASEM.CLI application.
    // Its responsibilities are:
    // 1. Configure and build the Dependency Injection (DI) container.
    // 2. Resolve the main application service.
    // 3. Run the application.
    public class YASEM
    {
        public static async Task<int> Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            var app = host.Services.GetRequiredService<IApplication>();
            return await app.RunAsync(args);
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Register application services with the DI container
                    services.AddSingleton<IApplication, Application>();
                    services.AddTransient<ITestCaseLoader, TestCaseLoader>();
                    services.AddTransient<IMailConnector, EmailConnector>();
                    services.AddSingleton<IValidationEngineFactory, ValidationEngineFactory>();
                    // Other services will be registered here.
                });
    }
}