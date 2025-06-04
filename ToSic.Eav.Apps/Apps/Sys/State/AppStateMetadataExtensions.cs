using ToSic.Eav.Data.Ancestors.Sys;
using ToSic.Eav.Data.Entities.Sys.Lists;
using ToSic.Eav.Data.Shared;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps;

[PrivateApi("WIP v13")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class AppStateMetadataTargetExtensions
{
    // 2021-11-22 2dm WIP - not used yet
    // Idea was to be able to get Metadata Targets, but I'm not sure if it's useful at all
    public static string? FindTargetTitle(this IAppReader appReader, int targetType, string key)
    {
        if (!Enum.IsDefined(typeof(TargetTypes), targetType))
            return null;

        var tType = (TargetTypes)targetType;
            
        switch (tType)
        {
            case TargetTypes.None: 
                return null;
            case TargetTypes.Attribute:
                if (!int.TryParse(key, out var keyInt))
                    return null;
                var attr = appReader.ContentTypes.FindAttribute(keyInt);
                return attr.ContentType?.Metadata?.Target.Title + "/" + attr.Attribute?.Metadata?.Target.Title;
            case TargetTypes.App:
                return appReader.Specs.Metadata?.Target.Title;
            case TargetTypes.Entity:
                return !Guid.TryParse(key, out var guidKey)
                    ? null
                    : appReader.List.One(guidKey)?.Metadata?.Target.Title;
            case TargetTypes.ContentType:
                return appReader.GetContentType(key)?.Metadata?.Target.Title;
            case TargetTypes.Zone:
            case TargetTypes.CmsItem:
                return null;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static (IContentType ContentType, IContentTypeAttribute Attribute) FindAttribute(this IEnumerable<IContentType> contentTypes, string idString) 
        => !int.TryParse(idString, out var keyInt) ? default : contentTypes.FindAttribute(keyInt);

    private static (IContentType ContentType, IContentTypeAttribute Attribute) FindAttribute(this IEnumerable<IContentType> contentTypes, int id)
    {
        var allLocalCts = contentTypes
            .Where(ct => !ct.HasAncestor());
        var attr = allLocalCts
            .SelectMany(ct => ct.Attributes.Select(at => (ct, at)))
            .FirstOrDefault(set => set.Item2.AttributeId == id);
        return attr;
    }
}