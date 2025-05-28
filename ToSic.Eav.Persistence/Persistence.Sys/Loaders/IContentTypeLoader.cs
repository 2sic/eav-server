using ToSic.Eav.Data;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Internal.Loaders;

public interface IContentTypeLoader
{
    /// <summary>
    /// Get all ContentTypes for specified AppId.
    /// </summary>
    IList<IContentType> ContentTypes(int appId, IHasMetadataSourceAndExpiring source);

}