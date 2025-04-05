using Backup;
using Shouldly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BackupTests
{
    internal class BackupHelper
    {
        static int _nextFreePort = 2000;
        static object _nextFreePortLock = new object();
        private static string[] GetArgs(string src)
        {
            return src.Split(' ')
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray();
        }

        public string Address { get; set; }
        public string BufferSize { get; set; }
        public IEnumerable<string> DestinationIgnorePatterns { get; set; }
        public IEnumerable<string> SourceIgnorePatterns { get; set; }
        public string CompareLargerFilesBySize { get; set; }
        public string ChecksumPartSize { get; set; }

        public NullDataInput DataInput { get; private set; }
        public NullLogger Logger { get; private set; }

        public BackupHelper(string address)
        {
            Address = address;
        }

        private int? _port;
        public int Port
        {
            get
            {
                lock (_nextFreePortLock)
                {
                    if (!_port.HasValue)
                    {
                        _port = _nextFreePort;
                        _nextFreePort++;
                    }
                    return _port.Value;
                }
            }
        }

        public static BackupHelper Default => new BackupHelper("127.0.0.1");
        public static BackupHelper WithLocalhostAddress => new BackupHelper("localhost");

        public async Task CreateBackup(string destination, params string[] source)
        {
            Init();

            await Task.WhenAll(
                CreateDestination(destination),
                CreateSource(source));
        }

        public void Init()
        {
            Program.Logger = Logger = new NullLogger();
            Program.DataInput = DataInput = new NullDataInput();
        }

        public Task CreateSource(params string[] source)
        {
            var src = string.Join(" ", source.Select(x => $"{FileHelpers.TestFilesDirectory}/{x}"));
            var args = GetArgs($"{CreateBaseParametersForSource()} -f {src}");

            return Task.Run(async () => await Program.Main(args));
        }

        public Task CreateDestination(string destination)
        {
            var args = GetArgs($"{CreateBaseParametersForDestination()} -f {FileHelpers.TestFilesDirectory}/{destination}");
            return Task.Run(async () => await Program.Main(args));
        }

        public void ResetErrors()
        {
            Logger.ResetErrors();
        }

        public void AssertDirectoryNotChanged()
        {
            DataInput.Called.ShouldBeFalse();
        }

        public void AssertDirectoryChanged()
        {
            DataInput.Called.ShouldBeTrue();
        }

        public void AssertErrorsCount(int errorsCount)
        {
            Logger.ErrorsCount.ShouldBe(errorsCount);
        }

        private string CreateBaseParameters()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(Address))
                sb.Append($"-a {Address} ");
            else
                sb.Append("-a 127.0.0.1 ");

            sb.Append($"-p {Port} ");

            if (!string.IsNullOrEmpty(BufferSize))
                sb.Append($"-bs {BufferSize} ");

            return sb.ToString();
        }
        private string CreateBaseParametersForSource()
        {
            var sb = new StringBuilder(CreateBaseParameters());

            sb.Append("-m source ");

            if (SourceIgnorePatterns != null)
            {
                var ignore = string.Join(" ", SourceIgnorePatterns);
                sb.Append($"-i {ignore} ");
            }

            return sb.ToString();
        }

        private string CreateBaseParametersForDestination()
        {
            var sb = new StringBuilder(CreateBaseParameters());

            sb.Append("-m destination ");

            if (DestinationIgnorePatterns != null)
            {
                var ignore = string.Join(" ", DestinationIgnorePatterns);
                sb.Append($"-i {ignore} ");
            }

            if (!string.IsNullOrEmpty(CompareLargerFilesBySize))
            {
                sb.Append($"-c {CompareLargerFilesBySize}");
            }

            if (!string.IsNullOrEmpty(ChecksumPartSize))
            {
                sb.Append($"-ch {ChecksumPartSize}");
            }

            return sb.ToString();
        }
    }
}
