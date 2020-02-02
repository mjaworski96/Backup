using Common;
using FilesystemModel;
using System;

namespace Backup
{
    public class ConsoleLogger : ILogger
    {
        private const char PROGRESS_BAR_FULL = '|';
        private const char PROGRESS_BAR_EMPTY = ' ';

        private long _currentProgress = 0;
        public long MaxProgress { private get; set; }

        public ConsoleLogger()
        {
            Directory.PREFIX_GROSS = "|   ";
        }

        public void Write(object message)
        {
            Console.WriteLine(message);
        }

        public void UpdateProgressBar(long progressGross)
        {
            int width = Console.WindowWidth;
            Console.SetCursorPosition(0, Console.CursorTop);
            _currentProgress += progressGross;
            int progressBarWidth = width - 6; //[]xxx%
            double percentProgress = (double)_currentProgress / MaxProgress;
            int progressFilled = (int)(percentProgress * progressBarWidth);

            Console.Write('[');
            WriteCharacter(PROGRESS_BAR_FULL, progressFilled);
            WriteCharacter(PROGRESS_BAR_EMPTY, progressBarWidth - progressFilled);
            Console.Write((int)(100 * percentProgress));
            Console.Write("%]");
        }
        public void ResetProgressBar()
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
