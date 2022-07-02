using Common;
using System.Collections.Generic;
using System.Linq;

namespace FilesystemModel.Extensions
{
    public static class DirectoryGuardExtensions
    {
        public static List<FileBase> GetFilesWithoutGuard(this Directory directory)
        {
            return directory.Content
                .Where(x => x.Type != FileType.FILE && x.Name != Consts.BackupDirectoryGuardFilePath)
                .ToList();
        }
    }
}
