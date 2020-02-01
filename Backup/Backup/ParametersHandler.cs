using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Backup
{
    public class ParametersHandler
    {
        private Dictionary<string, List<string>> MultiParams { get; set; }
        private Dictionary<string, string> SingleParams { get; set; }

        public ParametersHandler(string[] args, Dictionary<string, List<string>> defaults)
        {
            var multi = new Dictionary<string, List<string>>();
            List<string> currentParameter = multi.SafeGet("");
            foreach (var item in args)
            {
                if (item.StartsWith("-"))
                    currentParameter = multi.SafeGet(item);
                else
                    currentParameter.Add(item);
            }
            multi.AddIfNotContainsKey(defaults);
            MultiParams = multi;
            SingleParams = multi.Convert();
        }

        public string GetParameter(string key, string consoleMessage)
        {
            SingleParams.TryGetValue(key, out string value);
            if (string.IsNullOrEmpty(value))
            {
                value = GetFromConsole(consoleMessage);
                SingleParams.Add(key, value);
            }
            return value;
        }
        public List<string> GetParameters(string key, string consoleMessage)
        {
            MultiParams.TryGetValue(key, out List<string> value);
            if (value == null || !value.Any())
            {
                value = GetMultipleValuesFromConsole(consoleMessage).ToList();
                MultiParams.Add(key, value);
            }
            return value;
        }

        private static string GetFromConsole(string message)
        {
            Console.Write($"{message}: ");
            return Console.ReadLine();
        }

        private static IEnumerable<string> GetMultipleValuesFromConsole(string message)
        {
            bool hasValue = false;
            do
            {
                Console.Write($"{message}: ");
                string value = Console.ReadLine();
                hasValue = value != "";
                if (hasValue)
                    yield return value;
            } while (hasValue);
        }
    }
    static class DictionaryHelper
    {
        public static List<string> SafeGet(this Dictionary<string, List<string>> dict,
            string key)
        {
            if (!dict.ContainsKey(key))
                dict.Add(key, new List<string>());
            return dict[key];
        }
        public static Dictionary<string, string> Convert(this Dictionary<string, List<string>> dict)
        {
            var singleData = new Dictionary<string, string>();

            foreach (var item in dict)
            {
                if(item.Value.Count == 1)
                {
                    singleData.Add(item.Key, item.Value.First());
                }
            }

            return singleData;
        }
        public static void AddIfNotContainsKey(this Dictionary<string, List<string>> dict,
            Dictionary<string, List<string>> defaultValues)
        {
            foreach (var item in defaultValues)
            {
                if (!dict.ContainsKey(item.Key))
                {
                    dict.Add(item.Key, item.Value);
                }
            }
        }
    }
}
