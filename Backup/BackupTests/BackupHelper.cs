using Backup;
using Shouldly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupTests
{
    internal class BackupHelper
    {
        static int _nextFreePort = 2000;
        static object _nextFreePortLock = new object();
        static int GetPort()
        {
            lock (_nextFreePortLock)
            {
                var port = _nextFreePort;
                _nextFreePort++;
                return port;
            }
        }
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

        public NullDataInput DataInput { get; private set; }

        public BackupHelper(string address, string bufferSize, IEnumerable<string> destinationIgnorePatterns, IEnumerable<string> sourceIgnorePatterns)
        {
            Address = address;
            BufferSize = bufferSize;
            DestinationIgnorePatterns = destinationIgnorePatterns;
            SourceIgnorePatterns = sourceIgnorePatterns;
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

        public static BackupHelper Standard => new BackupHelper("127.0.0.1", null, null, null);

        public async Task CreateBackup(string destination, params string[] source)
        {
            Program.Logger = new NullLogger();
            Program.DataInput = DataInput = new NullDataInput();
            var port = GetPort();
            var destinationTask = Task.Run(() =>
            {
                Program.Main(GetArgs($"{CreateBaseParametersForDestination()} -f {FileHelpers.TestFilesDirectory}/{destination}"));
            });

            var sourceTask = Task.Run(() =>
            {
                var src = string.Join(" ", source.Select(x => $"{FileHelpers.TestFilesDirectory}/{x}"));
                Program.Main(GetArgs($"{CreateBaseParametersForSource()} -f {src}"));
            });

            await destinationTask;
            await sourceTask;
        }

        public void AssertDirectoryNotChanged()
        {
            DataInput.Called.ShouldBeFalse();
        }

        public void AssertDirectoryChanged()
        {
            DataInput.Called.ShouldBeTrue();
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

            return sb.ToString();
        }
    }
}
