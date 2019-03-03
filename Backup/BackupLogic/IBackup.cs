using FilesystemModel;
using System;

namespace BackupLogic
{
    public interface IBackup: IDisposable
    {
        void MakeBackup(Directory directory);
    }
}
