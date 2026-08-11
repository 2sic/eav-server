using ToSic.Eav.Models;
using ToSic.Sys.Caching.PiggyBack;

namespace ToSic.Eav.Data.ContentTypes.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class ContentTypeExtensions
{
    public static IContentTypeDetails? DetailsOrNull(this IContentType contentType) =>
        contentType.PiggyBackGetExpiring(
            expiring: contentType.Metadata,
            key: nameof(DetailsOrNull),
            create: contentType.GetMetadataModel<IContentTypeDetails>
        ).Value;
}
