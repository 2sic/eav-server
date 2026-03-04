using System.Globalization;

namespace ToSic.Eav.Data.Build.Sys;

internal static class ValueHelpers
{
    internal static decimal? ToNumber(this object? value)
    {
        var newDec = value as decimal?;
        if (newDec != null || value is null || (value is string s && s.IsEmptyOrWs()))
            return newDec;
        try
        {
            return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }

    internal static DateTime? ToDateTime(this object? value) =>
        value as DateTime? ??
        (System.DateTime.TryParse(value as string, CultureInfo.InvariantCulture, DateTimeStyles.None, out var typed)
            ? typed
            : null);

    internal static bool? ToBool(this object? value) =>
        value as bool? ?? (bool.TryParse(value as string, out var typed) ? typed : null);
}
