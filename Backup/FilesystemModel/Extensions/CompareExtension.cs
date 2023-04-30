using System.Collections.Generic;
using System.Linq;

namespace FilesystemModel.Extensions
{
    public static class CompareExtension
    {
        private class SearchableFile
        {
            public bool Found { get; set; }
            public FileBase FileBase { get; set; }

            public SearchableFile(bool found, FileBase fileBase)
            {
                Found = found;
                FileBase = fileBase;
            }
        }
        private class SearchableFileComparer : IComparer<SearchableFile>
        {
            public int Compare(SearchableFile x, SearchableFile y)
            {
                return x.FileBase.Name.CompareTo(y.FileBase.Name);
            }
        }
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
            Split(directory, toCompare, rootDirectory, ref newFiles, ref existingFiles, ref deletedFiles);
        }
        private static void Split(Directory firstDirectory, Directory secondDirectory,
            bool ignoreGuardFile,
            ref List<FileBase> newFiles,
            ref List<(FileBase InFirstDirectory, FileBase InSecondDirectory)> existingFiles,
            ref List<FileBase> deletedFiles)
        {
            var firstDirectoryContent = firstDirectory.Content.PrepareFilesToSearch();
            var secondDirectoryContent = (ignoreGuardFile ? secondDirectory.GetFilesWithoutGuard() : secondDirectory.Content).PrepareFilesToSearch();
            var comparer = new SearchableFileComparer();
            for (int i = 0; i < firstDirectoryContent.Count; i++)
            {
                var firstFile = firstDirectoryContent[i];
                var secondFileIndex = secondDirectoryContent.BinarySearch(firstFile, comparer);
                if (secondFileIndex >= 0)
                {
                    var secondFile = secondDirectoryContent[secondFileIndex];
                    existingFiles.Add((firstFile.FileBase, secondFile.FileBase));
                    firstFile.Found = true;
                    secondFile.Found = true;
                }
            }
            newFiles = firstDirectoryContent.Where(x => !x.Found).Select(x => x.FileBase).ToList();
            deletedFiles = secondDirectoryContent.Where(x => !x.Found).Select(x => x.FileBase).ToList();
        }
        private static List<SearchableFile> PrepareFilesToSearch(this IEnumerable<FileBase> source)
        {
            return source.OrderBy(x => x.Name).Select(x => new SearchableFile(false, x)).ToList();
        }
    }
}
