using BackupLogic;
using Communication;
using Communication.Serialization;
using System;
using FilesystemModel;
using System.Collections.Generic;

namespace Backup
{
    public class Program
    {
        private const int MODE_INDEX = 0;
        private const int IP_INDEX = 1;
        private const int PORT_INDEX = 2;
        private const int BUFFER_SIZE_INDEX = 3;
        private const int FILES_INDEX = 4;

        private bool GetFromArgs(string[] args, int index, out string value)
        {
            if (args.Length > index)
            {
                value = args[index];
                return true;
            }
            value = null;
            return false;
        }
        private bool GetFromArgs(string[] args, int startIndex, out string[] values)
        {
            if (args.Length > startIndex)
            {
                values = new string[args.Length - startIndex];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = args[startIndex + i];
                }
                return true;
            }
            values = null;
            return false;
        }
        private string GetFromConsole(string message)
        {
            Console.Write($"{message}: ");
            return Console.ReadLine();
        }
        private IEnumerable<string> GetMultipleValuesFromConsole(string message)
        {
            bool hasValue = false;
            do
            {
                Console.Write($"{message}: ");
                string value = Console.ReadLine();
                hasValue = value != "";
                if (hasValue)
                    yield return value;
            } while (hasValue);
        }
        private string GetMode(string[] args)
        {
            if (GetFromArgs(args, MODE_INDEX, out string value))
                return value;
            else
                return GetFromConsole("Mode (source/target)");
        }
        private string GetIp(string[] args)
        {
            if (GetFromArgs(args, IP_INDEX, out string value))
                return value;
            else
                return GetFromConsole("Address IP");
        }
        private int GetPort(string[] args)
        {
            string port = "";
            if (GetFromArgs(args, PORT_INDEX, out string value))
                port =  value;
            else
                port = GetFromConsole("Port");

            return int.Parse(port);
        }
        private int GetBufferSize(string[] args)
        {
            string bufferSize = "";
            if (GetFromArgs(args, BUFFER_SIZE_INDEX, out string value))
                bufferSize = value;
            else
                bufferSize = GetFromConsole("Buffer size");

            return int.Parse(bufferSize);
        }
        private string GetTargetDirectoryPath(string[] args)
        {
            if (GetFromArgs(args, FILES_INDEX, out string value))
                return value;
            else
                return GetFromConsole("Target directory");
        }
        private IEnumerable<string> GetSourceDirectoryContentPath(string[] args)
        {
            if (GetFromArgs(args, FILES_INDEX, out string[] values))
                return values;
            else
                return GetMultipleValuesFromConsole("File/directory (empty line to exit)");
        }
        private Directory GetTargetDirectory(string[] args)
        {
            return new Directory(GetTargetDirectoryPath(args), true);
        }

        private Directory GetSourceDirectory(string[] args)
        {
            VirtualDirectory directory = new VirtualDirectory();

            foreach (var item in GetSourceDirectoryContentPath(args))
            {
                directory.Add(FileFactory.Create(item, false));
            }

            return directory;
        }
        private Directory GetDirectory(string[] args, string mode)
        {
            if (mode == "source")
                return GetSourceDirectory(args);
            else if (mode == "target")
                return GetTargetDirectory(args);
            else
                throw new UnsupportedModeException(mode);
        }

        private IBackup GetBackup(string[] args, string mode)
        {
            if (mode == "target")
            {
                return new BackupTarget(
                    new TargetSocketCommunicator(
                        GetIp(args),
                        GetPort(args),
                        GetBufferSize(args),
                        new Json()));
            }
            else if (mode == "source")
            {
                return new BackupSource(
                    new SourceSocketCommunicator(
                        GetIp(args),
                        GetPort(args),
                        GetBufferSize(args),
                        new Json()));
            }
            throw new UnsupportedModeException(mode);
        }
        public void Main(string[] args)
        {
            try
            {
                string mode = GetMode(args);
                using (IBackup backup = GetBackup(args, mode))
                {
                    backup.MakeBackup(GetDirectory(args, mode));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
