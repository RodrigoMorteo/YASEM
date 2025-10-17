using System;
using System.IO;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using YASEM.CLI.Interfaces;
using YASEM.Core.Interfaces;
using YASEM.Core.Connectors;
using YASEM.Core.Validators;
using YASEM.Core.Configuration;
using YASEM.Core.Utilities;
using YASEM.Core.Models;
using YASEM.Core.Exceptions;
using static YASEM.Core.Utilities.CryptoUtil;
using static System.Text.Encoding;

namespace YASEM.CLI
{
    public class Application : IApplication
    {
        private readonly ITestCaseLoader _testCaseLoader;
        private readonly IMailConnector _mailConnector;
        private readonly IValidationEngineFactory _validationEngineFactory;
        private readonly IReportGenerator _reportGenerator;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Application> _logger;
        private readonly IStringLocalizer<Application> _localizer;

        public Application(ITestCaseLoader testCaseLoader,
                           IMailConnector mailConnector,
                           IValidationEngineFactory validationEngineFactory,
                           IReportGenerator reportGenerator,
                           IConfiguration configuration,
                           ILogger<Application> logger,
                           IStringLocalizer<Application> localizer)
        {
            _testCaseLoader = testCaseLoader;
            _mailConnector = mailConnector;
            _validationEngineFactory = validationEngineFactory;
            _reportGenerator = reportGenerator;
            _configuration = configuration;
            _logger = logger;
            _localizer = localizer;
            CultureInfo.CurrentCulture =
                CultureInfo.CurrentUICulture =
                    CultureInfo.GetCultureInfo("en-US"); //TODO: Refactorize localization https://learn.microsoft.com/en-us/dotnet/core/extensions/localization
        }


        public async Task<int> RunAsync(string[] args)
        {
            //TODO: Add localization to CLI options
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
                try
                {
                    var encryptString = context.ParseResult.GetValueForOption(encryptOption);
                    var createKeyFile = context.ParseResult.GetValueForOption(createKeyFileOption);
                    var keyPath = context.ParseResult.GetValueForOption(keyPathOption);
                    var jsonPath = context.ParseResult.GetValueForOption(jsonPathOption);
                    var reportPath = context.ParseResult.GetValueForOption(reportPathOption);

                    if (encryptString != null && ((createKeyFile != null) || (keyPath != null)))
                    {
                        byte[]? key = null;
                        string? keyFilePath = null;

                        if (createKeyFile != null)
                        {
                            keyFilePath = createKeyFile.FullName;
                            key = CryptoUtil.GenerateKey();
                            await File.WriteAllBytesAsync(keyFilePath, key);
                            //_logger.LogInformation(_localizer["GeneratedEncryptionKey"], keyFilePath);
                            _logger.LogInformation("Generated encryption key file: {0}", keyFilePath);
                            keyPath = new FileInfo(keyFilePath);
                        }
                        else if (keyPath != null)
                        {
                            keyFilePath = keyPath.FullName;
                            if (!File.Exists(keyFilePath))
                            {
                                //_logger.LogError(_localizer["Error_KeyFileNotFound"], keyFilePath);
                                _logger.LogError("Key file not found: {0}", keyFilePath);
                                context.ExitCode = 1;
                                return;
                            }
                        }
                        if (keyFilePath == null)
                        {
                            //_logger.LogError(_localizer["Error_KeyPathRequired"]);
                            _logger.LogError("Key path is required.");
                            context.ExitCode = 1;
                            return;
                        }

                        //_logger.LogInformation(_localizer["UsingEncryptionKey"], keyFilePath);
                        _logger.LogInformation("Using encryption key file: {0}", keyFilePath);
                        key = await File.ReadAllBytesAsync(keyFilePath);

                        if (encryptString != null)
                        {
                            string encrypted = CryptoUtil.Encrypt(encryptString, key);
                            //_logger.LogInformation(_localizer["EncryptedString"], encrypted);
                            _logger.LogInformation("Encrypted string: {0}", encrypted);
                        }
                        context.ExitCode = 0;
                        return;
                    }

                    if (jsonPath == null || reportPath == null)
                    {
                        //_logger.LogError(_localizer["Error_JsonPathReportPathRequired"]);
                        _logger.LogError("JSON path and report path are required.");
                        context.ExitCode = 1;
                        return;
                    }

                    //_logger.LogInformation(_localizer["ReceivedRequestToProcess"], jsonPath.FullName);
                    _logger.LogInformation("Received request to process: {0}", jsonPath.FullName);

                    var testCase = await _testCaseLoader.LoadAsync(jsonPath.FullName);

                    // If the test case file doesn't specify mail options, load them from appsettings.json
                    if (testCase.MailOptions == null)
                    {
                        testCase.MailOptions = new MailOptions();
                        _configuration.GetSection("MailSettings").Bind(testCase.MailOptions);
                    }

                    if (testCase.MailOptions.Password != null && !string.IsNullOrEmpty(testCase.MailOptions.Password.EncryptedValue) && keyPath == null)
                    {
                        _logger.LogError("Key path is required for encrypted password.");
                        context.ExitCode = 1;
                        return;
                    }
                    byte[]? decryptionKey = null;
                    string? decryptedPassword = null;

                    if (testCase.MailOptions.Password != null && !string.IsNullOrEmpty(testCase.MailOptions.Password.EncryptedValue))
                    {
                        decryptionKey = await File.ReadAllBytesAsync(keyPath.FullName);
                        decryptedPassword = CryptoUtil.Decrypt(testCase.MailOptions.Password.EncryptedValue, decryptionKey);
                    }

                    _logger.LogInformation("Successfully loaded test case: {0}", testCase.Name);

                    _logger.LogInformation("Connecting to mail server: {0}", testCase.MailOptions!.Server);

                    var messages = await _mailConnector.ConnectAndRetrieveMessagesAsync(testCase, decryptedPassword ?? string.Empty);
                    //_logger.LogInformation(_localizer["EmailsFoundForValidation"], messages.Count);
                    _logger.LogInformation("Emails found for validation: {0}", messages.Count);

                    if (messages.Count == 0)
                    {
                        //_logger.LogError(_localizer["NoEmailsFound"]);
                        _logger.LogError("No emails found.");
                        context.ExitCode = 1;
                        //throw new NoEmailsFoundException();
                    }

                    var validationEngine = _validationEngineFactory.Create(testCase.TestSteps);
                    var results = validationEngine.Execute(messages);

                    _reportGenerator.Generate(results, reportPath);
                    _logger.LogInformation("Report generated at {0}", reportPath);

                    //_logger.LogInformation(_localizer["TestExecutionFinished"]);
                    _logger.LogInformation("Test execution finished.");
                    context.ExitCode = 0;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error during application execution.");
                    context.ExitCode = 1; // Indicate error
                }
            });

            return await rootCommand.InvokeAsync(args);
        }
    }
}