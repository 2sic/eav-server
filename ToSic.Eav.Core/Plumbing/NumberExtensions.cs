using System.Globalization;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Plumbing;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class NumberExtensions
{
    public static NumberFormatInfo NumberSeparator => _numberSeparator.Get(() =>
    {
        var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        nfi.NumberGroupSeparator = "`";
        return nfi;
    });
    private static readonly GetOnce<NumberFormatInfo> _numberSeparator = new();


    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string ToAposString(this double value) => value.ToString("N2", NumberSeparator);
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string ToAposString(this int value) => value.ToString("N", NumberSeparator);
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static string ToAposString(this long value) => value.ToString("N", NumberSeparator);
}