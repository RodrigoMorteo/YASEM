using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using YASEM.CLI;
using YASEM.Core.Configuration;
using YASEM.Core.Interfaces;
using YASEM.Core.Connectors;
using YASEM.Core.Validators;
using YASEM.CLI.Interfaces;
using YASEM.Core.Models;
using YASEM.Core.Exceptions;
using MailKit.Net.Imap; // Added
using MailKit.Net.Pop3; // Added
using Serilog;
using YASEM.Core.Reporting;

namespace YASEM.CLI
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
            try
            {
                var host = CreateHostBuilder(args).Build();
                var app = host.Services.GetRequiredService<IApplication>();
                return await app.RunAsync(args);
            }
            catch (Exception ex)
            {
                // Use the static logger here only if the host fails to build.
                Log.Fatal(ex, "Application terminated unexpectedly");
                return 1;
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                })
                .UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
                    .MinimumLevel.Debug()
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File("logs/yasem-main-log.txt", rollingInterval: RollingInterval.Day))
                .ConfigureServices((context, services) =>
                {
                    // Register application services with the DI container
                    services.AddSingleton<IApplication, Application>();
                    services.AddTransient<ITestCaseLoader, TestCaseLoader>();
                    services.AddTransient<IMailConnector, EmailConnector>();
                    services.AddTransient<IImapClient, ImapClient>(); // Added
                    services.AddTransient<IPop3Client, Pop3Client>(); // Added
                    services.AddTransient<IValidationEngineFactory, ValidationEngineFactory>();
                    services.AddTransient<IReportGenerator, ReportGenerator>();
                    services.Configure<Config>(context.Configuration.GetSection("AppConfig"));
                    services.AddLocalization();
                    // Other services will be registered here.
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                    // TODO: Add file logging for comprehensive logging as per plan.md
                });
    }
}