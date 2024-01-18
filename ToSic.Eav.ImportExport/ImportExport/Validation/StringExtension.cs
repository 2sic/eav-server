using System.Text.RegularExpressions;

namespace ToSic.Eav.ImportExport.Validation;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class StringExtension
{
    /// <summary>
    /// Remove special characters like ?, &, %, - or spaces from a string.
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)] 
    public static string RemoveSpecialCharacters(this string str) => Regex.Replace(str, "[^a-zA-Z0-9]+", "");

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string RemoveNonFilenameCharacters(this string str) 
        => System.IO.Path.GetInvalidFileNameChars().Aggregate(str, (current, c) => current.Replace(c.ToString(), ""));
}