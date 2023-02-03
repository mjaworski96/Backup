using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FilesystemModel
{
    public class FileFactory
    {
        public List<Regex> IgnoreRegex;

        public FileFactory(List<Regex> ignoreRegex)
        {
            IgnoreRegex = ignoreRegex;
        }

        public bool MustBeIgnored(string filename)
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

        public FileBase Create(string filename, bool createDirectoryIfNotExists)
        {
            var attributes = System.IO.File.GetAttributes(filename.Split('*').First());
            if(attributes.HasFlag(FileAttributes.Directory))
            {
                return new Directory(this, filename, createDirectoryIfNotExists);
            }
            else
            {
                return new File(filename);
            }
        }
    }
}
