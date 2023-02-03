using System.Collections.Generic;

namespace FilesystemModel
{
    public class VirtualDirectory : Directory
    {
        public VirtualDirectory(FileFactory fileFactory) : base(fileFactory, "", false)
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
        public override void Copy(string destination)
        {
            foreach (var file in content)
            {
                file.Copy(BuildPath(destination, file.Name));
            }
        }
        public override void Refresh()
        {
            //Do nothing
        }
    }
}
