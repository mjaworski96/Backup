using System;
using System.Runtime.Serialization;

namespace Communication
{
    public class InvalidBufferSizeException : Exception
    {
        public InvalidBufferSizeException()
        {
        }

        public InvalidBufferSizeException(string message) : base(message)
        {
        }

        public InvalidBufferSizeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidBufferSizeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
