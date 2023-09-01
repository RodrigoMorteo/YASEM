using System.Collections.Generic;
using System.Net;
using System.Net.Security; 
using System.Net.Mail;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using MailKit;
using MailKit.Security;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MimeKit;

using qualityassurance.tools.JSON;

namespace YASEM
{
    public class EmailConnector
    {
        public List<MimeMessage> ConnectAndRetrieveMessages(MailOptions mailOptions)
        {
            List<MimeMessage> messages;
            mailOptions.Protocol = mailOptions.Protocol.ToLower(); //normalize protocol name
            ProtocolLogger logger = mailOptions.EnableDebugLog? new ProtocolLogger (Console.OpenStandardOutput ()) : null;
            //As the protocol clients do not share a sufficiently common ancestor, separate code was built for each
            //DEBUG
            Console.WriteLine ($"Connecting to server {mailOptions.Server} using {mailOptions.Protocol}.");
            switch(mailOptions.Protocol)
            {
                case "pop3":
                    Pop3Client popClient = mailOptions.EnableDebugLog? new Pop3Client(logger) : new Pop3Client();
                    if(mailOptions.IgnoreCertificateErrors){
                        popClient.ServerCertificateValidationCallback = (s,c,h,e) => true;
                        popClient.CheckCertificateRevocation = false;    
                    }
                    popClient.Connect (mailOptions.Server, mailOptions.Port, SecureSocketOptions.Auto);
                    popClient.Authenticate (mailOptions.Email, mailOptions.Password);
                    messages = GetMessages(popClient, mailOptions);
                    popClient.Disconnect (true);
                    break;
                case "imap":
                    ImapClient iClient;
                    iClient = mailOptions.EnableDebugLog? new ImapClient (logger) : new ImapClient ();
                    if(mailOptions.IgnoreCertificateErrors){
                        iClient.ServerCertificateValidationCallback = (s,c,h,e) => true;
                        iClient.CheckCertificateRevocation = false;    
                    }
                    iClient.Connect (mailOptions.Server, mailOptions.Port, SecureSocketOptions.None); //TODO: Test with SecureSocketOptions.Auto
                    iClient.Authenticate (mailOptions.Email, mailOptions.Password);
                    messages = GetMessages(iClient, mailOptions);                    
                    iClient.Disconnect (true);
                    break;
                default:
                    throw new Exception("Invalid mail protocol");
            }
            //DEBUG
            Console.WriteLine($"{messages.Count} email Messages found for {mailOptions.Email} ");
            return messages;
        }
        
        private bool CustomCertificateValidationCallback (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            //Check cert comparsion here. See implementation details in docs/acknowledgements.md[2]
            return false;
        }

        private List<MimeMessage> GetMessages(Pop3Client client, MailOptions options)
        {
            List<MimeMessage> messages = new List<MimeMessage> ();
            for (int i = 0; i < client.Count; i++) 
            {
                messages.Add(client.GetMessage(i));
                //DEBUG Console.WriteLine ("Subject: {0}", message.Subject);
            }
            /*
            foreach (var uid in folder.Search (SearchQuery.NotSeen)) {
                var message = folder.GetMessage (uid);
            }
            */
            return messages;
        }

        private List<MimeMessage> GetMessages(ImapClient client, MailOptions options)
        {
            List<MimeMessage> messages = new List<MimeMessage> ();
            // The Inbox folder is always available on all IMAP servers...
            var inbox = client.Inbox; //TODO:Configure Get folder in IMAP client from Settings.
            inbox.Open (FolderAccess.ReadOnly);
            //DEBUG
                //Console.WriteLine ("Total messages: {0}", inbox.Count);
                //Console.WriteLine ("Recent messages: {0}", inbox.Recent);

            for (int i = 0; i < inbox.Count; i++) {
                messages.Add(inbox.GetMessage (i));
                //DEBUG 
                    //Console.WriteLine ("Subject: {0}", message.Subject);
            }
            return messages;
        }
    }
}