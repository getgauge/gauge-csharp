using System.Collections.Generic;
using System.ComponentModel;

namespace Gauge.CSharp.Runner
{
    public class DataStore
    {
        private static Dictionary<string, object> _dictionary;

        public static void Initialize()
        {
            _dictionary = new Dictionary<string, object>();
        }

        public static object Get(string key)
        {
            object outVal;
            var valueExists = _dictionary.TryGetValue(key, out outVal);
            return valueExists ? outVal : null;
        }

        public static void Add(string key, object value)
        {
            _dictionary[key] = value;
        }

        public static void Clear()
        {
            _dictionary.Clear();
        }

        public static int Count
        {
            get
            {
                return _dictionary.Count;
            }
        }
    }
}