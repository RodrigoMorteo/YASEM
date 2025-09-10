using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MimeKit;
using MailKit.Search; // Added for SearchQuery
using YASEM.Core.Models; // Assuming Filter is here

namespace YASEM.Core.Interfaces
{
    public interface IMailConnector : IDisposable
    {
        Task ConnectAsync(string protocol, string host, int port, string user, string password, bool useSsl, bool ignoreCertificateErrors);
        Task<IEnumerable<MimeMessage>> FetchMessagesAsync(string folderName, List<Filter> filters);
        Task <List<MimeMessage>> ConnectAndRetrieveMessagesAsync(Config testCase, string decryptedPassword);
        Task DisconnectAsync();
    }
}