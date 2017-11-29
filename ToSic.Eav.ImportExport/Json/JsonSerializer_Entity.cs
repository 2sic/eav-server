using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ToSic.Eav.Data;
using ToSic.Eav.Enums;
using ToSic.Eav.ImportExport.Json.Format;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer
    {
        public override string Serialize(IEntity entity) => Serialize(entity, 0);

        public string Serialize(IEntity entity, int metadataDepth) => JsonConvert.SerializeObject(new JsonFormat
        {
            Entity = ToJson(entity, metadataDepth)
        }, JsonSerializerSettings());

        private static JsonEntity ToJson(IEntity entity, int metadataDepth = 0)
        {
            JsonMetadataFor mddic = null;
            if (entity.MetadataFor.IsMetadata)
                mddic = new JsonMetadataFor
                {
                    Target = Factory.Resolve<IGlobalMetadataProvider>().GetType(entity.MetadataFor.TargetType),
                    Guid = entity.MetadataFor.KeyGuid,
                    Number = entity.MetadataFor.KeyNumber,
                    String = entity.MetadataFor.KeyString
                };

            var attributesInUse = entity.Attributes.Values
                .OrderBy(a => a.Name)
                .Where(a => a.Values.Any(v => v.SerializableObject != null))
                .ToList();

            var attribs = new JsonAttributes();
            attributesInUse.GroupBy(a => a.ControlledType, a => a).ToList().ForEach(g =>
            {
                var gList = g.ToList();
                switch (g.Key)
                {
                    case AttributeTypeEnum.String:
                        attribs.String = ToTypedDictionary<string>(gList);
                        break;
                    case AttributeTypeEnum.Hyperlink:
                        attribs.Hyperlink = ToTypedDictionary<string>(gList);
                        break;
                    case AttributeTypeEnum.Custom:
                        attribs.Custom = ToTypedDictionary<string>(gList);
                        break;
                    case AttributeTypeEnum.Number:
                        attribs.Number = ToTypedDictionary<decimal?>(gList);
                        break;
                    case AttributeTypeEnum.DateTime:
                        attribs.DateTime = ToTypedDictionary<DateTime?>(gList);
                        break;
                    case AttributeTypeEnum.Boolean:
                        attribs.Boolean = ToTypedDictionary<bool?>(gList);
                        break;
                    case AttributeTypeEnum.Entity:
                        attribs.Entity = ToTypedDictionary<EntityRelationship>(gList)
                            .ToDictionary(a => a.Key, a => a.Value
                                .ToDictionary(b => b.Key, b => b.Value.Select(e => e?.EntityGuid).ToList()));
                        break;
                    case AttributeTypeEnum.Empty:
                    case AttributeTypeEnum.Undefined:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });

            // new: optionally include metadata
            List<JsonEntity> itemMeta = null;
            if (metadataDepth > 0)
                itemMeta = entity.Metadata.Select(m => ToJson(m, metadataDepth - 1)).ToList();

            var jEnt = new JsonEntity
            {
                Id = entity.EntityId,
                Guid = entity.EntityGuid,
                Version = entity.Version,
                Type = new JsonType
                {
                    Name = entity.Type.Name,
                    Id = entity.Type.StaticName,
                },
                Attributes = attribs,
                Owner = entity.Owner,
                For = mddic,
                Metadata = itemMeta
            };
            return jEnt;
        }


        private static string LanguageKey(IValue v)
        {
            return string.Join(",", v.Languages
                    .OrderBy(l => l.ReadOnly)
                    .Select(l => (l.ReadOnly ? ReadOnlyMarker : "") + l.Key)
                    .ToArray())
                .EmptyAlternative(NoLanguage);
        }

 
    }
}
