﻿using System.Collections.Generic;
using Common;
using FilesystemModel;
using FilesystemModel.Extensions;

namespace BackupCore
{
    public class BackupDestination: IBackup
    {
        private readonly IDestinationCommunicator _communicator;
        private readonly ILogger _logger;
        private readonly int _bufferSize;

        public BackupDestination(IDestinationCommunicator communicator,
            ILogger logger, int bufferSize)
        {
            _communicator = communicator;
            _logger = logger;
            _bufferSize = bufferSize;
        }

        public void MakeBackup(Directory destination)
        {
            Directory source = GetSource();
            MakeBackup(source, destination, destination.Path);
            _communicator.Finish();
        }
        private Directory GetSource()
        {
            Directory source = _communicator.GetDirectory();
            _logger.Write(source);
            return source;
        }
        private void MakeBackup(Directory source, Directory destination, string rootDirectory)
        {
            if(source.Attributes != destination.Attributes)
            {
                System.IO.File.SetAttributes(destination.Path, source.Attributes);
            }
            source.Compare(destination,
                out List<FileBase> newFiles,
                out List<(FileBase InFirstDirectory, FileBase InSecondDirectory)> existingFiles,
                out List<FileBase> deletedFiles);
            HandleNewFiles(newFiles, rootDirectory);
            HandleExistingFiles(existingFiles, rootDirectory);
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
                    System.IO.File.SetAttributes(path, item.Attributes);
                    HandleNewFiles(directory.Content, path);
                }
                else if (item.Type == FileType.FILE)
                {
                    _logger.Write($"Downloanding {item.Path}");
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
                    _logger.Write($"Deleting {path}");
                    SetNormalAttribute(path);
                    System.IO.Directory.Delete(path);
                }
                else if (item.Type == FileType.FILE)
                {
                    _logger.Write($"Deleting {path}");
                    SetNormalAttribute(path);
                    System.IO.File.Delete(path);
                }
            }
        }

        private static void SetNormalAttribute(string path)
        {
            System.IO.File.SetAttributes(path, System.IO.FileAttributes.Normal);
        }

        private void HandleExistingFiles(List<(FileBase InFirstDirectory, FileBase InSecondDirectory)> existingFiles,
            string rootDirectory)
        {
            foreach (var item in existingFiles)
            {
                if (item.InFirstDirectory.Type != item.InSecondDirectory.Type)
                    HandleDifrentTypes(item.InFirstDirectory, item.InSecondDirectory, rootDirectory);
                else
                    HandleSameTypes(item.InFirstDirectory, item.InSecondDirectory, rootDirectory);

            }
        }

        private void HandleSameTypes(FileBase inSource, FileBase inDestination, string rootDirectory)
        {
            if(inSource.Type == FileType.DIRECTORY)
                HandleDirectory(inSource, inDestination, rootDirectory);
            else
                HandleFile(inSource, inDestination);
        }

        private void HandleFile(FileBase inSource, FileBase inDestination)
        {
            File sourceFile = inSource as File;
            File destinationFile = inDestination as File;
            _logger.Write($"Checking checksum for {sourceFile.Path}");
            if (IsDiffrent(sourceFile.Path, destinationFile.CalculateCrc32(_bufferSize, _logger)))
            {
                _logger.Write($"Downloanding {sourceFile.Path}");
                _communicator.ReceiveFile(sourceFile.Path, inDestination.Path, sourceFile.Attributes);
            }
            if(sourceFile.Attributes != destinationFile.Attributes)
            {
                System.IO.File.SetAttributes(inDestination.Path, sourceFile.Attributes);
            }
        }

        private void HandleDirectory(FileBase inSource, FileBase inDestination, string rootDirectory)
        {
            Directory sourceDir = inSource as Directory;
            Directory destinationDir = inDestination as Directory;
            MakeBackup(sourceDir, destinationDir,
                FileBase.BuildPath(rootDirectory, inSource.Name));
        }

        private void HandleDifrentTypes(FileBase inSource, FileBase inDestination, string rootDirectory)
        {
            HandleDeletedFiles(new List<FileBase> { inDestination }, rootDirectory);
            HandleNewFiles(new List<FileBase> { inSource }, rootDirectory);
        }
        private bool IsDiffrent(string fileRequestPath, uint crc32)
        {
            return _communicator.GetCrc32(fileRequestPath) != crc32;
        }
        public void Dispose()
        {
            _communicator?.Dispose();
        }
    }
}
