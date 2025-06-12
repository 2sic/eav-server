using System.Collections.Immutable;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Sys;

namespace ToSic.Eav.Data.Values.Sys;

/// <summary>
/// Represents a typed Value object in the memory model
/// </summary>
/// <remarks>
/// * completely #immutable since v15.04
/// </remarks>
/// <typeparam name="T">Type of the actual Value</typeparam>
[PrivateApi("this is just fyi, always work with interface IValue<T>")]
[ShowApiWhenReleased(ShowApiMode.Never)]
internal record Value<T>(T? TypedContents, IImmutableList<ILanguage>? LanguagesImmutable = null) : IValue<T>
{
    /// <inheritdoc />
    public T? TypedContents { get; } = TypedContents;

    /// <inheritdoc />
    public IEnumerable<ILanguage> Languages => LanguagesImmutable;

    [field: AllowNull, MaybeNull]
    public IImmutableList<ILanguage> LanguagesImmutable
    {
        get => field ??= DataConstants.NoLanguages;
        init;
    } = LanguagesImmutable;

    /// <inheritdoc />
    public object? SerializableObject
    {
        get
        {
            if (TypedContents is not IEnumerable<IEntity> maybeRelationshipList)
                return TypedContents;

            // special case with list of related entities - should return array of guids
            var entityGuids = maybeRelationshipList
                .Select(e => e?.EntityGuid)
                .ToListOpt();
            return entityGuids;
        }
    }

    [PrivateApi("used only for xml-serialization, does very specific date-to-string conversions")]
    public string? Serialized
    {
        get
        {
            var obj = SerializableObject;
            if (obj is ICollection<Guid?> list)
                return string.Join(",", list.Select(y => y?.ToString() ?? EavConstants.EmptyRelationship));

            return (obj as DateTime?)?.ToString("yyyy-MM-ddTHH:mm:ss") 
                   ?? (obj as bool?)?.ToString() 
                   ?? (obj as decimal?)?.ToString(System.Globalization.CultureInfo.InvariantCulture)
                   ?? obj?.ToString();
        }
    }

    [PrivateApi]
    public IValue With(IImmutableList<ILanguage> newLanguages)
        => this with { LanguagesImmutable = newLanguages };

    [PrivateApi]
    public object? ObjectContents => TypedContents;
}