using Common.Translations;
using System;
using System.Runtime.Serialization;

namespace Communication
{
    public class InvalidBufferSizeException : Exception
    {
        public InvalidBufferSizeException(): base(Exceptions.BufferSizeGreaterThanZero)
        {
        }
    }
}
