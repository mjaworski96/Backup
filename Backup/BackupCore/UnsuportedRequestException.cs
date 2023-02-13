using Common.Translations;
using System;
using System.Runtime.Serialization;

namespace BackupCore
{
    internal class UnsuportedRequestException : Exception
    {
        public UnsuportedRequestException(int request): base(string.Format(Exceptions.UnsupportedRequest, request))
        {
        }
    }
}