using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.DataSources.Queries;

namespace ToSic.Eav.DataSources
{
    // Todo notes 2dm 2021-04-07
    // 1. Attributes should be made with Attribute Builder
    
    /// <inheritdoc />
    /// <summary>
    /// Remodels multi-language values in own fields (like NameDe, NameEn) to single multi-language fields
    /// </summary>
    [VisualQuery(
        GlobalName = "f390e460-46ff-4a6e-883f-f50fdeb363ee",
        NiceName = "Language Modeler",
        UiHint = "Combine values to multi-language values",
        Icon = "translate",
        PreviousNames = new[]
        {
            "f390e460-46ff-4a6e-883f-f50fdeb363ee",
            "ToSic.Eav.DataSources.FieldMapping, ToSic.Eav.DataSources.SharePoint" // originally came from SharePoint
        },
        Type = DataSourceType.Modify,
        ExpectsDataOfType = "7b4fce73-9c29-4517-af14-0a704da5b958",
        In = new[] { Constants.DefaultStreamName + "*" },
        HelpLink = "")] // ToDo: Helplink

    public sealed class LanguageModeler : DataSourceBase
    {
        private readonly AttributeBuilder _attributeBuilder;

        #region Constants / Properties

        public override string LogId => "Ds.FldMap"; // this text is added to all internal logs, so it's easier to debug

        private const string FieldMapConfigKey = "FieldMap";

        public string FieldMap
        {
            get => Configuration[FieldMapConfigKey];
            set => Configuration[FieldMapConfigKey] = value;
        }
        #endregion

        /// <summary>
        /// Initializes this data source
        /// </summary>
        public LanguageModeler(AttributeBuilder attributeBuilder)
        {
            _attributeBuilder = attributeBuilder;
            // Specify what out-streams this data-source provides. Usually just one, called "Default"
            Provide(MapLanguagesIntoValues);

            // Register the configurations we want as tokens, so that the values will be injected later on
            ConfigMask(FieldMapConfigKey, $"[Settings:{FieldMapConfigKey}]");
        }

        /// <summary>
        /// Internal helper that returns the entities
        /// </summary>
        /// <returns></returns>
        private IImmutableList<IEntity> MapLanguagesIntoValues()
        {
            var wrapLog = Log.Call<IImmutableList<IEntity>>();
            Configuration.Parse();

            #region Load configuration and verify everything is ok - or return an error-stream
            
            var fieldMap = FieldMap
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => new LanguageMap(line))
                .ToArray();
            
            var mapErrors = fieldMap
                .Where(lm => !string.IsNullOrWhiteSpace(lm.Error))
                .Select(lm => $"'{lm.Original}': {lm.Error}");
            
            var fieldMapErrors = string.Join(";", mapErrors);
            if (!string.IsNullOrWhiteSpace(fieldMapErrors)) 
                return wrapLog("error", SetError("Field Map Error", fieldMapErrors));
            #endregion
            
            Log.Add($"Field Map created - has {fieldMap.Length} parts");
            
            if (!GetRequiredInList(out var originals))
                return wrapLog("error", originals);

            var result = new List<IEntity>();
            foreach (var entity in originals)
            {
                var modifiedEntity = EntityBuilder.FullClone(entity, entity.Attributes.Copy(),
                    (entity.Relationships as RelationshipManager)?.AllRelationships);

                var attributes = modifiedEntity.Attributes;

                foreach(var map in fieldMap)
                    // if source value contains = it must be a language mapping
                    if (map.HasLanguages)
                    {
                        var allSourceAttrs = map.Fields.Select(f => f.OriginalField);

                        // inherit type from an existing value (we try all attributes to prevent issues)
                        var firstExistingValue = allSourceAttrs
                            .Where(s => attributes.ContainsKey(s))
                            .Select(s => attributes[s]).FirstOrDefault();
                        var newAttribute =
                            AttributeBuilder.CreateTyped(map.Target,
                                firstExistingValue?.Type ??
                                "string"); // if there are no values, we assume it's a string field
                        attributes.Add(map.Target, newAttribute);

                        foreach (var entry in map.Fields)
                        {
                            if (!attributes.ContainsKey( /*langSource*/entry.OriginalField))
                            {
                                // do not create values for fields which do not exist
                                Log.Add(
                                    $"Field mapping ignored for entity {entity.EntityId} and language { /*lang*/entry.Language} because source attribute { /*langSource*/entry.OriginalField} does not exist.");
                                continue;
                            }

                            var value = attributes[ /*langSource*/entry.OriginalField].Values.FirstOrDefault()
                                ?.ObjectContents;
                            _attributeBuilder.AddValue(attributes, map.Target, value, newAttribute.Type, /*lang*/ entry.Language);
                            //attributes.AddValue(map.Target, v, newAttribute.Type, /*lang*/ entry.Language);
                            attributes.Remove( /*langSource*/entry.OriginalField);
                        }
                    }
                    else // simple re-mapping / renaming
                    {
                        if (!attributes.ContainsKey(map.Source))
                        {
                            Log.Add(
                                $"Field mapping not possible for entity {entity.EntityId} because source attribute {map.Source} does not exist.");
                            continue;
                        }

                        // Make a copy to make sure the Name property of the attribute is set correctly
                        var sourceAttr = attributes[map.Source];
                        var newAttribute = AttributeBuilder.CreateTyped(map.Target, sourceAttr.Type,
                            (List<IValue>) sourceAttr.Values);
                        attributes.Add(map.Target, newAttribute);
                        attributes.Remove(map.Source);
                    }

                result.Add(modifiedEntity);
            }

            var immutableResults = result.ToImmutableArray();
            return wrapLog($"{immutableResults.Length}", immutableResults);
        }
    }



}
