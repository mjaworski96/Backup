using System.Collections.Generic;

namespace FilesystemModel
{
    public class VirtualDirectory : Directory
    {
        public VirtualDirectory() : base("", false)
        {
            content = new List<FileBase>();
        }
        public void Add(FileBase fileBase)
        {
            content.Add(fileBase);
        }
        public void Remove(FileBase fileBase)
        {
            content.Remove(fileBase);
        }
        public void Save()
        {
            Copy(Path);
        }
        public override void Copy(string target)
        {
            foreach (var file in content)
            {
                file.Copy(BuildPath(target, file.Name));
            }
        }
    }
}
