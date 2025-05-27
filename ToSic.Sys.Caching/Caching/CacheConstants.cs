namespace ToSic.Lib.Caching;

[PrivateApi]
public class CacheConstants
{
    private const int Duration1Hour = 60 * 60;
    private const int Duration1Day = 24 * Duration1Hour;
    private const int Duration1Week = 7 * Duration1Day;

    /// <summary>
    /// Explicit constant for AppCode to be sure it's not used by mistake.
    /// ATM set to 4 weeks - basically forever. 
    /// </summary>
    public const int DurationAppCode = 4 * Duration1Week;

    public const int DurationAppDlls = 4 * Duration1Week;

    /// <summary>
    /// Razor and classes for now should be cached for 1 week.
    /// This is almost forever, but less than the AppCode.
    /// </summary>
    public const int DurationRazorAndCode = Duration1Week;
}