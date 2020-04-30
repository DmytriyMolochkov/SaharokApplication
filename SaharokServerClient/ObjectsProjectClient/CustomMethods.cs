using System;
using System.Collections.Generic;

namespace ObjectsProjectClient
{
    public static class CustomMethods
    {
        public static IEnumerable<T> ForEachChained<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
                yield return item;
            }
        }

        public static IEnumerable<T> ForEachImmediate<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
            return source;
        }
    }
}
