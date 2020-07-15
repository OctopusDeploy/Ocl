using System;

namespace Tests
{
    public static class StringExtensions
    {
        public static string ToUnixLineEndings(this string str)
            => str.Replace("\r", "");
    }
}