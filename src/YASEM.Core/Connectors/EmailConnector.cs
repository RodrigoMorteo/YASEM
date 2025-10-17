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
            if (ignoreCertificateErrors)
            {
                _imapClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
                _imapClient.CheckCertificateRevocation = false;
                _popClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
                _popClient.CheckCertificateRevocation = false;
            }
            else
            {
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
                throw new MailConnectionException(_resourceManager.GetString("MailConnectionError") ?? "An error occurred while connecting to the mail server.", ex);
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
                throw new MailConnectionException(_resourceManager.GetString("MailConnectionError") ?? "An error occurred while fetching messages.", ex);
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
                throw new MailConnectionException(_resourceManager.GetString("MailConnectionError") ?? "An error occurred while getting messages.", ex);
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
                throw new MailConnectionException(_resourceManager.GetString("MailConnectionError") ?? "An error occurred while getting messages.", ex);
            }
        }

        private IEnumerable<MimeMessage> ApplyClientSideFilters(IEnumerable<MimeMessage> messages, List<Filter> filters)
        {
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
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                    if (!match) break;
                }
                return match;
            });
        }


        public static SearchQuery BuildSearchQuery(List<Filter> filters)
        {
            SearchQuery query = SearchQuery.All;

            if (filters == null || !filters.Any())
            {
                return query;
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
                            default:
                                throw new NotSupportedException($"Unsupported field filter name: {filter.Name}");
                        }
                        break;
                    default:
                        throw new NotSupportedException($"Unsupported filter type: {filter.Type}");
                }
            }
            return query;
        }

        public void Dispose()
        {
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
            if (testCase.MailOptions == null)
            {
                throw new InvalidOperationException("MailOptions are not configured in the test case.");
            }

            if (string.IsNullOrEmpty(testCase.MailOptions.Server))
            {
                throw new InvalidOperationException("Mail server is not configured in the test case.");
            }

            if (string.IsNullOrEmpty(testCase.MailOptions.Email))
            {
                throw new InvalidOperationException("Email is not configured in the test case.");
            }

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