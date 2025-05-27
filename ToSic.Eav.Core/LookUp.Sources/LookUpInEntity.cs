using ToSic.Eav.Context;
using ToSic.Lib.LookUp.Sources;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.LookUp.Sources;

/// <summary>
/// Get Values from an <see cref="IEntity"/>. <br/>
/// Read more about this in [](xref:Abyss.Parts.LookUp.Index)
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public class LookUpInEntity : LookUpIn<IEntity>
{
    private readonly string[] _dimensions;

    /// <summary>
    /// Constructs a new Entity LookUp
    /// </summary>
    /// <param name="name">Name of the LookUp, e.g. Settings</param>
    /// <param name="source"></param>
    /// <param name="dimensions">the languages / dimensions to use</param>
    public LookUpInEntity(string name, IEntity source, string[] dimensions, string description = default)
        : base(source, name, description ?? $"LookUp in Entity {source?.EntityId}")
    {
        _dimensions = dimensions ?? IZoneCultureResolverExtensions.SafeLanguagePriorityCodes(null);
    }

    // todo: might need to clarify what language/culture the key is taken from in an entity
    /// <summary>
    /// Special lookup command with format-provider.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    public override string Get(string key, string format)
    {
        // Return empty string if Entity is null
        if (Data == null)
            return string.Empty;

        // Try to just find, and format the result if all is ok
        var valueObject = Data.Get(key, languages: _dimensions);
        if (valueObject != null)
            return FormatValue(valueObject, format, _dimensions);

        // Not found yet, so check for Navigation-Property (e.g. Manager:Name)
        var subTokens = CheckAndGetSubToken(key);
        if (!subTokens.HasSubToken)
            return string.Empty;
        valueObject = Data.Get(subTokens.Source, languages: _dimensions);

        // Finally: Handle child-Entity-Field (sorted list of related entities) / null check
        if (valueObject is not IEnumerable<IEntity> relationshipList)
            return string.Empty;
        var first = relationshipList.FirstOrDefault();
        return first == null
            ? string.Empty
            : new LookUpInEntity("no-name", first, _dimensions).Get(subTokens.Rest, format);
    }
}