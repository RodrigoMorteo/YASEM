
using System;

namespace YASEM.Core.Exceptions
{
    public class NoEmailsFoundException : Exception
    {
        public NoEmailsFoundException()
        {
        }

        public NoEmailsFoundException(string message)
            : base(message)
        {
        }

        public NoEmailsFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
