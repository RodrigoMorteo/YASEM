using System.Collections.Generic;

namespace YASEM.Core.Models
{
    //TODO: check and set defaults
    public class PasswordSetting
    {
        public string EncryptedValue { get; set; }
    }

    public class MailOptions
    {
        public string MailServerType { get; set; } = "imap"; //set default to imap
        public string Server { get; set; } //no default
        public int Port { get; set; } = 143; //set default to not secured imap server port
        public bool UseSsl { get; set; }
        public string Email { get; set; } //no default
        public PasswordSetting Password { get; set; } //no default
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
        public string? Field { get; set; } // For field/header validations
        public string ValidationType { get; set; } //no default
        public string Assertion { get; set; } //no default
        public object ExpectedValue { get; set; } //no default
    }

    public class Filter
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
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
        public List<Filter> Filters { get; set; } = new List<Filter>();
    }
}