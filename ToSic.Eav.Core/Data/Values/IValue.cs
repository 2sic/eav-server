using System.Collections.Immutable;

namespace ToSic.Eav.Data;

/// <summary>
/// Represents a Value in the EAV system. Values belong to an attribute and can belong to multiple languages. 
/// </summary>
[PublicApi]
public interface IValue
{
    /// <summary>
    /// Gets the Language (<see cref="ILanguage"/>) assigned to this Value. Can be one or many. 
    /// </summary>
    /// <remarks>
    /// This was an IList up until 15.04. Since it's very internal, we felt safe to change it to immutable
    /// </remarks>
    IEnumerable<ILanguage> Languages { get; }

    /// <summary>
    /// The internal contents of the value as a .net object.
    /// Should usually not be used, it's more for internal use.
    /// </summary>
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    object ObjectContents { get; }

    /// <summary>
    /// Returns the inner value in a form that can be serialized, for JSON serialization etc.
    /// </summary>
    [PrivateApi("2dm - changed to private v15.04 - was public before")]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    object SerializableObject { get; }

    /// <summary>
    /// Older way to serialize the value - used for the XML export/import and save to DB but shouldn't be used elsewhere.
    /// </summary>
    [PrivateApi("not sure why we have two serialization systems, probably will deprecate this some day")]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    string Serialized { get; }

    [PrivateApi("WIP - not sure if it should be here - should ensure that we have a pure/functional object")]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    IValue With(IImmutableList<ILanguage> newLanguages);
}