using System.Linq;
using System.Text.RegularExpressions;

namespace ToSic.Eav.ImportExport.Validation
{
    public static class StringExtension
    {
        /// <summary>
        /// Remove special characters like ?, &, %, - or spaces from a string.
        /// </summary>
        public static string RemoveSpecialCharacters(this string str) => Regex.Replace(str, "[^a-zA-Z0-9]+", "");

        public static string RemoveNonFilenameCharacters(this string str) 
            => System.IO.Path.GetInvalidFileNameChars().Aggregate(str, (current, c) => current.Replace(c.ToString(), ""));
    }
}