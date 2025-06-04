using System.Reflection;

namespace ToSic.Eav.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class EavSystemInfo
{
    public static Version Version => field ??= Assembly.GetExecutingAssembly().GetName().Version;

    public static string VersionString { get; }= VersionToNiceFormat(Version);

    // Todo: probably move to plumbing or extension method?
    public static string VersionToNiceFormat(Version version)
        => $"{version.Major:00}.{version.Minor:00}.{version.Build:00}";


    // Version is used also as cache-break for js assets.
    // In past build revision was good cache-break value, but since assemblies are deterministic 
    // we use application start unix time as slow changing revision value for cache-break purpose. 
    public static string VersionWithStartUpBuild { get; }=
        VersionWithFakeBuildNumber(Assembly.GetExecutingAssembly().GetName().Version).ToString();

    /// <summary>
    /// application start unix time as slow changing revision value
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    private static Version VersionWithFakeBuildNumber(Version version) =>
        new(version.Major, version.Minor, version.Build,
            (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);

}