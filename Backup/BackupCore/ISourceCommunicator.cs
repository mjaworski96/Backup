using Common;
using FilesystemModel;
using System;

namespace BackupCore
{
    public interface ISourceCommunicator: IDisposable
    {
        void SendDirectory(Directory directory);
        Request GetRequest();
        string GetFilename();
        void SendFile(string filename);
        void SendCrc32(uint crc32);
        void SendFileSize(long size);
        void SendSourceConfiguration(ClientConfiguration sourceClientConfiguration);
        ConnectionStatus GetConnectionStatus();
    }
}
