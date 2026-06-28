using ToSic.Eav.Models;

namespace ToSic.Eav.Data.Sys.ContentTypes;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class ContentTypeExtensions
{
    public static ContentTypeDetails? DetailsOrNull(this IContentType contentType) =>
        contentType.PiggyBack.GetOrGenerate(
            parent: contentType.Metadata,
            key: nameof(DetailsOrNull),
            create: () => contentType.Metadata.FirstModel<ContentTypeDetails>()
        ).Value;
}
