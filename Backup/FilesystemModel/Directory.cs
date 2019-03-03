using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FilesystemModel
{
    public class Directory : FileBase
    {
        protected List<FileBase> content;

        public Directory(string path, bool createDirectoryIfNotExists) : base(path)
        {
            content = GetDirectoryContent(createDirectoryIfNotExists).ToList();
        }
        public override string ToString(string prefix)
        {
            StringBuilder stringBuilder = new StringBuilder(base.ToString(prefix));

            foreach (var file in content)
            {
                stringBuilder.Append('\n');
                stringBuilder.Append(file.ToString(prefix + "---"));
            }

            return stringBuilder.ToString();
        }
        protected IEnumerable<FileBase> GetDirectoryContent(bool createDirectoryIfNotExists)
        {
            if (createDirectoryIfNotExists && !System.IO.Directory.Exists(Path))
				System.IO.Directory.CreateDirectory(Path);
			
            if(System.IO.Directory.Exists(Path))
            {
                string[] files = System.IO.Directory.GetFileSystemEntries(Path, "*", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    yield return FileFactory.Create(file, false);
                }
            }
        }

        public override void Copy(string target)
        {
            System.IO.Directory.CreateDirectory(target);
            foreach (var file in content)
            {
                file.Copy(
                    BuildPath(target, file.Name)
                        );
            }
        }

        public override FileType Type => FileType.DIRECTORY;

        public List<FileBase> Content { get => content; set => content = value; }
    }
}
