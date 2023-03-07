using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Source;
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
                                  // Services.DataBuilder.ContentType.Transient(AppId, jEnt.Type.Name, jEnt.Type.Id)
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
            IImmutableDictionary<string, IAttribute> attributes = Services.DataBuilder.Attribute.Empty();
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
                attributes = BuildAttribsOfKnownType(jEnt.Attributes, contentType, dynRelationshipsSource);
            }


            l.A("build entity");
            var partsBuilder = EntityPartsBuilder.ForAppAndOptionalMetadata(source: AppPackageOrNull, metadata: mdItems);
            var newEntity = Services.DataBuilder.Entity.Create(
                appId: AppId,
                guid: jEnt.Guid,
                entityId: jEnt.Id,
                repositoryId: jEnt.Id,
                attributes: attributes,
                metadataFor: target,
                contentType: contentType,
                isPublished: true,
                //source: AppPackageOrNull,
                //metadataItems: mdItems,
                partsBuilder: partsBuilder,
                created: DateTime.MinValue,
                modified: DateTime.Now,
                owner: jEnt.Owner,
                version: jEnt.Version);


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
                BuildAttrib(jAtts.DateTime, ValueTypes.DateTime, null),
                BuildAttrib(jAtts.Boolean, ValueTypes.Boolean, null),
                BuildAttrib(jAtts.Custom, ValueTypes.Custom, null),
                BuildAttrib(jAtts.Json, ValueTypes.Json, null),
                BuildAttrib(jAtts.Entity, ValueTypes.Entity, relationshipsSource),
                BuildAttrib(jAtts.Hyperlink, ValueTypes.Hyperlink, null),
                BuildAttrib(jAtts.Number, ValueTypes.Number, null),
                BuildAttrib(jAtts.String, ValueTypes.String, null)
            };
            var final = attribs
                .Where(dic => dic != null)
                .SelectMany(pair => pair)
                .ToImmutableDictionary(pair => pair.Key, pair => pair.Value, InvariantCultureIgnoreCase);

            return final;
        });

        private Dictionary<string, IAttribute> BuildAttrib<T>(Dictionary<string, Dictionary<string, T>> list, ValueTypes type, IEntitiesSource relationshipsSource)
        {
            if (list == null) return null;

            var builder = Services.DataBuilder;
            var newAttributes = list.ToDictionary(
                a => a.Key,
                attrib => builder.Attribute.Create(attrib.Key, type,
                    attrib.Value.Select(v =>
                            builder.Value.Build(type, v.Value, RecreateLanguageList(v.Key),
                                relationshipsSource))
                        .ToList()),
                InvariantCultureIgnoreCase);


            return newAttributes;
        }

        private IImmutableDictionary<string, IAttribute> BuildAttribsOfKnownType(JsonAttributes jAtts, IContentType contentType, IEntitiesSource relationshipsSource = null
        ) => Log.Func(() => contentType.Attributes.ToImmutableDictionary(
            a => a.Name, 
            a => 
            {
                var values = GetValues(a, jAtts, relationshipsSource);
                return Services.DataBuilder.Attribute.Create(a.Name, a.Type, values);
            },
            InvariantCultureIgnoreCase));

        private IList<IValue> GetValues(IContentTypeAttribute a, JsonAttributes jAtts, IEntitiesSource relationshipsSource = null)
        {
            switch (a.Type)
            {
                case ValueTypes.Boolean: return BuildValues(jAtts.Boolean, a);
                case ValueTypes.DateTime:
                    return BuildValues(jAtts.DateTime, a);
                case ValueTypes.Entity:
                    if (!jAtts.Entity?.ContainsKey(a.Name) ?? true)
                        return new List<IValue>(); // just keep the empty definition, as that's fine
                    return jAtts.Entity[a.Name].Select(v => Services.DataBuilder.Value.Build(
                            a.Type,
                            v.Value,
                            // 2023-02-24 2dm #immutable
                            //RecreateLanguageList(v.Key),
                            DimensionBuilder.NoLanguages,
                            relationshipsSource ?? LazyRelationshipLookupList))
                        .ToList();
                case ValueTypes.Hyperlink: return BuildValues(jAtts.Hyperlink, a);
                case ValueTypes.Number: return BuildValues(jAtts.Number, a);
                case ValueTypes.String: return BuildValues(jAtts.String, a);
                case ValueTypes.Custom: return BuildValues(jAtts.Custom, a);
                case ValueTypes.Json: return BuildValues(jAtts.Json, a);
                // ReSharper disable RedundantCaseLabel
                case ValueTypes.Empty:
                case ValueTypes.Undefined:
                    // ReSharper restore RedundantCaseLabel
                    return new List<IValue>();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IList<IValue> BuildValues<T>(Dictionary<string, Dictionary<string, T>> list, IContentTypeAttribute attrDef)
        {
            if (!list?.ContainsKey(attrDef.Name) ?? true) return new List<IValue>();
            return list[attrDef.Name]
                .Select(v => Services.DataBuilder.Value.Build(attrDef.Type, v.Value, RecreateLanguageList(v.Key)))
                .ToList();

        }

        //private IList<IValue> BuildValues<T>(Dictionary<string, Dictionary<string, T>> list, IContentTypeAttribute attrDef, IAttribute target)
        //{
        //    if (!list?.ContainsKey(attrDef.Name) ?? true) return new List<IValue>();
        //    return target.Values = list[attrDef.Name]
        //        .Select(v => Services.DataBuilder.Value.Build(attrDef.Type, v.Value, RecreateLanguageList(v.Key)))
        //        .ToList();

        //}

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
