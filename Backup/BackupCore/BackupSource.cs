using Common;
using Common.Translations;
using FilesystemModel;
using FilesystemModel.Extensions;
using System.Threading.Tasks;

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

        public Task MakeBackup(Directory directory)
        {
            if (Connect())
            {
                _logger.Write(directory);
                _communicator.SendDirectory(directory);
                HandleRequests(directory);
            }
            return Task.CompletedTask;
        }

        private bool Connect()
        {
            _communicator.SendSourceConfiguration(new ClientConfiguration
            {
                BufferSize = _bufferSize
            });
            var status = _communicator.GetConnectionStatus();

            if (!status.IsConfigurationValid)
            {
                status.Errors.ForEach(error => { WriteConnectionError(error); });
            }

            return status.IsConfigurationValid;
        }

        private void WriteConnectionError(ConnectionError error)
        {
            switch (error.ErrorCode)
            {
                case ErrorCodes.INVALID_BUFFER_SIZE:
                    _logger.WriteError(string.Format(LoggerMessages.InvalidBufferSize, error.ExpectedParameterValue));
                    break;
                default:
                    _logger.WriteError(LoggerMessages.UnknownError);
                    break;
            }
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
            _communicator.SendCrc32(file.CalculateCrc32(_bufferSize, _logger, false));
        }

        private void SendFileSize(Directory directory)
        {
            var filename = _communicator.GetFilename();
            _logger.Write(string.Format(LoggerMessages.SendingFileSize, filename));
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
