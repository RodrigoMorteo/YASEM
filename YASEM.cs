using System;


using qualityassurance.tools;
using qualityassurance.tools.JSON;

namespace YASEM {
    class YASEM {
        static void Main(string [] args){
            string path = "testcase/json.json";
            ConfigLoader conf = new ConfigLoader(path);
            Console.WriteLine($"Loading Test Case: {conf.settings.Name}.");
            Console.WriteLine($"{conf.settings.TestSteps.Count} test test steps found.");

            EmailConnector server = new EmailConnector();
            server.StartSession(conf.settings.MailOptions);

            Console.WriteLine("Email retrieval finished.");
        }
    }
}