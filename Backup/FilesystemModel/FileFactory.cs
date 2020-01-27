using System.IO;
using System.Linq;

namespace FilesystemModel
{
    public static class FileFactory
    {
        public static FileBase Create(string filename, bool createDirectoryIfNotExists)
        {
            FileAttributes attributes = System.IO.File.GetAttributes(filename.Split('*').First());
            if(attributes.HasFlag(FileAttributes.Directory))
            {
                return new Directory(filename, createDirectoryIfNotExists);
            }
            else
            {
                return new File(filename);
            }
        }
    }
}
