using ToSic.Eav.Data.ContentTypes.Sys;
using ToSic.Eav.Data.Entities.Sys;
using ToSic.Eav.Metadata;
using ToSic.Eav.Metadata.Sys;
using ToSic.Lib.Services;

namespace ToSic.Eav.Data.Build;

/// <summary>
/// Builder to create / clone <see cref="IContentTypeAttribute"/> definitions.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ContentTypeAttributeBuilder() : ServiceBase("Eav.CtAtBl")
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
    /// <param name="guid"></param>
    /// <param name="sysSettings"></param>
    /// <param name="metadata"></param>
    /// <param name="metadataItems"></param>
    /// <param name="metaSourceFinder"></param>
    /// <returns></returns>
    public IContentTypeAttribute Create(
        int appId,
        string name,
        ValueTypes type,
        bool isTitle,
        int id = default,
        int sortOrder = default,
        Guid? guid = default,   // #SharedFieldDefinition
        ContentTypeAttributeSysSettings? sysSettings = default, // #SharedFieldDefinition
        IMetadataOf? metadata = default,
        List<IEntity>? metadataItems = default,
        Func<IHasMetadataSourceAndExpiring>? metaSourceFinder = null)
    {
        metadata ??= new ContentTypeAttributeMetadata(key: id, name: name, type: type,
            sysSettings: sysSettings, items: metadataItems, deferredSource: metaSourceFinder);

        return new ContentTypeAttribute
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


    public IContentTypeAttribute CreateFrom(
        IContentTypeAttribute original,
        int? appId = default,
        string? name = default,
        ValueTypes? type = default,
        bool? isTitle = default,
        int? id = default,
        int? sortOrder = default,
        IMetadataOf? metadata = default,
        List<IEntity>? metadataItems = default,
        Func<IHasMetadataSourceAndExpiring>? metaSourceFinder = null
    )
    {
        // Prepare parts which we also need for new Metadata Creation
        name ??= original.Name;
        id ??= original.AttributeId;
        var realType = type ?? original.Type;
        metadata ??= EntityPartsLazy.CloneMetadataFunc<int>(original.Metadata, items: metadataItems,
            deferredSource: metaSourceFinder)(id.Value, $"{name} ({realType})");

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