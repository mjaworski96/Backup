using BackupCore;
using Communication;
using Communication.Serialization;
using System;
using FilesystemModel;
using System.Collections.Generic;
using System.Linq;
using Common;
using System.Text.RegularExpressions;
using Backup.Translations;
using System.Globalization;
using System.Threading;

namespace Backup
{
    public static class Program
    {
        public static ILogger Logger { get; set; } = new ConsoleLogger();
        public static IDataInput DataInput { get; set; } = new ConsoleDataInput();
        public static void Main(string[] args)
        {
            try
            {
                ParametersHandler parameters = new ParametersHandler(args, Defaults.DEFAULTS_PARAMS);
                using (IBackup backup = GetBackup(parameters))
                {
                    FileFactory.IgnoreRegex = GetIgnoreRegex(parameters).ToList();
                    var directory = GetDirectory(parameters);
                    backup.MakeBackup(directory);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetType().FullName);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        private static string GetMode(ParametersHandler parameters)
        {
            return parameters.GetParameter(Defaults.MODE_KEY, Messages.Mode);
        }
        private static string GetIp(ParametersHandler parameters)
        {
            return parameters.GetParameter(Defaults.ADDRESS_KEY, Messages.Address);
        }
        private static int GetPort(ParametersHandler parameters)
        {
            string port = parameters.GetParameter(Defaults.PORT_KEY, Messages.Port);
            return int.Parse(port);
        }
        private static int GetBufferSize(ParametersHandler parameters)
        {
            string bufferSize = parameters.GetParameter(Defaults.BUFFER_KEY, Messages.BufferSize);
            return Parser.Parse(bufferSize);
        }
        private static string GetDestinationDirectoryPath(ParametersHandler parameters)
        {
            return parameters.GetParameter(Defaults.FILES_KEY, Messages.DestinationDirectory);
        }
        private static IEnumerable<string> GetSourceDirectoryContentPath(ParametersHandler parameters)
        {
            return parameters.GetParameters(Defaults.FILES_KEY, Messages.SourceFiles);
        }
        private static Directory GetDestinationDirectory(ParametersHandler parameters)
        {
            var guard = new DirectoryGuard(Logger, DataInput);
            Directory directory = null;
            do
            {
                if (directory != null)
                {
                    parameters.RemoveSingleParam(Defaults.FILES_KEY);
                }
                directory = new Directory(GetDestinationDirectoryPath(parameters), true);
            } while (!guard.CheckTargetDirectory(directory));
            
            return directory;
        }
        private static IEnumerable<Regex> GetIgnoreRegex(ParametersHandler parameters)
        {
            var patterns = parameters.GetParameters(Defaults.IGNORE_KEY, Messages.IngoreFilesPattern, false);
            return Parser.Parse(patterns);
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
            else if (mode == Defaults.MODE_DESTINATION)
                return GetDestinationDirectory(parameters);
            else
                throw new UnsupportedModeException(mode);
        }

        private static IBackup GetBackup(ParametersHandler parameters)
        {
            var mode = GetMode(parameters);

            if (mode == Defaults.MODE_DESTINATION)
            {
                return new BackupDestination(
                    new DestinationSocketCommunicator(
                        GetIp(parameters),
                        GetPort(parameters),
                        GetBufferSize(parameters),
                        new Json(),
                        Logger),
                    Logger,
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
                        Logger),
                    Logger,
                    GetBufferSize(parameters));
            }
            throw new UnsupportedModeException(mode);
        }
    }
}
