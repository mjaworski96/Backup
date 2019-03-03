using System;
using System.Collections.Generic;
using FilesystemModel;
using FilesystemModel.Extensions;

namespace BackupLogic
{
    public class BackupTarget: IBackup
    {
        private readonly ITargetCommunicator _communicator;

        public BackupTarget(ITargetCommunicator communicator)
        {
            _communicator = communicator;
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
            Console.WriteLine(source);
            return source;
        }
        private void MakeBackup(Directory source, Directory target, string rootDirectory)
        {
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
                    HandleNewFiles(directory.Content, path);
                }
                else if (item.Type == FileType.FILE)
                {
                    Console.WriteLine($"Downloanding {item.Path}");
                    _communicator.ReceiveFile(item.Path, path);
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
                    System.IO.Directory.Delete(path);
                }
                else if (item.Type == FileType.FILE)
                {
                    Console.WriteLine($"Deleting {path}");
                    System.IO.File.Delete(path);
                }
            }
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
            Console.WriteLine($"Checking checksum for {sourceFile.Path}");
            if (IsDiffrent(sourceFile.Path, targetFile.CalculateCrc32()))
            {
                Console.WriteLine($"Downloanding {sourceFile.Path}");
                _communicator.ReceiveFile(sourceFile.Path, inTarget.Path);
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
