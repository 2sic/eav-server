using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ToSic.Eav.Data;
using ToSic.Eav.Enums;
using ToSic.Eav.ImportExport.Json.Format;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer
    {
        public override string Serialize(IEntity entity) => Serialize(entity, 0);

        public string Serialize(IEntity entity, int metadataDepth) => JsonConvert.SerializeObject(new JsonFormat
        {
            Entity = ToJson(entity, metadataDepth, Log)
        }, JsonSerializerSettings());

        public static JsonEntity ToJson(IEntity entity, int metadataDepth = 0, Log parentLog = null)
        {
            var log = new Log("Jsn.Serlzr", parent: parentLog, className:"JsonSerializer");
            var wrapLog = log.Call("ToJson", $"id:{entity?.EntityId}, meta-depth:{metadataDepth}");
            // do a null-check, because sometimes code could ask to serialize not-yet existing entities
            if (entity == null)
            {
                wrapLog("is null");
                return null;
            }

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
                        attribs.String = ToTypedDictionary<string>(gList, log);
                        break;
                    case AttributeTypeEnum.Hyperlink:
                        attribs.Hyperlink = ToTypedDictionary<string>(gList, log);
                        break;
                    case AttributeTypeEnum.Custom:
                        attribs.Custom = ToTypedDictionary<string>(gList, log);
                        break;
                    case AttributeTypeEnum.Number:
                        attribs.Number = ToTypedDictionary<decimal?>(gList, log);
                        break;
                    case AttributeTypeEnum.DateTime:
                        attribs.DateTime = ToTypedDictionary<DateTime?>(gList, log);
                        break;
                    case AttributeTypeEnum.Boolean:
                        attribs.Boolean = ToTypedDictionary<bool?>(gList, log);
                        break;
                    case AttributeTypeEnum.Entity:
                        attribs.Entity = ToTypedDictionaryEntity(gList, log);
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
            if (metadataDepth > 0 && entity.Metadata.Any())
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
            wrapLog("ok");
            return jEnt;
        }

        /// <summary>
        /// this is a special helper to create typed entities-dictionaries
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, Dictionary<string, List<Guid?>>> ToTypedDictionaryEntity(List<IAttribute> gList, Log log)
        {
            // the following is a bit complex for the following reason
            // 1. either the relationship is guid based, and in that case, 
            //    it's possible that the items don't exist yet (because it's an import or similar)
            // 2. or it's int/id based, in which case the items exist, 
            //    but the relationship manager doesn't have a direct reference to the guid,
            //    but only to the items directly
            // so it tries to get the guids first, and otherwise uses the items
            var ents = ToTypedDictionary<EntityRelationship>(gList, log)
                .ToDictionary(a => a.Key, a => a.Value
                    .ToDictionary(b => b.Key, b => b.Value.ResolveGuids()));
            return ents;
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
