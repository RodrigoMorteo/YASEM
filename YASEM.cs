using System;
using qualityassurance.tools;
using qualityassurance.tools.JSON;


namespace YASEM {
    class YASEM {
        static void Main(string [] args){
            ValidationEngine testCase = new ValidationEngine();
            string path = args.Length == 0? "testcase/pop3.json" : args[0]; //TODO: Remove ternary operator and check for input parameter in args of fail.
            ConfigLoader conf = new ConfigLoader(path);

            Console.WriteLine($"Loading Test Case: {conf.settings.Id} {conf.settings.Name}.");
            Console.WriteLine($"{conf.settings.TestSteps.Count} test test steps found.");
            foreach (var step in conf.settings.TestSteps)
            {
                testCase.AddStep(step);
            }

            EmailConnector server = new EmailConnector();
            server.ConnectAndRetrieveMessages(conf.settings.MailOptions);


            Console.WriteLine("Email retrieval finished.");
        }
    }
}