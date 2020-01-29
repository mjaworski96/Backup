using FilesystemModel;
using System;

namespace BackupCore
{
    public interface IBackup: IDisposable
    {
        void MakeBackup(Directory directory);
    }
}
