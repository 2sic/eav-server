﻿using System.Collections.Immutable;
using System.Globalization;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Coding;
using ToSic.Lib.DI;
using ToSic.Lib.Services;
using static System.Globalization.CultureInfo;
using DateTime = System.DateTime;

namespace ToSic.Eav.Data.Build;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class ValueBuilder(LazySvc<IValueConverter> valueConverter) : ServiceBase("Eav.ValBld")
{
    private bool _allowUnknownValueTypes;

    #region Constructor

    #endregion

    public ValueBuilder Setup(bool allowUnknownValueTypes = false)
    {
        _allowUnknownValueTypes = allowUnknownValueTypes;
        return this;
    }

    /// <summary>
    /// Create/clone a value based on an original which will supply most of the values.
    /// </summary>
    /// <returns></returns>
    public IValue CreateFrom(IValue original,
        NoParamOrder noParamOrder = default,
        IImmutableList<ILanguage> languages = null)
        => languages == null ? original : original.Clone(languages);

    #region Simple Values: Bool, DateTime, Number, String

    public IValue<bool?> Bool(bool? value, IImmutableList<ILanguage> languages = null) => 
        new Value<bool?>(value, languages);

    public IValue<bool?> Bool(object value, IImmutableList<ILanguage> languages = null) => 
        Bool(value as bool? ?? (bool.TryParse(value as string, out var typed) ? typed : new bool?()), languages);

    public IValue<DateTime?> DateTime(DateTime? value, IImmutableList<ILanguage> languages = null) =>
        new Value<DateTime?>(value, languages);

    public IValue<DateTime?> DateTime(object value, IImmutableList<ILanguage> languages = null) =>
        DateTime(value as DateTime? ?? (System.DateTime.TryParse(value as string, InvariantCulture, DateTimeStyles.None, out var typed) ? typed : new DateTime?()), languages);

    public IValue<string> String(string value, IImmutableList<ILanguage> languages = null) =>
        new Value<string>(value, languages);

    public IValue<string> String(object value, IImmutableList<ILanguage> languages = null) =>
        new Value<string>(value as string, languages);

    public IValue<decimal?> Number(decimal? value, IImmutableList<ILanguage> languages = null)
        => new Value<decimal?>(value, languages);
    //public IValue<decimal?> Number(int? value, IImmutableList<ILanguage> languages = null)
    //    => new Value<decimal?>(value, languages);

    public IValue<decimal?> Number(object value, IImmutableList<ILanguage> languages = null)
    {
        var newDec = value as decimal?;
        if (newDec != null || value is null || (value is string s && s.IsEmptyOrWs()))
            return Number(newDec, languages);
        try
        {
            return Number(Convert.ToDecimal(value, InvariantCulture), languages);
        }
        catch
        {
            return Number(null, languages);
        }
    }

    #endregion

    #region Relationships


    #endregion

    public IValue<T> Create<T>(T value, IImmutableList<ILanguage> languages = null)
    {
        var type = typeof(T).UnboxIfNullable();
        if (type == typeof(bool)) return (IValue<T>)Bool(value as bool?, languages);
        if (type == typeof(DateTime)) return (IValue<T>)DateTime(value as DateTime?, languages);
        if (type.IsNumeric()) return (IValue<T>)Number(value as decimal?, languages);
        // Note: Entities not supported in this build-call
        return (IValue<T>)String(value as string, languages);
    }

    /// <summary>
    /// Creates a Typed Value Model
    /// </summary>
    /// <returns>
    /// An IValue, which is actually an IValue<string>, IValue<decimal>, IValue<IEnumerable<IEntity>> etc.
    /// </returns>
    public IValue Build(ValueTypes type, object value, IImmutableList<ILanguage> languages = null)
    {
        var langs = languages ?? DimensionBuilder.NoLanguages;
        var stringValue = value as string;
        try
        {
            return type switch
            {
                ValueTypes.Boolean => Bool(value, langs),
                ValueTypes.DateTime => DateTime(value, langs),
                ValueTypes.Number => Number(value, langs),
                ValueTypes.Entity => RelationshipWip(value, null),
                ValueTypes.Object => new Value<object>(value, languages),
                // ReSharper disable RedundantCaseLabel
                ValueTypes.String => // most common case
                    String(stringValue, langs) // new Value<string>(stringValue, langs);
                ,
                ValueTypes.Empty => // empty - should actually not contain anything!
                    String(stringValue, langs) // new Value<string>(stringValue, langs);
                ,
                ValueTypes.Custom => // custom value, currently just parsed as string for manual processing as needed
                    String(stringValue, langs) // new Value<string>(stringValue, langs);
                ,
                ValueTypes.Json => String(stringValue, langs) // new Value<string>(stringValue, langs);
                ,
                ValueTypes.Hyperlink => // special case, handled as string
                    String(stringValue, langs) // new Value<string>(stringValue, langs);
                ,
                ValueTypes.Undefined => // backup case, where it's not known...
                    String(stringValue, langs) // new Value<string>(stringValue, langs);
                ,
                _ => String(stringValue, langs)
            };
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



    public object PreConvertReferences(object value, ValueTypes valueType, bool resolveHyperlink) => Log.Func(() =>
    {
        if (value is IAttribute)
            throw new ArgumentException($"Value must be a simple value but it's an {nameof(IAttribute)}");
        if (resolveHyperlink && valueType == ValueTypes.Hyperlink && value is string stringValue)
        {
            var converted = valueConverter.Value.ToReference(stringValue);
            return (converted, $"Resolve hyperlink for '{stringValue}' - New value: '{converted}'");
        }
        return (value, "unmodified");
    });

}