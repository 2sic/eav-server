using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Serialization;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer
    {

        public IEntity Deserialize(string serialized, bool allowDynamic = false, bool skipUnknownType = false) 
            => Deserialize(UnpackAndTestGenericJsonV1(serialized).Entity, allowDynamic, skipUnknownType);

        internal IEntity DeserializeWithRelsWip(string serialized, int id, bool allowDynamic = false, bool skipUnknownType = false, IEntitiesSource dynRelationshipsSource = null)
        {
            var jsonEntity = UnpackAndTestGenericJsonV1(serialized).Entity;
            jsonEntity.Id = id;
            var entity = Deserialize(jsonEntity, allowDynamic, skipUnknownType, dynRelationshipsSource);
            return entity;
        }

        internal JsonFormat UnpackAndTestGenericJsonV1(string serialized) => Log.Func(l =>
        {
            JsonFormat jsonObj;
            try
            {
                jsonObj = System.Text.Json.JsonSerializer.Deserialize<JsonFormat>(serialized, JsonOptions.UnsafeJsonWithoutEncodingHtml);
            }
            catch (Exception ex)
            {
                throw l.Ex(new FormatException("cannot deserialize json - bad format", ex));
            }

            if (jsonObj._.V != 1)
                throw new ArgumentOutOfRangeException(nameof(serialized), "unexpected format version");
            return (jsonObj, "ok");
        });

        public IEntity Deserialize(JsonEntity jEnt, bool allowDynamic, bool skipUnknownType, IEntitiesSource dynRelationshipsSource = null
        ) => Log.Func($"guid: {jEnt.Guid}; allowDynamic:{allowDynamic} skipUnknown:{skipUnknownType}", l =>
        {
            // get type def - use dynamic if dynamic is allowed OR if we'll skip unknown types
            var contentType = GetContentType(jEnt.Type.Id)
                                       ?? (allowDynamic || skipUnknownType
                                           ? MultiBuilder.ContentType.Transient(AppId, jEnt.Type.Name, jEnt.Type.Id)
                                           : throw new FormatException(
                                               "type not found for deserialization and dynamic not allowed " +
                                               $"- cannot continue with {jEnt.Type.Id}")
                                       );

            // Metadata
            var ismeta = new Target();
            if (jEnt.For != null)
            {
                var md = jEnt.For;
                Log.A($"this is metadata; will construct 'For' object. Type: {md.Target} ({md.TargetType})");
                ismeta.TargetType = md.TargetType != 0 ? md.TargetType : MetadataTargets.GetId(md.Target); // #TargetTypeIdInsteadOfTarget
                ismeta.KeyGuid = md.Guid;
                ismeta.KeyNumber = md.Number;
                ismeta.KeyString = md.String;
            }

            l.A("build entity");
            var newEntity = MultiBuilder.Entity.EntityFromRepository(AppId, jEnt.Guid, jEnt.Id, jEnt.Id, ismeta, contentType, true,
                AppPackageOrNull, DateTime.MinValue, DateTime.Now, jEnt.Owner, jEnt.Version);

            // check if metadata was included
            if (jEnt.Metadata != null)
            {
                l.A("found more metadata, will deserialize");
                var mdItems = jEnt.Metadata
                    .Select(m => Deserialize(m, allowDynamic, skipUnknownType))
                    .ToList();

                // Attach the metadata, ensure that it won't reload data from the App otherwise the metadata would get reset again
                ((IMetadataInternals)newEntity.Metadata).Use(mdItems, false);
            }

            // build attributes - based on type definition
            if (contentType.IsDynamic)
            {
                if (allowDynamic)
                    BuildAttribsOfUnknownContentType(jEnt.Attributes, newEntity, dynRelationshipsSource);
                else
                    l.A("will not resolve attributes because dynamic not allowed, but skip was ok");
            }
            else
            {
                // Note v12.03: even though we're using known types, ATM there are edge cases 
                // with system types where the type is known, but the entities-source is not a full app-state
                // We'll probably correct this some day, but for now we're including the relationshipSource if defined
                BuildAttribsOfKnownType(jEnt.Attributes, contentType, newEntity, dynRelationshipsSource);
            }

            return (newEntity, "ok");
        });

        private void BuildAttribsOfUnknownContentType(JsonAttributes jAtts, Entity newEntity, IEntitiesSource relationshipsSource = null) => Log.Do(() =>
        {
            BuildAttrib(jAtts.DateTime, ValueTypes.DateTime, newEntity, null);
            BuildAttrib(jAtts.Boolean, ValueTypes.Boolean, newEntity, null);
            BuildAttrib(jAtts.Custom, ValueTypes.Custom, newEntity, null);
            BuildAttrib(jAtts.Json, ValueTypes.Json, newEntity, null);
            BuildAttrib(jAtts.Entity, ValueTypes.Entity, newEntity, relationshipsSource);
            BuildAttrib(jAtts.Hyperlink, ValueTypes.Hyperlink, newEntity, null);
            BuildAttrib(jAtts.Number, ValueTypes.Number, newEntity, null);
            BuildAttrib(jAtts.String, ValueTypes.String, newEntity, null);
        });

        private void BuildAttrib<T>(Dictionary<string, Dictionary<string, T>> list, ValueTypes type, Entity newEntity, IEntitiesSource relationshipsSource)
        {
            if (list == null) return;

            foreach (var attrib in list)
            {
                var newAtt = AttributeBuilder.CreateTyped(attrib.Key, type, attrib.Value
                    .Select(v => MultiBuilder.Value.Build(type, v.Value, RecreateLanguageList(v.Key), relationshipsSource)).ToList());
                newEntity.Attributes.Add(newAtt.Name, newAtt);
            }
        }

        private void BuildAttribsOfKnownType(JsonAttributes jAtts, IContentType contentType, Entity newEntity, IEntitiesSource relationshipsSource = null) => Log.Do(() =>
        {
            foreach (var definition in contentType.Attributes)
            {
                var newAtt = MultiBuilder.Attribute.CreateTyped(definition.Name, definition.Type);
                switch (definition.ControlledType)
                {
                    case ValueTypes.Boolean:
                        BuildValues(jAtts.Boolean, definition, newAtt);
                        break;
                    case ValueTypes.DateTime:
                        BuildValues(jAtts.DateTime, definition, newAtt);
                        break;
                    case ValueTypes.Entity:
                        if (!jAtts.Entity?.ContainsKey(definition.Name) ?? true)
                            break; // just keep the empty definition, as that's fine
                        newAtt.Values = jAtts.Entity[definition.Name]
                            .Select(v => MultiBuilder.Value.Build(
                                definition.Type, 
                                v.Value,
                                RecreateLanguageList(v.Key),
                                relationshipsSource ?? LazyRelationshipLookupList))
                            .ToList();
                        break;
                    case ValueTypes.Hyperlink:
                        BuildValues(jAtts.Hyperlink, definition,newAtt);
                        break;
                    case ValueTypes.Number:
                        BuildValues(jAtts.Number, definition, newAtt);
                        break;
                    case ValueTypes.String:
                        BuildValues(jAtts.String, definition, newAtt);
                        break;
                    case ValueTypes.Custom:
                        BuildValues(jAtts.Custom, definition, newAtt);
                        break;
                    case ValueTypes.Json:
                        BuildValues(jAtts.Json, definition, newAtt);
                        break;
                    // ReSharper disable RedundantCaseLabel
                    case ValueTypes.Empty:
                    case ValueTypes.Undefined:
                        // ReSharper restore RedundantCaseLabel
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                newEntity.Attributes.Add(newAtt.Name, newAtt);

                if (definition.IsTitle)
                    newEntity.SetTitleField(definition.Name);
            }
        });

        private void BuildValues<T>(Dictionary<string, Dictionary<string, T>> list, IContentTypeAttribute attrDef, IAttribute target)
        {
            if (!list?.ContainsKey(attrDef.Name) ?? true) return;
            target.Values = list[attrDef.Name]
                .Select(v => MultiBuilder.Value.Build(attrDef.Type, v.Value, RecreateLanguageList(v.Key))).ToList();

        }

        private static List<ILanguage> RecreateLanguageList(string languages) 
            => languages == NoLanguage
            ? new List<ILanguage>()
            : languages.Split(',')
                .Select(a => new Language {Key = a.Replace(ReadOnlyMarker, ""), ReadOnly = a.StartsWith(ReadOnlyMarker)} as ILanguage)
                .ToList();


        private Dictionary<string, Dictionary<string, string>> ConvertReferences(Dictionary<string, Dictionary<string, string>> links, Guid entityGuid
        ) => Log.Func(l =>
        {
            try
            {
                var converter = ((Dependencies)Deps).ValueConverter.Value;
                var converted = links.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.ToDictionary(
                        val => val.Key,
                        val => converter.ToValue(val.Value, entityGuid)
                    )
                );
                return (converted, "ok");
            }
            catch (Exception ex)
            {
                l.A("Ran into an error. Will log bug ignore and return original");
                l.Ex(ex);
                return (links, "error/ignored");
            }
        });

        private Dictionary<string, Dictionary<string, T>> ToTypedDictionary<T>(List<IAttribute> attribs) => Log.Func(l =>
        {
            var result = new Dictionary<string, Dictionary<string, T>>();
            attribs.Cast<IAttribute<T>>().ToList().ForEach(a =>
            {
                Dictionary<string, T> dimensions;
                try
                {
                    dimensions = a.Typed.ToDictionary(LanguageKey, v => v.TypedContents);
                }
                catch (Exception ex)
                {
                    string langList = null;
                    try
                    {
                        langList = string.Join(",", a.Typed.Select(LanguageKey));
                    }
                    catch { /* ignore */ }
                    l.W($"Error building languages list on '{a.Name}', probably multiple identical keys: {langList}");
                    throw l.Ex(ex);
                }

                try
                {
                    result.Add(a.Name, dimensions);
                }
                catch (Exception ex)
                {
                    l.W($"Error adding attribute '{a.Name}' to dictionary, probably multiple identical keys");
                    throw l.Ex(ex);
                }
            });
            return result;
        });

        public List<IEntity> Deserialize(List<string> serialized, bool allowDynamic = false) 
            => serialized.Select(s => Deserialize(s, allowDynamic)).ToList();
    }

}
