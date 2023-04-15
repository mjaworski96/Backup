using Common;
using System;

namespace Backup
{
    public class ConsoleDataInput : IDataInput
    {
        public string Get(string message)
        {
            Console.Write($"{message}: ");
            return Console.ReadLine();
        }
    }
}
