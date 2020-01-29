﻿using FilesystemModel;
using FilesystemModel.Extensions;

namespace BackupCore
{
    public class BackupSource: IBackup
    {
        private readonly ISourceCommunicator _communicator;

        public BackupSource(ISourceCommunicator communicator)
        {
            _communicator = communicator;
        }

        public void MakeBackup(Directory directory)
        {
            System.Console.WriteLine(directory);
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
            System.Console.WriteLine($"Calculating checksum for {filename}");
            File file = directory.Find(filename) as File;
            _communicator.SendCrc32(file.CalculateCrc32());
        }

        private void GetFile(Directory directory)
        {
            string filename = _communicator.GetFilename();
            System.Console.WriteLine($"Uploading {filename}");
            File file = directory.Find(filename) as File;
            _communicator.SendFile(file.Path);
        }

        public void Dispose()
        {
            _communicator?.Dispose();
        }
    }
}