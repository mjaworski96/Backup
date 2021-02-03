using System.IO;
using System.Threading;

namespace Common
{
    public static class SafeFileUsage
    {
        const int WAIT_TIME_IN_SECCONDS = 5;
        const long ERROR_SHARING_VIOLATION = 0x20;
        const long ERROR_LOCK_VIOLATION = 0x21;

        public static FileStream GetFile(string filename, 
            FileMode fileMode, FileAccess fileAccess,
            ILogger logger)
        {
            
            FileStream file = null;
            do
            {
                try
                {
                    file = new FileStream(filename, fileMode, fileAccess);
                }
                catch (IOException e)
                {
                    long errorCode = e.HResult & 0xFFFF;
                    if(errorCode == ERROR_LOCK_VIOLATION || errorCode == ERROR_SHARING_VIOLATION)
                    {
                        logger.Write($"File {filename} is used by other process. " +
                        $"Waiting {WAIT_TIME_IN_SECCONDS} seconds for access.");
                        Thread.Sleep(WAIT_TIME_IN_SECCONDS * 1000);
                    }
                    else
                    {
                        throw e;
                    }
                    
                }
            } while (file == null);
            return file;
        }
    }
}
