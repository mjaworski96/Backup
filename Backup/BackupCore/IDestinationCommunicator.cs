using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
