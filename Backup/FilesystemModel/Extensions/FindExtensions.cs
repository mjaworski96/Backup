using System.Linq;

namespace FilesystemModel.Extensions
{
    public static class FindExtensions
    {
        public static FileBase Find(this Directory directory,
            string path)
        {
            FileBase file = TryGetFile(directory, path);
            if (file != null)
                return file;

            directory = CheckDirectory(directory, path);
            if (!path.StartsWith(directory.Path))
                return null;
            path = GetPath(directory, path);
            string[] subPaths = FileBase.GetSubPaths(path);
            file = directory;

            for (int i = 0; i < subPaths.Length; i++)
            {
                if (file.Type != FileType.DIRECTORY)
                    return null;
                Directory dir = file as Directory;
                file = dir.Content.Where(x => x.Name == subPaths[i]).FirstOrDefault();
                if (file == null)
                    break;
            }
            return file;
        }
        private static FileBase TryGetFile(Directory directory, string path)
        {
            return directory.Content
                 .FirstOrDefault(x => x.Type == FileType.FILE && x.Path == path);
        }
        private static string GetPath(Directory directory, string path)
        {
            int legth = directory is VirtualDirectory ?
                directory.Path.Length : directory.Path.Length + 1;
            return path.Substring(legth);
        }
        private static Directory CheckDirectory(Directory directory, string path)
        {
            if (directory is VirtualDirectory)
            {
                Directory valid = directory.Content
                    .Where(x => x.Type == FileType.DIRECTORY &&
                     path.StartsWith(x.Path))
                    .OrderBy(x => x.Path.Length)
                    .FirstOrDefault() as Directory;
                if (valid != null)
                    return valid;
            }
            return directory;
        }
    }
}
