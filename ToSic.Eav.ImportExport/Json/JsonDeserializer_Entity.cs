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

        public IEntity Deserialize(string serialized, bool allowDynamic = false)
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
            var contentType = App.ContentTypes?.Values.SingleOrDefault(ct => ct.StaticName == jsonObj.Entity.Type.Id)
                              ?? (allowDynamic
                                  ? ContentTypeBuilder.DynamicContentType(App.AppId)
                                  : throw new FormatException(
                                      "type not found for deserialization and dynamic not allowed " +
                                      $"- cannot continue with {jsonObj.Entity.Type.Id}")
                              );

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
            var appId = App.AppId;
            var newEntity = new Entity(appId, jsonObj.Entity.Guid, jsonObj.Entity.Id, jsonObj.Entity.Id, ismeta, contentType, true, null, DateTime.Now, jsonObj.Entity.Owner, jsonObj.Entity.Version);


            // build attributes - based on type definition
            if (contentType.Name == Constants.DynamicType)
                BuildAttribsOfUnknownContentType(jsonObj, newEntity);
            else
                BuildAttribsOfKnownType(jsonObj, contentType, newEntity);

            return newEntity;
        }

        private void BuildAttribsOfUnknownContentType(JsonFormat jsonObj, Entity newEntity)
        {
            BuildAttrib(jsonObj.Entity.Attributes.DateTime, AttributeTypeEnum.DateTime, newEntity);
            BuildAttrib(jsonObj.Entity.Attributes.Boolean, AttributeTypeEnum.Boolean, newEntity);
            BuildAttrib(jsonObj.Entity.Attributes.Custom, AttributeTypeEnum.Custom, newEntity);
            BuildAttrib(jsonObj.Entity.Attributes.Entity, AttributeTypeEnum.Entity, newEntity);
            BuildAttrib(jsonObj.Entity.Attributes.Hyperlink, AttributeTypeEnum.Hyperlink, newEntity);
            BuildAttrib(jsonObj.Entity.Attributes.Number, AttributeTypeEnum.Number, newEntity);
            BuildAttrib(jsonObj.Entity.Attributes.String, AttributeTypeEnum.String, newEntity);

            // todo: decide what to do with title!

        }

        private void BuildAttrib<T>(Dictionary<string, Dictionary<string, T>> list, AttributeTypeEnum type, Entity newEntity)
        {
            if (list == null) return;

            foreach (var attrib in list)
            {
                var newAtt = AttributeBase.CreateTypedAttribute(attrib.Key, type, attrib.Value
                    .Select(v => Value.Build(type, v.Value, RecreateLanguageList(v.Key))).ToList());
                newEntity.Attributes.Add(newAtt.Name, newAtt);
            }
        }

        private void BuildAttribsOfKnownType(JsonFormat jsonObj, IContentType contentType, Entity newEntity)
        {
            var jAtts = jsonObj.Entity.Attributes;
            foreach (var definition in contentType.Attributes)
            {
                var newAtt = ((AttributeDefinition) definition).CreateAttribute();
                switch (definition.ControlledType)
                {
                    case AttributeTypeEnum.Boolean:
                        BuildValues(jAtts.Boolean, definition, newAtt);
                        //if (!jAtts.Boolean?.ContainsKey(definition.Name) ?? true) continue;
                        //newAtt.Values = jAtts.Boolean[definition.Name]
                        //    .Select(v => Value.Build(definition.Type, v.Value, RecreateLanguageList(v.Key))).ToList();
                        break;
                    case AttributeTypeEnum.DateTime:
                        BuildValues(jAtts.DateTime, definition, newAtt);
                        //if (!jAtts.DateTime?.ContainsKey(definition.Name) ?? true) continue;
                        //newAtt.Values = jAtts.DateTime[definition.Name]
                        //    .Select(v => Value.Build(definition.Type, v.Value, RecreateLanguageList(v.Key))).ToList();
                        break;
                    case AttributeTypeEnum.Entity:
                        if (!jAtts.Entity?.ContainsKey(definition.Name) ?? true) continue;
                        newAtt.Values = jAtts.Entity[definition.Name]
                            .Select(v => Value.Build(definition.Type, LookupGuids(v.Value), RecreateLanguageList(v.Key),
                                RelLookupList)).ToList();
                        break;
                    case AttributeTypeEnum.Hyperlink:
                        BuildValues(jAtts.Hyperlink, definition,newAtt);
                        //if (!jAtts.Hyperlink?.ContainsKey(definition.Name) ?? true) continue;
                        //newAtt.Values = jAtts.Hyperlink[definition.Name]
                        //    .Select(v => Value.Build(definition.Type, v.Value, RecreateLanguageList(v.Key))).ToList();
                        break;
                    case AttributeTypeEnum.Number:
                        BuildValues(jAtts.Number, definition, newAtt);
                        //if (!jAtts.Number?.ContainsKey(definition.Name) ?? true) continue;
                        //newAtt.Values = jAtts.Number[definition.Name]
                        //    .Select(v => Value.Build(definition.Type, v.Value, RecreateLanguageList(v.Key))).ToList();
                        break;
                    case AttributeTypeEnum.String:
                        BuildValues(jAtts.String, definition, newAtt);
                        //if (!jAtts.String?.ContainsKey(definition.Name) ?? true) continue;
                        //newAtt.Values = jAtts.String[definition.Name]
                        //    .Select(v => Value.Build(definition.Type, v.Value, RecreateLanguageList(v.Key))).ToList();
                        break;
                    case AttributeTypeEnum.Custom:
                        BuildValues(jAtts.Custom, definition, newAtt);
                        //if (!jAtts.Custom?.ContainsKey(definition.Name) ?? true) continue;
                        //newAtt.Values = jAtts.Custom[definition.Name]
                        //    .Select(v => Value.Build(definition.Type, v.Value, RecreateLanguageList(v.Key))).ToList();
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
        }

        private void BuildValues<T>(Dictionary<string, Dictionary<string, T>> list, IAttributeDefinition attrDef, IAttribute target)
        {
            if (!list?.ContainsKey(attrDef.Name) ?? true) return;
            target.Values = list[attrDef.Name]
                .Select(v => Value.Build(attrDef.Type, v.Value, RecreateLanguageList(v.Key))).ToList();

        }

        private List<int?> LookupGuids(List<Guid?> list)
            => list.Select(g => App.Entities.Values.FirstOrDefault(e => e.EntityGuid == g)?.EntityId)
                .ToList();

        private static List<ILanguage> RecreateLanguageList(string languages) 
            => languages == NoLanguage
            ? new List<ILanguage>()
            : languages.Split(',')
                .Select(a => new Dimension {Key = a.Replace(ReadOnlyMarker, ""), ReadOnly = a.StartsWith(ReadOnlyMarker)} as ILanguage)
                .ToList();


        private static Dictionary<string, Dictionary<string, T>> ToTypedDictionary<T>(List<IAttribute> attribs)
            => attribs.Cast<IAttribute<T>>()
                .ToDictionary(
                    a => a.Name,
                    a => a.Typed.ToDictionary(LanguageKey, v => v.TypedContents)
                );

        public List<IEntity> Deserialize(List<string> serialized, bool allowDynamic = false) 
            => serialized.Select(s => Deserialize(s, allowDynamic)).ToList();
    }

}
