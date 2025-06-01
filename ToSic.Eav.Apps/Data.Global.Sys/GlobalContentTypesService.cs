using ToSic.Eav.Apps;

namespace ToSic.Eav.Data.Global.Sys;
internal class GlobalContentTypesService(IAppReaderFactory appReaderFactory): IGlobalContentTypesService
{
    private IAppReadContentTypes GlobalAppOrNull => field ??= appReaderFactory.GetSystemPreset(nullIfNotLoaded: true);
    private IAppReadContentTypes GlobalAppRequired => field ??= appReaderFactory.GetSystemPreset(nullIfNotLoaded: false);

    public IContentType GetContentType(string name)
        => GlobalAppRequired.GetContentType(name);

    public IContentType GetContentTypeIfAlreadyLoaded(string name)
        => GlobalAppOrNull?.GetContentType(name);
}
