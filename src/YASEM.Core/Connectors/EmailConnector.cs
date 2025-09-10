using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using YASEM.Core.Interfaces;
using YASEM.Core.Models;
using YASEM.Core.Exceptions;
using System.Resources;

namespace YASEM.Core.Connectors
{
    public class EmailConnector : IMailConnector
    {
        private readonly IImapClient _imapClient;
        private readonly IPop3Client _popClient;
        private ProtocolType _activeProtocol; // To track which client is currently connected
        private readonly ResourceManager _resourceManager;

        private enum ProtocolType
        {
            None,
            Pop3,
            Imap
        }

        public EmailConnector(IImapClient imapClient, IPop3Client popClient)
        {
            _imapClient = imapClient ?? throw new ArgumentNullException(nameof(imapClient));
            _popClient = popClient ?? throw new ArgumentNullException(nameof(popClient));
            _activeProtocol = ProtocolType.None;
            _resourceManager = new ResourceManager("YASEM.Core.Resources.ErrorMessages", typeof(EmailConnector).Assembly);
        }

        public async Task ConnectAsync(string protocol, string host, int port, string user, string password, bool useSsl, bool ignoreCertificateErrors)
        {
            // Set certificate validation callback for both clients if needed
            if (ignoreCertificateErrors)
            {
                _imapClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
                _imapClient.CheckCertificateRevocation = false;
                _popClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
                _popClient.CheckCertificateRevocation = false;
            }
            else
            {
                // Reset to default validation if not ignoring errors
                _imapClient.ServerCertificateValidationCallback = null;
                _imapClient.CheckCertificateRevocation = true;
                _popClient.ServerCertificateValidationCallback = null;
                _popClient.CheckCertificateRevocation = true;
            }

            try
            {
                switch (protocol.ToLower())
                {
                    case "pop3":
                        await _popClient.ConnectAsync(host, port, SecureSocketOptions.Auto);
                        await _popClient.AuthenticateAsync(user, password);
                        _activeProtocol = ProtocolType.Pop3;
                        break;
                    case "imap":
                        await _imapClient.ConnectAsync(host, port, useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.None);
                        await _imapClient.AuthenticateAsync(user, password);
                        _activeProtocol = ProtocolType.Imap;
                        break;
                    default:
                        throw new NotSupportedException($"Invalid mail protocol specified: {protocol}");
                }
            }
            catch (Exception ex)
            {
                throw new MailConnectionException(_resourceManager.GetString("MailConnectionError"), ex);
            }
        }

        public async Task<IEnumerable<MimeMessage>> FetchMessagesAsync(string folderName, List<Filter> filters)
        {
            try
            {
                List<MimeMessage> messages = new List<MimeMessage>();

                switch (_activeProtocol)
                {
                    case ProtocolType.Pop3:
                        messages.AddRange(await GetMessagesAsync(_popClient));
                        // Apply client-side filtering for POP3
                        if (filters != null && filters.Any())
                        {
                            messages = ApplyClientSideFilters(messages, filters).ToList();
                        }
                        break;
                    case ProtocolType.Imap:
                        messages.AddRange(await GetMessagesAsync(_imapClient, folderName, filters));
                        break;
                    case ProtocolType.None:
                        throw new InvalidOperationException("Not connected to any mail server.");
                }
                return messages;
            }
            catch (Exception ex)
            {
                throw new MailConnectionException(_resourceManager.GetString("MailConnectionError"), ex);
            }
        }

        public async Task DisconnectAsync()
        {
            switch (_activeProtocol)
            {
                case ProtocolType.Pop3:
                    if (_popClient.IsConnected)
                    {
                        await _popClient.DisconnectAsync(true);
                    }
                    break;
                case ProtocolType.Imap:
                    if (_imapClient.IsConnected)
                    {
                        await _imapClient.DisconnectAsync(true);
                    }
                    break;
            }
            _activeProtocol = ProtocolType.None;
        }

        private async Task<List<MimeMessage>> GetMessagesAsync(IPop3Client client)
        {
            try
            {
                List<MimeMessage> messages = new List<MimeMessage>();
                for (int i = 0; i < client.Count; i++)
                {
                    messages.Add(await client.GetMessageAsync(i));
                }
                return messages;
            }
            catch (Exception ex)
            {
                throw new MailConnectionException(_resourceManager.GetString("MailConnectionError"), ex);
            }
        }

        private async Task<List<MimeMessage>> GetMessagesAsync(IImapClient client, string folderName, List<Filter> filters)
        {
            try
            {
                List<MimeMessage> messages = new List<MimeMessage>();
                var inbox = client.GetFolder(folderName);
                await inbox.OpenAsync(FolderAccess.ReadOnly);

                SearchQuery query = BuildSearchQuery(filters);
                var uids = await inbox.SearchAsync(query);

                foreach (var uid in uids)
                {
                    messages.Add(await inbox.GetMessageAsync(uid));
                }
                return messages;
            }
            catch (Exception ex)
            {
                throw new MailConnectionException(_resourceManager.GetString("MailConnectionError"), ex);
            }
        }

        // Client-side filtering for POP3
        private IEnumerable<MimeMessage> ApplyClientSideFilters(IEnumerable<MimeMessage> messages, List<Filter> filters)
        {
            // This is a basic client-side filter implementation.
            // For a robust solution, this would need to be more sophisticated,
            // potentially reusing logic from BuildSearchQuery or a dedicated FilterEngine.
            // [Trade-off]: This is a simplified implementation for POP3.
            return messages.Where(m =>
            {
                bool match = true;
                foreach (var filter in filters)
                {
                    switch (filter.Type.ToLower())
                    {
                        case "field":
                            switch (filter.Name.ToLower())
                            {
                                case "subject":
                                    if (!m.Subject.Contains(filter.Value, StringComparison.OrdinalIgnoreCase))
                                    {
                                        match = false;
                                    }
                                    break;
                                case "body":
                                    if (!m.TextBody.Contains(filter.Value, StringComparison.OrdinalIgnoreCase) &&
                                        !m.HtmlBody.Contains(filter.Value, StringComparison.OrdinalIgnoreCase))
                                    {
                                        match = false;
                                    }
                                    break;
                                default:
                                    // For unsupported fields, we might choose to not filter or throw.
                                    // For client-side, not filtering is safer.
                                    break;
                            }
                            break;
                        default:
                            // For unsupported filter types, we might choose to not filter or throw.
                            break;
                    }
                    if (!match) break; // If any filter doesn't match, stop checking
                }
                return match;
            });
        }


        public static SearchQuery BuildSearchQuery(List<Filter> filters)
        {
            SearchQuery query = SearchQuery.All;

            if (filters == null || !filters.Any())
            {
                return query; // Return SearchQuery.All if no filters
            }

            foreach (var filter in filters)
            {
                switch (filter.Type.ToLower())
                {
                    case "field":
                        switch (filter.Name.ToLower())
                        {
                            case "subject":
                                query = query.And(SearchQuery.SubjectContains(filter.Value));
                                break;
                            case "body":
                                query = query.And(SearchQuery.BodyContains(filter.Value));
                                break;
                            // TODO: Implement other field types (recipients, to, from, reply-to, cc, bcc, sentDate, receivedDate, size)
                            default:
                                throw new NotSupportedException($"Unsupported field filter name: {filter.Name}");
                        }
                        break;
                    // TODO: Implement other filter types (header, flag)
                    default:
                        throw new NotSupportedException($"Unsupported filter type: {filter.Type}");
                }
            }
            return query;
        }

        public void Dispose()
        {
            // Dispose the clients if they are connected
            if (_imapClient.IsConnected)
            {
                _imapClient.Disconnect(true);
            }
            if (_popClient.IsConnected)
            {
                _popClient.Disconnect(true);
            }
            _imapClient.Dispose();
            _popClient.Dispose();
        }

        public async Task<List<MimeMessage>> ConnectAndRetrieveMessagesAsync(Config testCase, string decryptedPassword)
        {
            await ConnectAsync(
                testCase.MailOptions.MailServerType,
                testCase.MailOptions.Server,
                testCase.MailOptions.Port,
                testCase.MailOptions.Email,
                decryptedPassword,
                testCase.MailOptions.UseSsl,
                testCase.MailOptions.IgnoreCertificateErrors
            );

            var messages = await FetchMessagesAsync(testCase.MailOptions.Folder, testCase.Filters);

            await DisconnectAsync();

            return messages.ToList();
        }
    }
}