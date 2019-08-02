using System.Linq;

namespace FilesystemModel
{
    public abstract class FileBase
    {
        public string Path { get; private set; }
        public string Name { get => Path.Split('/').Last(); }
        public FileBase(string path)
        {
            if (string.IsNullOrEmpty(path))
                Path = path;
            else
                Path = System.IO.Path.GetFullPath(
                path.Replace('\\', '/'))
                .Replace('\\', '/');
        }
        public abstract FileType Type { get; }
        public abstract void Copy(string target);
        public virtual string ToString(string prefix)
        {
            return prefix + Name;
        }
        public override string ToString()
        {
            return ToString("");
        }

        public static string BuildPath(params string[] subPaths)
        {
            return string.Join("/", subPaths);
        }
        public static string[] GetSubPaths(string path)
        {
            return path.Split('/');
        }
    }
}
