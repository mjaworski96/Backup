using BackupCore;
using FilesystemModel;
using System;

namespace Backup
{
    public class ConsoleLogger : ILogger
    {
        private long _currentProgress = 0;
        private int? _progressBarLine = null;
        public long MaxProgress { private get; set; }

        public void UpdateProgress(long progressGross)
        {
            if (_progressBarLine.HasValue)
                ClearConsoleTo(_progressBarLine.Value);
            else
            {
                _progressBarLine = Console.CursorTop;
                _currentProgress = 0;
            }
            _currentProgress += progressGross;

            int width = Console.WindowWidth;
            int progressBarWidth = width - 6; //[]xxx%
            double percentProgress = (double)_currentProgress / MaxProgress;
            int progressFilled = (int)(percentProgress * progressBarWidth);
            Console.Write('[');
            Console.Write(new string('|', progressFilled));
            Console.Write(new string(' ', progressBarWidth - progressFilled));
            Console.Write((int)(100 * percentProgress));
            Console.WriteLine("%]");
        }

        private void ClearConsoleTo(int line)
        {
            for (int i = Console.CursorTop; i >= line; i--)
            {
                ClearConsoleLine(i);
            }
        }

        public void Write(string message)
        {
            _progressBarLine = null;
            Console.WriteLine(message);
        }
        public void Write(Directory directory)
        {
            Directory.PREFIX_GROSS = "|   ";
            Console.WriteLine(directory.ToString());
        }
        void ClearConsoleLine(int line)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, line);
        }
    }
}
