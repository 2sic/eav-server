using ToSic.Eav.Data.Source;
using ToSic.Eav.ImportExport.Json.V1;
using IEntity = ToSic.Eav.Data.IEntity;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport.Json;

partial class JsonSerializer
{
    public bool AssumeUnknownTypesAreDynamic { get; set; } = false;

    public IContentType DeserializeContentType(string serialized)
    {
        var l = LogDsSummary.Fn<IContentType>($"{serialized?.Substring(0, Math.Min(50, serialized.Length))}...", timer: true);
        try
        {
            var jsonPackage = UnpackAndTestGenericJsonV1(serialized);
            var type = ConvertContentType(jsonPackage).ContentType;

            // new in 1.2 2sxc v12 - build relation relationships manager
            return l.Return(type, $"deserialized {type.Name}");
        }
        catch (Exception e)
        {
            l.Done(e);
            throw;
        }
    }

    public ContentTypeWithEntities ConvertContentType(JsonContentTypeSet json)
    {
        var lMain = LogDsDetails.Fn<ContentTypeWithEntities>(timer: true);
        var contentTypeSet = DirectEntitiesSource.Using(relationships =>
        {
            var preferredSource = AppReaderOrNull?.AppState as IEntitiesSource;
            var sharedEntSource = DeserializationSettings?.RelationshipsSource;
            var relationshipsSource = preferredSource ?? sharedEntSource ?? relationships.Source;
            // 2024-09-30 2dm warning: ATM the code always seemed to use the relationships source, even if a shared source was available.
            var preferredSourceMsg = preferredSource != null ? "AppReader" : sharedEntSource != null ? "Shared Entities" : "Standalone List";
            lMain.A($"Will use relationship source {preferredSourceMsg}; current size: {relationshipsSource.List?.Count()}");

            IEntity ConvertPart(JsonEntity e) => Deserialize(e, AssumeUnknownTypesAreDynamic, false, relationshipsSource);

            var l = LogDsDetails.Fn<ContentTypeWithEntities>();
            try
            {
                var directEntities = json.Entities?.Select(ConvertPart).ToList() ?? [];
                relationships.List?.AddRange(directEntities);

                // Verify that it has a Json ContentType
                var jsonType = json.ContentType ?? throw new(
                    "Tried to import JSON ContentType but JSON file didn't have any ContentType. Are you trying to import an Entity?");

                // Prepare ContentType Attributes
                l.A("deserialize attributes");
                var attribs = jsonType.Attributes
                    .Select((jsonAttr, pos) =>
                    {
                        var valType = ValueTypeHelpers.Get(jsonAttr.Type);

                        // #SharedFieldDefinition
                        var attrSysSettings = jsonAttr.SysSettings?.ToSysSettings();
                        var mdEntities = attrSysSettings?.InheritMetadata == true
                            ? null
                            : jsonAttr.Metadata?.Select(ConvertPart).ToList() ?? [];

                        var appSourceForMd = DeserializationSettings?.MetadataSource;

                        var attrMetadata = new ContentTypeAttributeMetadata(key: default, type: valType,
                            name: jsonAttr.Name, sysSettings: attrSysSettings, items: mdEntities, appSource: appSourceForMd);

                        var attDef = Services.DataBuilder.TypeAttributeBuilder
                            .Create(
                                appId: AppId,
                                name: jsonAttr.Name,
                                type: valType,
                                isTitle: jsonAttr.IsTitle,
                                sortOrder: pos,
                                // #SharedFieldDefinition
                                guid: jsonAttr.Guid,
                                sysSettings: attrSysSettings,
                                // metadataItems: mdEntities,
                                metadata: attrMetadata
                            );

                        // #SharedFieldDefinition
                        if (mdEntities != null)
                            relationships.List?.AddRange(mdEntities);
                        return (IContentTypeAttribute)attDef;
                    })
                    .ToList();

                // Prepare Content-Type Metadata
                l.A("deserialize metadata");
                var ctMeta = jsonType.Metadata?.Select(ConvertPart).ToList() ?? [];
                relationships.List?.AddRange(ctMeta);

                // Create the Content Type
                var type = Services.DataBuilder.ContentType.Create(
                    appId: AppId,
                    name: jsonType.Name,
                    nameId: jsonType.Id,
                    id: 0,
                    scope: jsonType.Scope, parentTypeId: jsonType.Sharing?.ParentId,
                    configZoneId: jsonType.Sharing?.ParentZoneId ?? 0,
                    configAppId: jsonType.Sharing?.ParentAppId ?? 0,
                    isAlwaysShared: jsonType.Sharing?.AlwaysShare ?? false,
                    attributes: attribs,
                    metadataItems: ctMeta
                );

                // new in 1.2 2sxc v12 - build relation relationships manager
                return l.Return(new() { ContentType = type, Entities = relationships.List ?? [] },
                    $"converted {type.Name} with {attribs.Count} attributes and {relationships.List?.Count} Relationships");
            }
            catch (Exception e)
            {
                l.Done(e);
                throw;
            }
        });
        return lMain.ReturnAsOk(contentTypeSet);
    }

    public ContentTypeAttributeSysSettings DeserializeAttributeSysSettings(string name, string json)
        => JsonDeserializeAttribute.SysSettings(name, json, LogDsDetails);
}