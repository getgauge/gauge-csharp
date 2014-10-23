using System.Collections.Generic;

namespace Gauge.CSharp.Runner
{
    public class DataStore
    {
        private Dictionary<string, object> _dictionary;

        public void Initialize()
        {
            _dictionary = new Dictionary<string, object>();
        }

        public object Get(string key)
        {
            object outVal;
            var valueExists = _dictionary.TryGetValue(key, out outVal);
            return valueExists ? outVal : null;
        }

        public void Add(string key, object value)
        {
            _dictionary[key] = value;
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public int Count
        {
            get
            {
                return _dictionary.Count;
            }
        }
    }
}