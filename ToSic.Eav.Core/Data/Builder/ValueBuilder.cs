using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data.Builder
{
    [PrivateApi]
    public class ValueBuilder
    {
        // WIP - constructor should never be called because we should use DI
        public ValueBuilder(DimensionBuilder dimensionBuilder)
        {
            LanguageBuilder = dimensionBuilder;
        }
        private DimensionBuilder LanguageBuilder { get; }

        public IValue Clone(IValue original, string type, IImmutableList<ILanguage> languages = null) 
            => Build(
                type, original.ObjectContents,
                // 2023-02-24 2dm #immutable - don't need to clone if it's immutable
                languages ?? /*LanguageBuilder.Clone(*/original.Languages.ToImmutableList() /*)*/, null);


        /// <summary>
        /// Creates a Typed Value Model
        /// </summary>
        public IValue Build(string attributeType, object value, IImmutableList<ILanguage> languages = null,
            IEntitiesSource fullEntityListForLookup = null)
            => Build((ValueTypes)Enum.Parse(typeof(ValueTypes), attributeType), value, languages?.ToImmutableList(), fullEntityListForLookup);

        public IValue BuildRelationship(List<int?> references, IEntitiesSource app)
        {
            return new Value<IEnumerable<IEntity>>(new LazyEntities(app, references), DimensionBuilder.NoLanguages);
        }

        /// <summary>
        /// Creates a Typed Value Model
        /// </summary>
        /// <returns>
        /// An IValue, which is actually an IValue<string>, IValue<decimal>, IValue<IEnumerable<IEntity>> etc.
        /// </returns>
        public IValue Build(ValueTypes type, object value, IImmutableList<ILanguage> languages, IEntitiesSource fullEntityListForLookup = null)
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

        private LazyEntities GetLazyEntitiesForRelationship(object value, IEntitiesSource fullEntityListForLookup)
        {
            var entityIds = (value as IEnumerable<int?>)?.ToList()
                            ?? (value as IEnumerable<int>)?.Select(x => (int?)x).ToList();
            if (entityIds != null)
                return new LazyEntities(fullEntityListForLookup, entityIds);
            if (value is IEnumerable<IEntity> relList)
                return new LazyEntities(fullEntityListForLookup, ((LazyEntities)relList).Identifiers);
            if (value is List<Guid?> guids)
                return new LazyEntities(fullEntityListForLookup, guids);
            return new LazyEntities(fullEntityListForLookup, GuidCsvToList(value));
        }


        private static List<Guid?> GuidCsvToList(object value)
        {
            var stringValue = value as string;
            var entityIdEnum = value as IEnumerable; // note: strings are also enum!
            if (value is string && !String.IsNullOrEmpty(stringValue))
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
            }).ToList() ?? new List<Guid?>(0);
            return entityGuids;
        }



        /// <summary>
        /// Generate a new empty relationship. This is important, because it's used often to create empty relationships...
        /// ...and then it must be a new object every time, 
        /// because the object could be changed at runtime, and if it were shared, then it would be changed in many places
        /// </summary>
        internal Value<IEnumerable<IEntity>> NewEmptyRelationship
            => new Value<IEnumerable<IEntity>>(new LazyEntities(null, identifiers: null), DimensionBuilder.NoLanguages);

        internal List<IValue> NewEmptyRelationshipValues => new List<IValue> { NewEmptyRelationship };
    }
}
