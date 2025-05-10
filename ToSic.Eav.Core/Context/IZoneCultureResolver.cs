namespace ToSic.Eav.Context;

/// <summary>
/// Any object implementing this interface can provide the EAV with information about the environment it's running in.
/// </summary>
[PrivateApi("this is not yet ready for publishing, as it's unclear what it actually is")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IZoneCultureResolver
{
    /// <summary>
    /// The current language in the running system. 
    /// This is often based on the url you are on or cookies
    /// </summary>
    /// <remarks>
    /// By convention should always be lower case, so make sure whenever you set this it's lower-cased. 
    /// </remarks>
    string CurrentCultureCode { get; }

    /// <summary>
    /// The primary language in the current environment. 
    /// This is important for value-fallback, as not-translated data
    /// will try to revert to the primary language of the environment
    /// </summary>
    /// <remarks>
    /// By convention should always be lower case, so make sure whenever you set this it's lower-cased. 
    /// </remarks>
    string DefaultCultureCode { get; }
}