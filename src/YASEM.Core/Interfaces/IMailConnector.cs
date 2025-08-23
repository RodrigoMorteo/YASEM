using System.Collections.Generic;
using System.Threading.Tasks;
using MimeKit;
using YASEM.Core.Models;

namespace YASEM.Core.Interfaces
{
    public interface IMailConnector
    {
        Task<List<MimeMessage>> ConnectAndRetrieveMessagesAsync(MailOptions mailOptions);
    }
}