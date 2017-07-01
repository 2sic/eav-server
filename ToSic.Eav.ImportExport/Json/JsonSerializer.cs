using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ToSic.Eav.App;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Enums;
using ToSic.Eav.Interfaces;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.Xml
{
    public class JsonSerializer: SerializerBase
    {
        const string ROmarker = "~";
        private const string NoLanguage = "*";

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

        private AppDataPackageDeferredList RelLookupList
        {
            get
            {
                if (_relList != null) return _relList;
                _relList = new AppDataPackageDeferredList();
                _relList.AttachApp(App);
                return _relList;
            }
        }
        private AppDataPackageDeferredList _relList;

        public /*override*/ IEntity Deserialize(string serialized)
        {
            JsonFormat jsonObj;
            try
            {
                jsonObj = JsonConvert.DeserializeObject<JsonFormat>(serialized, JsonSerializerSettings());
            }
            catch (Exception ex)
            {
                throw new FormatException("cannot deserialize json - bad format", ex);
            }

            if(jsonObj._.V != 1)
                throw new ArgumentOutOfRangeException(nameof(serialized), "unexpected format version");

            // get type def
            var contentType = App.ContentTypes.Values.SingleOrDefault(ct => ct.StaticName == jsonObj.Entity.Type.Id);
            if(contentType == null)
                throw new Exception($"type not found for deserialization - cannot continue{jsonObj.Entity.Type.Id}");

            // metadata
            var ismeta = new Metadata();
            if (jsonObj.Entity.For != null)
            {
                var md = jsonObj.Entity.For;
                ismeta.TargetType = App.GetMetadataType(md.Target);
                ismeta.KeyGuid = md.Guid;
                ismeta.KeyNumber = md.Number;
                ismeta.KeyString = md.String;
            }

            // todo: build entity
            var appId = 0;
            var newEntity = new Entity(appId, jsonObj.Entity.Guid, jsonObj.Entity.Id, jsonObj.Entity.Id, ismeta, contentType, true, null, DateTime.Now, jsonObj.Entity.Owner);


            // build attributes
            var jAtts = jsonObj.Entity.Attributes;
            foreach (var definition in contentType.Attributes)
            {
                var newAtt = ((AttributeDefinition)definition).CreateAttribute();
                switch (definition.ControlledType)
                {
                    case AttributeTypeEnum.Boolean:
                        if(!jAtts.Boolean?.ContainsKey(definition.Name) ?? true) continue;
                        newAtt.Values = jAtts.Boolean[definition.Name]
                            .Select(v => Value.Build(definition.Type, v.Value, RecreateLanguageList(v.Key))).ToList();
                        break;
                    case AttributeTypeEnum.DateTime:
                        if(!jAtts.DateTime?.ContainsKey(definition.Name) ?? true) continue;
                        newAtt.Values = jAtts.DateTime[definition.Name]
                            .Select(v => Value.Build(definition.Type, v.Value, RecreateLanguageList(v.Key))).ToList();
                        break;
                    case AttributeTypeEnum.Entity:
                        if(!jAtts.Entity?.ContainsKey(definition.Name) ?? true) continue;
                        newAtt.Values = jAtts.Entity[definition.Name]
                            .Select(v => Value.Build(definition.Type, LookupGuids(v.Value), RecreateLanguageList(v.Key), RelLookupList)).ToList();
                        break;
                    case AttributeTypeEnum.Hyperlink:
                        if(!jAtts.Hyperlink?.ContainsKey(definition.Name) ?? true) continue;
                        newAtt.Values = jAtts.Hyperlink[definition.Name]
                            .Select(v => Value.Build(definition.Type, v.Value, RecreateLanguageList(v.Key))).ToList();
                        break;
                    case AttributeTypeEnum.Number:
                        if (!jAtts.Number?.ContainsKey(definition.Name) ?? true) continue;
                        newAtt.Values = jAtts.Number[definition.Name]
                            .Select(v => Value.Build(definition.Type, v.Value, RecreateLanguageList(v.Key))).ToList();
                        break;
                    case AttributeTypeEnum.String:
                        if (!jAtts.String?.ContainsKey(definition.Name) ?? true) continue;
                        newAtt.Values = jAtts.String[definition.Name]
                            .Select(v => Value.Build(definition.Type, v.Value, RecreateLanguageList(v.Key))).ToList();
                        break;
                    case AttributeTypeEnum.Custom:
                        if (!jAtts.Custom?.ContainsKey(definition.Name) ?? true) continue;
                        newAtt.Values = jAtts.Custom[definition.Name]
                            .Select(v => Value.Build(definition.Type, v.Value, RecreateLanguageList(v.Key))).ToList();
                        break;
                    // ReSharper disable RedundantCaseLabel
                    case AttributeTypeEnum.Empty:
                    case AttributeTypeEnum.Undefined:
                    // ReSharper restore RedundantCaseLabel
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                // only add if we actually found something
                newEntity.Attributes.Add(newAtt.Name, newAtt);

                if (definition.IsTitle)
                    newEntity.SetTitleField(definition.Name);
            }

            return newEntity;
        }

        private List<int?> LookupGuids(List<Guid?> list)
        {
            return
                list.Select(g => App.Entities.Values.FirstOrDefault(e => e.EntityGuid == g)?.EntityId).ToList();
        }

        private static List<ILanguage> RecreateLanguageList(string languages)
        {
            return languages == NoLanguage
                ? new List<ILanguage>()
                : languages.Split(',')
                    .Select(a => new Dimension {Key = a.Replace(ROmarker, ""), ReadOnly = a.StartsWith(ROmarker)} as ILanguage)
                    .ToList();
        }


        private static Dictionary<string, Dictionary<string, T>> ToTypedDictionary<T>(List<IAttribute> attribs) 
            => attribs.Cast<IAttribute<T>>().ToDictionary(a => a.Name,
            a => a.Typed.ToDictionary(LanguageKey, v => v.TypedContents));

        private static string LanguageKey(IValue v)
        {
            return string.Join(",", v.Languages
                    .OrderBy(l => l.ReadOnly)
                    .Select(l => (l.ReadOnly ? ROmarker : "") + l.Key)
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

        internal class JsonHeader { public int V = 1; }

        internal class JsonMetadataFor
        {
            public string Target;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public string String;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public Guid? Guid;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public int? Number;
        }

        internal class JsonType { public string Name, Id; }

        internal class JsonFormat
        {
            public JsonHeader _ = new JsonHeader();
            public JsonEntity Entity;
        }

        internal class JsonEntity
        {
            public int Id;
            public Guid Guid;
            public JsonType Type;
            public JsonAttributes Attributes;
            public string Owner;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public JsonMetadataFor For;
        }

        internal class JsonAttributes
        {
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public Dictionary<string, Dictionary<string, string>> String;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public Dictionary<string, Dictionary<string, string>> Hyperlink;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public Dictionary<string, Dictionary<string, string>> Custom;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public Dictionary<string, Dictionary<string, List<Guid?>>> Entity;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public Dictionary<string, Dictionary<string, decimal?>> Number;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public Dictionary<string, Dictionary<string, DateTime?>> DateTime;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public Dictionary<string, Dictionary<string, bool?>> Boolean;
        }
 
    }

    public static class StringHelpers
    {
        public static string EmptyAlternative(this string s, string alternative) => String.IsNullOrEmpty(s) ? alternative : s;
    }
}
