namespace Common
{
    public interface ILogger
    {
        long MaxProgress { set; }
        void Write(object message);
        void UpdateProgressBar(long progressGross);
        void ShowCompleted();
    }
}
