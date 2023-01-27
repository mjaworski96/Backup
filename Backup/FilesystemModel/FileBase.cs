using System.Linq;

namespace FilesystemModel
{
    public abstract class FileBase
    {
        public string Path { get; private set; }
        public string Name { get => Alias ?? Path.Split('/').Last(); }
        public string Alias { get; private set; }
        public System.IO.FileAttributes Attributes { get; private set; }
        
        public FileBase() { }
        
        public FileBase(string path)
        {
            if(path.Contains("*"))
            {
                var split = path.Split('*');
                path = split.First();
                Alias = split.Last();
            }
            if (string.IsNullOrEmpty(path))
                Path = path;
            else
            {
                path = System.IO.Path.GetFullPath(
                    path.Replace('\\', '/'))
                .Replace('\\', '/');
                if (path.EndsWith("/")) 
                    path = path.Substring(0, path.Length- 1);
                Path = path;
                if(System.IO.File.Exists(Path) || System.IO.Directory.Exists(Path))
                    Attributes = System.IO.File.GetAttributes(Path);
            }             
        }
        public abstract FileType Type { get; }
        public abstract void Copy(string destination);
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
