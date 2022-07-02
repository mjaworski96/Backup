using Common;
using System;

namespace Backup
{
    public class ConsoleInput : IDataInput
    {
        public string Get(string message)
        {
            Console.Write(message);
            return Console.ReadLine();
        }
    }
}
