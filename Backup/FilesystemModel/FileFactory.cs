using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FilesystemModel
{
    public static class FileFactory
    {
        public static List<Regex> IgnoreRegex;

        public static bool MustBeIgnored(string filename)
        {
            filename = filename.Replace('\\', '/');
            if (IgnoreRegex != null)
            {
                foreach (var regex in IgnoreRegex)
                {
                    if (regex.IsMatch(filename))
                        return true;
                }
            }
            return false;
        }

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
