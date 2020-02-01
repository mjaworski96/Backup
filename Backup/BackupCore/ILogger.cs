using FilesystemModel;

namespace BackupCore
{
    public interface ILogger
    {
        long MaxProgress { set; }
        void UpdateProgress(long progressGross);
        void Write(string message);
        void Write(Directory directory);  
    }
}
