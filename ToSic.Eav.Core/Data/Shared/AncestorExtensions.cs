﻿namespace ToSic.Eav.Data.Shared;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
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
        return anc != null && anc.Id == Constants.PresetContentTypeFakeParent;
    }

    public static bool HasAncestor(this IEntity contentType)
    {
        var anc = contentType.GetDecorator<IAncestor>();
        return anc != null && anc.Id != 0;
    }

    public static bool HasPresetAncestor(this IEntity contentType)
    {
        var anc = contentType.GetDecorator<IAncestor>();
        return anc != null && anc.Id == Constants.PresetContentTypeFakeParent;
    }
}