using ToSic.Eav.Data.Sys.Entities;

namespace ToSic.Eav.Data.Sys.ContentTypes;
public static class ContentTypeExtensions
{
    public static ContentTypeDetails? DetailsOrNull(this IContentType contentType)
    {
        var (value, _) = contentType.PiggyBack.GetOrGenerate(contentType.Metadata,nameof(DetailsOrNull), () =>
        {
            var descEntity = contentType.Metadata.First(typeName: ContentTypeDetails.ContentTypeTypeName);
            return descEntity == null
                ? null
                : new ContentTypeDetails(descEntity);
        });
        return value;
    }
}
