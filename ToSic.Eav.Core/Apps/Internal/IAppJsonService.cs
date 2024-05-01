using System.Collections.Generic;
using ToSic.Eav.Apps.Internal.Specs;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Apps.Internal;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IAppJsonService
{
    void MoveAppJsonTemplateFromOldToNewLocation();

    AppJson GetAppJson(int appId);

    string AppJsonCacheKey(int appId);

    List<string> ExcludeSearchPatterns(string sourceFolder);
}