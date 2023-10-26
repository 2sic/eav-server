using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Source;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Serialization;
using ToSic.Lib.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer
    {
        public bool AssumeUnknownTypesAreDynamic { get; set; } = false;

        public IContentType DeserializeContentType(string serialized)
        {
            var l = Log.Fn<IContentType>($"{serialized?.Substring(0, Math.Min(50, serialized.Length))}...");
            try
            {
                var jsonPackage = UnpackAndTestGenericJsonV1(serialized);
                var type = ConvertContentType(jsonPackage);

                // new in 1.2 2sxc v12 - build relation relationships manager
                return l.Return(type, $"deserialized {type.Name}");
            }
            catch (Exception e)
            {
                l.Done(e);
                throw;
            }
        }

        public IContentType ConvertContentType(JsonContentTypeSet json)
        {
            var lMain = Log.Fn<IContentType>();
            var contentType = DirectEntitiesSource.Using(relationships =>
            {
                var relationshipsSource = AppOrNull as IEntitiesSource ?? relationships.Source;

                IEntity ConvertPart(JsonEntity e) =>
                    Deserialize(e, AssumeUnknownTypesAreDynamic, false, relationshipsSource);

                var l = Log.Fn<IContentType>();
                try
                {
                    var directEntities = json.Entities?.Select(ConvertPart).ToList() ?? new List<IEntity>();
                    relationships.List?.AddRange(directEntities);

                    // Verify that it has a Json ContentType
                    var jsonType = json.ContentType ?? throw new Exception(
                        "Tried to import JSON ContentType but JSON file didn't have any ContentType. Are you trying to import an Entity?");

                    // Prepare ContentType Attributes
                    l.A("deserialize attributes");
                    var attribs = jsonType.Attributes
                        .Select((jsonAttr, pos) =>
                        {
                            var valType = ValueTypeHelpers.Get(jsonAttr.Type);

                            // #SharedFieldDefinition
                            var inheritMetadata = jsonAttr.SysSettings?.SourceGuid != null && jsonAttr.SysSettings?.InheritMetadata == true;
                            var sourceGuid = inheritMetadata ? jsonAttr.SysSettings?.SourceGuid : null;
                            var mdEntities = inheritMetadata
                                ? null
                                : jsonAttr.Metadata?.Select(ConvertPart).ToList() ?? new List<IEntity>();

                            var appSourceForMd = DeserializationSettings?.CtAttributeMetadataAppState ?? AppOrNull;

                            var attrMetadata = new ContentTypeAttributeMetadata(key: default, type: valType,
                                name: jsonAttr.Name, sourceGuid: sourceGuid, items: mdEntities, appSource: appSourceForMd);

                            var attDef = Services.DataBuilder.TypeAttributeBuilder
                                .Create(
                                    appId: AppId,
                                    name: jsonAttr.Name,
                                    type: valType,
                                    isTitle: jsonAttr.IsTitle,
                                    sortOrder: pos,
                                    // #SharedFieldDefinition
                                    guid: jsonAttr.Guid,
                                    sysSettings: jsonAttr.SysSettings?.ToSysSettings(),
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
                    var ctMeta = jsonType.Metadata?.Select(ConvertPart).ToList() ?? new List<IEntity>();
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
                    return l.Return(type, $"converted {type.Name} with {attribs.Count} attributes");
                }
                catch (Exception e)
                {
                    l.Done(e);
                    throw;
                }
            });
            return lMain.ReturnAsOk(contentType);
        }

        public ContentTypeAttributeSysSettings DeserializeAttributeSysSettings(string serialized)
        {
            var l = Log.Fn<ContentTypeAttributeSysSettings>($"{serialized?.Substring(0, Math.Min(50, serialized.Length))}...");
            if (serialized.IsEmpty()) return l.Return(null, "empty serialized");
            try
            {
                var json = System.Text.Json.JsonSerializer.Deserialize<JsonAttributeSysSettings>(serialized, JsonOptions.UnsafeJsonWithoutEncodingHtml);
                return l.Return(json.ToSysSettings(), $"deserialized sysSettings");
            }
            catch (Exception e)
            {
                l.Done(e);
                throw;
            }
        }
    }
}
