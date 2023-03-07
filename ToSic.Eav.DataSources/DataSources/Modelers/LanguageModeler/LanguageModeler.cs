using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
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

        /// <summary>
        /// Contains the field map which configures how fields should be connected.
        /// </summary>
        [Configuration]
        public string FieldMap
        {
            get => Configuration.GetThis();
            set => Configuration.SetThis(value);
        }
        #endregion

        /// <summary>
        /// Initializes this data source
        /// </summary>
        [PrivateApi]
        public LanguageModeler(DataBuilder dataBuilder, MyServices services): base(services, $"{DataSourceConstants.LogPrefix}.LngMod")
        {
            ConnectServices(
                _dataBuilder = dataBuilder
            );
            Provide(MapLanguagesIntoValues);
        }

        private readonly DataBuilder _dataBuilder;


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
                return (Error.Create(title: "Field Map Error", message: fieldMapErrors), "error");

            #endregion

            l.A($"Field Map created - has {fieldMap.Length} parts");

            var source = TryGetIn();
            if (source is null) return (Error.TryGetInFailed(this), "error");

            var result = new List<IEntity>();
            foreach (var entity in source)
            {
                var attributes = _dataBuilder.Attribute.Mutable(entity.Attributes);

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
                            _dataBuilder.Attribute.Create(map.Target,
                                firstExistingValue?.Type ?? ValueTypes.String); // if there are no values, we assume it's a string field
                        // #immutableTodo
                        attributes.Add(map.Target, newAttribute);

                        foreach (var entry in map.Fields)
                        {
                            if (!attributes.ContainsKey(entry.OriginalField))
                            {
                                // do not create values for fields which do not exist
                                l.A($"Field mapping ignored for entity {entity.EntityId} and language {entry.Language} because source attribute {entry.OriginalField} does not exist.");
                                continue;
                            }

                            var currentAttribute = attributes[entry.OriginalField];
                            var value = currentAttribute.Values.FirstOrDefault()?.ObjectContents;
                            // Remove first, in case the new name replaces an old one
                            // #immutableTodo
                            //attributes.Remove(entry.OriginalField);
                            // Now add the resulting new attribute
                            var temp = _dataBuilder.Attribute.CreateOrUpdate(currentAttribute, map.Target, value, newAttribute.Type, entry.Language);
                            attributes = _dataBuilder.Attribute.Replace(attributes, temp);
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
                        var newAttribute = _dataBuilder.Attribute.Create(map.Target, sourceAttr.Type, sourceAttr.Values.ToList());
                        // Remove first, in case the new name replaces an old one
                        // #immutableTodo
                        attributes.Remove(map.Source);
                        // Now add the resulting new attribute
                        // #immutableTodo
                        attributes.Add(map.Target, newAttribute);
                    }

                var modifiedEntity = _dataBuilder.Entity.Clone(entity, attributes: _dataBuilder.Attribute.Create(attributes));

                result.Add(modifiedEntity);
            }

            var immutableResults = result.ToImmutableList();
            return (immutableResults, $"{immutableResults.Count}");
        });
    }



}
