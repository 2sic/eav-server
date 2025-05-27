using ToSic.Eav.Apps.State;
using ToSic.Eav.Metadata;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Helpers;
using ToSic.Sys.Utils;

namespace ToSic.Eav.Data;

/// <summary>
/// WIP
/// #SharedFieldDefinition
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ContentTypeAttributeMetadata(
    int key,
    string name,
    ValueTypes type,
    ContentTypeAttributeSysSettings sysSettings = default,
    IReadOnlyCollection<IEntity> items = default,
    IHasMetadataSourceAndExpiring appSource = default,
    Func<IHasMetadataSourceAndExpiring> deferredSource = default)
    : MetadataOf<int>(targetType: (int)TargetTypes.Attribute, key: key, title: $"{name} ({type})", items: items,
        appSource: appSource, deferredSource: deferredSource)
{
    private ContentTypeAttributeSysSettings SysSettings { get; } = sysSettings ?? new ContentTypeAttributeSysSettings(); // make sure it's never null

    /// <summary>
    /// Check if this metadata belongs to this attribute directly.
    /// If not, it's either not metadata of this attribute, or it's from another source.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public bool IsDirectlyOwned(IEntity entity) => _directlyOwnedMd?.Contains(entity) ?? false;

    /// <summary>
    /// This list is populated every time the metadata is loaded.
    /// </summary>
    private List<IEntity> _directlyOwnedMd;

    /// <summary>
    /// Override data loading.
    /// In some cases, the source Attribute has directly attached metadata (like loaded from JSON)
    /// so this handles that case.
    /// </summary>
    protected override List<IEntity> LoadFromProviderInsideLock(IList<IEntity> additions = default)
    {
        // If nothing to inherit, behave using standard key mechanisms
        var ownMd = _directlyOwnedMd = base.LoadFromProviderInsideLock();
        if (!SysSettings.InheritMetadata) return ownMd;

        // Assemble all the pieces from the sources it inherits from
        var final = new List<IEntity>();
        try
        {
            foreach (var sourceDef in SysSettings.InheritMetadataOf)
            {
                var fromCurrent = GetMdOfOneSource(sourceDef.Key, sourceDef.Value, ownMd);
                if (fromCurrent == null) continue;
                var ofNewTypes = fromCurrent.Where(e => !final.Any(f => f.Type.Is(e.Type.NameId))).ToList();
                if (ofNewTypes.SafeAny())
                    final.AddRange(ofNewTypes);
            }
        }
        catch { /* ignore - in case something breaks here, just return empty list */ }

        return final;
    }

    private List<IEntity> GetMdOfOneSource(Guid source, string filter, List<IEntity> ownEntities)
    {
        var entities = source == Guid.Empty ? ownEntities : MetadataOfOneSource(source);
        return !filter.HasValue()
            ? entities
            : entities?.OfType(filter)?.ToList();
    }

    private List<IEntity> MetadataOfOneSource(Guid guid)
    {
        // Empty refers to the own MD, should never get to here
        if (guid == Guid.Empty) return null;

        // Find source and its metadata
        var source = SourceAttributes?.FirstOrDefault(a => a.Guid == guid);

        // Null-Check & cast inner source to this type, so we can access it's private .Source later on
        if (source?.Metadata is not ContentTypeAttributeMetadata sourceMd) return null;

        var md = (
            sourceMd.Source.SourceDirect?.List
            ?? GetMetadataSource()?.GetMetadata((int)TargetTypes.Attribute, source.AttributeId)
        )?.ToList();

        return md; // can be null
    }


    /// <summary>
    /// The Source Attribute (if any) which would provide the real metadata
    /// </summary>
    private List<IContentTypeAttribute> SourceAttributes => _sourceAttributes.Get(() =>
    {
        // Check all the basics to ensure we can work
        if (!SysSettings.InheritMetadata) return null;
        if (Source.MainSource is not IAppStateCache appState) return null;

        // Get all the keys in the source-list except Empty (self-reference)
        var sourceKeys = SysSettings.InheritMetadataOf.Keys
            .Where(guid => guid != Guid.Empty)
            .Cast<Guid?>()
            .ToArray();

        // Get all attributes in all content-types of the App and keep the ones we need
        var appAttribs = ((AppState)appState).ContentTypes.SelectMany(ct => ct.Attributes);
        return appAttribs.Where(a => sourceKeys.Contains(a.Guid)).ToList();
    });
    private readonly GetOnce<List<IContentTypeAttribute>> _sourceAttributes = new();

}