using ToSic.Eav.Models;

namespace ToSic.Eav.Data.ContentTypes.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class ContentTypeExtensions
{
    public static IContentTypeDetails? DetailsOrNull(this IContentType contentType) =>
        contentType.PiggyBack.GetOrGenerate(
            parent: contentType.Metadata,
            key: nameof(DetailsOrNull),
            create: () => contentType.GetMetadataModel<IContentTypeDetails>()
        ).Value;
}
