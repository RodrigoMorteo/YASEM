using System;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using OpenPop.Mime;
using OpenPop.Pop3;

using qualityassurance.tools;
using qualityassurance.tools.JSON;

namespace YASEM {
    class YASEM {
        static void Main(string [] args){
            string path = "testcase/json.json";
            ConfigLoader conf = new ConfigLoader(path);
            Console.WriteLine($"Loading Test Case: {conf.settings.Name}.");
            Console.WriteLine($"{conf.settings.TestSteps.Count} test test steps found.");

            switch(conf.settings.MailOptions.Protocol)
            {
                case "pop3":
                    using (Pop3Client client = new Pop3Client())
                    {
                        client.Connect(conf.settings.MailOptions.Server, conf.settings.MailOptions.Port, true);
                        client.Authenticate(conf.settings.MailOptions.Email, conf.settings.MailOptions.Password);

                        int messageCount = client.GetMessageCount();
                        client.Disconnect();
 
                    }
                    break;
                case "imap":
                //TODO: Implement IMAP logic
                    break;
                default:
                    throw new Exception("Invalid mail protocol");
            }

        }
    }
}