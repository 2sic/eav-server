using System;
using System.Collections.Generic;
using ToSic.Eav.Metadata;
using ToSic.Lib.Services;

namespace ToSic.Eav.Data.Build;

/// <summary>
/// Builder to create / clone <see cref="IContentTypeAttribute"/> definitions.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ContentTypeAttributeBuilder() : ServiceBase("Eav.CtAtBl")
{
    public ContentTypeAttribute Create(
        int appId,
        string name,
        ValueTypes type,
        bool isTitle,
        int id = default,
        int sortOrder = default,
        Guid? guid = default,   // #SharedFieldDefinition
        ContentTypeAttributeSysSettings sysSettings = default, // #SharedFieldDefinition
        IMetadataOf metadata = default,
        List<IEntity> metadataItems = default,
        Func<IHasMetadataSource> metaSourceFinder = null)
    {
        metadata ??= new ContentTypeAttributeMetadata(key: id, name: name, type: type,
            sysSettings: sysSettings, items: metadataItems, deferredSource: metaSourceFinder);

        return new(appId: appId, name: name, type: type, isTitle: isTitle,
            attributeId: id, sortOrder: sortOrder, guid: guid, sysSettings: sysSettings, metadata: metadata);
    }


    public IContentTypeAttribute CreateFrom(
        IContentTypeAttribute original,
        int? appId = default,
        string name = default,
        ValueTypes? type = default,
        bool? isTitle = default,
        int? id = default,
        int? sortOrder = default,
        IMetadataOf metadata = default,
        List<IEntity> metadataItems = default,
        Func<IHasMetadataSource> metaSourceFinder = null
    )
    {
        // Prepare parts which we also need for new Metadata Creation
        name ??= original.Name;
        id ??= original.AttributeId;
        var realType = type ?? original.Type;
        metadata ??= EntityPartsBuilder.CloneMetadataFunc<int>(original.Metadata, items: metadataItems,
            deferredSource: metaSourceFinder)(id.Value, $"{name} ({realType})");

        return Create(
            appId: appId ?? original.AppId,
            name: name,
            type: realType,
            isTitle: isTitle ?? original.IsTitle,
            id: id.Value,
            sortOrder: sortOrder ?? original.SortOrder,
            metadata: metadata
        );
    }
}