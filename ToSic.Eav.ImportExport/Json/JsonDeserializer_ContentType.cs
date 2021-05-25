using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer
    {
        public bool AssumeUnknownTypesAreDynamic { get; set; } = false;

        public IContentType DeserializeContentType(string serialized)
        {
            var wrap = Log.Call($"{serialized?.Substring(0, Math.Min(50, serialized.Length))}...");
            try
            {
                var jsonPackage = UnpackAndTestGenericJsonV1(serialized);

                // new in v1.2 2sxc 12
                var allEntities = new List<IEntity>();
                var relationshipsSource = AppPackageOrNull == null ? new DirectEntitiesSource(allEntities) : null;
                
                var directEntities = jsonPackage.Entities?.Any() == true
                    ? jsonPackage.Entities.Select(e => Deserialize(e, AssumeUnknownTypesAreDynamic, false, relationshipsSource)).ToList()
                    : new List<IEntity>();
                
                allEntities.AddRange(directEntities); 
               

                var jsonType = jsonPackage.ContentType;

                var type = new ContentType(AppId, jsonType.Name, jsonType.Id, 0,
                    jsonType.Scope,
                    jsonType.Description,
                    jsonType.Sharing?.ParentId, jsonType.Sharing?.ParentZoneId ?? 0,
                    jsonType.Sharing?.ParentAppId ?? 0,
                    jsonType.Sharing?.AlwaysShare ?? false,
                    null);

                Log.Add("deserialize metadata");
                var ctMeta =
                    jsonType.Metadata?.Select(je => Deserialize(je, AssumeUnknownTypesAreDynamic, false, relationshipsSource)).ToList()
                    ?? new List<IEntity>();
                allEntities.AddRange(ctMeta);
                type.Metadata.Use(ctMeta);

                Log.Add("deserialize attributes");
                var attribs = jsonType.Attributes.Select((attr, pos) =>
                {
                    var attDef = new ContentTypeAttribute(AppId, attr.Name, attr.Type, attr.IsTitle, 0, pos);
                    var mdEntities = attr.Metadata?.Select(m => Deserialize(m, AssumeUnknownTypesAreDynamic, false, relationshipsSource)).ToList() ??
                             new List<IEntity>();
                    allEntities.AddRange(mdEntities);
                    attDef.Metadata.Use(mdEntities);
                    return (IContentTypeAttribute) attDef;
                }).ToList();

                type.Attributes = attribs;
                wrap($"deserialized {type.Name} with {attribs.Count} attributes");
                
                // new in 1.2 2sxc v12 - build relation relationships manager
                
                return type;
            }
            catch (Exception e)
            {
                wrap("failed, error '" + e.GetType().FullName + "':" + e.Message);
                throw;
            }
        }

    }
}
