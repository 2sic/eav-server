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

        public IContentType DeserializeContentType(string serialized)
        {
            var wrap = Log.Fn<IContentType>($"{serialized?.Substring(0, Math.Min(50, serialized.Length))}...");
            try
            {
                var jsonPackage = UnpackAndTestGenericJsonV1(serialized);
                var type = ConvertContentType(jsonPackage);

                // new in 1.2 2sxc v12 - build relation relationships manager
                return wrap.Return(type, $"deserialized {type.Name}");
            }
            catch (Exception e)
            {
                wrap.ReturnNull("failed, error '" + e.GetType().FullName + "':" + e.Message);
                throw;
            }
        }

        public IContentType ConvertContentType(JsonContentTypeSet json)
        {
            var wrap = Log.Fn<IContentType>($"convert JsonContentTypeSet to IContentType");
            try
            {
                // new in v1.2 2sxc 12
                var allEntities = new List<IEntity>();
                var relationshipsSource = AppPackageOrNull == null ? new DirectEntitiesSource(allEntities) : null;

                var directEntities = json.Entities?.Any() == true
                    ? json.Entities.Select(e => Deserialize(e, AssumeUnknownTypesAreDynamic, false, relationshipsSource)).ToList()
                    : new List<IEntity>();

                allEntities.AddRange(directEntities);


                var jsonType = json.ContentType ?? throw new Exception("Tried to import JSON ContentType but JSON file didn't have any ContentType. Are you trying to import an Entity?");

                var type = new ContentType(AppId, jsonType.Name, jsonType.Id, 0,
                    jsonType.Scope,
                    // #RemoveContentTypeDescription #2974 - #remove ca. Feb 2023 if all works
                    //jsonType.Description,
                    jsonType.Sharing?.ParentId,
                    jsonType.Sharing?.ParentZoneId ?? 0,
                    jsonType.Sharing?.ParentAppId ?? 0,
                    jsonType.Sharing?.AlwaysShare ?? false);

                Log.A("deserialize metadata");
                var ctMeta =
                    jsonType.Metadata?.Select(je => Deserialize(je, AssumeUnknownTypesAreDynamic, false, relationshipsSource)).ToList()
                    ?? new List<IEntity>();
                allEntities.AddRange(ctMeta);
                type.Metadata.Use(ctMeta);

                Log.A("deserialize attributes");
                var attribs = jsonType.Attributes.Select((attr, pos) =>
                {
                    var attDef = new ContentTypeAttribute(AppId, attr.Name, attr.Type, attr.IsTitle, 0, pos);
                    var mdEntities = attr.Metadata?.Select(m => Deserialize(m, AssumeUnknownTypesAreDynamic, false, relationshipsSource)).ToList() ??
                             new List<IEntity>();
                    allEntities.AddRange(mdEntities);
                    ((IMetadataInternals)attDef.Metadata).Use(mdEntities);
                    return (IContentTypeAttribute)attDef;
                }).ToList();

                type.Attributes = attribs;

                // new in 1.2 2sxc v12 - build relation relationships manager
                return wrap.Return(type, $"converted {type.Name} with {attribs.Count} attributes");
            }
            catch (Exception e)
            {
                wrap.ReturnNull("failed, error '" + e.GetType().FullName + "':" + e.Message);
                throw;
            }
        }

    }
}
