﻿using System.Collections.Immutable;
using ToSic.Eav.Data.Sys.Attributes;
using ToSic.Eav.Data.Sys.Entities.Sources;

namespace ToSic.Eav.Data.Build;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class AttributeBuilder(ValueBuilder valueBuilder, DimensionBuilder languageBuilder)
    : ServiceWithSetup<DataBuilderOptions>("DaB.AttBld", connect: [languageBuilder, valueBuilder])
{

    /// <summary>
    /// Get Attribute for specified Typ
    /// </summary>
    /// <returns><see cref="Attribute{ValueType}"/></returns>
    [PrivateApi("probably move to some attribute-builder or something")]
    public IAttribute Create(string name, ValueTypes type, IList<IValue> values)
    {
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        var imValues = values.ToImmutableSafe();

        return type switch
        {
            ValueTypes.Boolean => new Attribute<bool?> { Name = name, Type = type, ValuesImmutable = imValues },
            ValueTypes.DateTime => new Attribute<DateTime?> { Name = name, Type = type, ValuesImmutable = imValues },
            ValueTypes.Number => new Attribute<decimal?> { Name = name, Type = type, ValuesImmutable = imValues },
            // Note 2023-02-24 2dm - up until now the values on Entity were never used
            // in this case, so relationships created here were always empty
            // Could break something, but I don't think it will
            ValueTypes.Entity => new Attribute<IEnumerable<IEntity>>
            {
                Name = name,
                Type = type,
                ValuesImmutable = imValues.SafeAny()
                    ? imValues
                    : valueBuilder.NewEmptyRelationshipValues
            },
            ValueTypes.Object => new Attribute<object> { Name = name, Type = type, ValuesImmutable = imValues },
            // ReSharper disable PatternIsRedundant
            ValueTypes.String or
                ValueTypes.Hyperlink or
                ValueTypes.Custom or
                ValueTypes.Json or
                ValueTypes.Undefined or
                ValueTypes.Empty or
                _ => new Attribute<string> { Name = name, Type = type, ValuesImmutable = imValues }
            // ReSharper restore PatternIsRedundant
        };
    }


    /// <summary>
    /// Add a value to the attribute specified. To do so, set the name, type and string of the value, as 
    /// well as some language properties.
    /// </summary>
    public IAttribute CreateOrUpdate(
        IAttribute? originalOrNull,
        string name,
        object? value,
        ValueTypes type,
        IValue? valueToReplace = default,
        string? language = default,
        bool languageReadOnly = false
    )
    {
        var l = Log.IfDetails(Options.LogSettings).Fn<IAttribute>($"name:{name}, value:{value}, type:{type}, lang:{language}");
        var valueLanguages = languageBuilder.GetBestValueLanguages(language, languageReadOnly);

        var valueWithLanguages = valueBuilder.Build(type, value, valueLanguages.ToImmutableOpt());

        // add or replace to the collection
        if (originalOrNull == null)
            return l.Return(Create(name, type, new List<IValue> { valueWithLanguages }));

        // maybe: test if the new model has the same type as the attribute we're adding to
        // ca: if(attrib.ControlledType != valueModel.)
        var updatedValueList = valueBuilder.Replace(originalOrNull.Values, valueToReplace, valueWithLanguages);
        return l.Return(originalOrNull.With(updatedValueList));
    }




    public IAttribute CreateFrom(IAttribute original, IImmutableList<IValue> values)
        => original.With(values);

    public IAttribute<IEnumerable<IEntity>> Relationship(string name, IEnumerable<IEntity> directSource) => 
        new Attribute<IEnumerable<IEntity>>
        {
            Name = name,
            Type = ValueTypes.Entity,
            ValuesImmutable = valueBuilder.Relationships(directSource),
        };

    public IAttribute<IEnumerable<IEntity>> Relationship(string name, IEnumerable<object> keys, ILookup<object, IEntity> lookup) => 
        Relationship(name, new LookUpEntitiesSource<object>(keys, lookup));
}