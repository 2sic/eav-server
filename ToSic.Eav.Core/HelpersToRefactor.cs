using System;
using System.Globalization;

namespace ToSic.Eav
{
    public class HelpersToRefactor
    {
        /// <summary>
        /// Serialize Value to a String for SQL Server or XML Export
        /// </summary>
        public static string SerializeValue(object newValue)
        {
            string newValueSerialized;
            if (newValue is DateTime dateTime)
                newValueSerialized = dateTime.ToString("s");
            else if (newValue is double doubleValue)
                newValueSerialized = doubleValue.ToString(CultureInfo.InvariantCulture);
            else if (newValue is decimal decValue)
                newValueSerialized = decValue.ToString(CultureInfo.InvariantCulture);
            else if (newValue == null)
                newValueSerialized = string.Empty;
            else
                newValueSerialized = newValue.ToString();
            newValueSerialized = CleanInvalidXmlChars(newValueSerialized);
            return newValueSerialized;
        }

        /// <summary>
        /// Because entities are converted to XML (for the history / timeline table), all values
        /// need to be convertable to XML. Removes invalid characters from a string.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string CleanInvalidXmlChars(string text)
        {
            // From xml spec valid chars: 
            // #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]     
            // any Unicode character, excluding the surrogate blocks, FFFE, and FFFF.
            var re = @"[^\x09\x0A\x0D\x20-\uD7FF\uE000-\uFFFD\u10000-\u10FFFF]";
            return System.Text.RegularExpressions.Regex.Replace(text, re, "");
        }
    }
}