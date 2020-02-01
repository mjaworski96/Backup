using BackupCore;
using FilesystemModel;
using System;

namespace Backup
{
    public class ConsoleLogger : ILogger
    {
        private long _currentProgress = 0;
        public long MaxProgress { private get; set; }

        public void Write(string message)
        {
            Console.WriteLine(message);
        }

        public void Write(Directory directory)
        {
            Directory.PREFIX_GROSS = "|   ";
            Console.WriteLine(directory);
        }

        public void UpdateProgress(long progressGross)
        {
            int width = Console.WindowWidth;
            Console.SetCursorPosition(0, Console.CursorTop);
            _currentProgress += progressGross;
            int progressBarWidth = width - 6; //[]xxx%
            double percentProgress = (double)_currentProgress / MaxProgress;
            int progressFilled = (int)(percentProgress * progressBarWidth);

            Console.Write('[');
            WriteCharacter('|', progressFilled);
            WriteCharacter(' ', progressBarWidth - progressFilled);
            Console.Write((int)(100 * percentProgress));
            Console.Write("%]");
        }
        public void ResetProgress()
        {
            _currentProgress = 0;
        }
        private void WriteCharacter(char c, int count)
        {
            if (count > 0)
                Console.Write(new string(c, count));
        }
    }
}
