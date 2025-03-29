using System;
using Common;
using FilesystemModel;

namespace BackupCore
{
    public interface IDestinationCommunicator: IDisposable
    {
        Directory GetDirectory();
        void ReceiveFile(string fileRequestPath,
            string saveFileAs,
            System.IO.FileAttributes attributes);
        uint GetCrc32(string fileRequestPath);
        void Finish();
        long GetFileSize(string fileRequestPath);
        void Connect();
        void Reset();
        ClientConfiguration GetSourceConfiguration();
        void SendConnectionStatus(ConnectionStatus connectionStatus);
    }
}
