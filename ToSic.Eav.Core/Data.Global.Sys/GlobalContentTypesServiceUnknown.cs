using ToSic.Eav.Internal.Unknown;
#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.Data.Global.Sys;
internal class GlobalContentTypesServiceUnknown(WarnUseOfUnknown<GlobalContentTypesServiceUnknown> _): IGlobalContentTypesService
{
    public IContentType GetContentType(string name)
        => null;

    public IContentType GetContentTypeIfAlreadyLoaded(string name)
        => null;
}
