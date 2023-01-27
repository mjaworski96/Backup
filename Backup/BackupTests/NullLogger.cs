using Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace BackupTests
{
    internal class NullLogger : ILogger
    {
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
            
        }
    }
}
