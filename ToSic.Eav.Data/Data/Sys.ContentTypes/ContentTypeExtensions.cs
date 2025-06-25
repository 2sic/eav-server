using ToSic.Eav.Data.Entities.Sys.Lists;

namespace ToSic.Eav.Data.Sys.ContentTypes;
public static class ContentTypeExtensions
{
    public static ContentTypeDetails? DetailsOrNull(this IContentType contentType)
    {
        var (value, _) = contentType.PiggyBack.GetOrGenerate(contentType.Metadata,nameof(DetailsOrNull), () =>
        {
            var descEntity = contentType.Metadata.FirstOrDefaultOfType(ContentTypeDetails.ContentTypeTypeName);
            return descEntity == null
                ? null
                : new ContentTypeDetails(descEntity);
        });
        return value;
    }
}
