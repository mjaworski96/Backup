using FilesystemModel;
using System;
using System.Threading.Tasks;

namespace BackupCore
{
    public interface IBackup: IDisposable
    {
        Task MakeBackup(Directory directory);
    }
}
