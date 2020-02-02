namespace Common
{
    public interface ILogger
    {
        long MaxProgress { set; }
        void Write(object message);
        void UpdateProgress(long progressGross);
        void ResetProgress();
        
    }
}
