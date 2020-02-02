using BackupCore;
using Communication;
using Communication.Serialization;
using System;
using FilesystemModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Common;

namespace Backup
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                ParametersHandler parameters = new ParametersHandler(args, Defaults.DEFAULTS_PARAMS);
                using (IBackup backup = GetBackup(parameters))
                {
                    var directory = GetDirectory(parameters);
                    backup.MakeBackup(directory);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        private static string GetMode(ParametersHandler parameters)
        {
            return parameters.GetParameter(Defaults.MODE_KEY, Defaults.MODE_MESSAGE);
        }
        private static string GetIp(ParametersHandler parameters)
        {
            return parameters.GetParameter(Defaults.ADDRESS_KEY, Defaults.ADDRESS_MESSAGE);
        }
        private static int GetPort(ParametersHandler parameters)
        {
            string port = parameters.GetParameter(Defaults.PORT_KEY, Defaults.PORT_MESSAGE);
            return int.Parse(port);
        }
        private static int GetBufferSize(ParametersHandler parameters)
        {
            string bufferSize = parameters.GetParameter(Defaults.BUFFER_KEY, Defaults.BUFFER_SIZE_MESSAGE);
            return Parser.Parse(bufferSize);
        }
        private static string GetTargetDirectoryPath(ParametersHandler parameters)
        {
            return parameters.GetParameter(Defaults.FILES_KEY, Defaults.TARGET_DIRECTORY_MESSAGE);
        }
        private static IEnumerable<string> GetSourceDirectoryContentPath(ParametersHandler parameters)
        {
            return parameters.GetParameters(Defaults.FILES_KEY, Defaults.SOURCE_FILES_MESSAGE);
        }
        private static Directory GetTargetDirectory(ParametersHandler parameters)
        {
            return new Directory(GetTargetDirectoryPath(parameters), true);
        }

        private static Directory GetSourceDirectory(ParametersHandler parameters)
        {
            VirtualDirectory directory = new VirtualDirectory();

            foreach (var item in GetSourceDirectoryContentPath(parameters))
            {
                directory.Add(FileFactory.Create(item, false));
            }

            return directory;
        }
        private static Directory GetDirectory(ParametersHandler parameters)
        {
            var mode = GetMode(parameters);
            if (mode == Defaults.MODE_SOURCE)
                return GetSourceDirectory(parameters);
            else if (mode == Defaults.MODE_TARGET)
                return GetTargetDirectory(parameters);
            else
                throw new UnsupportedModeException(mode);
        }

        private static IBackup GetBackup(ParametersHandler parameters)
        {
            var mode = GetMode(parameters);
            ILogger logger = new ConsoleLogger();

            if (mode == Defaults.MODE_TARGET)
            {
                return new BackupTarget(
                    new TargetSocketCommunicator(
                        GetIp(parameters),
                        GetPort(parameters),
                        GetBufferSize(parameters),
                        new Json(),
                        logger),
                    logger,
                    GetBufferSize(parameters));
            }
            else if (mode == Defaults.MODE_SOURCE)
            {
                return new BackupSource(
                    new SourceSocketCommunicator(
                        GetIp(parameters),
                        GetPort(parameters),
                        GetBufferSize(parameters),
                        new Json(),
                        logger),
                    logger,
                    GetBufferSize(parameters));
            }
            throw new UnsupportedModeException(mode);
        }
    }
}
