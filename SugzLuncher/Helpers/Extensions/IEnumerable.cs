using System;
using System.Collections.Generic;

namespace SugzLuncher.Helpers
{
    internal static class IEnumerableExtensions
    {
        internal static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T element in source)
                action(element);
        }
    }
}
