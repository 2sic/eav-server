using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer
    {
        public bool AssumeUnknownTypesAreDynamic { get; set; } = false;

        public IContentType DeserializeContentType(string serialized)
        {
            var wrap = Log.Call("DeserializeContentType",$"{serialized?.Substring(0, Math.Min(50, serialized.Length))}...");
            try
            {
                var jsonObj = UnpackAndTestGenericJsonV1(serialized);

                var jsonType = jsonObj.ContentType;

                var type = new ContentType(AppId, jsonType.Name, jsonType.Id, 0,
                    jsonType.Scope,
                    jsonType.Description,
                    jsonType.Sharing?.ParentId, jsonType.Sharing?.ParentZoneId ?? 0,
                    jsonType.Sharing?.ParentAppId ?? 0,
                    jsonType.Sharing?.AlwaysShare ?? false,
                    null);

                Log.Add("deserialize metadata");
                var ctMeta =
                    jsonType.Metadata?.Select(je => Deserialize(je, AssumeUnknownTypesAreDynamic, false)).ToList()
                    ?? new List<IEntity>();
                type.Metadata.Use(ctMeta);

                Log.Add("deserialize attributes");
                var attribs = jsonType.Attributes.Select((attr, pos) =>
                {
                    var attDef = new AttributeDefinition(AppId, attr.Name, attr.Type, attr.IsTitle, 0, pos);
                    var md = attr.Metadata?.Select(m => Deserialize(m, AssumeUnknownTypesAreDynamic, false)).ToList() ??
                             new List<IEntity>();
                    attDef.Metadata.Use(md);
                    return (IAttributeDefinition) attDef;
                }).ToList();

                type.Attributes = attribs;
                wrap($"deserialized {type.Name} with {attribs.Count} attributes");
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
