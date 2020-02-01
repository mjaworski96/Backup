using System;
using System.Collections.Generic;
using System.Text;

namespace Backup
{
    public class Parser
    {
        private const int KILO = 1024;
        private const int MEGA = 1024 * 1024;
        private const int GIGA = 1024 * 1024 * 1024;
        public static int Parse(string size)
        {
            if (CanBeParsed(size, "k"))
                return Parse(size, KILO);
            if (CanBeParsed(size, "m"))
                return Parse(size, MEGA);
            if (CanBeParsed(size, "g"))
                return Parse(size, GIGA);

            return int.Parse(size);
        }
        private static bool CanBeParsed(string size, string prefix)
        {
            return size.ToLower().Contains(prefix);
        }
        private static int Parse(string str, int multiplier)
        {
            string rawSize = str.Substring(0, str.Length - 1);
            return int.Parse(rawSize) * multiplier;
        }
    }
}
