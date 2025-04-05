using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FilesystemModel
{
    public class Directory : FileBase
    {
        public static string PREFIX_GROSS = "";

        private readonly FileFactory _fileFactory;
        public List<FileBase> Content { get; set; }

        public Directory() { }

        public Directory(FileFactory fileFactory, string path, bool createDirectoryIfNotExists) : base(path)
        {
            _fileFactory = fileFactory;
            Content = GetDirectoryContent(createDirectoryIfNotExists).ToList();
        }
        public override string ToString(string prefix)
        {
            var stringBuilder = new StringBuilder(base.ToString(prefix));

            foreach (var file in Content)
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
                var files = System.IO.Directory.GetFileSystemEntries(Path, "*", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    if(!_fileFactory.MustBeIgnored(file))
                        yield return _fileFactory.Create(file, false);
                }
            }
        }

        public override void Copy(string destination)
        {
            System.IO.Directory.CreateDirectory(destination);
            foreach (var file in Content)
            {
                file.Copy(
                    BuildPath(destination, file.Name));
            }
        }

        public override FileType Type => FileType.DIRECTORY;


        public bool Empty => !Content.Any();

        public virtual void Refresh()
        {
            Content = GetDirectoryContent(false).ToList();
        }
    }
}
