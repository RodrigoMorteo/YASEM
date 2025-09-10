using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
            catch (Exception ex) when (ex is MailConnectionException || ex is InvalidConfigurationException || ex is ValidationException || ex is DecryptionException || ex is NoEmailsFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(ex.Message);
                Console.ResetColor();
                return 1;
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // Register application services with the DI container
                    services.AddSingleton<IApplication, Application>();
                    services.AddTransient<ITestCaseLoader, TestCaseLoader>();
                    services.AddTransient<IMailConnector, EmailConnector>();
                    services.AddTransient<IImapClient, ImapClient>(); // Added
                    services.AddTransient<IPop3Client, Pop3Client>(); // Added
                    services.AddSingleton<IValidationEngineFactory, ValidationEngineFactory>();
                    services.Configure<Config>(context.Configuration.GetSection("AppConfig"));
                    // Other services will be registered here.
                });
    }
}