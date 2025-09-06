using ToSic.Eav.Data.Sys.Entities.Sources;
using ToSic.Eav.Metadata;
using ToSic.Eav.Metadata.Sys;

namespace ToSic.Eav.Data.Sys.ContentTypes;

/// <summary>
/// WIP
/// #SharedFieldDefinition
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ContentTypeAttributeMetadata(int key, string name, ValueTypes type, IMetadataProvider source, ContentTypeAttributeSysSettings? sysSettings = null)
    : Metadata<int>(targetType: (int)TargetTypes.Attribute, key: key, title: $"{name} ({type})", source: source)
{
    private ContentTypeAttributeSysSettings SysSettings { get; } = sysSettings ?? new ContentTypeAttributeSysSettings();

    internal IMetadataProvider SourceForUseOfInheritingAttributes => Source;

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
    private ICollection<IEntity>? _directlyOwnedMd;

    /// <summary>
    /// Override data loading.
    /// In some cases, the source Attribute has directly attached metadata (like loaded from JSON)
    /// so this handles that case.
    /// </summary>
    protected override ICollection<IEntity> LoadFromProviderInsideLock(IList<IEntity>? additions = null)
    {
        // If nothing to inherit, behave using standard key mechanisms
        var ownMd = _directlyOwnedMd = base.LoadFromProviderInsideLock(null);
        if (!SysSettings.InheritMetadata)
            return ownMd;

        // Assemble all the pieces from the sources it inherits from
        var final = new List<IEntity>();
        try
        {
            var helper = new ContentTypeAttributeMetadataLookup(SysSettings, Source, GetMetadataSource);
            // First get all source attributes
            // This should not be cached, since an early access can happen during App State Build
            // Where it won't be able to find them yet.
            foreach (var sourceDef in SysSettings.InheritMetadataOf!)
            {
                var toAdd = helper.GetMdOfOneSource(sourceDef.Key, sourceDef.Value, ownMd);
                if (toAdd.Count == 0)
                    continue;

                // Prevent multiple additions of the same type, like multiple @All metadata
                // If we have multiple places we inherit from, then only add types
                // which previous iterations didn't add yet.
                var addOnlyAdditionalTypes = toAdd
                    .Where(e => !final.Any(f => f.Type.Is(e.Type.NameId)))
                    .ToListOpt();

                if (addOnlyAdditionalTypes.Any())
                    final.AddRange(addOnlyAdditionalTypes);
            }
        }
        catch { /* ignore - in case something breaks here, just return empty list */ }

        return final;
    }

}