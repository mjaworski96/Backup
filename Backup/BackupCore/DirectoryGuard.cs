using Common;
using Common.Translations;
using FilesystemModel;
using FilesystemModel.Extensions;
using System.Linq;

namespace BackupCore
{
    public class DirectoryGuard
    {
        private readonly ILogger _logger;
        private readonly IDataInput _dataInput;

        public DirectoryGuard(ILogger logger, IDataInput dataInput)
        {
            _logger = logger;
            _dataInput = dataInput;
        }

        public bool CheckTargetDirectory(Directory directory)
        {
            if (directory.Empty)
            {
                return true;
            }
            var quardFilePath = FileBase.BuildPath(directory.Path, Consts.BackupDirectoryGuardFilePath);
            var guardFile = directory.Find(quardFilePath);
            if (guardFile != null)
            {
                var guardFileContent = System.IO.File.ReadAllText(quardFilePath);
                if (guardFileContent != directory.ToString())
                {
                    return AskUser(directory, LoggerMessages.SelectedDirectoryWasModified, LoggerMessages.UseModifiedDirectory);
                }
                return true;
            }
            return AskUser(directory, LoggerMessages.SelectedDirectoryIsNotBackupDirectory, LoggerMessages.UseNewDirectory);
        }

        private bool AskUser(Directory directory, string errorMessage, string question)
        {
            _logger.WriteError(string.Format(errorMessage, directory.Path));
            var decision = _dataInput.Get(question);
            return string.Compare(decision, DataInputs.Yes, true) == 0 ||
                string.Compare(decision, DataInputs.YesSingleChar, true) == 0;
        }
    }
}
