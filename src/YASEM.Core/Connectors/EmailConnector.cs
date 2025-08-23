using System.Collections.Generic;
using System.Net;
using System.Net.Security; 
using System.Net.Mail;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using YASEM.Core.Interfaces;

using MailKit;
using MailKit.Security;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MimeKit;
using YASEM.Core.Models;

namespace YASEM.Core.Connectors
{
    public class EmailConnector : IMailConnector
    {
        public async Task<List<MimeMessage>> ConnectAndRetrieveMessagesAsync(MailOptions mailOptions)
        {
            List<MimeMessage> messages = new();
            mailOptions.Protocol = mailOptions.Protocol.ToLower(); //normalize protocol name
            ProtocolLogger logger = mailOptions.EnableDebugLog? new ProtocolLogger (Console.OpenStandardOutput ()) : null;
            //As the protocol clients do not share a sufficiently common ancestor, separate code was built for each
            //DEBUG
            Console.WriteLine ($"Connecting to server {mailOptions.Server} using {mailOptions.Protocol}.");
            switch(mailOptions.Protocol)
            {
                case "pop3":
                    using (var popClient = mailOptions.EnableDebugLog ? new Pop3Client(logger) : new Pop3Client())
                    {
                        if (mailOptions.IgnoreCertificateErrors)
                        {
                            popClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
                            popClient.CheckCertificateRevocation = false;
                        }
                        await popClient.ConnectAsync(mailOptions.Server, mailOptions.Port, SecureSocketOptions.Auto);
                        await popClient.AuthenticateAsync(mailOptions.Email, mailOptions.Password);
                        messages = await GetMessagesAsync(popClient, mailOptions);
                        await popClient.DisconnectAsync(true);
                    }
                    break;
                case "imap":
                    using (var iClient = mailOptions.EnableDebugLog ? new ImapClient(logger) : new ImapClient())
                    {
                        if (mailOptions.IgnoreCertificateErrors)
                        {
                            iClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
                            iClient.CheckCertificateRevocation = false;
                        }
                        await iClient.ConnectAsync(mailOptions.Server, mailOptions.Port, SecureSocketOptions.None); //TODO: Test with SecureSocketOptions.Auto
                        await iClient.AuthenticateAsync(mailOptions.Email, mailOptions.Password);
                        messages = await GetMessagesAsync(iClient, mailOptions);
                        await iClient.DisconnectAsync(true);
                    }
                    break;
                default:
                    throw new NotSupportedException($"Invalid mail protocol specified: {mailOptions.Protocol}");
            }
            //DEBUG
            Console.WriteLine($"{messages.Count} email Messages found for {mailOptions.Email} ");
            return messages;
        }

        private async Task<List<MimeMessage>> GetMessagesAsync(Pop3Client client, MailOptions options)
        {
            List<MimeMessage> messages = new List<MimeMessage> ();
            for (int i = 0; i < client.Count; i++) 
            {
                messages.Add(await client.GetMessageAsync(i));
                //DEBUG Console.WriteLine ("Subject: {0}", message.Subject);
            }
            /*
            foreach (var uid in folder.Search (SearchQuery.NotSeen)) {
                var message = folder.GetMessage (uid);
            }
            */
            return messages;
        }

        private async Task<List<MimeMessage>> GetMessagesAsync(ImapClient client, MailOptions options)
        {
            List<MimeMessage> messages = new List<MimeMessage> ();
            var inbox = client.GetFolder(options.Folder);
            await inbox.OpenAsync(FolderAccess.ReadOnly);

            for (int i = 0; i < inbox.Count; i++) {
                messages.Add(await inbox.GetMessageAsync(i));
                //DEBUG 
                    //Console.WriteLine ("Subject: {0}", message.Subject);
            }
            return messages;
        }
    }
}