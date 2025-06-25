using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Sys;

namespace ToSic.Eav.Data.Sys.Ancestors;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class AncestorExtensions
{
    public static bool HasAncestor(this IContentType contentType)
    {
        var anc = contentType.GetDecorator<IAncestor>();
        return anc != null && anc.Id != 0;
    }

    public static bool HasPresetAncestor(this IContentType contentType)
    {
        var anc = contentType.GetDecorator<IAncestor>();
        return anc != null && anc.Id == EavConstants.PresetContentTypeFakeParent;
    }

    public static bool HasAncestor(this IEntity contentType)
    {
        var anc = contentType.GetDecorator<IAncestor>();
        return anc != null && anc.Id != 0;
    }

    public static bool HasPresetAncestor(this IEntity contentType)
    {
        var anc = contentType.GetDecorator<IAncestor>();
        return anc != null && anc.Id == EavConstants.PresetContentTypeFakeParent;
    }
}