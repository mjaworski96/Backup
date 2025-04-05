using Common.Translations;
using System;

namespace Communication
{
    public class InvalidBufferSizeException : Exception
    {
        public InvalidBufferSizeException(): base(Exceptions.BufferSizeGreaterThanZero)
        {
        }
    }
}
