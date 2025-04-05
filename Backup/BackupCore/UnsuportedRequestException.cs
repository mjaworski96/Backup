using Common.Translations;
using System;

namespace BackupCore
{
    internal class UnsuportedRequestException : Exception
    {
        public UnsuportedRequestException(int request): base(string.Format(Exceptions.UnsupportedRequest, request))
        {
        }
    }
}