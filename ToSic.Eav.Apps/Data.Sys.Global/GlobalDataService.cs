using System.Collections.Immutable;
using ToSic.Eav.Apps;
using ToSic.Eav.Data.Sys.Global;

namespace ToSic.Eav.Data.Global.Sys;
internal class GlobalDataService(IAppReaderFactory appReaderFactory): IGlobalDataService
{
    private IAppReader? GlobalAppOrNull => field ??= appReaderFactory.TryGetSystemPreset(nullIfNotLoaded: true);

    [field: AllowNull, MaybeNull]
    private IAppReader GlobalAppRequired => field ??= appReaderFactory.GetSystemPreset();

    public IContentType? GetContentType(string name)
        => GlobalAppRequired.TryGetContentType(name);

    public IContentType? GetContentTypeIfAlreadyLoaded(string name)
        => GlobalAppOrNull?.TryGetContentType(name);

    //public IImmutableList<IEntity> AllEntitiesIfAlreadyLoaded => GlobalAppOrNull?.List ?? [];

    public IImmutableList<IEntity> ListRequired => GlobalAppRequired.List;
}
