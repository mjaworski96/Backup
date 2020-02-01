using System;
using System.Collections.Generic;
using FilesystemModel;
using FilesystemModel.Extensions;

namespace BackupCore
{
    public class BackupTarget: IBackup
    {
        private readonly ITargetCommunicator _communicator;
        private readonly ILogger _logger;

        public BackupTarget(ITargetCommunicator communicator,
            ILogger logger)
        {
            _communicator = communicator;
            _logger = logger;
        }

        public void MakeBackup(Directory target)
        {
            Directory source = GetSource();
            MakeBackup(source, target, target.Path);
            _communicator.Finish();
        }
        private Directory GetSource()
        {
            Directory source = _communicator.GetDirectory();
            _logger.Write(source);
            return source;
        }
        private void MakeBackup(Directory source, Directory target, string rootDirectory)
        {
            if(source.Attributes != target.Attributes)
            {
                System.IO.File.SetAttributes(target.Path, source.Attributes);
            }
            source.Compare(target,
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

        private void HandleSameTypes(FileBase inSource, FileBase inTarget, string rootDirectory)
        {
            if(inSource.Type == FileType.DIRECTORY)
                HandleDirectory(inSource, inTarget, rootDirectory);
            else
                HandleFile(inSource, inTarget);
        }

        private void HandleFile(FileBase inSource, FileBase inTarget)
        {
            File sourceFile = inSource as File;
            File targetFile = inTarget as File;
            _logger.Write($"Checking checksum for {sourceFile.Path}");
            if (IsDiffrent(sourceFile.Path, targetFile.CalculateCrc32()))
            {
                _logger.Write($"Downloanding {sourceFile.Path}");
                _communicator.ReceiveFile(sourceFile.Path, inTarget.Path, sourceFile.Attributes);
            }
            if(sourceFile.Attributes != targetFile.Attributes)
            {
                System.IO.File.SetAttributes(inTarget.Path, sourceFile.Attributes);
            }
        }

        private void HandleDirectory(FileBase inSource, FileBase inTarget, string rootDirectory)
        {
            Directory sourceDir = inSource as Directory;
            Directory targetDir = inTarget as Directory;
            MakeBackup(sourceDir, targetDir,
                FileBase.BuildPath(rootDirectory, inSource.Name));
        }

        private void HandleDifrentTypes(FileBase inSource, FileBase inTarget, string rootDirectory)
        {
            HandleDeletedFiles(new List<FileBase> { inTarget }, rootDirectory);
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
