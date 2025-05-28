using ToSic.Eav.Apps.Internal.Specs;

namespace ToSic.Eav.Apps.Internal;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppJsonService: IAppJsonConfigBaseService
{
    void MoveAppJsonTemplateFromOldToNewLocation();

    AppJson GetAppJson(int appId, bool useShared = false);

    string AppJsonCacheKey(int appId, bool useShared = false);

    List<string> ExcludeSearchPatterns(string sourceFolder, int appId, bool useShared = false);
}