using System.Text.RegularExpressions;

namespace ToSic.Eav.ImportExport.Validation
{
    public static class StringExtension
    {
        /// <summary>
        /// Remove special characters like ?, &, %, - or spaces from a string.
        /// </summary>
        public static string RemoveSpecialCharacters(this string str) => Regex.Replace(str, "[^a-zA-Z0-9]+", "");
    }
}