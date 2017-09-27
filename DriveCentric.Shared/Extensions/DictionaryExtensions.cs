using System.Collections.Generic;

namespace DriveCentric.Shared.Extensions
{
    public static class DictionaryExtensions
    {

        /// <summary>
        /// Look up a key in a dictionary and return its value, or the value type's default value if the
        /// key is not in the dictionary.
        /// </summary>
        public static V GetValueOrDefault<K, V>(this IDictionary<K, V> dict, K key)
        {
            V val;
            if (!dict.TryGetValue(key, out val))
            {
                val = default(V);
            }
            return val;
        }

        /// <summary>
        /// Look up a key in a dictionary and return its value, or a specified default value if the key
        /// is not in the dictionary.
        /// </summary>
        public static V GetValueOrDefault<K, V>(this IDictionary<K, V> dict, K key, V defaultValue)
        {
            V val;
            if (!dict.TryGetValue(key, out val))
            {
                val = defaultValue;
            }
            return val;
        }

        /// <summary>
        /// Add all entries from another dictionary into this dictionary.
        /// </summary>
        public static void AddRange<K, V>(this IDictionary<K, V> dict, IDictionary<K, V> otherDict)
        {
            foreach (var entry in otherDict)
            {
                dict[entry.Key] = entry.Value;
            }
        }

    }
}
