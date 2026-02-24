using System.Collections.Immutable;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Values;
using DateTime = System.DateTime;

namespace ToSic.Eav.Data.Build.Sys;

/// <summary>
/// The internal system to create value objects.
/// </summary>
/// <remarks>
/// This is used in the builders to create the values for the attributes.
/// It can also be used by external code to create values, but it's not really meant for that, so it's not public API.
/// It's more of an internal helper class, which is why it's not in the Sys namespace.
///
/// Important: everything is **functional** meaning that object given in will never be modified.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[method: PrivateApi]
public class ValueAssembler() : ServiceWithSetup<DataAssemblerOptions>("DaB.ValBld")
{
    #region Simple Values: Bool, DateTime, Number, String

    public IValue<bool?> Bool(bool? value, IImmutableList<ILanguage> languages) => 
        new Value<bool?>(value, languages);


    public IValue<DateTime?> DateTime(DateTime? value, IImmutableList<ILanguage> languages) =>
        new Value<DateTime?>(value, languages);


    public IValue<string> String(string? value, IImmutableList<ILanguage> languages) =>
        new Value<string>(value, languages);


    public IValue<decimal?> Number(decimal? value, IImmutableList<ILanguage> languages) =>
        new Value<decimal?>(value, languages);


    #endregion

    /// <summary>
    /// Create/clone a value based on an original which will supply most of the values.
    /// </summary>
    /// <returns></returns>
    public IValue CreateFrom(IValue original, NoParamOrder npo = default, IImmutableList<ILanguage>? languages = null) =>
        languages == null
            ? original
            : original.With(languages);

    /// <summary>
    /// Create a basic typed value. This only supports bool, DateTime, number and string. For other types, use the Create with ValueTypes parameter and handle as needed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="languages"></param>
    /// <returns></returns>
    public IValue<T> Create<T>(T value, IImmutableList<ILanguage> languages)
    {
        var type = typeof(T).UnboxIfNullable();
        if (type == typeof(bool))
            return (IValue<T>)Bool(value as bool?, languages);
        if (type == typeof(DateTime))
            return (IValue<T>)DateTime(value as DateTime?, languages);
        if (type.IsNumeric())
            return (IValue<T>)Number(value as decimal?, languages);
        // Note: Entities not supported in this build-call
        return (IValue<T>)String(value as string, languages);
    }

    /// <summary>
    /// Creates a Typed Value Model
    /// </summary>
    /// <returns>
    /// An IValue, which is actually an IValue{string}, IValue{decimal}, IValue{IEnumerable{IEntity}} etc.
    /// </returns>
    public IValue Create(ValueTypes type, object? value, IImmutableList<ILanguage>? languages = null)
    {
        var langs = languages ?? DataConstants.NoLanguages;
        var stringValue = value as string;
        try
        {
            return type switch
            {
                ValueTypes.Boolean => Bool(value.ToBool(), langs),
                ValueTypes.DateTime => DateTime(value.ToDateTime(), langs),
                ValueTypes.Number => Number(value.ToNumber(), langs),
                ValueTypes.Entity => RelationshipAssembler.RelationshipQ(RelationshipAssembler.GetLazyEntitiesForRelationshipWip(value, null)),
                ValueTypes.Object => new Value<object>(value, languages),
                // ReSharper disable RedundantCaseLabel
                ValueTypes.String => String(stringValue, langs),    // most common case
                ValueTypes.Empty => String(stringValue, langs),     // empty - should actually not contain anything!
                ValueTypes.Custom => String(stringValue, langs),    // custom value, currently just parsed as string for manual processing as needed
                ValueTypes.Json => String(stringValue, langs),
                ValueTypes.Hyperlink => String(stringValue, langs), // special case, handled as string
                ValueTypes.Undefined => String(stringValue, langs), // backup case, where it's not known...
                _ => String(stringValue, langs)
            };
        }
        catch
        {
            return new Value<string>(stringValue, langs);
        }
    }
}