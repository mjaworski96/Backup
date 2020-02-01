using FilesystemModel;

namespace BackupCore
{
    public interface ILogger
    {
        long MaxProgress { set; }
        void Write(string message);
        void Write(Directory directory);
        void UpdateProgress(long progressGross);
        void ResetProgress();
        
    }
}
