using System;
using qualityassurance.tools;
using qualityassurance.tools.JSON;


namespace YASEM {
    class YASEM {
        static void Main(string [] args){
            string path = args.Length == 0? "testcase/pop3.json" : args[0]; //TODO: Remove ternary operator and check for input parameter in args of fail.
            try{
                ConfigLoader conf = new ConfigLoader(path); //Load test case settings and steps
                Console.WriteLine($"Loading Test Case: {conf.settings.Id} {conf.settings.Name}."); 
                Console.WriteLine($"{conf.settings.TestSteps.Count} test test steps found.");
                ValidationEngine testCase = new ValidationEngine(conf.settings.TestSteps); //Create a validator for each test step
                Console.WriteLine($"Connecting to {conf.settings.MailOptions.Server} to retrieve emails.");
                EmailConnector server = new EmailConnector();
                List<ValidationResult> results = testCase.Execute(server.ConnectAndRetrieveMessages(conf.settings.MailOptions)); //connect and retrieve emails from server and execute the test steps
            }
            catch(Exception e) //catch Bubble Exceptions
            {
                Console.WriteLine($"ERROR while executing the test case: {e.Message}");
            }
            Console.WriteLine("Test execution finished!");
        }
    }
}