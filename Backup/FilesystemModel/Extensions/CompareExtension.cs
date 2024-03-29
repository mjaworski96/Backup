﻿using System.Collections.Generic;

namespace FilesystemModel.Extensions
{
    public static class CompareExtension
    {
        public static void Compare(this Directory directory,
            Directory toCompare,
            out List<FileBase> newFiles,
            out List<(FileBase current, FileBase inOtherDirectory)> existingFiles,
            out List<FileBase> deletedFiles,
            bool rootDirectory)
        {
            newFiles = new List<FileBase>();
            existingFiles = new List<(FileBase current, FileBase inOtherDirectory)>();
            deletedFiles = new List<FileBase>();
            directory.CompareHelper(toCompare, ref newFiles, ref existingFiles, ref deletedFiles, rootDirectory);
        }
        private static void CompareHelper(this Directory directory,
            Directory toCompare,
            ref List<FileBase> newFiles,
            ref List<(FileBase InFirstDirectory, FileBase InSecondDirectory)> existingFiles,
            ref List<FileBase> deletedFiles,
            bool rootDirectory)
        {
            FindNew(toCompare, directory, ref newFiles, false);
            FindNew(directory, toCompare, ref deletedFiles, rootDirectory);
            FindOld(directory, toCompare, ref existingFiles);
        }
        private static void FindNew(Directory oldDirectory,
            Directory newDirectory,
            ref List<FileBase> newFiles,
            bool ignoreGuardFile)
        {
            var files = ignoreGuardFile ? newDirectory.GetFilesWithoutGuard() : newDirectory.Content;
            foreach (var newItem in files)
            {
                if (Search(oldDirectory, newItem) == null)
                    newFiles.Add(newItem);
            }
        }
        private static void FindOld(Directory firstDirectory,
            Directory secondDirectory,
            ref List<(FileBase InFirstDirectory, FileBase InSecondDirectory)> oldFiles)
        {
            foreach (var first in firstDirectory.Content)
            {
                FileBase second = Search(secondDirectory, first);
                if (second != null)
                    oldFiles.Add((first, second));
            }
        }
        private static FileBase Search(Directory directory, 
            FileBase item)
        {
            foreach (var oldItem in directory.Content)
            {
                if (item.Name == oldItem.Name)
                    return oldItem;
            }

            return null;
        }
    }
}
