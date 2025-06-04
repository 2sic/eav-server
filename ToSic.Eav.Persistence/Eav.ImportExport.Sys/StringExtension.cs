using System.Text.RegularExpressions;

namespace ToSic.Eav.ImportExport.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StringExtension
{
    /// <summary>
    /// Remove special characters like ?, &, %, - or spaces from a string.
    /// </summary>
    [ShowApiWhenReleased(ShowApiMode.Never)] 
    public static string RemoveSpecialCharacters(this string str) => Regex.Replace(str, "[^a-zA-Z0-9]+", "");

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static string RemoveNonFilenameCharacters(this string str) 
        => System.IO.Path.GetInvalidFileNameChars().Aggregate(str, (current, c) => current.Replace(c.ToString(), ""));
}