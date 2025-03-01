using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Translations;
using FilesystemModel;
using FilesystemModel.Extensions;

namespace BackupCore
{
    public class BackupDestination : IBackup
    {
        private readonly IDestinationCommunicator _communicator;
        private readonly ILogger _logger;
        private readonly int _bufferSize;
        private readonly int _compareLargerFilesBySize;
        public BackupDestination(IDestinationCommunicator communicator,
            ILogger logger, int bufferSize, int compareLargerFilesBySize)
        {
            _communicator = communicator;
            _logger = logger;
            _bufferSize = bufferSize;
            _compareLargerFilesBySize = compareLargerFilesBySize;
        }

        public async Task MakeBackup(Directory destination)
        {
            Directory source = GetSource();
            await MakeBackup(source, destination, destination.Path, true);
            _communicator.Finish();
            CreateBackupDirectoryGuardFile(destination);
        }
        private Directory GetSource()
        {
            Directory source = _communicator.GetDirectory();
            _logger.Write(source);
            return source;
        }
        private async Task MakeBackup(Directory source, Directory destination, string rootDirectory, bool isBackupRoot)
        {
            if (source.Attributes != destination.Attributes)
            {
                source.Attributes.Set(destination.Path, _logger);
            }
            source.Compare(destination,
                out List<FileBase> newFiles,
                out List<(FileBase InFirstDirectory, FileBase InSecondDirectory)> existingFiles,
                out List<FileBase> deletedFiles,
                isBackupRoot);
            HandleNewFiles(newFiles, rootDirectory);
            await HandleExistingFiles(existingFiles, rootDirectory);
            HandleDeletedFiles(deletedFiles, rootDirectory);
        }
        private void HandleNewFiles(List<FileBase> newFiles, string rootDirectory)
        {
            foreach (var item in newFiles)
            {
                string path = FileBase.BuildPath(rootDirectory, item.Name);
                if (item.Type == FileType.DIRECTORY)
                {
                    Directory directory = item as Directory;
                    System.IO.Directory.CreateDirectory(path);
                    item.Attributes.Set(path, _logger);
                    HandleNewFiles(directory.Content, path);
                }
                else if (item.Type == FileType.FILE)
                {
                   _communicator.ReceiveFile(item.Path, path, item.Attributes);
                }
            }
        }
        private void HandleDeletedFiles(List<FileBase> deletedFiles, string rootDirectory)
        {
            foreach (var item in deletedFiles)
            {
                string path = FileBase.BuildPath(rootDirectory, item.Name);
                if (item.Type == FileType.DIRECTORY)
                {
                    Directory directory = item as Directory;
                    HandleDeletedFiles(directory.Content, path);
                    _logger.Write(string.Format(LoggerMessages.Deleting, path));
                    SetNormalAttribute(path);
                    System.IO.Directory.Delete(path);
                }
                else if (item.Type == FileType.FILE)
                {
                    _logger.Write(string.Format(LoggerMessages.Deleting, path));
                    SetNormalAttribute(path);
                    System.IO.File.Delete(path);
                }
            }
        }

        private void SetNormalAttribute(string path)
        {
            System.IO.FileAttributes.Normal.Set(path, _logger);
        }

        private async Task HandleExistingFiles(List<(FileBase InFirstDirectory, FileBase InSecondDirectory)> existingFiles,
            string rootDirectory)
        {
            foreach (var item in existingFiles)
            {
                if (item.InFirstDirectory.Type != item.InSecondDirectory.Type)
                    HandleDifrentTypes(item.InFirstDirectory, item.InSecondDirectory, rootDirectory);
                else
                    await HandleSameTypes(item.InFirstDirectory, item.InSecondDirectory, rootDirectory);

            }
        }

        private async Task HandleSameTypes(FileBase inSource, FileBase inDestination, string rootDirectory)
        {
            if (inSource.Type == FileType.DIRECTORY)
                await HandleDirectory(inSource, inDestination, rootDirectory);
            else
                await HandleFile(inSource, inDestination);
        }

        private async Task HandleFile(FileBase inSource, FileBase inDestination)
        {
            File sourceFile = inSource as File;
            File destinationFile = inDestination as File;
            _logger.Write(string.Format(LoggerMessages.CheckingFileSize, sourceFile.Path));
            if (await IsDiffrent(sourceFile.Path, destinationFile.Size, () => destinationFile.CalculateCrc32(_bufferSize, _logger, true)))
            {
                _communicator.ReceiveFile(sourceFile.Path, inDestination.Path, sourceFile.Attributes);
            }
            if (sourceFile.Attributes != destinationFile.Attributes)
            {
                sourceFile.Attributes.Set(inDestination.Path, _logger);
            }
        }

        private async Task HandleDirectory(FileBase inSource, FileBase inDestination, string rootDirectory)
        {
            Directory sourceDir = inSource as Directory;
            Directory destinationDir = inDestination as Directory;
            await MakeBackup(sourceDir, destinationDir,
                FileBase.BuildPath(rootDirectory, inSource.Name),
                false);
        }

        private void HandleDifrentTypes(FileBase inSource, FileBase inDestination, string rootDirectory)
        {
            HandleDeletedFiles(new List<FileBase> { inDestination }, rootDirectory);
            HandleNewFiles(new List<FileBase> { inSource }, rootDirectory);
        }

        private async Task<bool> IsDiffrent(string fileRequestPath, long fileSize, Func<uint> crc32)
        {
            long sourceFileSize = _communicator.GetFileSize(fileRequestPath);
            if (sourceFileSize != fileSize)
            {
                return true;
            }

            //If file (on source and destination) have the same size and is larger than _compareLargerFilesBySize assume it didn't change
            if (_compareLargerFilesBySize > 0 && fileSize > _compareLargerFilesBySize)
            {
                return false;
            }

            _logger.Write(string.Format(LoggerMessages.CheckingChecksum, fileRequestPath));
            var crcCurrent = Task.Run(crc32);
            var crcRemote = _communicator.GetCrc32(fileRequestPath);
            return crcRemote != await crcCurrent;
        }

        public static void CreateBackupDirectoryGuardFile(Directory directory)
        {
            var guardFilePath = FileBase.BuildPath(directory.Path, Consts.BackupDirectoryGuardFilePath);
            if (!directory.Content.Any(x => x.Name == Consts.BackupDirectoryGuardFilePath))
            {
                System.IO.File.Create(guardFilePath).Dispose();
            }
            directory.Refresh();
            System.IO.File.WriteAllText(guardFilePath, directory.ToString());
        }

        public void Dispose()
        {
            _communicator?.Dispose();
        }
    }
}
