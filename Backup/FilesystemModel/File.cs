using Force.Crc32;
using System.IO;

namespace FilesystemModel
{
    public class File : FileBase
    {
        public File(string path) : base(path)
        {
        }

        public override FileType Type => FileType.FILE;

        public uint CalculateCrc32()
        {
            return Crc32CAlgorithm.Compute(System.IO.File.ReadAllBytes(Path));
        }

        public override void Copy(string target)
        {
            System.IO.File.Copy(Path, target, true);
        }

        public override string ToString(string prefix)
        {
            return base.ToString(prefix);
        }
    }
}
