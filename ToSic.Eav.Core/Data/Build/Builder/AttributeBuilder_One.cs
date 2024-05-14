using System.Collections.Immutable;
using ToSic.Eav.Data.Source;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Data.Build;

partial class AttributeBuilder
{

    /// <summary>
    /// Get Attribute for specified Typ
    /// </summary>
    /// <returns><see cref="Attribute{ValueType}"/></returns>
    [PrivateApi("probably move to some attribute-builder or something")]
    public IAttribute Create(string name, ValueTypes type, IList<IValue> values = null)
    {
        var imValues = values?.ToImmutableList();
        return type switch
        {
            ValueTypes.Boolean => new Attribute<bool?>(name, type, imValues),
            ValueTypes.DateTime => new Attribute<DateTime?>(name, type, imValues),
            ValueTypes.Number => new Attribute<decimal?>(name, type, imValues),
            ValueTypes.Entity =>
                // Note 2023-02-24 2dm - up until now the values were never used
                // in this case, so relationships created here were always empty
                // Could break something, but I don't think it will
                new Attribute<IEnumerable<IEntity>>(name, type,
                    imValues.SafeAny() ? imValues : ValueBuilder.NewEmptyRelationshipValues),
            // ReSharper disable RedundantCaseLabel
            ValueTypes.String => new Attribute<string>(name, type, imValues),
            ValueTypes.Hyperlink => new Attribute<string>(name, type, imValues),
            ValueTypes.Custom => new Attribute<string>(name, type, imValues),
            ValueTypes.Json => new Attribute<string>(name, type, imValues),
            ValueTypes.Undefined => new Attribute<string>(name, type, imValues),
            ValueTypes.Empty => new Attribute<string>(name, type, imValues),
            _ => new Attribute<string>(name, type, imValues)
        };
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
        var valueLanguages = languageBuilder.GetBestValueLanguages(language, languageReadOnly);

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