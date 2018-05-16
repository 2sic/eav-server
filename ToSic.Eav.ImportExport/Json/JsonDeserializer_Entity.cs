using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Enums;
using ToSic.Eav.ImportExport.Json.Format;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer: IThingDeserializer
    {

        public IEntity Deserialize(string serialized, bool allowDynamic = false, bool skipUnknownType = false) 
            => Deserialize(UnpackAndTestGenericJsonV1(serialized).Entity, allowDynamic, skipUnknownType);


        private static JsonFormat UnpackAndTestGenericJsonV1(string serialized)
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

            if (jsonObj._.V != 1)
                throw new ArgumentOutOfRangeException(nameof(serialized), "unexpected format version");
            return jsonObj;
        }

        public IEntity Deserialize(JsonEntity jEnt, bool allowDynamic, bool skipUnknownType)
        {
            Log.Add($"deserializing {jEnt.Guid} with allowDyn:{allowDynamic} skipUnknown:{skipUnknownType}");

            // get type def - use dynamic if dynamic is allowed OR if we'll skip unknown types
            IContentType contentType = GetContentType(jEnt.Type.Id)
                                       ?? (allowDynamic || skipUnknownType
                                           ? ContentTypeBuilder.DynamicContentType(AppId, jEnt.Type.Name, jEnt.Type.Id)
                                           : throw new FormatException(
                                               "type not found for deserialization and dynamic not allowed " +
                                               $"- cannot continue with {jEnt.Type.Id}")
                                       );

            // Metadata
            var ismeta = new MetadataFor();
            if (jEnt.For != null)
            {
                var md = jEnt.For;
                ismeta.TargetType = Factory.Resolve<IGlobalMetadataProvider>().GetType(md.Target);
                ismeta.KeyGuid = md.Guid;
                ismeta.KeyNumber = md.Number;
                ismeta.KeyString = md.String;
            }

            var newEntity = EntityBuilder.EntityFromRepository(AppId, jEnt.Guid, jEnt.Id, jEnt.Id, ismeta, contentType, true,
                AppPackageOrNull, DateTime.Now, jEnt.Owner, jEnt.Version);

            // check if metadata was included
            if (jEnt.Metadata != null)
            {
                var mdItems = jEnt.Metadata
                    .Select(m => Deserialize(m, allowDynamic, skipUnknownType))
                    .ToList();
                newEntity.Metadata.Use(mdItems);
            }

            // build attributes - based on type definition
            if (contentType.IsDynamic)
            {
                if (allowDynamic)
                    BuildAttribsOfUnknownContentType(jEnt.Attributes, newEntity);
                else
                    Log.Add("will not resolve attributes because dynamic not allowed, but skip was ok");
            }
            else
                BuildAttribsOfKnownType(jEnt.Attributes, contentType, newEntity);

            return newEntity;
        }

        private void BuildAttribsOfUnknownContentType(JsonAttributes jAtts, Entity newEntity)
        {
            BuildAttrib(jAtts.DateTime, AttributeTypeEnum.DateTime, newEntity);
            BuildAttrib(jAtts.Boolean, AttributeTypeEnum.Boolean, newEntity);
            BuildAttrib(jAtts.Custom, AttributeTypeEnum.Custom, newEntity);
            BuildAttrib(jAtts.Entity, AttributeTypeEnum.Entity, newEntity);
            BuildAttrib(jAtts.Hyperlink, AttributeTypeEnum.Hyperlink, newEntity);
            BuildAttrib(jAtts.Number, AttributeTypeEnum.Number, newEntity);
            BuildAttrib(jAtts.String, AttributeTypeEnum.String, newEntity);
        }

        private void BuildAttrib<T>(Dictionary<string, Dictionary<string, T>> list, AttributeTypeEnum type, Entity newEntity)
        {
            if (list == null) return;

            foreach (var attrib in list)
            {
                var newAtt = AttributeBase.CreateTypedAttribute(attrib.Key, type, attrib.Value
                    .Select(v => ValueBuilder.Build(type, v.Value, RecreateLanguageList(v.Key))).ToList());
                newEntity.Attributes.Add(newAtt.Name, newAtt);
            }
        }

        private void BuildAttribsOfKnownType(JsonAttributes jAtts, IContentType contentType, Entity newEntity)
        {
            foreach (var definition in contentType.Attributes)
            {
                var newAtt = ((AttributeDefinition) definition).CreateAttribute();
                switch (definition.ControlledType)
                {
                    case AttributeTypeEnum.Boolean:
                        BuildValues(jAtts.Boolean, definition, newAtt);
                        break;
                    case AttributeTypeEnum.DateTime:
                        BuildValues(jAtts.DateTime, definition, newAtt);
                        break;
                    case AttributeTypeEnum.Entity:
                        if (!jAtts.Entity?.ContainsKey(definition.Name) ?? true)
                            break; // just keep the empty definition, as that's fine
                        newAtt.Values = jAtts.Entity[definition.Name]
                            .Select(v => ValueBuilder.Build(definition.Type, v.Value, RecreateLanguageList(v.Key),
                                RelLookupList)).ToList();
                        break;
                    case AttributeTypeEnum.Hyperlink:
                        BuildValues(jAtts.Hyperlink, definition,newAtt);
                        break;
                    case AttributeTypeEnum.Number:
                        BuildValues(jAtts.Number, definition, newAtt);
                        break;
                    case AttributeTypeEnum.String:
                        BuildValues(jAtts.String, definition, newAtt);
                        break;
                    case AttributeTypeEnum.Custom:
                        BuildValues(jAtts.Custom, definition, newAtt);
                        break;
                    // ReSharper disable RedundantCaseLabel
                    case AttributeTypeEnum.Empty:
                    case AttributeTypeEnum.Undefined:
                        // ReSharper restore RedundantCaseLabel
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                newEntity.Attributes.Add(newAtt.Name, newAtt);

                if (definition.IsTitle)
                    newEntity.SetTitleField(definition.Name);
            }
        }

        private void BuildValues<T>(Dictionary<string, Dictionary<string, T>> list, IAttributeDefinition attrDef, IAttribute target)
        {
            if (!list?.ContainsKey(attrDef.Name) ?? true) return;
            target.Values = list[attrDef.Name]
                .Select(v => ValueBuilder.Build(attrDef.Type, v.Value, RecreateLanguageList(v.Key))).ToList();

        }

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
