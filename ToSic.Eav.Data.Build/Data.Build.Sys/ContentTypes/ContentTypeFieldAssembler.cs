using ToSic.Eav.Data.ContentTypes.Fields.Sys;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Data.Sys.Entities.Sources;
using ToSic.Eav.Metadata;
using ToSic.Eav.Metadata.Sys;

#pragma warning disable CA1822 // Don't use static methods for public APIs as some day it may need DI.

namespace ToSic.Eav.Data.Build.Sys;

/// <summary>
/// Internal helper to assemble <see cref="IContentTypeField"/> definitions.
/// </summary>
/// <remarks>
/// Technically this could all be done with static methods,
/// but because we want to be able to inject dependencies in the future, this is a class.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[method: PrivateApi]
public class ContentTypeFieldAssembler() : ServiceWithSetup<DataAssemblerOptions>("Eav.CtAtBl")
{
    /// <summary>
    /// Create a ContentType Attribute.
    /// This contains the definition of a single attribute of a content type.
    /// Specifically its name, what value type it accepts etc.
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="isTitle"></param>
    /// <param name="id"></param>
    /// <param name="sortOrder"></param>
    /// <param name="guid">The Attribute-Guid is relevant when sharing field definitions.</param>
    /// <param name="sysSettings">The system settings for the attribute, relevant when sharing field definitions.</param>
    /// <param name="metadata"></param>
    /// <param name="metadataItems"></param>
    /// <param name="metaSourceFinder"></param>
    /// <returns></returns>
    public IContentTypeField Create(
        int appId,
        string name,
        ValueTypes type,
        bool isTitle,
        int id = default,
        int sortOrder = default,
        Guid? guid = default,   // #SharedFieldDefinition
        ContentTypeFieldSysSettings? sysSettings = default, // #SharedFieldDefinition
        IMetadata? metadata = default,
        IList<IEntity>? metadataItems = default,
        Func<IHasMetadataSourceAndExpiring>? metaSourceFinder = null)
    {
        metadata ??= new ContentTypeFieldMetadata(key: id, name: name, type: type,
            sysSettings: sysSettings,
            source: MetadataProvider.Create(metadataItems, sourceDeferred: metaSourceFinder));
            //source: new MetadataSourceWipOld(metadataItems == null ? null : new ImmutableEntitiesSource(metadataItems.ToImmutableOpt()), null, metaSourceFinder));
            //items: metadataItems, deferredSource: metaSourceFinder);

        return new ContentTypeField
        {
            AppId = appId,
            AttributeId = id,
            SortOrder = sortOrder,
            IsTitle = isTitle,
            Guid = guid,
            SysSettings = sysSettings,
            Metadata = metadata,

            Name = name,
            Type = type,
        };
    }


    public IContentTypeField CreateFrom(
        IContentTypeField original,
        int? appId = default,
        string? name = default,
        ValueTypes? type = default,
        bool? isTitle = default,
        int? id = default,
        int? sortOrder = default,
        IMetadata? metadata = default,
        List<IEntity>? metadataItems = default
    )
    {
        // Prepare parts which we also need for new Metadata Creation
        name ??= original.Name;
        id ??= original.AttributeId;
        var realType = type ?? original.Type;
        metadata ??= EntityPartsLazy.CloneMetadataFunc<int>(
            original.Metadata,
            items: metadataItems
            /*deferredSource: metaSourceFinder*/)(id.Value, $"{name} ({realType})");

        return Create(
            appId: appId ?? original.AppId,
            name: name,
            type: realType,
            isTitle: isTitle ?? original.IsTitle,
            id: id.Value,
            sortOrder: sortOrder ?? original.SortOrder,
            guid: original.Guid,
            sysSettings: original.SysSettings,
            metadata: metadata
        );
    }
}