using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Serialization;
using static System.StringComparer;
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

        public JsonFormat UnpackAndTestGenericJsonV1(string serialized) => Log.Func(l =>
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

        public IEntity Deserialize(JsonEntity jEnt,
            bool allowDynamic,
            bool skipUnknownType,
            IEntitiesSource dynRelationshipsSource = default
        ) => Log.Func($"guid: {jEnt.Guid}; allowDynamic:{allowDynamic} skipUnknown:{skipUnknownType}", l =>
        {
            // get type def - use dynamic if dynamic is allowed OR if we'll skip unknown types
            var contentType = GetContentType(jEnt.Type.Id)
                              ?? (allowDynamic || skipUnknownType
                                  ? GetTransientContentType(jEnt.Type.Name, jEnt.Type.Id) 
                                  // Services.MultiBuilder.ContentType.Transient(AppId, jEnt.Type.Name, jEnt.Type.Id)
                                  : throw new FormatException(
                                      "type not found for deserialization and dynamic not allowed " +
                                      $"- cannot continue with {jEnt.Type.Id}")
                              );

            // Metadata Target
            var target = DeserializeEntityTarget(jEnt);

            // Metadata Items - it's important that they are null, if no metadata was specified
            // This is to ensure that without own metadata, it will access the app (if available) to find metadata.
            l.A($"found metadata: {jEnt.Metadata != null}, will deserialize");
            var mdItems = jEnt.Metadata?
                .Select(m => Deserialize(m, allowDynamic, skipUnknownType))
                .ToList();

            // build attributes - based on type definition
            IImmutableDictionary<string, IAttribute> attributes = Services.MultiBuilder.Attribute.Empty();// new Dictionary<string, IAttribute>();
            if (contentType.IsDynamic)
            {
                if (allowDynamic)
                    attributes = BuildAttribsOfUnknownContentType(jEnt.Attributes, null, dynRelationshipsSource);
                else
                    l.A("will not resolve attributes because dynamic not allowed, but skip was ok");
            }
            else
            {
                // Note v12.03: even though we're using known types, ATM there are edge cases 
                // with system types where the type is known, but the entities-source is not a full app-state
                // We'll probably correct this some day, but for now we're including the relationshipSource if defined
                attributes = BuildAttribsOfKnownType(jEnt.Attributes, contentType, null, dynRelationshipsSource);
            }


            l.A("build entity");
            var newEntity = Services.MultiBuilder.Entity.EntityFromRepository(
                appId: AppId, entityGuid: jEnt.Guid, entityId: jEnt.Id, repositoryId: jEnt.Id,
                attributes: attributes,
                metadataFor: target, type: contentType,
                isPublished: true,
                source: AppPackageOrNull, metadataItems: mdItems,
                created: DateTime.MinValue, modified: DateTime.Now,
                owner: jEnt.Owner, version: jEnt.Version);

            //if (jEnt.Metadata != null)
            //{

            //    // Attach the metadata, ensure that it won't reload data from the App otherwise the metadata would get reset again
            //    ((IMetadataInternals)newEntity.Metadata).Use(mdItems, false);
            //}

            
            return (newEntity, l.Try(() => $"'{newEntity?.GetBestTitle()}'", "can't get title"));
        });

        private Target DeserializeEntityTarget(JsonEntity jEnt) => Log.Func(() =>
        {
            if (jEnt.For == null) return (new Target(), "no for found");

            var mdFor = jEnt.For;
            var target = new Target(
                targetType: mdFor.TargetType != 0
                    ? mdFor.TargetType
                    : MetadataTargets.GetId(mdFor.Target), identifier // #TargetTypeIdInsteadOfTarget
                : null,
                keyString: mdFor.String,
                keyNumber: mdFor.Number,
                keyGuid: mdFor.Guid
            );

            return (target, $"this is metadata; will construct 'For' object. Type: {mdFor.Target} ({mdFor.TargetType})");
        });

        private IImmutableDictionary<string, IAttribute> BuildAttribsOfUnknownContentType(JsonAttributes jAtts, Entity newEntity, IEntitiesSource relationshipsSource = null) => Log.Func(() =>
        {
            var attribs = new Dictionary<string, IAttribute>[]
            {
                BuildAttrib(jAtts.DateTime, ValueTypes.DateTime, newEntity, null),
                BuildAttrib(jAtts.Boolean, ValueTypes.Boolean, newEntity, null),
                BuildAttrib(jAtts.Custom, ValueTypes.Custom, newEntity, null),
                BuildAttrib(jAtts.Json, ValueTypes.Json, newEntity, null),
                BuildAttrib(jAtts.Entity, ValueTypes.Entity, newEntity, relationshipsSource),
                BuildAttrib(jAtts.Hyperlink, ValueTypes.Hyperlink, newEntity, null),
                BuildAttrib(jAtts.Number, ValueTypes.Number, newEntity, null),
                BuildAttrib(jAtts.String, ValueTypes.String, newEntity, null)
            };
            var final = attribs
                .Where(dic => dic != null)
                .SelectMany(pair => pair)
                .ToImmutableDictionary(pair => pair.Key, pair => pair.Value, InvariantCultureIgnoreCase);

            return final;
        });

        private Dictionary<string, IAttribute> BuildAttrib<T>(Dictionary<string, Dictionary<string, T>> list, ValueTypes type, Entity newEntity, IEntitiesSource relationshipsSource)
        {
            if (list == null) return null;

            var newAttributes = list.ToDictionary(
                    a => a.Key,
                    attrib
                    => Services.MultiBuilder.Attribute.CreateTyped(attrib.Key, type,
                        attrib.Value.Select(v =>
                                Services.MultiBuilder.Value.Build(type, v.Value, RecreateLanguageList(v.Key),
                                    relationshipsSource))
                            .ToList()),
                    InvariantCultureIgnoreCase);
                

            //foreach (var attrib in list)
            //{
            //    var newAtt = Services.MultiBuilder.Attribute.CreateTyped(attrib.Key, type, attrib.Value
            //        .Select(v => Services.MultiBuilder.Value.Build(type, v.Value, RecreateLanguageList(v.Key), relationshipsSource))
            //        .ToList());
            //    // #immutableTodo
            //    newEntity.Attributes.Add(newAtt.Name, newAtt);
            //}

            return newAttributes;
        }

        private IImmutableDictionary<string, IAttribute> BuildAttribsOfKnownType(JsonAttributes jAtts, IContentType contentType, Entity newEntity, IEntitiesSource relationshipsSource = null
        ) => Log.Func(() => contentType.Attributes.ToImmutableDictionary(
            a => a.Name, 
            a => 
            {
                var newAtt = Services.MultiBuilder.Attribute.CreateTyped(a.Name, a.Type);
                switch (a.ControlledType)
                {
                    case ValueTypes.Boolean:
                        BuildValues(jAtts.Boolean, a, newAtt);
                        break;
                    case ValueTypes.DateTime:
                        BuildValues(jAtts.DateTime, a, newAtt);
                        break;
                    case ValueTypes.Entity:
                        if (!jAtts.Entity?.ContainsKey(a.Name) ?? true)
                            break; // just keep the empty definition, as that's fine
                        newAtt.Values = jAtts.Entity[a.Name]
                            .Select(v => Services.MultiBuilder.Value.Build(
                                a.Type,
                                v.Value,
                                // 2023-02-24 2dm #immutable
                                //RecreateLanguageList(v.Key),
                                DimensionBuilder.NoLanguages,
                                relationshipsSource ?? LazyRelationshipLookupList))
                            .ToList();
                        break;
                    case ValueTypes.Hyperlink:
                        BuildValues(jAtts.Hyperlink, a, newAtt);
                        break;
                    case ValueTypes.Number:
                        BuildValues(jAtts.Number, a, newAtt);
                        break;
                    case ValueTypes.String:
                        BuildValues(jAtts.String, a, newAtt);
                        break;
                    case ValueTypes.Custom:
                        BuildValues(jAtts.Custom, a, newAtt);
                        break;
                    case ValueTypes.Json:
                        BuildValues(jAtts.Json, a, newAtt);
                        break;
                    // ReSharper disable RedundantCaseLabel
                    case ValueTypes.Empty:
                    case ValueTypes.Undefined:
                        // ReSharper restore RedundantCaseLabel
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return newAtt;
            },
            InvariantCultureIgnoreCase));

        private void BuildValues<T>(Dictionary<string, Dictionary<string, T>> list, IContentTypeAttribute attrDef, IAttribute target)
        {
            if (!list?.ContainsKey(attrDef.Name) ?? true) return;
            target.Values = list[attrDef.Name]
                .Select(v => Services.MultiBuilder.Value.Build(attrDef.Type, v.Value, RecreateLanguageList(v.Key)))
                .ToList();

        }

        private static IImmutableList<ILanguage> RecreateLanguageList(string languages) 
            => languages == NoLanguage
            ? DimensionBuilder.NoLanguages
            : languages.Split(',')
                // 2023-02-24 2dm #immutable
                //.Select(a => new Language { Key = a.Replace(ReadOnlyMarker, ""), ReadOnly = a.StartsWith(ReadOnlyMarker) } as ILanguage)
                .Select(a => new Language(a.Replace(ReadOnlyMarker, ""), a.StartsWith(ReadOnlyMarker)) as ILanguage)
                .ToImmutableList();


        private Dictionary<string, Dictionary<string, string>> ConvertReferences(Dictionary<string, Dictionary<string, string>> links, Guid entityGuid
        ) => Log.Func(l =>
        {
            try
            {
                var converter = ((MyServices)Services).ValueConverter.Value;
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
