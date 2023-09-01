using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using log4net;
using log4net.Config;
using log4net.Layout;
using log4net.Appender;
using log4net.Filter;
using log4net.Repository;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json;

using qualityassurance.tools;

namespace qualityassurance.tools.JSON
{
    public class ConfigLoader
    {
        public Config settings;
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ConfigLoader(string configFile)
        {
            configFile = FileUtils.CheckOrSetFullyQuallifiedFilePath(configFile);
            if (File.Exists(configFile))
            {
                string jsonContent = File.ReadAllText(configFile);
                // Deserialize JSON content to C# object
                try
                {
                    settings = JsonConvert.DeserializeObject<Config>(jsonContent);
                }
                catch (JsonException ex)
                {
                    throw new Exception ("Error deserializing JSON: " + ex.Message, ex);
                }
            }
            else
            {
                throw new Exception("JSON configuration file not found in " + configFile);
            }
            Setup();
        }
        private void Setup ()
        {
            string logFileName = String.Format(@"YASEM-{0}.log",DateTime.Now.ToString(settings.LogOptions.DateTimeFormat));
            string logFilePath = FileUtils.CheckOrSetFullyQuallifiedFilePath($"{settings.LogOptions.DirectoryPath}{Path.DirectorySeparatorChar}{logFileName}"); //set fully qualified path for the log file
            string logPath = FileUtils.CheckOrCreatePath(logFilePath); //recursively create directories if the path does not exsist
            if(!settings.LogOptions.Disable)
            {
                ConfigureAppenders(logFilePath);
            }
            Logger.Debug(logPath);
            Logger.Info(logFilePath);
            Logger.Warn("This is a warning message.");
            Logger.Error("This is an error message.");
            Logger.Fatal("This is a fatal message.");
        }

        private Level getLogLevel()
        {
            string logLevel = StringUtils.FirstCharToUpperString(settings.LogOptions.LogLevel.ToLower()); //get log level from settings
            switch (logLevel)
            {
                case "Debug":
                    return Level.Debug;
                case "Info":
                    return Level.Info;
                case "Warn":
                    return Level.Warn;
                case "Error":
                    return  Level.Error;
                case "Fatal":
                    return  Level.Fatal;
                default:
                    return Level.Error;
            }
        }

        public void ConfigureAppenders(string filePath)
        {
            IAppender consoleAppender;
            IAppender fileAppender;
            if(settings.LogOptions.Appenders.Contains("console"))
            {
                consoleAppender = GetConsoleAppender();
                BasicConfigurator.Configure(consoleAppender);
            }
            if(settings.LogOptions.Appenders.Contains("file"))
            {
                fileAppender = GetFileAppender(filePath);
                BasicConfigurator.Configure(fileAppender);
            }
            //BasicConfigurator.Configure(consoleAppender, fileAppender);
            ((Hierarchy)LogManager.GetRepository()).Root.Level = Level.All; //Set logger root level (appender filters will apply)
        }

        private IAppender GetConsoleAppender()
        {
            var appender = new ConsoleAppender
            {
                Layout = new PatternLayout("%date [%thread] %-5level %logger - %message%newline")
            };
            return appender;
        }
        private IAppender GetFileAppender(string logFileName)
        {
            var layout = new PatternLayout("%date:%-5level:[%thread]:[%logger::%method]:%line:%message%newline");
            layout.ActivateOptions();
            var currentLogLevel = getLogLevel();
            var appender = new FileAppender
            {
                File = logFileName,
                Encoding = Encoding.UTF8,
                Threshold = currentLogLevel,
                Layout = layout
            };

            LevelRangeFilter filter = new LevelRangeFilter();
            filter.LevelMax = Level.Error;
            filter.LevelMin = currentLogLevel;
            
            appender.AddFilter(filter);
            appender.ActivateOptions();

            return appender;
        }

        /*private IAppender GetRollingFileAppender(string logFileName) //TODO: Add RollingFileAppender
        {
            var appender = new RollingFileAppender
        }
        */
    }

    //TODO: check and set defaults
    public class MailOptions
    {
        public string Protocol { get; set; } = "imap"; //set default to imap
        /// </summary>
        public string Server { get; set; } //no default
        public int Port { get; set; } = 143; //set default to not secured imap server port
        public string Email { get; set; } //no default
        public string Password { get; set; } //no default
        public string Folder { get; set; } = "INBOX"; //set default folder to INBOX (for imap)
        public bool IgnoreCertificateErrors {get; set; } = true; //ignore errors by default
        public bool EnableDebugLog {get; set; } = false; //disable mailkit debug log by default

    }

    public class LogOptions
    {
        public string DirectoryPath { get; set; } //TODO: Set current path (.?)
        public string LogLevel { get; set; } = "Error"; //Default to Error log level
        public string DateTimeFormat { get; set; } = "yyyymmddd"; //Default format
        public bool Disable {get; set;} = false; //Default to false
        public List<string> Appenders { get; set; } = new List<string>() {"file"};  //Default to file 
    }

    public class ReportingOptions
    {
        public string DirectoryPath { get; set; } //TODO: Set current path (.?)
        public string NameSuffix { get; set; } = ""; //Default to empty string
    }

    public class TestStep
    {
        public string Description { get; set; } //no default
        public string ValidationType { get; set; } //no default
        public string Assertion { get; set; } //no default
        public string ExpectedValue { get; set; } //no default
    }

    public class Config
    {
        public string Id {get;  set;} //no default
        public string Name { get; set; } //no default
        public string Author { get; set; } //TODO: Get System Username
        public string Environment { get; set; } ="QA"; //default to QA
        public MailOptions MailOptions { get; set; } //see defaults in MailOptions class
        public LogOptions LogOptions { get; set; } //see defaults in LogOptions class
        public ReportingOptions ReportingOptions { get; set; } //see defaults in reporting Options class
        public List<TestStep> TestSteps { get; set; } //see defaults in TestSteps class
    }

}
