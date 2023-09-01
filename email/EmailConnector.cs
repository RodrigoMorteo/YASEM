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
        public bool StartSession(MailOptions mailOptions)
        {
            switch(mailOptions.Protocol)
            {
                case "pop3":
                    using (var client = new Pop3Client ()) {
                    client.Connect (mailOptions.Server, mailOptions.Port, false);

                    client.Authenticate (mailOptions.Email, mailOptions.Password);
                    Console.WriteLine(client.Count);
                    /*for (int i = 0; i < client.Count; i++) {
                        var message = client.GetMessage (i);
                        Console.WriteLine ("Subject: {0}", message.Subject);
                    }*/

                    client.Disconnect (true);
            }
                    break;
                case "imap":
                    using (var client = new ImapClient ()) {
                        if(mailOptions.IgnoreCertificateErrors){
                            //client.ServerCertificateValidationCallback = 
                            client.CheckCertificateRevocation = false;    
                        }
                        
                        client.Connect (mailOptions.Server, mailOptions.Port, SecureSocketOptions.None);
                        client.Authenticate (mailOptions.Email, mailOptions.Password);

                        // The Inbox folder is always available on all IMAP servers...
                        var inbox = client.Inbox; //TODO:Configure Get folder in IMAP client from Settings.
                        inbox.Open (FolderAccess.ReadOnly);

                        Console.WriteLine ("Total messages: {0}", inbox.Count);
                        Console.WriteLine ("Recent messages: {0}", inbox.Recent);

                        /*for (int i = 0; i < inbox.Count; i++) {
                            var message = inbox.GetMessage (i);
                            Console.WriteLine ("Subject: {0}", message.Subject);
                        }*/
                        client.Disconnect (true);
                    }
                    break;
                default:
                    throw new Exception("Invalid mail protocol");
            }
            return true;
        }
        bool IgnoreCertificateValidationCallback (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        bool CustomCertificateValidationCallback (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            // Note: The following code casts to an X509Certificate2 because it's easier to get the
            // values for comparison, but it's possible to get them from an X509Certificate as well.
            if (certificate is X509Certificate2 certificate2) {
                var cn = certificate2.GetNameInfo (X509NameType.SimpleName, false);
                var fingerprint = certificate2.Thumbprint;
                var serial = certificate2.SerialNumber;
                var issuer = certificate2.Issuer;
                //TODO: Remove sample data (from http://www.mimekit.net/docs/html/Frequently-Asked-Questions.htm)
                return cn == "imap.gmail.com" && issuer == "CN=GTS CA 1O1, O=Google Trust Services, C=US" &&
                    serial == "00BABE95B167C9ECAF08000000006065B6" &&
                    fingerprint == "E79A011EF55EEC72D2B7E391D193761372796836";
            }

            return false;
        }
    }
}