using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data.Source;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Data.Build
{
    public partial class AttributeBuilder
    {

        /// <summary>
        /// Get Attribute for specified Typ
        /// </summary>
        /// <returns><see cref="Attribute{ValueType}"/></returns>
        [PrivateApi("probably move to some attribute-builder or something")]
        public IAttribute Create(string name, ValueTypes type, IList<IValue> values = null)
        {
            var imValues = values?.ToImmutableList();
            switch (type)
            {
                case ValueTypes.Boolean:
                    return new Attribute<bool?>(name, type, imValues);
                case ValueTypes.DateTime:
                    return new Attribute<DateTime?>(name, type, imValues);
                case ValueTypes.Number:
                    return new Attribute<decimal?>(name, type, imValues);
                case ValueTypes.Entity:
                    // Note 2023-02-24 2dm - up until now the values were never used
                    // in this case, so relationships created here were always empty
                    // Could break something, but I don't think it will
                    return new Attribute<IEnumerable<IEntity>>(name, type,
                        imValues.SafeAny() ? imValues : ValueBuilder.NewEmptyRelationshipValues);
                // ReSharper disable RedundantCaseLabel
                case ValueTypes.String:
                case ValueTypes.Hyperlink:
                case ValueTypes.Custom:
                case ValueTypes.Json:
                case ValueTypes.Undefined:
                case ValueTypes.Empty:
                default:
                    return new Attribute<string>(name, type, imValues);
                // ReSharper restore RedundantCaseLabel
            }
        }


        /// <summary>
        /// Add a value to the attribute specified. To do so, set the name, type and string of the value, as 
        /// well as some language properties.
        /// </summary>
        public IAttribute CreateOrUpdate(
            IAttribute originalOrNull,
            string name,
            object value,
            ValueTypes type,
            IValue valueToReplace = default,
            string language = default,
            bool languageReadOnly = false
        ) => Log.Func($"..., {name}, {value} ({type}), {language}, ...", l =>
        {
            var valueLanguages = _languageBuilder.GetBestValueLanguages(language, languageReadOnly);

            var valueWithLanguages = ValueBuilder.Build(type, value, valueLanguages?.ToImmutableList());

            // add or replace to the collection
            if (originalOrNull == null) return Create(name, type, new List<IValue> { valueWithLanguages });

            // maybe: test if the new model has the same type as the attribute we're adding to
            // ca: if(attrib.ControlledType != valueModel.)
            var updatedValueList = ValueBuilder.Replace(originalOrNull.Values, valueToReplace, valueWithLanguages);
            return originalOrNull.CloneWithNewValues(updatedValueList);
        });




        public IAttribute CreateFrom(IAttribute original, IImmutableList<IValue> values)
            => original.CloneWithNewValues(values);

        //public IAttribute<IEnumerable<IEntity>> Relationship(string name, IImmutableList<IEntity> relationships) => 
        //    new Attribute<IEnumerable<IEntity>>(name, ValueTypes.Entity, ValueBuilder.Relationships(relationships));

        public IAttribute<IEnumerable<IEntity>> Relationship(string name, IEnumerable<IEntity> directSource) => 
            new Attribute<IEnumerable<IEntity>>(name, ValueTypes.Entity, ValueBuilder.Relationships(directSource));

        public IAttribute<IEnumerable<IEntity>> Relationship(string name, IEnumerable<object> keys, ILookup<object, IEntity> lookup) => 
            Relationship(name, new LookUpEntitiesSource<object>(keys, lookup));
    }
}
