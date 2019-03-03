using FilesystemModel;
using System;

namespace BackupLogic
{
    public interface ISourceCommunicator: IDisposable
    {
        void SendDirectory(Directory directory);
        Request GetRequest();
        string GetFilename();
        void SendFile(string filename);
        void SendCrc32(uint crc32);
    }
}
