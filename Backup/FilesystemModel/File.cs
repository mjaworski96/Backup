using Common;
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

        public uint CalculateCrc32(int bufferSize, ILogger logger)
        {
            uint crc32 = 0;
            using (Stream stream =
                new FileStream(Path, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = stream.Length > bufferSize ?
                    new byte[bufferSize] : new byte[stream.Length];
                logger.MaxProgress = stream.Length;
                while (stream.Position != stream.Length)
                {
                    int count = stream.Read(buffer, 0, buffer.Length);
                    logger.UpdateProgressBar(count);
                    crc32 = Crc32CAlgorithm.Append(crc32, buffer, 0, count);
                }
                logger.ResetProgressBar();
            }
            return crc32;
        }

        public override void Copy(string destination)
        {
            System.IO.File.Copy(Path, destination, true);
        }

        public override string ToString(string prefix)
        {
            return base.ToString(prefix);
        }
    }
}
