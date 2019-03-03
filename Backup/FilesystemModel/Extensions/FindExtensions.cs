using System;
using System.Linq;

namespace FilesystemModel.Extensions
{
    public static class FindExtensions
    {
        public static FileBase Find(this Directory directory,
            string path)
        {
			if (!path.Contains(directory.Path))
				return null;
            path = GetPath(directory, path);
            string[] subPaths = FileBase.GetSubPaths(path);
            FileBase file = directory;

            for(int i = 0; i < subPaths.Length; i++)
            {
                if (file.Type != FileType.DIRECTORY)
                    break;
                Directory dir = file as Directory;
                file = dir.Content.Where(x => x.Name == subPaths[i]).FirstOrDefault();
                if (file == null)
                    break;
            }
            return file;
        }

        private static string GetPath(Directory directory, string path)
        {
            int legth = directory is VirtualDirectory ? 
                directory.Path.Length : directory.Path.Length + 1;
            return path.Substring(legth);
        }
    }
}
