using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Metadata;
using ToSic.Eav.Serialization;
using ToSic.Lib.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer
    {
        public override string Serialize(IEntity entity) => Serialize(entity, 0);

        public string Serialize(IEntity entity, int metadataDepth) => System.Text.Json.JsonSerializer.Serialize(new JsonFormat
        {
            Entity = ToJson(entity, metadataDepth)
        }, JsonOptions.UnsafeJsonWithoutEncodingHtml);

        public JsonEntity ToJson(IEntity entity, int metadataDepth = 0)
        {
            // var log = new Log("Jsn.Serlzr", Log);
            var wrapLog = Log.Fn<JsonEntity>($"id:{entity?.EntityId}, meta-depth:{metadataDepth}");
            // do a null-check, because sometimes code could ask to serialize not-yet existing entities
            if (entity == null)
                return wrapLog.ReturnNull("is null");

            JsonMetadataFor jsonFor = null;
            if (entity.MetadataFor.IsMetadata)
                jsonFor = new JsonMetadataFor
                {
                    // #TargetTypeIdInsteadOfTarget - the Target should become obsolete
                    Target = MetadataTargets.GetName(entity.MetadataFor.TargetType),
                    TargetType = entity.MetadataFor.TargetType,
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
                    case ValueTypes.String:
                        attribs.String = ToTypedDictionary<string>(gList, Log);
                        break;
                    case ValueTypes.Hyperlink:
                        attribs.Hyperlink = ToTypedDictionary<string>(gList, Log);
                        break;
                    case ValueTypes.Custom:
                        attribs.Custom = ToTypedDictionary<string>(gList, Log);
                        break;
                    case ValueTypes.Json:
                        attribs.Json = ToTypedDictionary<string>(gList, Log);
                        break;
                    case ValueTypes.Number:
                        attribs.Number = ToTypedDictionary<decimal?>(gList, Log);
                        break;
                    case ValueTypes.DateTime:
                        attribs.DateTime = ToTypedDictionary<DateTime?>(gList, Log);
                        break;
                    case ValueTypes.Boolean:
                        attribs.Boolean = ToTypedDictionary<bool?>(gList, Log);
                        break;
                    case ValueTypes.Entity:
                        attribs.Entity = ToTypedDictionaryEntity(gList, false, Log);
                        break;
                    case ValueTypes.Empty:
                    case ValueTypes.Undefined:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });

            // new: optionally include metadata
            List<JsonEntity> itemMeta = null;
            var metaList = (entity.Metadata as MetadataOf<Guid>)?.AllWithHidden ?? entity.Metadata as IEnumerable<IEntity>;
            if (metadataDepth > 0 && metaList.Any())
                itemMeta = metaList.Select(m => ToJson(m, metadataDepth - 1)).ToList();

            var jEnt = new JsonEntity
            {
                Id = entity.EntityId,
                Guid = entity.EntityGuid,
                Version = entity.Version,
                Type = new JsonType(entity),
                Attributes = attribs,
                Owner = entity.Owner,
                For = jsonFor,
                Metadata = itemMeta
            };
            return wrapLog.ReturnAsOk(jEnt);
        }

        /// <summary>
        /// this is a special helper to create typed entities-dictionaries
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, Dictionary<string, List<Guid?>>> 
            ToTypedDictionaryEntity(List<IAttribute> gList, bool fullObjects,  ILog log)
        {
            // the following is a bit complex for the following reason
            // 1. either the relationship is guid based, and in that case, 
            //    it's possible that the items don't exist yet (because it's an import or similar)
            // 2. or it's int/id based, in which case the items exist, 
            //    but the relationship manager doesn't have a direct reference to the guid,
            //    but only to the items directly
            // so it tries to get the guids first, and otherwise uses the items
            var entities = ToTypedDictionary<IEnumerable<IEntity>>(gList, log)
                .ToDictionary(a => a.Key, a => a.Value
                    .ToDictionary(b => b.Key, b => ((LazyEntities)b.Value).ResolveGuids()));
            return entities;
        }

        private static string LanguageKey(IValue v) =>
            string.Join(",", v.Languages
                    .OrderBy(l => l.ReadOnly)
                    .Select(l => (l.ReadOnly ? ReadOnlyMarker : "") + l.Key)
                    .ToArray())
                .EmptyAlternative(NoLanguage);
    }
}
