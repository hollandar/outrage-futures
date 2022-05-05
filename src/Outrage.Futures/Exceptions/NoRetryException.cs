using System;
using System.Runtime.Serialization;

namespace Outrage.Futures.Exceptions
{
    [Serializable]
    internal class NoRetryException : Exception
    {
        public NoRetryException()
        {
        }

        public NoRetryException(string? message) : base(message)
        {
        }

        public NoRetryException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected NoRetryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}