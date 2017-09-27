using System;
using System.Collections.Generic;

namespace DriveCentric.Shared.Extensions
{
    public static class ListExtensions
    {
        public static void AddRangeNullSafe<T>(this List<T> self, IEnumerable<T> newCollection)
        {
            if (newCollection != null)
            {
                self.AddRange(newCollection);
            }
        }

        /// <summary>
        /// Enumerate a collection and perform an action for each element.
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> self, Action<T> action)
        {
            foreach (var entry in self)
            {
                action?.Invoke(entry);
            }
        }
    }
}
