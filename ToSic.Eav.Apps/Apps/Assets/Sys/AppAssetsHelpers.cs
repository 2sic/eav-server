namespace ToSic.Eav.Apps.Assets.Sys;

/// <summary>
/// Constants / helpers for App Assets. Mainly for views, so actually part of 2sxc,
/// but because various services depend on this, we move it to Eav temporarily.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppAssetsHelpers
{
    /// <summary>
    /// App file is located in the site itself.
    /// This is the Key used up until 2sxc 12.01 and will still exist in old data for a long time. 
    /// </summary>
    /// <remarks>
    /// It's not used in our code, but leave here for reference
    /// </remarks>
#pragma warning disable 414
    private const string PortalFileSystem = "Portal File System";
#pragma warning restore 414

    /// <summary>
    /// App file is located in the shared location. 
    /// This is the Key used up until 2sxc 12.01 and will still exist in old data for a long time. 
    /// </summary>
    private const string HostFileSystem = "Host File System";

    // New terms used in 12.02+
    internal static string AppInSite = "Site";
    internal static string AppInGlobal = "Global";

    internal static bool IsShared(string key)
    {
        return HostFileSystem.Equals(key, StringComparison.OrdinalIgnoreCase)
               || AppInGlobal.Equals(key, StringComparison.OrdinalIgnoreCase);
    }


    internal const string LocationSite = "site";
    internal const string LocationShared = "shared";
    internal const string LocationAuto = "auto";

    /// <summary>
    /// Return true/false or null to allow upstream to do auto-detect
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static bool? DetermineShared(string? location)
        => (location?.ToLowerInvariant() ?? LocationAuto) switch
        {
            LocationAuto => null,
            LocationShared => true,
            LocationSite => false,
            _ => throw new ArgumentException($@"should be null, {LocationAuto}, {LocationSite} or {LocationShared}", nameof(location))
        };
}