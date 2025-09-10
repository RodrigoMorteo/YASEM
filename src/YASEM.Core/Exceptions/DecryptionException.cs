
using System;

namespace YASEM.Core.Exceptions
{
    public class DecryptionException : Exception
    {
        public DecryptionException()
        {
        }

        public DecryptionException(string message)
            : base(message)
        {
        }

        public DecryptionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
