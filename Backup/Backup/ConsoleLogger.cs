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
        private long _maxProgress = 0;
        public long MaxProgress 
        { 
            private get
            {
                return _maxProgress;
            }
            set
            {
                _maxProgress = value;
                _currentProgress = 0;
                DrawProgress();
            }
        }

        public ConsoleLogger()
        {
            Directory.PREFIX_GROSS = "|   ";
        }

        public void Write(object message)
        {
            Console.WriteLine(message);
        }

        public void WriteError(object message)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = color;
        }

        public void UpdateProgressBar(long progressGross)
        {
            _currentProgress += progressGross;
            DrawProgress();
        }

        private void DrawProgress()
        {
            var width = Console.WindowWidth;
            Console.SetCursorPosition(0, Console.CursorTop);
            var percentProgress = (double)_currentProgress / MaxProgress;
            var percentText = ((int)(100 * percentProgress)).ToString();
            var progressBarWidth = width - 3 - percentText.Length; //[]xxx%
            var progressFilled = (int)(percentProgress * progressBarWidth);

            Console.Write('[');
            WriteCharacter(PROGRESS_BAR_FULL, progressFilled);
            WriteCharacter(PROGRESS_BAR_EMPTY, progressBarWidth - progressFilled);
            Console.Write(percentText);
            Console.Write("%]");
        }

        private void WriteCharacter(char c, int count)
        {
            if (count > 0)
                Console.Write(new string(c, count));
        }

        public void ShowCompleted()
        {
            _maxProgress = 1;
            _currentProgress = 1;
            DrawProgress();
        }
    }
}
