using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using ToSic.Eav.Data.Source;
using ToSic.Eav.Plumbing;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Data.Build
{
    [PrivateApi]
    public class ValueBuilder: ServiceBase
    {
        // WIP - constructor should never be called because we should use DI
        public ValueBuilder(DimensionBuilder dimensionBuilder, LazySvc<IValueConverter> valueConverter): base("Eav.ValBld")
        {
            _languageBuilder = dimensionBuilder;
            _valueConverter = valueConverter;
        }

        private DimensionBuilder _languageBuilder;
        private readonly LazySvc<IValueConverter> _valueConverter;

        public IValue Clone(IValue original, IImmutableList<ILanguage> languages = null) 
            => languages == null ? original : original.Clone(languages);


        public IValue BuildRelationship(List<int?> references, IEntitiesSource app) 
            => BuildRelationship(new LazyEntities(app, references));

        public IValue BuildRelationship(IEnumerable<IEntity> directList) 
            => new Value<IEnumerable<IEntity>>(directList, DimensionBuilder.NoLanguages);

        public IValue CloneRelationship(IRelatedEntitiesValue value, IEntitiesSource app) 
            => BuildRelationship(new LazyEntities(app, value.Identifiers));

        /// <summary>
        /// Creates a Typed Value Model
        /// </summary>
        /// <returns>
        /// An IValue, which is actually an IValue<string>, IValue<decimal>, IValue<IEnumerable<IEntity>> etc.
        /// </returns>
        public IValue Build(ValueTypes type, object value, IImmutableList<ILanguage> languages = null, IEntitiesSource fullEntityListForLookup = null)
        {
            var langs = languages ?? DimensionBuilder.NoLanguages;
            var stringValue = value as string;
            try
            {
                switch (type)
                {
                    case ValueTypes.Boolean:
                        return new Value<bool?>(value as bool? ?? (bool.TryParse(stringValue, out var typedBoolean)
                            ? typedBoolean
                            : new bool?()), langs);
                    case ValueTypes.DateTime:
                        return new Value<DateTime?>(value as DateTime? ?? (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture,
                                                     DateTimeStyles.None, out var typedDateTime)
                                                     ? typedDateTime
                                                     : new DateTime?()), langs);

                    case ValueTypes.Number:
                        decimal? newDec = null;
                        if (value != null && !(value is string s && string.IsNullOrEmpty(s)))
                        {
                            // only try converting if it's not an empty string
                            try
                            {
                                newDec = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                            }
                            catch { /* ignored */ }
                        }

                        return new Value<decimal?>(newDec, langs);

                    case ValueTypes.Entity:
                        var rel = GetLazyEntitiesForRelationship(value, fullEntityListForLookup);
                        return new Value<IEnumerable<IEntity>>(rel, DimensionBuilder.NoLanguages);
                    // ReSharper disable RedundantCaseLabel
                    case ValueTypes.String:  // most common case
                    case ValueTypes.Empty:   // empty - should actually not contain anything!
                    case ValueTypes.Custom:  // custom value, currently just parsed as string for manual processing as needed
                    case ValueTypes.Json:
                    case ValueTypes.Hyperlink:// special case, handled as string
                    case ValueTypes.Undefined:// backup case, where it's not known...
                    // ReSharper restore RedundantCaseLabel
                    default:
                        return new Value<string>(stringValue, langs);
                }
            }
            catch
            {
                return new Value<string>(stringValue, langs);
            }
        }

        public IImmutableList<IValue> Replace(IEnumerable<IValue> values, IValue oldValue, IValue newValue)
        {
            var editable = values.ToList();
            // note: should preserve order
            var index = editable.IndexOf(oldValue);
            if (index == -1)
                editable.Add(newValue);
            else
                editable[index] = newValue;
            return editable.ToImmutableList();
        }


        private LazyEntities GetLazyEntitiesForRelationship(object value, IEntitiesSource fullLookupList)
        {
            var entityIds = (value as IEnumerable<int?>)?.ToList()
                            ?? (value as IEnumerable<int>)?.Select(x => (int?)x).ToList();
            if (entityIds != null)
                return new LazyEntities(fullLookupList, entityIds);
            if (value is IRelatedEntitiesValue relList)
                return new LazyEntities(fullLookupList, relList.Identifiers);
            if (value is List<Guid?> guids)
                return new LazyEntities(fullLookupList, guids);
            return new LazyEntities(fullLookupList, GuidCsvToList(value));
        }


        private static List<Guid?> GuidCsvToList(object value)
        {
            var entityIdEnum = value as IEnumerable; // note: strings are also enum!
            if (value is string stringValue && stringValue.HasValue())
                entityIdEnum = stringValue.Split(',').ToList();
            // this is the case when we get a CSV-string with GUIDs
            var entityGuids = entityIdEnum?.Cast<object>().Select(x =>
            {
                var v = x?.ToString().Trim();
                // this is the case when an export contains a list with nulls as a special code
                if (v == null || v == Constants.EmptyRelationship)
                    return new Guid?();
                var guid = Guid.Parse(v);
                return guid == Guid.Empty ? new Guid?() : guid;
            }).ToList() ?? new List<Guid?>();
            return entityGuids;
        }



        /// <summary>
        /// Generate a new empty relationship. This is important, because it's used often to create empty relationships...
        /// ...and then it must be a new object every time, 
        /// because the object could be changed at runtime, and if it were shared, then it would be changed in many places
        /// </summary>
        private Value<IEnumerable<IEntity>> NewEmptyRelationship
            => new Value<IEnumerable<IEntity>>(new LazyEntities(null, identifiers: null), DimensionBuilder.NoLanguages);

        internal IImmutableList<IValue> NewEmptyRelationshipValues => new List<IValue> { NewEmptyRelationship }.ToImmutableList();


        public object PreConvertReferences(object value, ValueTypes valueType, bool resolveHyperlink) => Log.Func(() =>
        {
            if (resolveHyperlink && valueType == ValueTypes.Hyperlink && value is string stringValue)
            {
                var converted = _valueConverter.Value.ToReference(stringValue);
                return (converted, $"Resolve hyperlink for '{stringValue}' - New value: '{converted}'");
            }
            return (value, "unmodified");
        });

    }
}
