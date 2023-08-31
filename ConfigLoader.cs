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

using log4net;

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
            string logPath = FileUtils.CheckOrCreateFile(logFilePath); //recursively create directories if the path does not exsist
            ConfigureAppenders(logFilePath);
            Logger.Debug(logPath);
            Logger.Info(logFilePath);
            Logger.Warn("This is a warning message.");
            Logger.Error("This is an error message.");
            Logger.Fatal("This is a fatal message.");
        }

        private Level getLogLevel()
        {
            /*string logLevel = ; //TODO: Get log level from settings
            switch(logLevel)
            {
                case "0":
                    return Level.Debug;
                case "1":
                    return Level.Info;
                case "2":
                    return Level.Warn;
                default:
                    return Level.Error;
            }*/
            return Level.Info;
        }

        public void ConfigureAppenders(string filePath)
        {
            //TODO: Add appenders according to the settings
            var fileAppender = GetFileAppender(filePath);
            var consoleAppender = GetConsoleAppender();
            BasicConfigurator.Configure(consoleAppender, fileAppender);
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
            var currentLogLevel = getLogLevel(); //TODO: Set log level from config file
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

    public class MailOptions
    {
        public string Protocol { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Folder { get; set; }
    }

    public class LogOptions
    {
        public string DirectoryPath { get; set; }
        public string LogLevel { get; set; }
        public string DateTimeFormat { get; set; } = "yyyymmddd"; //Default format 
    }

    public class ReportingOptions
    {
        public string DirectoryPath { get; set; }
        public string NameSuffix { get; set; }
    }

    public class TestStep
    {
        public string Description { get; set; }
        public string ValidationType { get; set; }
        public string Assertion { get; set; }
        public string ExpectedValue { get; set; }
    }

    public class Config
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Environment { get; set; }
        public MailOptions MailOptions { get; set; }
        public LogOptions LogOptions { get; set; }
        public ReportingOptions ReportingOptions { get; set; }
        public List<TestStep> TestSteps { get; set; }
    }


}
