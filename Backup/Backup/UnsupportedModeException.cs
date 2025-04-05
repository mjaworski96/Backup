using System;
using System.Runtime.Serialization;

namespace Backup
{
    [Serializable]
    internal class UnsupportedModeException : Exception
    {
        public UnsupportedModeException(string message) : base(message)
        {
        }
    }
}