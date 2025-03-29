using Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace BackupTests
{
    internal class NullLogger : ILogger
    {
        private object _lock = new object();

        public int ErrorsCount { get; set; }
        public long MaxProgress { get; set; }

        public void ShowCompleted()
        {
            
        }

        public void UpdateProgressBar(long progressGross)
        {
            
        }

        public void Write(object message)
        {

        }

        public void WriteError(object message)
        {
            lock (_lock)
            {
                ErrorsCount++;
            }
        }

        internal void ResetErrors()
        {
            lock(_lock)
            {
                ErrorsCount = 0;
            }
        }
    }
}
