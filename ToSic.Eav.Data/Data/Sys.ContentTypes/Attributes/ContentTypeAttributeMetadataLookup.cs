using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Data.Sys.Entities.Sources;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data.Sys.ContentTypes;
internal class ContentTypeAttributeMetadataLookup(ContentTypeAttributeSysSettings SysSettings, IMetadataProvider Source, Func<IMetadataSource?> GetMetadataSource)
{
    [field: AllowNull, MaybeNull]
    ICollection<IContentTypeAttribute> SourceAttributes => field ??= GetSourceAttributes() ?? [];

    /// <summary>
    /// The Source Attribute (if any) which would provide the real metadata
    /// </summary>
    internal ICollection<IContentTypeAttribute>? GetSourceAttributes()
    {
        // Check all the basics to ensure we can work
        if (!SysSettings.InheritMetadata)
            return null;
        if (Source.LookupSource is not IAppStateCache appState)
            return null;

        // Get all the keys in the source-list except Empty (self-reference)
        var sourceKeys = SysSettings.InheritMetadataOf!.Keys
            .Where(guid => guid != Guid.Empty)
            .Cast<Guid?>()
            .ToArray();

        // Get all attributes in all content-types of the App and keep the ones we need
        var appAttribs = appState.ContentTypes
            .SelectMany(ct => ct.Attributes);
        return appAttribs
            .Where(a => sourceKeys.Contains(a.Guid))
            .ToListOpt();
    }


    internal ICollection<IEntity> GetMdOfOneSource(Guid sourceGuid, string? filter, ICollection<IEntity> ownEntities)
    {
        // If it's referencing empty, it means self-reference, so just return own entities, else do a lookup
        var entities = sourceGuid == Guid.Empty
            ? ownEntities
            : MetadataOfOneSource(sourceGuid);

        // If the reference has a filter, it should only take metadata of the specified content-type
        // This is for scenarios where the metadata would come from multiple attributes; like one providing description, another dropdown options, etc.
        var list = !filter.HasValue()
            ? entities
            : entities?.AllOfType(filter).ToListOpt();

        return list ?? [];
    }

    private ICollection<IEntity>? MetadataOfOneSource(Guid sourceGuid)
    {
        // Empty refers to the own MD, should never get to here
        if (sourceGuid == Guid.Empty)
            return null;

        // Find source and its metadata
        var sourceAttribute = SourceAttributes.FirstOrDefault(a => a.Guid == sourceGuid);

        // Null-Check & cast inner source to this type, so we can access it's private .Source later on
        if (sourceAttribute?.Metadata is not ContentTypeAttributeMetadata sourceMd)
            return null;

        var md = (
                sourceMd.SourceForUseOfInheritingAttributes.List?.List
                ?? GetMetadataSource()?.GetMetadata((int)TargetTypes.Attribute, sourceAttribute.AttributeId)
            )
            ?.ToListOpt();

        return md; // can be null
    }
}
