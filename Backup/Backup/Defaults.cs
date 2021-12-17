using System.Collections.Generic;

namespace Backup
{
    public static class Defaults
    {
        
        public const string MODE_KEY = "-m";
        public const string ADDRESS_KEY = "-a";
        public const string PORT_KEY = "-p";
        public const string BUFFER_KEY = "-bs";
        public const string FILES_KEY = "-f";
        public const string IGNORE_KEY = "-i";

        public const string MODE_DESTINATION = "destination";
        public const string MODE_SOURCE = "source";
        

        public static Dictionary<string, List<string>> DEFAULTS_PARAMS =>
            new Dictionary<string, List<string>>
            {
                { ADDRESS_KEY, new List<string>() { "127.0.0.1" } },
                { PORT_KEY, new List<string>() { "7000" } },
                { BUFFER_KEY, new List<string>() { "10M" } },
            };                
    }
}
