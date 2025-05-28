namespace ToSic.Eav.Apps.Internal;

/// <summary>
/// Reduced signature of the AppJsonConfigService.
/// It's provided at Eav.Core because it's also used by the Persistence layer.
///
/// The larger object is found in the Apps project.
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppJsonConfigBaseService
{
    List<string> ExcludeSearchPatterns(string sourceFolder, int appId, bool useShared = false);
}