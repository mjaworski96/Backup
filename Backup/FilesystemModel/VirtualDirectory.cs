using System.Collections.Generic;

namespace FilesystemModel
{
    public class VirtualDirectory : Directory
    {
        public VirtualDirectory(FileFactory fileFactory) : base(fileFactory, "", false)
        {
            Content = new List<FileBase>();
        }
        public void Add(FileBase fileBase)
        {
            Content.Add(fileBase);
        }
        public void Remove(FileBase fileBase)
        {
            Content.Remove(fileBase);
        }
        public void Save()
        {
            Copy(Path);
        }
        public override void Copy(string destination)
        {
            foreach (var file in Content)
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
