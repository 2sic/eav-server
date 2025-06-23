﻿using System.Globalization;
using System.Text.RegularExpressions;
using ToSic.Sys.Utils.Culture;

namespace ToSic.Eav.LookUp.Sources;
public class LookUpHelpers
{
    #region Sub-Token analysis and splitting

    // this is needed by some key accesses which support sub-properties like Content:Publisher:Location:City...
    // todo: should optimize to use named matches, to ensure that reg-ex changes doesn't change numbering...
    private static readonly Regex SubProperties = new("([a-z]+):([a-z:]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);



    /// <summary>
    /// Check if it has sub-tokens and split up the material for further use
    /// </summary>
    /// <param name="original"></param>
    /// <returns></returns>
    [PrivateApi]
    public static SubToken CheckAndGetSubToken(string original)
    {
        // Do quick-check - without a ":" it doesn't have sub-tokens so stop here
        if (!original.Contains(":"))
            return new() { HasSubToken = false };

        var match = SubProperties.Match(original);
        return match.Success
            ? new SubToken
            {
                HasSubToken = true,
                Source = match.Groups[1].Value,
                Rest = match.Groups[2].Value,
            }
            : new() { HasSubToken = false };
    }
    #endregion


    #region Helper functions to Format the result
    /// <summary>
    /// Returns a formatted String if a format is given, otherwise it returns the unchanged value.
    /// </summary>
    /// <param name="value">string to be formatted</param>
    /// <param name="format">format specification</param>
    /// <returns>formatted string</returns>
    /// <remarks></remarks>
    [PrivateApi]
    public static string FormatString(string value, string format)
    {
        // if no format, don't convert
        if (string.IsNullOrWhiteSpace(format)) return value;
        // format if there was a value
        return string.IsNullOrEmpty(value) ? string.Format(format, value) : string.Empty;
    }

    public static string Format(bool value) => value.ToString().ToLowerInvariant();

    public static string Format(DateTime value) => value.ToUniversalTime()
        .ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

    /// <summary>
    /// Take a specific result object (usually from an IEntity property) and format as needed.
    /// </summary>
    /// <returns></returns>
    public static string FormatValue(object valueObject, string format, string?[] dimensions)
    {
        return Type.GetTypeCode(valueObject.GetType()) switch
        {
            TypeCode.String => FormatString((string)valueObject, format),
            TypeCode.Boolean => Format((bool)valueObject),
            // make sure datetime is converted as universal time with the correct format specifier if no format is given
            TypeCode.DateTime => !string.IsNullOrWhiteSpace(format)
                ? ((DateTime)valueObject).ToString(format,
                    CultureHelpers.SafeCultureInfo(dimensions))
                : Format((DateTime)valueObject),
            // make sure numbers are converted to a neutral number format with "." notation if no format was given
            TypeCode.Double
                or TypeCode.Single
                or TypeCode.Int16
                or TypeCode.Int32
                or TypeCode.Int64
                or TypeCode.Decimal
                => !string.IsNullOrWhiteSpace(format)
                    ? ((IFormattable)valueObject).ToString(format,
                        CultureHelpers.SafeCultureInfo(dimensions))
                    : ((IFormattable)valueObject).ToString("G", CultureInfo.InvariantCulture),
            _ => FormatString(valueObject.ToString() ?? "", format)
        };
    }

    #endregion
}
