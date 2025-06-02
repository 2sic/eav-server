using System.Collections.Immutable;
using ToSic.Eav.Apps;

namespace ToSic.Eav.Data.Global.Sys;
internal class GlobalDataService(IAppReaderFactory appReaderFactory): IGlobalDataService
{
    private IAppReader GlobalAppOrNull => field ??= appReaderFactory.GetSystemPreset(nullIfNotLoaded: true);
    private IAppReader GlobalAppRequired => field ??= appReaderFactory.GetSystemPreset(nullIfNotLoaded: false);

    public IContentType GetContentType(string name)
        => GlobalAppRequired.GetContentType(name);

    public IContentType GetContentTypeIfAlreadyLoaded(string name)
        => GlobalAppOrNull?.GetContentType(name);

    //public IImmutableList<IEntity> AllEntitiesIfAlreadyLoaded => GlobalAppOrNull?.List ?? [];

    public IImmutableList<IEntity> ListRequired => GlobalAppRequired.List;
}
