using ToSic.Eav.Metadata.Sys;

namespace ToSic.Eav.Persistence.Sys.Loaders;

public interface IContentTypeLoader
{
    /// <summary>
    /// Get all ContentTypes for specified AppId.
    /// </summary>
    ICollection<IContentType> ContentTypes(int appId, IHasMetadataSourceAndExpiring source);

}