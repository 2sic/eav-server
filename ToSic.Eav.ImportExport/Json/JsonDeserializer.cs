using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ToSic.Eav.App;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Enums;
using ToSic.Eav.ImportExport.Json.Format;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer: IThingDeserializer
    {

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

        public IEntity Deserialize(string serialized)
        {
            JsonFormat jsonObj;
            try
            {
                jsonObj = JsonConvert.DeserializeObject<JsonFormat>(serialized, JsonSerializer.JsonSerializerSettings());
            }
            catch (Exception ex)
            {
                throw new FormatException("cannot deserialize json - bad format", ex);
            }

            if(jsonObj._.V != 1)
                throw new ArgumentOutOfRangeException(nameof(serialized), "unexpected format version");

            // get type def
            var contentType = Enumerable.SingleOrDefault<IContentType>(App.ContentTypes.Values, ct => ct.StaticName == jsonObj.Entity.Type.Id);
            if(contentType == null)
                throw new Exception($"type not found for deserialization - cannot continue{jsonObj.Entity.Type.Id}");

            // Metadata
            var ismeta = new Metadata();
            if (jsonObj.Entity.For != null)
            {
                var md = jsonObj.Entity.For;
                ismeta.TargetType = App.GetMetadataType(md.Target);
                ismeta.KeyGuid = md.Guid;
                ismeta.KeyNumber = md.Number;
                ismeta.KeyString = md.String;
            }

            // Build entity
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

        private List<int?> LookupGuids(List<Guid?> list) => list.Select(g => Enumerable.FirstOrDefault<IEntity>(App.Entities.Values, e => e.EntityGuid == g)?.EntityId).ToList();

        private static List<ILanguage> RecreateLanguageList(string languages) => languages == JsonSerializer.NoLanguage
            ? new List<ILanguage>()
            : languages.Split(',')
                .Select(a => new Dimension {Key = a.Replace(JsonSerializer.ReadOnlyMarker, ""), ReadOnly = a.StartsWith(JsonSerializer.ReadOnlyMarker)} as ILanguage)
                .ToList();


        private static Dictionary<string, Dictionary<string, T>> ToTypedDictionary<T>(List<IAttribute> attribs) 
            => attribs.Cast<IAttribute<T>>().ToDictionary(a => a.Name,
            a => a.Typed.ToDictionary(JsonSerializer.LanguageKey, v => v.TypedContents));

        public List<IEntity> Deserialize(List<string> serialized) => serialized.Select(Deserialize).ToList();
    }

}
