﻿using System;
using System.Linq;

namespace ToSic.Eav.Plumbing
{
    public static class StringExtensions
    {
        //public static bool IsEmpty(this string value) => string.IsNullOrEmpty(value);
        //public static bool IsEmptyOrWs(this string value) => string.IsNullOrWhiteSpace(value);
        public static bool HasValue(this string value) => !string.IsNullOrWhiteSpace(value);


        public static string[] SplitNewLine(this string value) 
            => value?.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        public static string[] TrimmedAndWithoutEmpty(this string[] value) 
            => value?
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();
    }
}
