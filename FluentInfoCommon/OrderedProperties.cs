using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentInfoCommon
{
    public class OrderedProperties
    {
        private readonly List<(string, string)> storage = [];
        private readonly Dictionary<string, string> dictionary = [];

        public int Count { get { return storage.Count; } }

        public void Add(string key, string value)
        {
            storage.Add((key, value));
            dictionary[key] = value;
        }

        public string Get(string key)
        {
            if (dictionary.TryGetValue(key, out var value)) return value;
            return null;
        }

        public List<(string, string)> GetPairs()
        {
            return storage;
        }
    }
}
