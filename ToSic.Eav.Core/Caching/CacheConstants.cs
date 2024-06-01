namespace ToSic.Eav.Caching;

[PrivateApi]
public class CacheConstants
{
    public const int Duration1Hour = 60 * 60;
    private const int Duration1Day = 24 * Duration1Hour;
    private const int Duration1Week = 7 * Duration1Day;

    /// <summary>
    /// Explicit constant for AppCode to be sure it's not used by mistake.
    /// </summary>
    public const int DurationAppCode1Day = Duration1Day;

    public const int DurationRazor8Hours = 8 * Duration1Hour;
}