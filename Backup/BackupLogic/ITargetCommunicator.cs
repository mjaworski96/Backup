using System;
using System.Collections.Generic;
using System.Text;
using FilesystemModel;

namespace BackupLogic
{
    public interface ITargetCommunicator: IDisposable
    {
        Directory GetDirectory();
        void ReceiveFile(string fileRequestPath, string saveFileAs, int attributes);
        uint GetCrc32(string fileRequestPath);
        void Finish();
    }
}
