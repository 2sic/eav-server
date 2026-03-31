namespace ToSic.Eav.Apps.Assets.Sys;

/// <inheritdoc />
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
[method: PrivateApi]
public class SizeInfo(long bytes) : ISizeInfo
{
    private const int Factor = 1024;

    /// <inheritdoc />
    public long Bytes { get; } = bytes;

    /// <inheritdoc />
    public decimal Kb => Rounded((decimal)Bytes / Factor);

    /// <inheritdoc />
    public decimal Mb => Rounded((decimal)Bytes / Factor / Factor);

    /// <inheritdoc />
    public decimal Gb => Rounded((decimal)Bytes / Factor / Factor / Factor);

    /// <inheritdoc />
    public decimal BestSize => Rounded(BestSizeCache.Size);

    /// <inheritdoc />
    public string BestUnit => BestSizeCache.Unit;

    /// <summary>
    /// Trunc/rounding factor used on the numbers.
    /// If you change it, the precision of the numbers returned would change.
    /// we don't plan to publish this, as the web designer must usually to a ToString(#,##) for it to look right anyhow
    /// </summary>
    private int Decimals { get; set; } = 4;

    private decimal Rounded(decimal number) => Math.Round(number, Decimals);


    private (decimal Size, string Unit) BestSizeCache => _bestSizeCache.Get(() => Bytes switch
    {
        < Factor * Factor => (Kb, "KB"),
        < Factor * Factor * Factor => (Mb, "MB"),
        _ => (Gb, "GB")
    });
    private readonly GetOnce<(decimal, string)> _bestSizeCache = new();

    public override string ToString() => $"{BestSize} {BestUnit}";

    public string ToString(string format) => $"{BestSize.ToString(format)} {BestUnit}";
}