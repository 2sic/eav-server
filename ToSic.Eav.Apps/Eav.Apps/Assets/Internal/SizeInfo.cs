using System;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Apps.Assets.Internal;

/// <inheritdoc />
[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
[method: PrivateApi]
public class SizeInfo(int bytes) : ISizeInfo
{
    private const int Factor = 1024;

    /// <inheritdoc />
    public int Bytes { get; } = bytes;

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


    private (decimal Size, string Unit) BestSizeCache => _bestSizeCache.Get(() =>
    {
        if (Bytes < Factor * Factor) return (Kb, "KB");
        if (Bytes < Factor * Factor * Factor) return (Mb, "MB");
        return (Gb, "GB");
    });
    private readonly GetOnce<(decimal, string)> _bestSizeCache = new();
}