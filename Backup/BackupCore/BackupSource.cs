using Common;
using Common.Translations;
using FilesystemModel;
using FilesystemModel.Extensions;

namespace BackupCore
{
    public class BackupSource: IBackup
    {
        private readonly ISourceCommunicator _communicator;
        private readonly ILogger _logger;
        private readonly int _bufferSize;

        public BackupSource(ISourceCommunicator communicator,
            ILogger logger, int bufferSize)
        {
            _communicator = communicator;
            _logger = logger;
            _bufferSize = bufferSize;
        }

        public void MakeBackup(Directory directory)
        {
            _logger.Write(directory);
            _communicator.SendDirectory(directory);
            HandleRequests(directory);
        }
        private void HandleRequests(Directory directory)
        {
            Request request;
            do
            {
                request = _communicator.GetRequest();
                switch (request)
                {
                    case Request.GET_FILE:
                        SendFile(directory);
                        break;
                    case Request.GET_CRC32:
                        SendCrc32(directory);
                        break;
                    case Request.GET_FILE_SIZE:
                        SendFileSize(directory);
                        break;
                    case Request.FINISH:
                        break;
                    default:
                        throw new UnsuportedRequestException((int)request);
                }
            } while (request != Request.FINISH);
        }

        private void SendCrc32(Directory directory)
        {
            var filename = _communicator.GetFilename();
            _logger.Write(string.Format(LoggerMessages.CalculatingChecksum, filename));
            var file = directory.Find(filename) as File;
            _communicator.SendCrc32(file.CalculateCrc32(_bufferSize, _logger));
        }

        private void SendFileSize(Directory directory)
        {
            var filename = _communicator.GetFilename();
            var file = directory.Find(filename) as File;
            _communicator.SendFileSize(file.Size);
        }

        private void SendFile(Directory directory)
        {
            var filename = _communicator.GetFilename();
            _logger.Write(string.Format(LoggerMessages.Uploading, filename));
            var file = directory.Find(filename) as File;
            _communicator.SendFile(file.Path);
        }

        public void Dispose()
        {
            _communicator?.Dispose();
        }
    }
}
