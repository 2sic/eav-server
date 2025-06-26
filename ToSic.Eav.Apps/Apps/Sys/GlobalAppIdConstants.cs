namespace ToSic.Eav.Apps.Sys;

internal class GlobalAppIdConstants
{
    /// <summary>
    /// Special constant to make our global numbers have a lot of trailing zeros
    /// </summary>
    private const int MagicZeroMaker = 10000000;

    public const int GlobalContentTypeMin = int.MaxValue / MagicZeroMaker * MagicZeroMaker;
    public const int GlobalContentTypeSourceSkip = 1000;

    public const int GlobalEntityIdMin = int.MaxValue / MagicZeroMaker * MagicZeroMaker;
    public const int GlobalEntitySourceSkip = 10000;
}
