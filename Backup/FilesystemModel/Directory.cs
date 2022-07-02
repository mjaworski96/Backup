using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FilesystemModel
{
    public class Directory : FileBase
    {
        public static string PREFIX_GROSS = "";
        protected List<FileBase> content;

        public Directory() { }

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
                stringBuilder.Append(file.ToString(prefix + PREFIX_GROSS));
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
                    if(!FileFactory.MustBeIgnored(file))
                        yield return FileFactory.Create(file, false);
                }
            }
        }

        public override void Copy(string destination)
        {
            System.IO.Directory.CreateDirectory(destination);
            foreach (var file in content)
            {
                file.Copy(
                    BuildPath(destination, file.Name));
            }
        }

        public override FileType Type => FileType.DIRECTORY;

        public List<FileBase> Content { get => content; set => content = value; }

        public bool Empty => !Content.Any();

        public virtual void Refresh()
        {
            content = GetDirectoryContent(false).ToList();
        }
    }
}
