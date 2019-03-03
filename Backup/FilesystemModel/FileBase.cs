using System.Linq;

namespace FilesystemModel
{
    public abstract class FileBase
    {
        public string Path { get; set; }
        public string Name { get => Path.Split('/').Last(); }
        public FileBase(string path)
        {
            Path = path.Replace('\\', '/');
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
