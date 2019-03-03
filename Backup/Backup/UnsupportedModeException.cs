using System;
using System.Runtime.Serialization;

namespace Backup
{
    [Serializable]
    internal class UnsupportedModeException : Exception
    {
        public UnsupportedModeException()
        {
        }

        public UnsupportedModeException(string message) : base(message)
        {
        }

        public UnsupportedModeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnsupportedModeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}