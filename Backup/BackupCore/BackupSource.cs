using FilesystemModel;
using FilesystemModel.Extensions;

namespace BackupCore
{
    public class BackupSource: IBackup
    {
        private readonly ISourceCommunicator _communicator;
        private readonly ILogger _logger;

        public BackupSource(ISourceCommunicator communicator,
            ILogger logger)
        {
            _communicator = communicator;
            _logger = logger;
        }

        public void MakeBackup(Directory directory)
        {
            _logger.Write(directory);
            _communicator.SendDirectory(directory);
            HandleRequests(directory);
        }
        private void HandleRequests(Directory directory)
        {
            Request request = Request.FINISH;
            do
            {
                request = _communicator.GetRequest();
                switch (request)
                {
                    case Request.GET_FILE:
                        GetFile(directory);
                        break;
                    case Request.GET_CRC32:
                        GetCrc32(directory);
                        break;
                    case Request.FINISH:
                    default:
                        break;
                }
            } while (request != Request.FINISH);
        }

        private void GetCrc32(Directory directory)
        {
            string filename = _communicator.GetFilename();
            _logger.Write($"Calculating checksum for {filename}");
            File file = directory.Find(filename) as File;
            _communicator.SendCrc32(file.CalculateCrc32());
        }

        private void GetFile(Directory directory)
        {
            string filename = _communicator.GetFilename();
            _logger.Write($"Uploading {filename}");
            File file = directory.Find(filename) as File;
            _communicator.SendFile(file.Path);
        }

        public void Dispose()
        {
            _communicator?.Dispose();
        }
    }
}
