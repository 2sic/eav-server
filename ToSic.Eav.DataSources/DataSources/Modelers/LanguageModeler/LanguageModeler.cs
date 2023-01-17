using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Remodels multi-language values in own fields (like NameDe, NameEn) to single multi-language fields like Name
    /// </summary>
    /// <remarks>
    /// New in v11.20
    /// </remarks>
    [VisualQuery(
        GlobalName = "f390e460-46ff-4a6e-883f-f50fdeb363ee",
        NiceName = "Language Modeler",
        UiHint = "Combine values to multi-language values",
        Icon = Icons.Translate,
        PreviousNames = new[]
        {
            "f390e460-46ff-4a6e-883f-f50fdeb363ee",
            "ToSic.Eav.DataSources.FieldMapping, ToSic.Eav.DataSources.SharePoint" // originally came from SharePoint
        },
        Type = DataSourceType.Modify,
        ExpectsDataOfType = "7b4fce73-9c29-4517-af14-0a704da5b958",
        In = new[] { Constants.DefaultStreamName + "*" },
        HelpLink = "https://r.2sxc.org/DsLanguageModeler")]
    [PublicApi("Brand new in v11.20, WIP, may still change a bit")]
    public sealed class LanguageModeler : DataSource
    {

        #region Constants / Properties

        private const string FieldMapConfigKey = "FieldMap";

        /// <summary>
        /// Contains the field map which configures how fields should be connected.
        /// </summary>
        public string FieldMap
        {
            get => Configuration[FieldMapConfigKey];
            set => Configuration[FieldMapConfigKey] = value;
        }
        #endregion

        /// <summary>
        /// Initializes this data source
        /// </summary>
        [PrivateApi]
        public LanguageModeler(MultiBuilder multiBuilder, Dependencies dependencies): base(dependencies, $"{DataSourceConstants.LogPrefix}.LngMod")
        {
            _multiBuilder = multiBuilder;
            // Specify what out-streams this data-source provides. Usually just one, called "Default"
            Provide(MapLanguagesIntoValues);

            // Register the configurations we want as tokens, so that the values will be injected later on
            ConfigMask(FieldMapConfigKey, $"[Settings:{FieldMapConfigKey}]");
        }

        private readonly MultiBuilder _multiBuilder;


        /// <summary>
        /// Internal helper that returns the entities
        /// </summary>
        /// <returns></returns>
        private IImmutableList<IEntity> MapLanguagesIntoValues() => Log.Func(l =>
        {
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
                return (SetError("Field Map Error", fieldMapErrors), "error");

            #endregion

            l.A($"Field Map created - has {fieldMap.Length} parts");

            if (!GetRequiredInList(out var originals))
                return (originals, "error");

            var result = new List<IEntity>();
            foreach (var entity in originals)
            {
                var modifiedEntity = _multiBuilder.Entity.Clone(entity,
                    _multiBuilder.Attribute.Clone(entity.Attributes), 
                    (entity.Relationships as RelationshipManager)?.AllRelationships);

                var attributes = modifiedEntity.Attributes;

                foreach (var map in fieldMap)
                    // if source value contains = it must be a language mapping
                    if (map.HasLanguages)
                    {
                        var allSourceAttrs = map.Fields.Select(f => f.OriginalField);

                        // inherit type from an existing value (we try all attributes to prevent issues)
                        var firstExistingValue = allSourceAttrs
                            .Where(s => attributes.ContainsKey(s))
                            .Select(s => attributes[s]).FirstOrDefault();
                        var newAttribute =
                            _multiBuilder.Attribute.CreateTyped(map.Target,
                                firstExistingValue?.Type ??
                                "string"); // if there are no values, we assume it's a string field
                        attributes.Add(map.Target, newAttribute);

                        foreach (var entry in map.Fields)
                        {
                            if (!attributes.ContainsKey(entry.OriginalField))
                            {
                                // do not create values for fields which do not exist
                                l.A($"Field mapping ignored for entity {entity.EntityId} and language {entry.Language} because source attribute {entry.OriginalField} does not exist.");
                                continue;
                            }

                            var value = attributes[entry.OriginalField].Values.FirstOrDefault()
                                ?.ObjectContents;
                            // Remove first, in case the new name replaces an old one
                            attributes.Remove(entry.OriginalField);
                            // Now add the resulting new attribute
                            _multiBuilder.Attribute.AddValue(attributes, map.Target, value, newAttribute.Type, entry.Language);
                        }
                    }
                    else // simple re-mapping / renaming
                    {
                        if (!attributes.ContainsKey(map.Source))
                        {
                            l.A($"Field mapping not possible for entity {entity.EntityId} because source attribute {map.Source} does not exist.");
                            continue;
                        }

                        // Make a copy to make sure the Name property of the attribute is set correctly
                        var sourceAttr = attributes[map.Source];
                        var newAttribute = _multiBuilder.Attribute.CreateTyped(map.Target, sourceAttr.Type,
                            (List<IValue>)sourceAttr.Values);
                        // Remove first, in case the new name replaces an old one
                        attributes.Remove(map.Source);
                        // Now add the resulting new attribute
                        attributes.Add(map.Target, newAttribute);
                    }

                result.Add(modifiedEntity);
            }

            var immutableResults = result.ToImmutableArray();
            return (immutableResults, $"{immutableResults.Length}");
        });
    }



}
