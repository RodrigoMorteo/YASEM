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
using YASEM.Core.Utilities;
using static YASEM.Core.Utilities.CryptoUtil;

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
            var timeoutOption = new Option<int>("--timeout", "Number of seconds to wait for test emails to arrive.");
            var encryptOption = new Option<string>("--encrypt", "Utility function. Encrypts the given string.");
            var createKeyFileOption = new Option<FileInfo>("--create-key-file", "Utility function. Generates a new random encryption key file.");

            var rootCommand = new RootCommand("YASEM: Yet Another Simple Email Multiplatform Test Automation Tool");
            rootCommand.AddOption(jsonPathOption);
            rootCommand.AddOption(reportPathOption);
            rootCommand.AddOption(keyPathOption);
            rootCommand.AddOption(timeoutOption);
            rootCommand.AddOption(encryptOption);
            rootCommand.AddOption(createKeyFileOption);

            // 2. Set the handler for the root command
            rootCommand.SetHandler(async (context) =>
            {
                var encryptString = context.ParseResult.GetValueForOption(encryptOption);
                var createKeyFile = context.ParseResult.GetValueForOption(createKeyFileOption);
                var keyPath = context.ParseResult.GetValueForOption(keyPathOption);

                // Handle utility functions first
                if (encryptString != null || createKeyFile != null)
                {
                    byte[] key = null;
                    string keyFilePath = null;

                    if (createKeyFile != null)
                    {
                        keyFilePath = createKeyFile.FullName;
                        key = CryptoUtil.GenerateKey();
                        await File.WriteAllBytesAsync(keyFilePath, key);
                        Console.WriteLine($"Generated new encryption key at: {keyFilePath}");
                    }
                    else if (keyPath != null)
                    {
                        keyFilePath = keyPath.FullName;
                        if (!File.Exists(keyFilePath))
                        {
                            Console.Error.WriteLine($"Error: Key file not found at {keyFilePath}");
                            context.ExitCode = 1;
                            return;
                        }
                        key = await File.ReadAllBytesAsync(keyFilePath);
                    }
                    else
                    {
                        Console.Error.WriteLine("Error: --encrypt requires either --key-path or --create-key-file.");
                        context.ExitCode = 1;
                        return;
                    }

                    if (encryptString != null)
                    {
                        string encrypted = CryptoUtil.Encrypt(encryptString, key);
                        Console.WriteLine($"Encrypted string: {encrypted}");
                    }
                    context.ExitCode = 0;
                    return;
                }

                // Main application logic
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