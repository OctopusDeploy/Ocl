using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public static class Extensions
    {
        public static string ToUnixLineEndings(this string str)
            => str.Replace("\r", "");

        public static bool None<T>(this IEnumerable<T> items)
            => !items.Any();
    }
}