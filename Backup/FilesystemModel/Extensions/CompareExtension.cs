using FilesystemModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FilesystemModel.Extensions
{
    public static class CompareExtension
    {
        public static void Compare(this Directory directory,
            Directory toCompare,
            out List<FileBase> newFiles,
            out List<(FileBase current, FileBase inOtherDirectory)> existingFiles,
            out List<FileBase> deletedFiles)
        {
            newFiles = new List<FileBase>();
            existingFiles = new List<(FileBase current, FileBase inOtherDirectory)>();
            deletedFiles = new List<FileBase>();
            directory.CompareHelper(toCompare, ref newFiles, ref existingFiles, ref deletedFiles);
        }
        private static void CompareHelper(this Directory directory,
            Directory toCompare,
            ref List<FileBase> newFiles,
            ref List<(FileBase InFirstDirectory, FileBase InSecondDirectory)> existingFiles,
            ref List<FileBase> deletedFiles)
        {
            FindNew(toCompare, directory, ref newFiles);
            FindNew(directory, toCompare, ref deletedFiles);
            FindOld(directory, toCompare, ref existingFiles);
        }
        private static void FindNew(Directory oldDirectory,
            Directory newDirectory,
            ref List<FileBase> newFiles)
        {
            foreach (var newItem in newDirectory.Content)
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
                {
                    return oldItem;
                }
            }

            return null;
        }
    }
}
