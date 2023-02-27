using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using IEntity = ToSic.Eav.Data.IEntity;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer
    {
        public bool AssumeUnknownTypesAreDynamic { get; set; } = false;

        public IContentType DeserializeContentType(string serialized) => Log.Func($"{serialized?.Substring(0, Math.Min(50, serialized.Length))}...", l =>
        {
            try
            {
                var jsonPackage = UnpackAndTestGenericJsonV1(serialized);
                var type = ConvertContentType(jsonPackage);

                // new in 1.2 2sxc v12 - build relation relationships manager
                return (type, $"deserialized {type.Name}");
            }
            catch (Exception e)
            {
                throw l.Ex(e);
            }
        });

        public IContentType ConvertContentType(JsonContentTypeSet json) => Log.Func(l =>
        {
            var allEntities = new List<IEntity>();
            // new in v1.2 2sxc 12
            var relationshipsSource = AppPackageOrNull == null ? new DirectEntitiesSource(allEntities) : null;

            IEntity ConvertPart(JsonEntity e) => Deserialize(e, AssumeUnknownTypesAreDynamic, false, relationshipsSource);

            try
            {
                var directEntities = json.Entities?.Select(ConvertPart).ToList() ?? new List<IEntity>();
                allEntities.AddRange(directEntities);

                // Verify that it has a Json ContentType
                var jsonType = json.ContentType ?? throw new Exception("Tried to import JSON ContentType but JSON file didn't have any ContentType. Are you trying to import an Entity?");

                // Prepare ContentType Attributes
                l.A("deserialize attributes");
                var attribs = jsonType.Attributes
                    .Select((jsonAttr, pos) =>
                    {
                        var mdEntities = jsonAttr.Metadata?.Select(ConvertPart).ToList() ?? new List<IEntity>();
                        var attDef = new ContentTypeAttribute(AppId, jsonAttr.Name, jsonAttr.Type, jsonAttr.IsTitle, sortOrder: pos, mdEntities);
                        allEntities.AddRange(mdEntities);
                        //((IMetadataInternals)attDef.Metadata).Use(mdEntities);
                        return (IContentTypeAttribute)attDef;
                    })
                    .ToList();

                // Prepare Content-Type Metadata
                l.A("deserialize metadata");
                var ctMeta = jsonType.Metadata?.Select(ConvertPart).ToList() ?? new List<IEntity>();
                allEntities.AddRange(ctMeta);

                // Create the Content Type
                var type = Services.MultiBuilder.ContentType.Create(
                    appId: AppId, 
                    name: jsonType.Name,
                    nameId: jsonType.Id,
                    id: 0,
                    scope: jsonType.Scope,
                    // #RemoveContentTypeDescription #2974 - #remove 2023 Q2 if all works
                    //jsonType.Description,
                    parentTypeId: jsonType.Sharing?.ParentId,
                    configZoneId: jsonType.Sharing?.ParentZoneId ?? 0,
                    configAppId: jsonType.Sharing?.ParentAppId ?? 0,
                    isAlwaysShared: jsonType.Sharing?.AlwaysShare ?? false, 
                    attributes: attribs,
                    metadataItems: ctMeta
                    );

                //type.Metadata.Use(ctMeta);

                //type.Attributes = attribs;

                // new in 1.2 2sxc v12 - build relation relationships manager
                return (type, $"converted {type.Name} with {attribs.Count} attributes");
            }
            catch (Exception e)
            {
                throw l.Ex(e);
            }
        });

    }
}
