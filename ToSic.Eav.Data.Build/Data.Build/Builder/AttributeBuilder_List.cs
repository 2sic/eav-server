﻿using System.Collections.Immutable;
using ToSic.Eav.Data.Sys;
using static System.StringComparer;

namespace ToSic.Eav.Data.Build;

partial class AttributeBuilder
{
    private bool _allowUnknownValueTypes;

    public AttributeBuilder Setup(bool allowUnknownValueTypes = false)
    {
        _allowUnknownValueTypes = allowUnknownValueTypes;
        return this;
    }

    public IImmutableDictionary<string, IAttribute> Empty() => EmptyList;
    public static readonly IImmutableDictionary<string, IAttribute> EmptyList = new Dictionary<string, IAttribute>().ToImmutableDictionary();
    
    public IReadOnlyDictionary<string, IAttribute> Create(IContentType contentType, ILookup<string, IValue>? preparedValues)
    {
        var attributes = contentType
            .Attributes
            .ToImmutableDictionary(
                a => a.Name,
                a =>
                {
                    // It's important that we only get a list if there are values, otherwise we create empty lists
                    // which breaks other code
                    var values = preparedValues?.Contains(a.Name) == true
                        ? preparedValues[a.Name].ToListOpt()
                        : null;
                    var entityAttribute = Create(a.Name, a.Type, values ?? []);
                    return entityAttribute;
                }, InvariantCultureIgnoreCase);
        return attributes;
    }


    public IReadOnlyDictionary<string, IAttribute> Create(IDictionary<string, IAttribute>? attributes)
        => attributes?.ToImmutableInvIgnoreCase() ?? Empty();

    public IReadOnlyDictionary<string, IAttribute> Create(IDictionary<string, object?>? attributes, IImmutableList<ILanguage>? languages = null)
        => attributes == null
            ? Empty()
            : CreateInternal(attributes, languages)
                .ToImmutableInvIgnoreCase();


    /// <summary>
    /// Convert a NameValueCollection-Like List to a Dictionary of IAttributes
    /// </summary>
    private Dictionary<string, IAttribute> CreateInternal(IDictionary<string, object?> objAttributes, IImmutableList<ILanguage>? languages = null)
        => objAttributes.ToDictionary(
            pair => pair.Key,
            pair =>
            {
                // in case the object is already an IAttribute, use that, don't rebuild it
                if (pair.Value is IAttribute typedValue)
                    return typedValue;

                // Not yet a proper IAttribute, construct from value
                var attributeType = DataTypes.GetAttributeTypeName(pair.Value, _allowUnknownValueTypes);
                var valuesModelList = new List<IValue>();
                if (pair.Value != null)
                {
                    var valueModel = ValueBuilder.Build(attributeType, pair.Value, languages);
                    valuesModelList.Add(valueModel);
                }

                var attributeModel = Create(pair.Key, attributeType, valuesModelList);

                return attributeModel;
            },
            InvariantCultureIgnoreCase);

    #region Mutable operations - WIP

    public IDictionary<string, IAttribute> Mutable(IReadOnlyDictionary<string, IAttribute>? attributes)
        => attributes?.ToDictionary(pair => pair.Key, pair => pair.Value, InvariantCultureIgnoreCase)
           ?? new Dictionary<string, IAttribute>(InvariantCultureIgnoreCase);

    public IDictionary<string, IAttribute> Replace(IDictionary<string, IAttribute> target, IAttribute newAttribute)
    {
        // ReSharper disable once UseObjectOrCollectionInitializer
        var result = new Dictionary<string, IAttribute>(target, InvariantCultureIgnoreCase);
        // Do this in a separate step, so it lands at the end of the list
        result[newAttribute.Name] = newAttribute;
        return result;
    }
    public IDictionary<string, IAttribute> Replace(IReadOnlyDictionary<string, IAttribute> target, IEnumerable<IAttribute> newAttributes)
    {
        // ReSharper disable once UseObjectOrCollectionInitializer
        var result = target.ToDictionary(pair => pair.Key, pair => pair.Value, InvariantCultureIgnoreCase);
        // Do this in a separate step, so it lands at the end of the list
        foreach (var newAttribute in newAttributes)
            result[newAttribute.Name] = newAttribute;
        return result;
    }

    #endregion

}