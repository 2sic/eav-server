using System.Globalization;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Plumbing;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class NumberExtensions
{
    public static NumberFormatInfo NumberSeparator =>
        _numberSeparator.Get(() =>
        {
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = "`";
            return nfi;
        })!;

    private static readonly GetOnce<NumberFormatInfo> _numberSeparator = new();


    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static string ToAposString(this double value) => value.ToString("N2", NumberSeparator);
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static string ToAposString(this int value) => value.ToString("N", NumberSeparator);
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static string ToAposString(this long value) => value.ToString("N", NumberSeparator);
}