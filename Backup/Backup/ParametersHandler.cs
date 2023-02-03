using Common;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Backup
{
    public class ParametersHandler
    {
        private Dictionary<string, List<string>> MultiParams { get; set; }
        private Dictionary<string, string> SingleParams { get; set; }
        private IDataInput DataInput { get; set;}
        public ParametersHandler(IDataInput dataInput, string[] args, Dictionary<string, List<string>> defaults)
        {
            DataInput = dataInput;
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

        public string GetParameter(string key, string message, bool mandatory = true)
        {
            var containsKey = SingleParams.TryGetValue(key, out string value);
            if (string.IsNullOrEmpty(value))
            {
                if (mandatory || containsKey)
                {
                    value = DataInput.Get(message);
                    SingleParams.AddOrUpdate(key, value);
                }
                else
                    value = "";
                
            }
            return value;
        }
        public List<string> GetParameters(string key, string consoleMessage, bool mandatory = true)
        {
            var containsKey = MultiParams.TryGetValue(key, out List<string> value);
            if (value == null || !value.Any())
            {
                if (mandatory || containsKey)
                {
                    value = GetMultipleValuesFromConsole(consoleMessage).ToList();
                    MultiParams.AddOrUpdate(key, value);
                }    
                else
                    value = new List<string>();
                
            }
            return value;
        }

        private static IEnumerable<string> GetMultipleValuesFromConsole(string message)
        {
            bool hasValue;
            do
            {
                Console.Write($"{message}: ");
                string value = Console.ReadLine();
                hasValue = value != "";
                if (hasValue)
                    yield return value;
            } while (hasValue);
        }
        public void RemoveSingleParam(string key)
        {
            SingleParams.Remove(key);
        }
        public void RemoveMultiParam(string key)
        {
            MultiParams.Remove(key);
        }
    }
    static class DictionaryHelper
    {
        public static TValue SafeGet<TKey, TValue>(this Dictionary<TKey, TValue> dict,
            TKey key) where TValue: new ()
        {
            if (!dict.ContainsKey(key))
                dict.Add(key, new TValue());
            return dict[key];
        }
        public static Dictionary<TKey, TKey> Convert<TKey, TValue>(
            this Dictionary<TKey, TValue> dict) where TValue: List<TKey>
        {
            var singleData = new Dictionary<TKey, TKey>();

            foreach (var item in dict)
            {
                if (item.Value.Count == 1)
                {
                    singleData.Add(item.Key, item.Value.First());
                }
                else if (item.Value.Count == 0)
                {
                    singleData.Add(item.Key, default(TKey));
                } 
            }

            return singleData;
        }
        public static void AddIfNotContainsKey<TKey, TValue>(this Dictionary<TKey, TValue> dict,
            Dictionary<TKey, TValue> defaultValues)
        {
            foreach (var item in defaultValues)
            {
                if (!dict.ContainsKey(item.Key))
                {
                    dict.Add(item.Key, item.Value);
                }
            }
        }
        public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dict,
            TKey key, TValue value)
        {
            if (!dict.ContainsKey(key))
                dict.Add(key, value);
            else
                dict[key] = value;
        }
    }
}
