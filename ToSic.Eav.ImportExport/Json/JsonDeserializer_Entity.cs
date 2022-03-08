using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Logging;
using ToSic.Eav.Metadata;
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

        protected JsonFormat UnpackAndTestGenericJsonV1(string serialized)
        {
            var wrapLog = Log.Call();
            JsonFormat jsonObj;
            try
            {
                jsonObj = JsonConvert.DeserializeObject<JsonFormat>(serialized, JsonSettings.Defaults());
            }
            catch (Exception ex)
            {
                const string msg = "cannot deserialize json - bad format";
                wrapLog(msg);
                throw new FormatException(msg, ex);
            }

            if (jsonObj._.V != 1)
                throw new ArgumentOutOfRangeException(nameof(serialized), "unexpected format version");
            wrapLog("ok");
            return jsonObj;
        }

        public IEntity Deserialize(JsonEntity jEnt, bool allowDynamic, bool skipUnknownType, IEntitiesSource dynRelationshipsSource = null)
        {
            var wrapLog = Log.Call($"guid: {jEnt.Guid}; allowDynamic:{allowDynamic} skipUnknown:{skipUnknownType}");

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
                Log.Add($"this is metadata; will construct 'For' object. Type: {md.Target} ({md.TargetType})");
                ismeta.TargetType = md.TargetType != 0 ? md.TargetType : MetadataTargets.GetId(md.Target); // #TargetTypeIdInsteadOfTarget
                ismeta.KeyGuid = md.Guid;
                ismeta.KeyNumber = md.Number;
                ismeta.KeyString = md.String;
            }

            Log.Add("build entity");
            var newEntity = MultiBuilder.Entity.EntityFromRepository(AppId, jEnt.Guid, jEnt.Id, jEnt.Id, ismeta, contentType, true,
                AppPackageOrNull, DateTime.MinValue, DateTime.Now, jEnt.Owner, jEnt.Version);

            // check if metadata was included
            if (jEnt.Metadata != null)
            {
                Log.Add("found more metadata, will deserialize");
                var mdItems = jEnt.Metadata
                    .Select(m => Deserialize(m, allowDynamic, skipUnknownType))
                    .ToList();
                ((IMetadataInternals)newEntity.Metadata).Use(mdItems);
            }

            // build attributes - based on type definition
            if (contentType.IsDynamic)
            {
                if (allowDynamic)
                    BuildAttribsOfUnknownContentType(jEnt.Attributes, newEntity, dynRelationshipsSource);
                else
                    Log.Add("will not resolve attributes because dynamic not allowed, but skip was ok");
            }
            else
            {
                // Note v12.03: even though we're using known types, ATM there are edge cases 
                // with system types where the type is known, but the entities-source is not a full app-state
                // We'll probably correct this some day, but for now we're including the relationshipSource if defined
                BuildAttribsOfKnownType(jEnt.Attributes, contentType, newEntity, dynRelationshipsSource);
            }

            wrapLog("ok");
            return newEntity;
        }

        private void BuildAttribsOfUnknownContentType(JsonAttributes jAtts, Entity newEntity, IEntitiesSource relationshipsSource = null)
        {
            var wrapLog = Log.Call();
            BuildAttrib(jAtts.DateTime, ValueTypes.DateTime, newEntity, null);
            BuildAttrib(jAtts.Boolean, ValueTypes.Boolean, newEntity, null);
            BuildAttrib(jAtts.Custom, ValueTypes.Custom, newEntity, null);
            BuildAttrib(jAtts.Json, ValueTypes.Json, newEntity, null);
            BuildAttrib(jAtts.Entity, ValueTypes.Entity, newEntity, relationshipsSource);
            BuildAttrib(jAtts.Hyperlink, ValueTypes.Hyperlink, newEntity, null);
            BuildAttrib(jAtts.Number, ValueTypes.Number, newEntity, null);
            BuildAttrib(jAtts.String, ValueTypes.String, newEntity, null);
            wrapLog("ok");
        }

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

        private void BuildAttribsOfKnownType(JsonAttributes jAtts, IContentType contentType, Entity newEntity, IEntitiesSource relationshipsSource = null)
        {
            var wrapLog = Log.Call();
            foreach (var definition in contentType.Attributes)
            {
                var newAtt = MultiBuilder.Attribute.CreateTyped(definition.Name, definition.Type); // ((ContentTypeAttribute) definition).CreateAttribute();
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
            wrapLog("ok");
        }

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


        private static Dictionary<string, Dictionary<string, T>> ToTypedDictionary<T>(List<IAttribute> attribs, ILog log)
        {
            var result = new Dictionary<string, Dictionary<string, T>>();
            attribs.Cast<IAttribute<T>>().ToList().ForEach(a =>
            {
                Dictionary<string, T> dimensions;
                try
                {
                    dimensions = a.Typed.ToDictionary(LanguageKey, v => v.TypedContents);
                }
                catch
                {
                    string langList = null;
                    try
                    {
                        langList = string.Join(",", a.Typed.Select(LanguageKey));
                    }
                    catch { /* ignore */ }
                    log.Warn($"Error building languages list on '{a.Name}', probably multiple identical keys: {langList}");
                    throw;
                }
                try
                {
                    result.Add(a.Name, dimensions);
                }
                catch
                {
                    log.Warn($"Error adding attribute '{a.Name}' to dictionary, probably multiple identical keys");
                    throw;
                }

                //.ToDictionary(
                //    a => a.Name,
                //    a => a.Typed.ToDictionary(LanguageKey, v => v.TypedContents)
                //)               
            });
            return result;
        }

        public List<IEntity> Deserialize(List<string> serialized, bool allowDynamic = false) 
            => serialized.Select(s => Deserialize(s, allowDynamic)).ToList();
    }

}
