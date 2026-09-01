namespace ToSic.Sys.Utils;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class NumberExtensions
{
    public static readonly NumberFormatInfo NumberSeparator = Create();

    private static NumberFormatInfo Create()
    {
        var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        nfi.NumberGroupSeparator = "`";
        return nfi;
    }

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static string ToAposString(this double value) => value.ToString("N2", NumberSeparator);
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static string ToAposString(this int value) => value.ToString("N", NumberSeparator);
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static string ToAposString(this long value) => value.ToString("N", NumberSeparator);
}