using System;
using System.IO;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Configuration;
using YASEM.CLI.Interfaces;
using YASEM.Core.Interfaces;
using YASEM.Core.Connectors;
using YASEM.Core.Validators;
using YASEM.Core.Configuration;
using YASEM.Core.Utilities;
using YASEM.Core.Models;
using YASEM.Core.Exceptions;
using static YASEM.Core.Utilities.CryptoUtil;

namespace YASEM.CLI
{
    public class Application : IApplication
    {
        private readonly ITestCaseLoader _testCaseLoader;
        private readonly IMailConnector _mailConnector;
        private readonly IValidationEngineFactory _validationEngineFactory;
        private readonly IConfiguration _configuration;

        public Application(ITestCaseLoader testCaseLoader, 
                           IMailConnector mailConnector, 
                           IValidationEngineFactory validationEngineFactory,
                           IConfiguration configuration)
        {
            _testCaseLoader = testCaseLoader;
            _mailConnector = mailConnector;
            _validationEngineFactory = validationEngineFactory;
            _configuration = configuration;
        }
        

        public async Task<int> RunAsync(string[] args)
        {
            var jsonPathOption = new Option<FileInfo>("--json-path", "Path to the test case JSON file.");
            var reportPathOption = new Option<string>("--report-path", "Path for the output HTML report.");
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

            rootCommand.SetHandler(async (context) =>
            {
                var encryptString = context.ParseResult.GetValueForOption(encryptOption);
                var createKeyFile = context.ParseResult.GetValueForOption(createKeyFileOption);
                var keyPath = context.ParseResult.GetValueForOption(keyPathOption);
                var jsonPath = context.ParseResult.GetValueForOption(jsonPathOption);
                var reportPath = context.ParseResult.GetValueForOption(reportPathOption);

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

                    if (encryptString != null)
                    {
                        if (key == null)
                        {
                            Console.Error.WriteLine("Error: --encrypt requires either --key-path or --create-key-file.");
                            context.ExitCode = 1;
                            return;
                        }
                        string encrypted = CryptoUtil.Encrypt(encryptString, key);
                        Console.WriteLine($"Encrypted string: {encrypted}");
                    }
                    context.ExitCode = 0;
                    return;
                }

                if (jsonPath == null || reportPath == null)
                {
                    Console.Error.WriteLine("Error: --json-path and --report-path are required for a test run.");
                    context.ExitCode = 1;
                    return;
                }

                Console.WriteLine($"Received request to process: {jsonPath.FullName}");

                var testCase = await _testCaseLoader.LoadAsync(jsonPath.FullName);

                var mailOptions = new MailOptions();
                _configuration.GetSection("MailSettings").Bind(mailOptions);

                var config = new Config
                {
                    Name = testCase.Name,
                    Filters = testCase.Filters,
                    TestSteps = testCase.TestSteps,
                    MailOptions = mailOptions
                };

                if (keyPath == null)
                {
                    Console.Error.WriteLine("Error: --key-path is required for a test run with an encrypted password.");
                    context.ExitCode = 1;
                    return;
                }
                var decryptionKey = await File.ReadAllBytesAsync(keyPath.FullName);
                var decryptedPassword = CryptoUtil.Decrypt(mailOptions.Password.EncryptedValue, decryptionKey);

                Console.WriteLine($"Successfully loaded test case: {config.Name}");

                Console.WriteLine($"Connecting to {config.MailOptions.Server} to retrieve emails...");
                var messages = await _mailConnector.ConnectAndRetrieveMessagesAsync(config, decryptedPassword);
                Console.WriteLine($"{messages.Count} email(s) found for validation.");

                if (messages.Count == 0)
                {
                    throw new NoEmailsFoundException();
                }

                var validationEngine = _validationEngineFactory.Create(config.TestSteps);
                var results = validationEngine.Execute(messages);

                // TODO: Process results and generate report
                Console.WriteLine("Test execution finished!");
                // TODO: Return a proper exit code based on results
            });

            return await rootCommand.InvokeAsync(args);
        }
    }
}