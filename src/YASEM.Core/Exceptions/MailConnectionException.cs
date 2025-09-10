
using System;

namespace YASEM.Core.Exceptions
{
    public class MailConnectionException : Exception
    {
        public MailConnectionException()
        {
        }

        public MailConnectionException(string message)
            : base(message)
        {
        }

        public MailConnectionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
