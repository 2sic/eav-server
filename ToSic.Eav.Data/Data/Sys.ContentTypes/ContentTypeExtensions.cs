namespace ToSic.Eav.Data.Sys.ContentTypes;
public static class ContentTypeExtensions
{
    public static ContentTypeDetails? DetailsOrNull(this IContentType contentType) =>
        contentType.PiggyBack.GetOrGenerate(
            contentType.Metadata,
            nameof(DetailsOrNull),
            () => contentType.Metadata.First<ContentTypeDetails>()
        ).Value;
}
