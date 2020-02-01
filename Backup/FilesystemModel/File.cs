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

        public uint CalculateCrc32(int bufferSize)
        {
            uint crc32 = 0;
            byte[] buffer = new byte[bufferSize];

            using (Stream stream =
                new FileStream(Path, FileMode.Open, FileAccess.Read))
            {
                while (stream.Position != stream.Length)
                {
                    int count = stream.Read(buffer, 0, bufferSize);
                    Crc32CAlgorithm.Append(crc32, buffer, 0, count);
                }
            }
            return crc32;
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
