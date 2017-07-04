using System;
using System.Linq;
using Newtonsoft.Json;
using ToSic.Eav.Data;
using ToSic.Eav.Enums;
using ToSic.Eav.ImportExport.Json.Format;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence.Xml;

namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer: SerializerBase
    {
        public const string ReadOnlyMarker = "~";
        public const string NoLanguage = "*";

        public override string Serialize(IEntity entity) 
        {
            JsonMetadataFor mddic = null;
            if (entity.Metadata.IsMetadata)
                mddic = new JsonMetadataFor
                {
                    Target = App.GetMetadataType(entity.Metadata.TargetType),
                    Guid = entity.Metadata.KeyGuid,
                    Number = entity.Metadata.KeyNumber,
                    String = entity.Metadata.KeyString
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

            var typedObj = new JsonFormat
            {
                Entity = new JsonEntity
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
                    For = mddic
                }
            };

            var simple = JsonConvert.SerializeObject(typedObj, JsonSerializerSettings());
            return simple;
            
        }

        private static string LanguageKey(IValue v)
        {
            return string.Join(",", v.Languages
                    .OrderBy(l => l.ReadOnly)
                    .Select(l => (l.ReadOnly ? ReadOnlyMarker : "") + l.Key)
                    .ToArray())
                .EmptyAlternative(NoLanguage);
        }

        private static JsonSerializerSettings JsonSerializerSettings()
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };
            return settings;
        }




 
    }

    internal static class StringHelpers
    {
        public static string EmptyAlternative(this string s, string alternative) => String.IsNullOrEmpty(s) ? alternative : s;
    }
}
