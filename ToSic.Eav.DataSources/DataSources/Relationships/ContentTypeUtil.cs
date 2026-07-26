using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.ContentTypes.Sys;
using ToSic.Eav.Data.Raw.Sys;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources.Sys;

// TODO: THIS should be moved to the right place, using the new IRawEntity setup
internal class ContentTypeUtil
{
    /// <summary>
    /// Options to generate data - can't be placed in the ContentType class,
    /// because the DataFactoryOptions doesn't exist at that level.
    /// </summary>
    public static DataFactoryOptions Options = new()
    {
        TitleField = nameof(IContentType.Name),
        Type = typeof(ContentType)
    };


    private static Dictionary<string, object?> BuildDictionary(IContentType t) => new()
    {
        { nameof(IContentType.Name), t.Name },
        // 2024-10-29 v18.03 2dm disabled, as deprecated, must see if something breaks, but don't really expect it...
        // noticed that it's actually used quite a bit in our internal fields, would have to change that first...
        // I must also assume that it may have been used elsewhere too, but I don't really think so...
        { "StaticName", t.NameId }, // TODO: This should be removed, but JS code still uses it, so it much be change first
        { nameof(t.NameId), t.NameId },
        { nameof(IContentType.IsDynamic), t.IsDynamic },

        { nameof(IContentType.Scope), t.Scope },
        { nameof(IContentType.Attributes) + "Count", t.Attributes.Count() },

        { nameof(IContentType.RepositoryType), t.RepositoryType.ToString() },
        { nameof(IContentType.RepositoryAddress), t.RepositoryAddress },
    };

    internal static RawEntity ToRaw(IContentType t) =>
        new()
        {
            Id = t.Id,
            Guid = SafeConvertGuid(t) ?? Guid.Empty,
            Values = BuildDictionary(t),
            Metadata = t.Metadata,
        };

    private static Guid? SafeConvertGuid(IContentType t)
    {
        try
        {
            if (Guid.TryParse(t.NameId, out var g))
                return g;
        }
        catch
        {
            /* ignore */
        }

        return null;
    }
}