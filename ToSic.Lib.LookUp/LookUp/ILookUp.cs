using ToSic.Lib.LookUp.Engines;

namespace ToSic.Lib.LookUp;

/// <summary>
/// Describes a LookUp source similar to a dictionary, which can be used to resolve keys to values.
/// </summary>
/// <remarks>
/// An important aspect of this source is that it is named, since many such sources will be used together.
/// 
/// It's usually used to get pre-stored configuration or to get settings from the context.
/// 
/// Read more about this in [](xref:Abyss.Parts.LookUp.Index)
/// </remarks>
[PublicApi]
public interface ILookUp : ICanBeLookUp
{
    /// <summary>
    /// Gets the Name of this LookUp, e.g. QueryString or PipelineSettings
    /// </summary>
    /// <returns>The name which is used to identify this LookUp, like in a <see cref="ILookUpEngine"/></returns>
    string Name { get; }

    /// <summary>
    /// Additional description to better understand what each LookUp is for.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets a value by Name/key, will simply return the string or an empty string, in rare cases a null-value.
    /// </summary>
    /// <param name="key"></param>
    /// <returns>The resolved value, or an empty string if not found. Note that it could also resolve to an empty string if found - use Has to check for that case.</returns>
    string Get(string key);

    /// <summary>
    /// Gets a value by Name/key and tries to format it in a special way (like for dates)
    /// </summary>
    /// <param name="key">Name of the Property</param>
    /// <param name="format">Format String</param>
    ///// <returns>A string with the result, empty-string if not found.</returns>
    string Get(string key, string format);
}