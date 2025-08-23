using System;
using System.IO;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Invocation;
using YASEM.CLI.Interfaces;
using YASEM.Core.Interfaces;
using YASEM.Core.Connectors;
using YASEM.Core.Validators;
using YASEM.Core.Configuration;

namespace YASEM.CLI
{
    public class Application : IApplication
    {
        private readonly ITestCaseLoader _testCaseLoader;
        private readonly IMailConnector _mailConnector;
        private readonly IValidationEngineFactory _validationEngineFactory;

        // Dependencies will be injected here
        public Application(ITestCaseLoader testCaseLoader, 
                           IMailConnector mailConnector, 
                           IValidationEngineFactory validationEngineFactory)
        {
            _testCaseLoader = testCaseLoader;
            _mailConnector = mailConnector;
            _validationEngineFactory = validationEngineFactory;
        }

        public async Task<int> RunAsync(string[] args)
        {
            // 1. Define CLI options and commands using System.CommandLine
            var jsonPathOption = new Option<FileInfo>("--json-path", "Path to the test case JSON file.") { IsRequired = true };
            var reportPathOption = new Option<string>("--report-path", "Path for the output HTML report.") { IsRequired = true };
            var keyPathOption = new Option<FileInfo>("--key-path", "Path to the encryption key file.");
            // TODO: Implement all other options from description.md

            var rootCommand = new RootCommand("YASEM: Yet Another Simple Email Multiplatform Test Automation Tool");
            rootCommand.AddOption(jsonPathOption);
            rootCommand.AddOption(reportPathOption);
            rootCommand.AddOption(keyPathOption);

            // 2. Set the handler for the root command
            rootCommand.SetHandler(async (context) =>
            {
                var jsonPath = context.ParseResult.GetValueForOption(jsonPathOption);
                Console.WriteLine($"Received request to process: {jsonPath.FullName}");

                // 3. Orchestrate Core Logic
                // This replaces the old logic from the Main method
                var testCase = await _testCaseLoader.LoadAsync(jsonPath.FullName);
                Console.WriteLine($"Successfully loaded test case: {testCase.Id} - {testCase.Name}");

                Console.WriteLine($"Connecting to {testCase.MailOptions.Server} to retrieve emails...");
                var messages = await _mailConnector.ConnectAndRetrieveMessagesAsync(testCase.MailOptions);
                Console.WriteLine($"{messages.Count} email(s) found for validation.");

                var validationEngine = _validationEngineFactory.Create(testCase.TestSteps);
                var results = validationEngine.Execute(messages);

                // TODO: Process results and generate report
                Console.WriteLine("Test execution finished!");
                // TODO: Return a proper exit code based on results
            });

            // 4. Invoke the command line parser
            return await rootCommand.InvokeAsync(args);
        }
    }
}